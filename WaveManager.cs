using System;
using System.Collections.Generic;
using PVZBloonTest.Bloons;
using PVZBloonTest.Events;

namespace PVZBloonTest
{
    public class WaveManager
    {
        private readonly Difficulty _diff;
        private readonly int   _totalWaves;
        private readonly float _waveDuration;
        private readonly float _baseInterval;
        private readonly float _minInterval;
        private float _segTimer, _spawnTimer;
        private int   _currentSeg    = 0;
        private bool[] _eventFired;
        private bool   _roundComplete = false;
        private readonly List<EventWave> _pool = new();
        private readonly HashSet<string> _used = new();
        private readonly Random          _rng  = new();

        private readonly int[] _waveBudgets;

        // Pre-generated wave queue
        private readonly Queue<Func<Bloon>> _waveQueue = new();
        private int _bloonsTotal;
        private int _bloonsRemaining;

        // % cleared → early event trigger
        private readonly HashSet<Bloon> _waveBloons      = new();
        private int                     _waveDeployed    = 0;
        private bool                    _earlyTriggered  = false;

        // ── Tunable thresholds ───────────────────────────────────────────────
        /// <summary>Fraction of deployed bloons that must be defeated to trigger the early rush.</summary>
        private const float EARLY_TRIGGER_THRESHOLD    = 0.8f;
        /// <summary>Minimum fraction of the wave's total bloons that must be deployed before the threshold is checked.
        /// Prevents an instant trigger at the very start of a wave when only 1-2 bloons have spawned.</summary>
        private const float EARLY_TRIGGER_MIN_DEPLOYED = 0.8f;
        /// <summary>Seconds of "WAVE INCOMING!" warning before the rush actually fires.</summary>
        private const float EARLY_TRIGGER_DELAY        = 2.5f;

        // Early-trigger countdown (-1 = not pending)
        private float _earlyTriggerCountdown = -1f;

        // Scaled column rush
        private int   _columnRushPending      = 0;
        private float _columnRushTimer        = 0f;
        private int   _columnRushBudgetPerCol = 0;
        private const float COLUMN_RUSH_DELAY = 1.5f;

        public bool   RoundComplete => _roundComplete;
        public string WaveLabel     => _currentSeg < _totalWaves ? $"Wave {_currentSeg + 1} / {_totalWaves}" : "Done!";
        public float  WaveProgress  => _currentSeg < _totalWaves ? Math.Clamp(_segTimer / _waveDuration, 0f, 1f) : 1f;
        public int    BudgetLeft    => _bloonsRemaining;
        public int    TotalBudget   => _bloonsTotal;

        private sealed record BloonDef(Func<int, float, Bloon> Factory, int Cost, int Weight);

        public WaveManager(Difficulty diff)
        {
            _diff = diff;
            switch (diff)
            {
                case Difficulty.Easy:
                    _totalWaves = 2; _waveDuration = 90f;
                    _baseInterval = 7f; _minInterval = 1.5f;
                    _waveBudgets  = new[] { 50, 90 };
                    _pool.AddRange(new EventWave[] { new SpeedyWindsEvent(), new ToughRubberEvent() });
                    break;
                case Difficulty.Normal:
                    _totalWaves = 3; _waveDuration = 85f;
                    _baseInterval = 5f; _minInterval = 1.0f;
                    _waveBudgets  = new[] { 60, 110, 190 };
                    _pool.AddRange(new EventWave[] { new SpeedyWindsEvent(), new ToughRubberEvent(), new AmbushEvent(), new FeelTheThunderEvent() });
                    break;
                default: // Hard
                    _totalWaves = 4; _waveDuration = 80f;
                    _baseInterval = 3.5f; _minInterval = 0.7f;
                    _waveBudgets  = new[] { 80, 140, 250, 400 };
                    _pool.AddRange(new EventWave[] { new SpeedyWindsEvent(), new ToughRubberEvent(), new AmbushEvent(), new FeelTheThunderEvent(), new SweetDreamsEvent() });
                    break;
            }
            _eventFired = new bool[_totalWaves];
            PreGenerateWave(0);
        }

        // ── Available bloon pool per segment/difficulty ──────────────────────
        private List<BloonDef> GetAvailableBloons(int seg)
        {
            var list = new List<BloonDef>();
            if (_diff == Difficulty.Easy)
            {
                list.Add(new BloonDef((r, sx) => new RedBloon(r, sx),  1, 4000));
                list.Add(new BloonDef((r, sx) => new BlueBloon(r, sx), 2, 3000));
                if (seg >= 1) list.Add(new BloonDef((r, sx) => new YellowBloon(r, sx), 3, 2000));
            }
            else if (_diff == Difficulty.Normal)
            {
                list.Add(new BloonDef((r, sx) => new RedBloon(r, sx),    1, 4000));
                list.Add(new BloonDef((r, sx) => new BlueBloon(r, sx),   2, 3000));
                list.Add(new BloonDef((r, sx) => new YellowBloon(r, sx), 3, 2200));
                if (seg >= 1)
                {
                    list.Add(new BloonDef((r, sx) => new PinkBloon(r, sx), 4, 1800));
                    list.Add(new BloonDef((r, sx) => new CamoBloon(r, sx), 5, 1200));
                }
                if (seg >= 2) list.Add(new BloonDef((r, sx) => new LeadBloon(r, sx), 8, 700));
            }
            else // Hard
            {
                list.Add(new BloonDef((r, sx) => new RedBloon(r, sx),    1, 4000));
                list.Add(new BloonDef((r, sx) => new BlueBloon(r, sx),   2, 3000));
                list.Add(new BloonDef((r, sx) => new YellowBloon(r, sx), 3, 2200));
                list.Add(new BloonDef((r, sx) => new CamoBloon(r, sx),   5, 1500));
                if (seg >= 1)
                {
                    list.Add(new BloonDef((r, sx) => new PinkBloon(r, sx),   4, 1800));
                    list.Add(new BloonDef((r, sx) => new LeadBloon(r, sx),   8,  700));
                }
                if (seg >= 2) list.Add(new BloonDef((r, sx) => new CeramicBloon(r, sx), 15, 250));
            }
            return list;
        }

        // ── Pre-generate full wave composition upfront (PVZ-style) ───────────
        private void PreGenerateWave(int seg)
        {
            _waveQueue.Clear();
            _waveBloons.Clear();
            _waveDeployed        = 0;
            _earlyTriggered      = false;
            _earlyTriggerCountdown = -1f;

            float sx        = Constants.GRID_X + Constants.GRID_COLS * Constants.CELL_W + 20f;
            int   budget    = _waveBudgets[seg];
            var   available = GetAvailableBloons(seg);
            var   generated = new List<Func<Bloon>>();

            while (budget > 0)
            {
                var affordable = available.FindAll(e => e.Cost <= budget);
                if (affordable.Count == 0) break;

                var   chosen  = WeightedPick(affordable);
                int   capRow  = _rng.Next(0, Constants.GRID_ROWS);
                float capSx   = sx;
                var   capFact = chosen.Factory;
                generated.Add(() => capFact(capRow, capSx));
                budget -= chosen.Cost;
            }

            Shuffle(generated);

            // Guarantee the very first bloon of wave 1 is always a Red
            if (seg == 0)
            {
                int   redRow = _rng.Next(0, Constants.GRID_ROWS);
                float capSx  = sx;
                generated.Insert(0, () => new RedBloon(redRow, capSx));
            }

            foreach (var f in generated) _waveQueue.Enqueue(f);
            _bloonsTotal     = _waveQueue.Count;
            _bloonsRemaining = _bloonsTotal;
        }

        // ── Weighted random pick ─────────────────────────────────────────────
        private BloonDef WeightedPick(List<BloonDef> pool)
        {
            int total = 0;
            foreach (var e in pool) total += e.Weight;
            int roll = _rng.Next(0, total), cum = 0;
            foreach (var e in pool) { cum += e.Weight; if (roll < cum) return e; }
            return pool[pool.Count - 1];
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            { int j = _rng.Next(i + 1); (list[i], list[j]) = (list[j], list[i]); }
        }

        /// <summary>Current spawn interval ramping down as the wave progresses.</summary>
        private float CurrentInterval()
        {
            float prog     = Math.Clamp(_segTimer / _waveDuration, 0f, 1f);
            float segBonus = MathF.Pow(0.60f, _currentSeg);
            float interval = _baseInterval * (1f - prog * 0.65f) * segBonus;
            return Math.Max(_minInterval, interval);
        }

        public void Update(float dt, GameContext ctx)
        {
            if (_roundComplete) return;
            if (ctx.EventNotificationTimer > 0f) ctx.EventNotificationTimer -= dt;

            // ── Tick column rush ─────────────────────────────────────────────
            if (_columnRushPending > 0)
            {
                _columnRushTimer -= dt;
                if (_columnRushTimer <= 0f)
                {
                    SpawnOneColumn(ctx, _columnRushBudgetPerCol);
                    _columnRushPending--;
                    _columnRushTimer = COLUMN_RUSH_DELAY;
                }
            }

            if (_currentSeg >= _totalWaves)
            {
                if (ctx.Bloons.Count == 0 && ctx.BloonsToAdd.Count == 0)
                    _roundComplete = true;
                return;
            }

            _segTimer   += dt;
            _spawnTimer += dt;

            // ── Early-trigger: threshold check → countdown → fire ────────────
            // Guard: only arm once EARLY_TRIGGER_MIN_DEPLOYED fraction of the
            // pre-generated wave has actually been sent (prevents instant trigger
            // at wave start when only 1-2 bloons have spawned and been popped).
            int minDeployedNeeded = Math.Max(3, (int)(_bloonsTotal * EARLY_TRIGGER_MIN_DEPLOYED));
            if (!_eventFired[_currentSeg] && !_earlyTriggered && _waveDeployed >= minDeployedNeeded)
            {
                int alive = 0;
                foreach (var b in _waveBloons) if (!b.IsDead) alive++;
                float defeatedFrac = 1f - (float)alive / _waveDeployed;

                // Threshold crossed → start countdown and show warning banner
                if (defeatedFrac >= EARLY_TRIGGER_THRESHOLD && _earlyTriggerCountdown < 0f)
                {
                    _earlyTriggerCountdown     = EARLY_TRIGGER_DELAY;
                    ctx.EventNotification      = "WAVE INCOMING!";
                    ctx.EventNotificationTimer = EARLY_TRIGGER_DELAY + 0.5f;
                }
            }

            // Countdown to actual skip
            if (_earlyTriggerCountdown >= 0f && !_eventFired[_currentSeg])
            {
                _earlyTriggerCountdown -= dt;
                if (_earlyTriggerCountdown <= 0f)
                {
                    _earlyTriggered      = true;
                    _earlyTriggerCountdown = -1f;
                    // Drain remaining queue immediately
                    while (_waveQueue.Count > 0)
                    {
                        var b = _waveQueue.Dequeue()();
                        ApplyEventMults(b, ctx);
                        ctx.BloonsToAdd.Add(b);
                    }
                    _bloonsRemaining = 0;
                    // Force event trigger this frame
                    _segTimer = _waveDuration;
                }
            }

            // ── Regular spawn from pre-generated queue ───────────────────────
            if (_spawnTimer >= CurrentInterval() && _waveQueue.Count > 0)
            {
                _spawnTimer = 0f;
                float prog = Math.Clamp(_segTimer / _waveDuration, 0f, 1f);
                int spawnCount = prog < 0.4f ? 1 : prog < 0.75f ? 2 : 3;

                for (int i = 0; i < spawnCount && _waveQueue.Count > 0; i++)
                {
                    var b = _waveQueue.Dequeue()();
                    ApplyEventMults(b, ctx);
                    ctx.BloonsToAdd.Add(b);
                    _waveBloons.Add(b);
                    _waveDeployed++;
                    _bloonsRemaining = _waveQueue.Count;
                }
            }

            // ── Wave-end event trigger ───────────────────────────────────────
            if (_segTimer >= _waveDuration && !_eventFired[_currentSeg])
            {
                _eventFired[_currentSeg] = true;
                InitiateColumnRush();
                TriggerEvent(ctx);
            }

            // ── Advance to next segment after grace period ───────────────────
            if (_segTimer >= _waveDuration + 5f)
            {
                _segTimer      = 0f;
                _spawnTimer    = 0f;
                _currentSeg++;
                ctx.IsNightMode    = false;
                ctx.SpeedEventMult = 1f;
                ctx.HpEventMult    = 1f;
                if (_currentSeg < _totalWaves)
                    PreGenerateWave(_currentSeg);
            }
        }

        // ── Column rush: wave N fires N columns with a short delay each ──────
        private void InitiateColumnRush()
        {
            _columnRushPending      = _currentSeg + 1;          // wave 1→1, wave 2→2, ...
            _columnRushTimer        = 0f;                        // first column fires immediately
            _columnRushBudgetPerCol = 10 + 15 * (_currentSeg + 1); // grows with each wave
        }

        /// <summary>Spawn one column (all rows) using a weighted+budget pick.</summary>
        private void SpawnOneColumn(GameContext ctx, int budget)
        {
            float sx      = Constants.GRID_X + Constants.GRID_COLS * Constants.CELL_W + 20f;
            var available = GetAvailableBloons(_currentSeg);

            for (int row = 0; row < Constants.GRID_ROWS; row++)
            {
                var affordable = available.FindAll(e => e.Cost <= budget);
                BloonDef entry = affordable.Count > 0
                    ? WeightedPick(affordable)
                    : available[0]; // fallback to Red

                if (affordable.Count > 0) budget -= entry.Cost;

                var b = entry.Factory(row, sx);
                ApplyEventMults(b, ctx);
                ctx.BloonsToAdd.Add(b);
            }
        }

        private static void ApplyEventMults(Bloon b, GameContext ctx)
        {
            b.MaxHp     = b.Hp = b.Hp * ctx.HpEventMult;
            b.SpeedMult = ctx.SpeedEventMult;
        }

        private void TriggerEvent(GameContext ctx)
        {
            if (_diff == Difficulty.Hard && _currentSeg == _totalWaves - 1)
            { var mv = new MoabEvent(); mv.Trigger(ctx, _diff); SetNotification(ctx, mv.Name); return; }

            var avail = new List<EventWave>();
            foreach (var e in _pool) if (!_used.Contains(e.Name)) avail.Add(e);
            if (avail.Count == 0) return;

            var ev = avail[_rng.Next(avail.Count)];
            _used.Add(ev.Name);
            ev.Trigger(ctx, _diff);
            SetNotification(ctx, ev.Name);
        }

        private static void SetNotification(GameContext ctx, string name)
        {
            ctx.EventNotification      = $"EVENT: {name}";
            ctx.EventNotificationTimer = 4f;
        }
    }
}
