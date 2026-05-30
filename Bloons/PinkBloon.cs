using Raylib_cs;
namespace PVZBloonTest.Bloons
{
    public class PinkBloon : Bloon
    {
        public override int   Cost     => 4;
        public override Color PopColor => new Color(255, 140, 200, 255);
        public PinkBloon(int row, float x) : base(row, x) { MaxHp = Hp = 25f; BaseSpeed = 2.2f; }
        public override void Draw() => DrawBase(new Color(255, 140, 200, 255), new Color(200, 60, 140, 255), Sprites.BloonPink);
    }
}
