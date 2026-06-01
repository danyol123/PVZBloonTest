using Raylib_cs;
namespace PVZBloonTest.Projectiles
{
    public class SunBlast : Projectile
    {
        private readonly bool _laser;
        public SunBlast(int row, float x, float dmg, int pierce, bool laser, bool lead, bool camo)
            : base(row, x, dmg, pierce) { _laser = laser; HasLead = lead; HasCamo = camo; Speed =  _laser ? 640f : 320f; }
        public override void Draw()
        {
            Texture2D tex = _laser ? Sprites.SunLaser
                          :          Sprites.SunBlast;
            float rX = _laser ? 64f
                     :          32f;
            Sprites.DrawCentered(tex, X, Y, rX, 32f);
        }
    }
}
