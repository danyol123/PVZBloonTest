using Raylib_cs;
namespace PVZBloonTest.UI
{
    public class LevelSelectScreen
    {
        public bool      StartGame         { get; private set; } = false;
        public bool      GoBack            { get; private set; } = false;
        public Difficulty CurrentDifficulty { get; private set; }
        private Difficulty _diff;

        public void SetDifficulty(Difficulty d) { _diff = d; CurrentDifficulty = d; StartGame = false; GoBack = false; }

        public void Update(float dt)
        {
            StartGame = GoBack = false;
            if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return;
            var mp = Raylib.GetMousePosition();
            int mx = (int)mp.X, my = (int)mp.Y;
            if (mx >= 440 && mx < 840 && my >= 420 && my < 480) StartGame = true;
            if (mx >= 440 && mx < 840 && my >= 500 && my < 555) GoBack    = true;
        }

        public void Draw()
        {
            Raylib.ClearBackground(new Color(20, 35, 20, 255));
            string title = $"{_diff} Difficulty";
            int tw = Fonts.Measure(title, 40);
            Color tc = _diff switch { Difficulty.Easy => new Color(80,220,80,255), Difficulty.Normal => new Color(240,200,40,255), _ => new Color(240,80,60,255) };
            Fonts.Draw(title, Constants.SCREEN_W / 2 - tw / 2, 130, 40, tc);

            string[] desc = _diff switch {
                Difficulty.Easy   => new[] { "Bloons: Red, Blue, Yellow", "2 Event Waves", "No upgrades available", "Reward: Unlock Normal + Tier 1 upgrades" },
                Difficulty.Normal => new[] { "Bloons: + Camo, Pink, Lead", "3 Event Waves", "Tier 1 upgrades available", "Reward: Unlock Hard + Tier 2 upgrades" },
                _                 => new[] { "Bloons: + Ceramic, MOAB", "4 Event Waves (MOAB finale!)", "All upgrades available", "Reward: Champion badge!" }
            };
            for (int i = 0; i < desc.Length; i++)
                Fonts.Draw(desc[i], 370, 230 + i * 38, 26, Color.LightGray);

            Raylib.DrawRectangle(440, 420, 400, 60, new Color(50, 140, 50, 230));
            Raylib.DrawRectangleLines(440, 420, 400, 60, Color.Lime);
            string sl = "START GAME";
            Fonts.Draw(sl, 640 - Fonts.Measure(sl, 26) / 2, 436, 26, Color.White);

            Raylib.DrawRectangle(440, 500, 400, 50, new Color(80, 50, 50, 200));
            Raylib.DrawRectangleLines(440, 500, 400, 50, Color.Gray);
            string bl = "Back";
            Fonts.Draw(bl, 640 - Fonts.Measure(bl, 22) / 2, 513, 22, Color.LightGray);
        }
    }
}
