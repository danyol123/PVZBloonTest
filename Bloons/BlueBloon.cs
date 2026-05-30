using Raylib_cs;
namespace PVZBloonTest.Bloons
{
    public class BlueBloon : Bloon
    {
        public override int   Cost     => 2;
        public override Color PopColor => new Color(60, 100, 220, 255);
        public BlueBloon(int row, float x) : base(row, x) { MaxHp = Hp = 10f; BaseSpeed = 1.1f; }
        public override void Draw() => DrawBase(new Color(60, 100, 220, 255), Color.DarkBlue, Sprites.BloonBlue);
    }
}
