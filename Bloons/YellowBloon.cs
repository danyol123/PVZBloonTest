using Raylib_cs;
namespace PVZBloonTest.Bloons
{
    public class YellowBloon : Bloon
    {
        public override int   Cost     => 3;
        public override Color PopColor => new Color(255, 230, 40, 255);
        public YellowBloon(int row, float x) : base(row, x) { MaxHp = Hp = 30f; BaseSpeed = 1.2f; }
        public override void Draw() => DrawBase(new Color(255, 230, 40, 255), Color.Gold, Sprites.BloonYellow);
    }
}
