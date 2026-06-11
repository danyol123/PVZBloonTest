using Raylib_cs;
using PVZBloonTest.Bloons;
using PVZBloonTest.Projectiles;
using PVZBloonTest.Particles;

namespace PVZBloonTest.Plants
{
    public class Peashooter : Plant
    {
        public override PlantType  PlantType    => PlantType.Peashooter;
        public override int        BaseCost     => 100;
        public override string[][] UpgradeNames => new[] {
            new[] { "Repeater",       "Gatling Pea"   },  // path 0: fire rate
            new[] { "Fire Pea",       "Explosive Peas"},  // path 1: damage type
            new[] { "Enhanced Sight", "Threepeater"   }   // path 2: coverage
        };
        public override int[][] UpgradeCosts => new[] {
            new[] { 100, 300 }, new[] { 75, 250 }, new[] { 25, 275 }
        };

        private int   _peaCount   = 1;
        private float _damage     = 1f;
        private bool  _isFire     = false;
        private bool  _isExplosive = false;
        private bool  _threeLanes = false;
        private float _fireTimer  = 0f;
        private float _interval   = 1.5f;

        public Peashooter() { MaxHp = Hp = 10f; Block = 1; PlaceCooldown = 5f; }

        protected override void ApplyUpgrade(int path, int tier)
        {
            switch (path, tier)
            {
                case (0, 1): _peaCount = 2; break;
                case (0, 2): _peaCount = 4; break;
                case (1, 1): _isFire   = true; HasLeadDetection = true; _damage = 1.5f; break;
                case (1, 2): _isExplosive = true; break;
                case (2, 1): HasCamoDetection = true; break;
                case (2, 2): _threeLanes = true; break;
            }
        }

        public override void Update(float dt, GameContext ctx)
        {
            _fireTimer += dt;
            if (_fireTimer < _interval) return;
            bool fired = false;
            if (_threeLanes)
            {
                for (int dr = -1; dr <= 1; dr++)
                {
                    int r = Row + dr;
                    if (r < 0 || r >= Constants.GRID_ROWS) continue;
                    if (HasTargetInRow(ctx, r)) 
                    { 
                        for (int rr = -1; rr <= 1; rr++) { FireRow(ctx, Row + rr); }
                        fired = true; 
                        break;
                    }
                }
            }
            else if (FindTarget(ctx, HasCamoDetection, HasLeadDetection) != null)
            { FireRow(ctx, Row); fired = true; }
            if (fired) _fireTimer = 0f;
        }

        private bool HasTargetInRow(GameContext ctx, int row)
        {
            foreach (var b in ctx.Bloons)
                if (!b.IsDead && b.Row == row && b.X > CenterX) return true;
            return false;
        }

        private void FireRow(GameContext ctx, int row)
        {
            ParticleManager.Instance.ProjectileHit(CenterX + 45f, CenterY - 10);
            for (int i = 0; i < _peaCount; i++)
                ctx.Projectiles.Add(new Pea(row, CenterX + 45f + i * 40f, _damage, 1,
                    _isFire, HasCamoDetection, HasLeadDetection,
                    plasma: false, explosive: _isExplosive));
        }

        public override void Draw()
        {
            DrawUpgradeSprites(Sprites.Peashooter, Sprites.PeashooterT1,
                               Sprites.PeashooterT2, 100f, 100f);
            DrawPhotoAura();
            DrawHpBar();
        }
    }
}
