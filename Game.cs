using Raylib_cs;
using PVZBloonTest.Plants;
using PVZBloonTest.Bloons;
using PVZBloonTest.Particles;
using PVZBloonTest.UI;

namespace PVZBloonTest
{
    public class Game
    {
        private GameState              _state    = GameState.MainMenu;
        private readonly GameContext        _ctx      = new();
        private readonly MainMenuScreen    _mainMenu  = new();
        private readonly LevelSelectScreen _levelSel  = new();
        private readonly HUD               _hud       = new();
        private readonly UpgradePanel      _panel     = new();
        private float _endTimer = 0f;

        // private Font _font; -- Font is now managed globally via Fonts.cs

        public Game() { _ctx.UnlockManager = new UnlockManager(); }

        // ── Main loop ────────────────────────────────────────────────
        public void Update(float dt)
        {
            switch (_state)
            {
                case GameState.MainMenu:    UpdateMenu(dt);     break;
                case GameState.LevelSelect: UpdateLevelSel(dt); break;
                case GameState.Playing:     UpdatePlaying(dt);  break;
                case GameState.Victory:
                case GameState.GameOver:    UpdateEnd(dt);      break;
            }
        }

        public void Draw()
        {
            Raylib.BeginDrawing();
            switch (_state)
            {
                case GameState.MainMenu:    _mainMenu.Draw(_ctx.UnlockManager); break;
                case GameState.LevelSelect: _levelSel.Draw(); break;
                case GameState.Playing:     DrawPlaying(); break;
                case GameState.Victory:     DrawOverlay("VICTORY!",  new Color(80, 255, 60, 255), GetVictoryMsg()); break;
                case GameState.GameOver:    DrawOverlay("GAME OVER", new Color(255, 60, 40, 255), "The bloons broke through!"); break;
            }
            Raylib.EndDrawing();
        }

        // ── Menus ────────────────────────────────────────────────────
        private void UpdateMenu(float dt)
        {
            _mainMenu.Update(dt, _ctx.UnlockManager);
            if (_mainMenu.SelectedDifficulty.HasValue)
            { _levelSel.SetDifficulty(_mainMenu.SelectedDifficulty.Value); _state = GameState.LevelSelect; }
        }
        private void UpdateLevelSel(float dt)
        {
            _levelSel.Update(dt);
            if (_levelSel.StartGame) StartGame(_levelSel.CurrentDifficulty);
            if (_levelSel.GoBack)   _state = GameState.MainMenu;
        }

        // ── Start game ───────────────────────────────────────────────
        private void StartGame(Difficulty diff)
        {
            _ctx.Reset(diff);
            _ctx.WaveManager = new WaveManager(diff);
            _hud.Deselect();
            _panel.Close();
            _state = GameState.Playing;
        }

        // ── Playing ──────────────────────────────────────────────────
        private void UpdatePlaying(float dt)
        {
            // ── Slow down simulation while upgrade panel is open ─────────────────
            // (Placing a plant does NOT pause: panel-only)
            bool paused  = _panel.IsOpen;
            float gameDt = paused ? dt * 0.5f : dt;

            _ctx.SunManager.Update(gameDt, _ctx);  // sun slows down while paused
            _hud.UpdateCooldowns(gameDt);           // cooldowns slows down while paused

            var mp = Raylib.GetMousePosition();
            int mx = (int)mp.X, my = (int)mp.Y;
            bool leftClicked = Raylib.IsMouseButtonPressed(MouseButton.Left);

            // ── Sun drops ────────────────────────────────────────────
            foreach (var d in _ctx.SunDrops) d.Update(gameDt);

            // Hover collection — sun gathered by moving mouse over it
            foreach (var d in _ctx.SunDrops)
                if (!d.SunClaimed && d.TryHover(mx, my))
                { _ctx.SunManager.AddSun(d.Value); d.SunClaimed = true; ParticleManager.Instance.SunCollect(d.X, d.Y); }

            // Auto-collect any that reached the bottom of the lawn
            foreach (var d in _ctx.SunDrops)
                if (d.IsExpired && !d.SunClaimed) { _ctx.SunManager.AddSun(d.Value); d.SunClaimed = true; ParticleManager.Instance.SunCollect(d.X, d.Y); }
            _ctx.SunDrops.RemoveAll(d => d.IsExpired);

            // ── Upgrade panel ────────────────────────────────────────
            bool panelConsumed = _panel.Update(_ctx);

            // Close panel when clicking outside its rectangle
            if (leftClicked && !panelConsumed && _panel.IsOpen)
            {
                int px = Constants.SCREEN_W - 290 - 10, py = Constants.HUD_H + 10;
                bool inPanel = mx >= px && mx < px + 290 && my >= py && my < py + 452;
                if (!inPanel) _panel.Close();
            }

            if (!panelConsumed)
            {
                _hud.Update(_ctx);
                if (leftClicked && my >= Constants.HUD_H)
                {
                    if (Grid.ScreenToCell(mx, my, out int col, out int row))
                        HandleGridClick(col, row);
                    else if (_panel.IsOpen)
                        _panel.Close();
                }
                if (Raylib.IsMouseButtonPressed(MouseButton.Right)) _hud.Deselect();
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            { if (_panel.IsOpen) _panel.Close(); else _hud.Deselect(); }

            // ── Plants ───────────────────────────────────────────────
            foreach (var p in _ctx.Grid.AllPlants()) if (!p.IsDead) p.Update(gameDt, _ctx);
            for (int c = 0; c < Constants.GRID_COLS; c++)
            {
                for (int r = 0; r < Constants.GRID_ROWS; r++)
                {
                    var p = _ctx.Grid.GetPlant(c, r);
                    if (p != null && p.IsDead)
                    {
                        if (p is Wallnut wn && wn.HasExplode)
                            ExplodeArea(c, r, 30f);
                        if (_panel.TargetPlant == p) _panel.Close();
                        ParticleManager.Instance.PlantDeath(Grid.CellCenterX(c), Grid.CellCenterY(r));
                        _ctx.Grid.RemovePlant(c, r);
                    }
                }
            }

            // ── Bloons ───────────────────────────────────────────────
            HandleBlocking();
            foreach (var b in _ctx.Bloons) b.Update(gameDt, _ctx);
            _ctx.Bloons.RemoveAll(b => b.IsDead);
            _ctx.Bloons.AddRange(_ctx.BloonsToAdd);
            _ctx.BloonsToAdd.Clear();

            // ── Projectiles ──────────────────────────────────────────
            foreach (var p in _ctx.Projectiles) p.Update(gameDt, _ctx);
            _ctx.Projectiles.RemoveAll(p => p.IsDead);

            // ── Particles ────────────────────────────────────────────
            ParticleManager.Instance.Update(gameDt);

            // ── Waves ────────────────────────────────────────────────
            _ctx.WaveManager.Update(gameDt, _ctx);

            if (_ctx.Lives <= 0) { _endTimer = 0; _state = GameState.GameOver; }
            if (_ctx.WaveManager.RoundComplete && _ctx.Bloons.Count == 0)
            { _ctx.UnlockManager.UnlockAfterBeating(_ctx.Difficulty); _endTimer = 0; _state = GameState.Victory; }
        }

        private void HandleGridClick(int col, int row)
        {
            var existing = _ctx.Grid.GetPlant(col, row);

            // Shovel mode
            if (_hud.ShovelSelected)
            {
                if (existing != null)
                {
                    if (_panel.TargetPlant == existing) _panel.Close();
                    ParticleManager.Instance.PlantDeath(Grid.CellCenterX(col), Grid.CellCenterY(row));
                    _ctx.Grid.RemovePlant(col, row);
                }
                return;
            }

            // Click existing plant → open/switch/close upgrade panel
            if (existing != null && _hud.SelectedCard < 0)
            {
                if (_panel.IsOpen && _panel.TargetPlant == existing) _panel.Close();
                else _panel.TargetPlant = existing;
                return;
            }

            // Click empty cell → close panel if open and no card selected
            if (existing == null && _panel.IsOpen && _hud.SelectedCard < 0)
            { _panel.Close(); return; }

            // Place plant
            if (_hud.SelectedCard >= 0 && _hud.SelectedCard < 4 && existing == null)
            {
                if (!_hud.IsOnCooldown(_hud.SelectedCard) && _ctx.SunManager.SpendSun(_hud.SelectedPlantCost))
                {
                    Plant plant = _hud.SelectedPlantType switch
                    {
                        PlantType.Sunflower  => new Sunflower(),
                        PlantType.Wallnut    => new Wallnut(),
                        PlantType.Peashooter => new Peashooter(),
                        PlantType.Cactus     => new Cactus(),
                        _                    => new Sunflower()
                    };
                    _ctx.Grid.PlacePlant(col, row, plant);
                    ParticleManager.Instance.PlantPlace(Grid.CellCenterX(col), Grid.CellCenterY(row));
                    _hud.StartCooldown(_hud.SelectedCard);  // start cooldown for this card
                    _hud.Deselect();                         // deselect after planting
                }
            }
        }

        private void HandleBlocking()
        {
            foreach (var b in _ctx.Bloons)
            {
                if (b.IsDead || b.IsBlocked) continue;
                int bc = (int)((b.X - Constants.GRID_X) / Constants.CELL_W);
                if (bc < 0 || bc >= Constants.GRID_COLS) { b.IsPassthrough = false; continue; }
                var plant = _ctx.Grid.GetPlant(bc, b.Row);
                if (plant == null || plant.IsDead) { b.IsPassthrough = false; continue; }
                if (plant.CurrentlyBlocking < plant.Block) b.SetBlock(plant);
                else b.IsPassthrough = true;
            }
        }

        private void ExplodeArea(int col, int row, float dmg)
        {
            ParticleManager.Instance.Explosion(Grid.CellCenterX(col), Grid.CellCenterY(row));
            foreach (var b in _ctx.Bloons)
            {
                if (b.IsDead) continue;
                int bc = (int)((b.X - Constants.GRID_X) / Constants.CELL_W);
                if (System.Math.Abs(bc - col) <= 1 && System.Math.Abs(b.Row - row) <= 1)
                { b.TakeDamage(dmg, true, true); if (b.IsDead) b.OnDeath(_ctx); }
            }
        }

        // ── Draw playing ─────────────────────────────────────────────
        private void DrawPlaying()
        {
            Raylib.ClearBackground(new Color(15, 25, 15, 255));
            _ctx.Grid.Draw(_ctx.IsNightMode);

            // ── Plant-tile hover highlight (no card held, not shovel) ─────────
            // Hints to the player that clicking a plant opens its upgrade panel.
            {
                var mpH = Raylib.GetMousePosition();
                if (_hud.SelectedCard < 0
                    && Grid.ScreenToCell((int)mpH.X, (int)mpH.Y, out int hCol, out int hRow)
                    && _ctx.Grid.GetPlant(hCol, hRow) != null)
                {
                    Raylib.DrawRectangle(
                        Constants.GRID_X + hCol * Constants.CELL_W,
                        Constants.GRID_Y + hRow * Constants.CELL_H,
                        Constants.CELL_W, Constants.CELL_H,
                        new Color(255, 210, 60, 45));
                    Raylib.DrawRectangleLines(
                        Constants.GRID_X + hCol * Constants.CELL_W,
                        Constants.GRID_Y + hRow * Constants.CELL_H,
                        Constants.CELL_W, Constants.CELL_H,
                        new Color(255, 220, 80, 160));
                }
            }

            foreach (var p in _ctx.Grid.AllPlants()) p.Draw();

            if (_panel.IsOpen && _panel.TargetPlant != null)
            {
                var tp = _panel.TargetPlant;
                Raylib.DrawRectangleLines(
                    Constants.GRID_X + tp.Col * Constants.CELL_W,
                    Constants.GRID_Y + tp.Row * Constants.CELL_H,
                    Constants.CELL_W, Constants.CELL_H, new Color(255, 220, 50, 255));
            }

            foreach (var b in _ctx.Bloons)    b.Draw();
            foreach (var p in _ctx.Projectiles) p.Draw();
            ParticleManager.Instance.Draw();     // particles above bloons
            foreach (var d in _ctx.SunDrops)  d.Draw();

            _hud.Draw(_ctx);
            _panel.Draw(_ctx);

            // ── Tile hover highlight when holding a plant card ───────────────
            if (_hud.SelectedCard >= 0 && _hud.SelectedCard < 4)
            {
                var mp2 = Raylib.GetMousePosition();
                if (Grid.ScreenToCell((int)mp2.X, (int)mp2.Y, out int hCol, out int hRow))
                {
                    bool isEmpty   = _ctx.Grid.GetPlant(hCol, hRow) == null;
                    bool canAfford = _ctx.SunManager.Sun >= _hud.SelectedPlantCost;
                    bool valid     = isEmpty && canAfford && !_hud.IsOnCooldown(_hud.SelectedCard);
                    Color fill   = valid ? new Color(100, 220, 100, 70) : new Color(220, 70, 70, 70);
                    Color border = valid ? new Color(100, 255, 100, 200) : new Color(255, 70, 70, 200);
                    Raylib.DrawRectangle(
                        Constants.GRID_X + hCol * Constants.CELL_W,
                        Constants.GRID_Y + hRow * Constants.CELL_H,
                        Constants.CELL_W, Constants.CELL_H, fill);
                    Raylib.DrawRectangleLines(
                        Constants.GRID_X + hCol * Constants.CELL_W,
                        Constants.GRID_Y + hRow * Constants.CELL_H,
                        Constants.CELL_W, Constants.CELL_H, border);
                }
                // Cursor ghost circle
                var mp = Raylib.GetMousePosition();
                Raylib.DrawCircle((int)mp.X, (int)mp.Y, 12f, new Color(100, 200, 100, 60));
            }

            // ── Event banner ─────────────────────────────────────────
            if (_ctx.EventNotificationTimer > 0f && _ctx.EventNotification.Length > 0)
            {
                float alpha = System.Math.Min(1f, _ctx.EventNotificationTimer / 0.5f);
                int ia  = (int)(220 * alpha);
                string msg = _ctx.EventNotification;
                int   tw  = Fonts.Measure(msg, 30);
                int   bw  = tw + 40, bh = 50;
                int   bx  = Constants.SCREEN_W / 2 - bw / 2;
                int   by  = Constants.SCREEN_H / 2 - 60;
                Raylib.DrawRectangle(bx, by, bw, bh, new Color(10, 10, 30, ia));
                Raylib.DrawRectangleLines(bx, by, bw, bh, new Color(200, 150, 255, ia));
                Fonts.Draw(msg, Constants.SCREEN_W / 2 - tw / 2, by + 10, 30, new Color(255, 220, 80, ia));
            }

            // ── DEBUG: wave budget ────────────────────────────────────────
            if (_ctx.WaveManager != null)
            {
                string dbg = $"Budget: {_ctx.WaveManager.BudgetLeft} / {_ctx.WaveManager.TotalBudget}";
                Fonts.Draw(dbg, Constants.SCREEN_W - Fonts.Measure(dbg, 14) - 6, Constants.HUD_H + 4, 14, new Color(180, 255, 180, 180));
            }
        }

        // ── End screens ───────────────────────────────────────────────
        private void UpdateEnd(float dt)
        {
            _endTimer += dt;
            if (_endTimer > 1.5f && (Raylib.IsMouseButtonPressed(MouseButton.Left) || Raylib.IsKeyPressed(KeyboardKey.Escape)))
                _state = GameState.MainMenu;
        }

        private void DrawOverlay(string heading, Color headCol, string sub)
        {
            DrawPlaying();
            Color tint = heading == "VICTORY!" ? new Color(0, 60, 0, 170) : new Color(60, 0, 0, 170);
            Raylib.DrawRectangle(0, 0, Constants.SCREEN_W, Constants.SCREEN_H, tint);
            int hw = Fonts.Measure(heading, 80);
            Fonts.Draw(heading, Constants.SCREEN_W / 2 - hw / 2, 260, 80, headCol);
            int sw = Fonts.Measure(sub, 22);
            Fonts.Draw(sub, Constants.SCREEN_W / 2 - sw / 2, 370, 22, Color.White);
            string hint = "Click or ESC to return to menu";
            Fonts.Draw(hint, Constants.SCREEN_W / 2 - Fonts.Measure(hint, 18) / 2, 430, 18, Color.LightGray);
        }

        private string GetVictoryMsg() => _ctx.Difficulty switch
        {
            Difficulty.Easy   => "Normal + Tier 1 upgrades unlocked!",
            Difficulty.Normal => "Hard + Tier 2 upgrades unlocked!",
            _                 => "You earned the Champion badge!"
        };
    }
}
