using System.Collections.Generic;
using Raylib_cs;
using PVZBloonTest.Bloons;
using PVZBloonTest.Particles;

namespace PVZBloonTest.Projectiles
{
    public abstract class Projectile
    {
        public float X       { get; set; }
        public float Y       { get; private set; }
        public int   Row     { get; private set; }
        public float Damage  { get; set; }
        public float Speed   { get; protected set; } = 420f;
        public int   Pierce  { get; set; }
        public bool  HasCamo { get; set; } = false;
        public bool  HasLead { get; set; } = false;
        public bool  IsDead  { get; set; } = false;

        private readonly HashSet<Bloon> _hit = new();
        private static readonly float _rightEdge = Constants.GRID_X + Constants.GRID_COLS * Constants.CELL_W + 60f;

        protected Projectile(int row, float startX, float damage, int pierce)
        {
            Row = row; X = startX;
            Y = Grid.CellCenterY(row);
            Damage = damage; Pierce = pierce;
        }

        public virtual void Update(float dt, GameContext ctx)
        {
            if (IsDead) return;
            X += Speed * dt;                     // projectiles move RIGHT toward incoming bloons
            if (X > _rightEdge) { IsDead = true; return; }

            foreach (var b in ctx.Bloons)
            {
                if (b.IsDead || b.Row != Row || _hit.Contains(b)) continue;
                if (System.MathF.Abs(b.X - X) < b.Radius + 8f)
                {
                    bool hit = b.TakeDamage(DamageForBloon(b), HasLead, HasCamo);
                    if (hit)
                    {
                        _hit.Add(b);
                        if (b.IsDead) b.OnDeath(ctx);
                        OnHit(b, ctx);
                        ParticleManager.Instance.ProjectileHit(b.X, b.Y);
                        Pierce--;
                    }
                    else if (b.IsLead && !HasLead) { IsDead = true; return; }
                    if (Pierce <= 0) { IsDead = true; return; }
                }
            }
        }

        public virtual float DamageForBloon(Bloon b) => Damage;
        protected virtual void OnHit(Bloon b, GameContext ctx) { }
        public abstract void Draw();
    }
}
