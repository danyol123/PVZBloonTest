using Raylib_cs;
namespace PVZBloonTest.UI
{
    public class MainMenuScreen
    {
        private float _time = 0f;
        public Difficulty? SelectedDifficulty { get; private set; } = null;

        public void Update(float dt, UnlockManager unlock)
        {
            _time += dt;
            SelectedDifficulty = null;
            if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return;
            var mp = Raylib.GetMousePosition();
            int mx = (int)mp.X, my = (int)mp.Y;
            if (InBtn(mx, my, 440, 310, 400, 55))
                SelectedDifficulty = Difficulty.Easy;
            else if (InBtn(mx, my, 440, 385, 400, 55) && unlock.IsDifficultyUnlocked(Difficulty.Normal))
                SelectedDifficulty = Difficulty.Normal;
            else if (InBtn(mx, my, 440, 460, 400, 55) && unlock.IsDifficultyUnlocked(Difficulty.Hard))
                SelectedDifficulty = Difficulty.Hard;
        }

        public void Draw(UnlockManager unlock)
        {
            float s = System.MathF.Sin(_time * 0.6f) * 0.5f + 0.5f;
            Raylib.DrawRectangleGradientV(0, 0, Constants.SCREEN_W, Constants.SCREEN_H,
                new Color(15, (int)(35 + 15 * s), 15, 255),
                new Color((int)(20 + 15 * s), 55, 20, 255));

            for (int i = 0; i < 14; i++)
            {
                float gx = (i * 95f + _time * 18f) % Constants.SCREEN_W;
                Raylib.DrawRectangle((int)gx, Constants.SCREEN_H - 55, 55, 55, new Color(35, 110, 35, 180));
                Raylib.DrawCircle((int)gx + 27, Constants.SCREEN_H - 65, 27f, new Color(45, 140, 45, 200));
            }

            string title = "PLANTS vs. BLOONS";
            int tw = Fonts.Measure(title, 64);
            Fonts.Draw(title, Constants.SCREEN_W / 2 - tw / 2 + 3, 113, 64, new Color(0, 0, 0, 150));
            Fonts.Draw(title, Constants.SCREEN_W / 2 - tw / 2,     110, 64, new Color(90, 220, 70, 255));

            string sub = "Defend your lawn from BLOONS!";
            int sw = Fonts.Measure(sub, 32);
            Fonts.Draw(sub, Constants.SCREEN_W / 2 - sw / 2, 200, 32, new Color(170, 210, 150, 220));

            DrawBtn(440, 310, 400, 55, "EASY",   unlock.IsDifficultyUnlocked(Difficulty.Easy),   new Color(60, 180, 60, 255));
            DrawBtn(440, 385, 400, 55, "NORMAL", unlock.IsDifficultyUnlocked(Difficulty.Normal), new Color(220, 180, 40, 255));
            DrawBtn(440, 460, 400, 55, "HARD",   unlock.IsDifficultyUnlocked(Difficulty.Hard),   new Color(220, 60, 40, 255));

            if (!unlock.IsDifficultyUnlocked(Difficulty.Normal))
                Fonts.Draw("Beat Easy to unlock", 450, 394, 16, Color.Gray);
            if (!unlock.IsDifficultyUnlocked(Difficulty.Hard))
                Fonts.Draw("Beat Normal to unlock", 450, 469, 16, Color.Gray);
            if (unlock.HardBeaten)
            {
                string badge = "** CHAMPION **";
                Fonts.Draw(badge, Constants.SCREEN_W / 2 - Fonts.Measure(badge, 22) / 2, 535, 22, new Color(255, 220, 30, 255));
            }
        }

        private static void DrawBtn(int x, int y, int w, int h, string label, bool on, Color col)
        {
            Color bg  = on ? new Color(col.R / 2, col.G / 2, col.B / 2, 230) : new Color(40, 40, 40, 200);
            Color txt = on ? col : Color.DarkGray;
            Raylib.DrawRectangle(x, y, w, h, bg);
            Raylib.DrawRectangleLines(x, y, w, h, on ? col : Color.DarkGray);
            int tw = Fonts.Measure(label, 26);
            Fonts.Draw(label, x + w / 2 - tw / 2, y + h / 2 - 13, 26, txt);
        }

        private static bool InBtn(int mx, int my, int x, int y, int w, int h)
            => mx >= x && mx < x + w && my >= y && my < y + h;
    }
}
