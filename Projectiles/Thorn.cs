using System.Numerics;
using Raylib_cs;

namespace PVZBloonTest.Projectiles
{
    /// <summary>Short-range melee thorn projectile. Always pops lead and camo.</summary>
    public class Thorn : Projectile
    {
        private readonly float _startX;
        private readonly float _maxRange;
        private readonly bool  _isTier2;

        public Thorn(int row, float startX, float damage, int pierce, float rangeTiles, bool isTier2)
            : base(row, startX, damage, pierce)
        {
            _startX   = startX;
            _maxRange = rangeTiles * Constants.CELL_W;
            _isTier2 = isTier2;
            Speed     = 500f;
            HasLead   = true;
            HasCamo   = true;
        }

        public override void Update(float dt, GameContext ctx)
        {
            if (X - _startX >= _maxRange) { IsDead = true; return; }
            base.Update(dt, ctx);
        }

        public override void Draw()
        {
            Texture2D tex = _isTier2 ? Sprites.ThornPlasma : Sprites.ThornNormal;
            Sprites.DrawCentered(tex, X, Y, 32f, 64f);
        }
    }
}
