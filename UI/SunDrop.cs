using Raylib_cs;
namespace PVZBloonTest.UI
{
    public class SunDrop
    {
        public float X          { get; private set; }
        public float Y          { get; private set; }
        public int   Value      { get; }
        public bool  Clicked    { get; private set; } = false;
        public bool  SunClaimed { get; set; } = false;
        private float _life = 10f, _angle = 0f;
        private static readonly float _bottomY = Constants.GRID_Y + Constants.GRID_ROWS * Constants.CELL_H;

        public SunDrop(float x, float y, int value, bool fromBloon) { X = x; Y = y; Value = value; }

        public void Update(float dt)
        {
            _life  -= dt;
            _angle += dt * 120f;
            Y      += 22f * dt;
            if (Y >= _bottomY) Clicked = true;
        }

        public bool IsExpired => _life <= 0f || Clicked;

        public bool TryHover(int mx, int my)
        {
            if (SunClaimed) return false;
            float dx = mx - X, dy = my - Y;
            if (dx * dx + dy * dy < 26f * 26f) { Clicked = true; return true; }
            return false;
        }

        public void Draw()
        {
            float pulse = 1f + 0.08f * System.MathF.Sin(_angle * 0.05f);
            float r = 22f * pulse;
            int cx = (int)X, cy = (int)Y;
            // Soft glow halo
            Raylib.DrawCircle(cx, cy, r + 6f, new Color(255, 220, 0, 35));
            // Sprite (scales with pulse)
            Sprites.DrawCentered(Sprites.SunDropTex, X, Y, r * 2f, r * 2f);
            // "+value" label below
            string lbl = $"+{Value}";
            int tw = Fonts.Measure(lbl, 11);
            Fonts.Draw(lbl, cx - tw / 2, cy + (int)r + 3, 11, new Color(200, 160, 0, 255));
        }
    }
}
