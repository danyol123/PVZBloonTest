using Raylib_cs;
namespace PVZBloonTest.Bloons
{
    public class MoabBloon : Bloon
    {
        public override int   Cost     => 50;
        public override Color PopColor => new Color(60, 100, 255, 255);
        public MoabBloon(int row, float x) : base(row, x)
        { MaxHp = Hp = 800f; BaseSpeed = 0.6f; Block = 10; DmgPerSec = 20f; Radius = 64f; }

        public override void Draw()
        {
            // MOAB is a wide blimp — draw it wider than tall, centred on DrawY
            float w = Radius * 2.5f, h = Radius * 1.4f;
            Sprites.DrawCentered(Sprites.BloonMoab, X, DrawY, w, h);
            // HP bar
            float pct = MaxHp > 0 ? Hp / MaxHp : 0;
            int cx = (int)X, cy = (int)DrawY;
            Raylib.DrawRectangle(cx - 35, cy - (int)(h / 2f) - 8, 70, 5, Color.DarkGray);
            Raylib.DrawRectangle(cx - 35, cy - (int)(h / 2f) - 8, (int)(70 * pct), 5, Color.Lime);
        }

        public override void OnDeath(GameContext ctx)
            => ctx.BloonsToAdd.Add(new CeramicBloon(Row, X));
    }
}
