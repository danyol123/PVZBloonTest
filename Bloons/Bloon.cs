using Raylib_cs;
using PVZBloonTest.Plants;
using PVZBloonTest.Particles;

namespace PVZBloonTest.Bloons
{
    public abstract class Bloon
    {
        public float X          { get; set; }
        public float Y          { get; private set; }
        public int   Row        { get; private set; }
        public float Hp         { get; set; }
        public float MaxHp      { get; set; }
        public float BaseSpeed  { get; protected set; }
        public float SpeedMult  { get; set; } = 1f;
        public int   Block      { get; protected set; } = 1;
        public float DmgPerSec  { get; protected set; } = 2f;
        public float Radius     { get; protected set; } = 32f;
        public bool  IsCamo     { get; protected set; } = false;
        public bool  IsLead     { get; set; }             = false;
        public bool  IsDead     { get; set; }             = false;
        public bool  PhotoBuff  { get; set; }             = false;
        public virtual int Cost { get; } = 1;

        public Plant? BlockingPlant { get; set; } = null;
        public bool   IsBlocked     => BlockingPlant != null;
        public bool   IsPassthrough { get; set; } = false;

        public float EffectiveSpeed => BaseSpeed * SpeedMult * Constants.SPEED_UNIT;

        // Bobbing state variables
        private float _bobTimer = 0f;
        private float _damageParticleTimer = 0f;

        /// <summary>Subclasses return their pop-burst colour for particles.</summary>
        public virtual Color PopColor => Color.White;

        /// <summary>Vertical sine-wave bob offset (±4 px).</summary>
        protected float BobOffset => System.MathF.Sin(_bobTimer * 3.5f) * 4f;

        /// <summary>The Y position used for drawing (includes bob).</summary>
        protected float DrawY => Y + BobOffset;

        protected Bloon(int row, float startX)
        {
            Row = row;
            X   = startX;
            Y   = Grid.CellCenterY(row);
        }

        public virtual void Update(float dt, GameContext ctx)
        {
            _bobTimer += dt;   // always animate bob regardless of state
            if (IsDead) return;
            if (IsBlocked)
            {
                if (BlockingPlant!.IsDead) { ReleaseBlock(); }
                else
                {
                    BlockingPlant.TakeDamage(DmgPerSec * dt);
                    _damageParticleTimer -= dt;
                    if (_damageParticleTimer <= 0f)
                    {
                        _damageParticleTimer = 0.33f;
                        ParticleManager.Instance.PlantHit(
                            Grid.CellCenterX(BlockingPlant.Col),
                            Grid.CellCenterY(BlockingPlant.Row));
                    }
                    return;
                }
            }
            float spd = IsPassthrough ? EffectiveSpeed * 0.5f : EffectiveSpeed;
            X -= spd * dt;
            if (X < Constants.GRID_X - Radius)
            {
                ctx.Lives -= (int)System.Math.Ceiling(Hp);
                if (ctx.Lives < 0) ctx.Lives = 0;
                IsDead = true;
            }
        }

        public void SetBlock(Plant plant) { BlockingPlant = plant; plant.CurrentlyBlocking++; }

        public void ReleaseBlock()
        {
            if (BlockingPlant != null)
            {
                BlockingPlant.CurrentlyBlocking = System.Math.Max(0, BlockingPlant.CurrentlyBlocking - 1);
                BlockingPlant = null;
            }
            IsPassthrough = false;
        }

        public virtual bool TakeDamage(float dmg, bool leadDetect, bool camoDetect)
        {
            if (IsCamo && !camoDetect) return false;
            if (IsLead && !leadDetect) return false;
            if (IsLead && leadDetect) { IsLead = false; ParticleManager.Instance.LeadShatter(X, Y); }
            Hp -= dmg;
            if (Hp <= 0)
            {
                Hp = 0; IsDead = true; ReleaseBlock();
                ParticleManager.Instance.Burst(X, Y, PopColor);
            }
            return true;
        }

        public virtual void OnDeath(GameContext ctx) { }

        public abstract void Draw();

        // Base drawing helper

        /// <summary>
        /// Draw using a sprite texture (preferred) or coloured circles as fallback,
        /// plus an HP bar. All drawing uses DrawY so the bloon bobs.
        /// </summary>
        protected void DrawBase(Color body, Color outline, Texture2D? tex = null,
                               Color spriteTint = default)
        {
            if (spriteTint.Equals(default(Color))) spriteTint = Color.White;
            int cx = (int)X, cy = (int)DrawY;
            if (tex.HasValue)
            {
                Sprites.DrawCentered(tex.Value, X, DrawY, Radius * 2f, Radius * 2f, spriteTint);
            }
            else
            {
                Raylib.DrawCircle(cx, cy, Radius, body);
                Raylib.DrawCircleLines(cx, cy, Radius, outline);
            }
            // HP bar bobs with the bloon
            float pct = MaxHp > 0 ? Hp / MaxHp : 0;
            Raylib.DrawRectangle(cx - 20, cy - (int)Radius - 8, 40, 5, Color.DarkGray);
            Raylib.DrawRectangle(cx - 20, cy - (int)Radius - 8, (int)(40 * pct), 5, Color.Lime);
        }
    }
}
