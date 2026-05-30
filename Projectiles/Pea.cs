using Raylib_cs;
using PVZBloonTest.Bloons;
using PVZBloonTest.Particles;
using PVZBloonTest.UI;

namespace PVZBloonTest.Projectiles
{
    public class Pea : Projectile
    {
        public bool IsFire      { get; private set; }
        public bool IsPlasma    { get; private set; }
        public bool IsExplosive { get; }

        public Pea(int row, float x, float dmg, int pierce, bool fire, bool camo, bool lead,
                   bool plasma = false, bool explosive = false)
            : base(row, x, dmg, pierce)
        {
            IsFire      = fire   || plasma;
            IsPlasma    = plasma;
            IsExplosive = explosive;
            HasCamo = camo;
            HasLead = lead || fire || plasma;
            Speed   = 420f;
        }

        /// <summary>Called by Torchwood: upgrades a normal pea to fire pea.</summary>
        public bool UpgradeToFire()
        {
            if (IsFire || IsPlasma) return false;
            IsFire  = true;
            HasLead = true;
            Damage *= 1.4f;
            return true;
        }

        /// <summary>Called by Torchwood: upgrades a fire pea to plasma pea.</summary>
        public bool UpgradeToPlasma()
        {
            if (IsPlasma) return false;
            if (!IsFire) { UpgradeToFire(); }
            IsPlasma = true;
            HasLead  = true;
            Damage  *= 2f;
            return true;
        }

        protected override void OnHit(Bloon b, GameContext ctx)
        {
            if (b.IsDead && b.PhotoBuff)
                ctx.SunDrops.Add(new SunDrop(b.X, b.Y, 25, false));

            if (IsExplosive)
            {
                // AoE explosion on hit — damages bloons in a small radius
                float ex = b.X, ey = Grid.CellCenterY(b.Row);
                ParticleManager.Instance.Explosion(ex, ey);
                foreach (var other in ctx.Bloons)
                {
                    if (other == b || other.IsDead) continue;
                    float dx = other.X - ex;
                    float dy = Grid.CellCenterY(other.Row) - ey;
                    if (dx * dx + dy * dy < 65f * 65f)
                    {
                        other.TakeDamage(Damage * 0.6f, HasLead, HasCamo);
                        if (other.IsDead) other.OnDeath(ctx);
                    }
                }
            }
        }

        public override void Draw()
        {
            Texture2D tex = IsPlasma ? Sprites.PeaPlasma
                          : IsFire   ? Sprites.PeaFire
                          :             Sprites.Pea;
            Sprites.DrawCentered(tex, X, Y, 32f, 32f);
        }
    }
}
