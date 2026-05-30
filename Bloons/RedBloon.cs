using Raylib_cs;
namespace PVZBloonTest.Bloons
{
    public class RedBloon : Bloon
    {
        public override int   Cost     => 1;
        public override Color PopColor => new Color(220, 50, 50, 255);
        public RedBloon(int row, float x) : base(row, x) { MaxHp = Hp = 5f; BaseSpeed = 1f; }
        public override void Draw() => DrawBase(new Color(220, 50, 50, 255), Color.Maroon, Sprites.BloonRed);
    }
}
