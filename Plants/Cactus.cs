using Raylib_cs;
using PVZBloonTest.Projectiles;

namespace PVZBloonTest.Plants
{
    public class Cactus : Plant
    {
        public override PlantType  PlantType    => PlantType.Cactus;
        public override int        BaseCost     => 150;
        public override string[][] UpgradeNames => new[] {
            new[] { "Heat-tipped Spikes", "Headshot!"          },
            new[] { "Thorns",             "Garden of Thorns"   },
            new[] { "Razor Sharp Spikes", "Super-Charged Spikes" }
        };
        public override int[][] UpgradeCosts => new[] {
            new[] { 75, 500 }, new[] { 50, 450 }, new[] { 150, 450 }
        };

        private bool  _heat, _head, _thorns, _garden;
        private int   _extraPierce;
        private float _spikeDmg = 5f, _spikeTimer, _thornTimer;

        public Cactus() { MaxHp = Hp = 10f; Block = 1; PlaceCooldown = 5f; }

        protected override void ApplyUpgrade(int path, int tier)
        {
            switch (path, tier)
            {
                case (0, 1): _heat = true; HasLeadDetection = true; break;
                case (0, 2): _head = true; _spikeDmg += 20f; break;
                case (1, 1): _thorns = true; HasCamoDetection = true; break;
                case (1, 2): _garden = true; break;
                case (2, 1): _extraPierce++; break;
                case (2, 2): _extraPierce += 3; break;
            }
        }

        public override void Update(float dt, GameContext ctx)
        {
            // ── Main spike attack ────────────────────────────────────
            _spikeTimer += dt;
            if (_spikeTimer >= 6f)
            {
                _spikeTimer = 0f;
                if (FindTarget(ctx, HasCamoDetection, HasLeadDetection) != null)
                    ctx.Projectiles.Add(new Spike(Row, CenterX, _spikeDmg, 1 + _extraPierce,
                        _heat, _head, HasCamoDetection));
            }

            // ── Thorns melee attack ───────────────────────────────────
            if (_thorns)
            {
                _thornTimer += dt;
                float interval = 1.5f;
                if (_thornTimer >= interval)
                {
                    // Range 2 tiles; garden: 3 lanes, higher dmg and pierce
                    float thornRange = 2f;
                    float dmg        = _garden ? 8f  : 2f;
                    int   pierce     = _garden ? 3   : 1;
                    int   lanes      = _garden ? 3   : 1;
                    float rangePx    = thornRange * Constants.CELL_W;
                    bool  firedAny   = false;

                    for (int dl = -(lanes / 2); dl <= lanes / 2; dl++)
                    {
                        int r = Row + dl;
                        if (r < 0 || r >= Constants.GRID_ROWS) continue;

                        // Fire if any bloon is within 2 tiles to the right
                        bool hasTarget = false;
                        foreach (var b in ctx.Bloons)
                        {
                            if (b.IsDead || b.Row != r) continue;
                            if (b.X >= CenterX && b.X <= CenterX + rangePx)
                            { hasTarget = true; break; }
                        }
                        if (hasTarget)
                        {
                            ctx.Projectiles.Add(new Thorn(r, CenterX + 20, dmg, pierce, thornRange, _garden));
                            firedAny = true;
                        }
                    }
                    if (firedAny) _thornTimer = 0f;
                }
            }
        }

        public override void Draw()
        {
            DrawUpgradeSprites(Sprites.Cactus, Sprites.CactusT1,
                               Sprites.CactusT2, 100f, 100f);
            DrawPhotoAura();
            DrawHpBar();
        }
    }
}
