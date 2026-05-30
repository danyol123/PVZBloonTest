using Raylib_cs;
namespace PVZBloonTest.Bloons
{
    public class CamoBloon : Bloon
    {
        public override int   Cost     => 5;
        public override Color PopColor => new Color(120, 200, 120, 255);
        public CamoBloon(int row, float x) : base(row, x) { MaxHp = Hp = 10f; BaseSpeed = 1.5f; IsCamo = true; }
        public override void Draw()
        {
            // Slightly transparent — alpha 160/255 so background shows through
            DrawBase(new Color(120, 200, 120, 160), new Color(60, 140, 60, 200),
                     Sprites.BloonCamo, new Color(255, 255, 255, 160));
        }
    }
}
