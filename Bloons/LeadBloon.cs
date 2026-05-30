using Raylib_cs;
namespace PVZBloonTest.Bloons
{
    public class LeadBloon : Bloon
    {
        public override int   Cost     => 8;
        public override Color PopColor => new Color(160, 160, 180, 255);
        public LeadBloon(int row, float x) : base(row, x) { MaxHp = Hp = 60f; BaseSpeed = 0.8f; IsLead = true; Radius = 26f; }

        public override void Draw()
        {
            if (IsLead)
            {
                // Shielded: draw lead_shield sprite slightly bigger than the base radius
                float shieldSize = Radius * 2.6f;
                Sprites.DrawCentered(Sprites.BloonLeadShield, X, DrawY, shieldSize, shieldSize);
            }
            else
            {
                // Shield stripped: draw normal bloon sprite at standard size
                DrawBase(new Color(160, 160, 180, 255), new Color(80, 80, 100, 255), Sprites.BloonLead);
            }
        }
    }
}
