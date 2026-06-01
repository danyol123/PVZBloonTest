using System;
using Raylib_cs;
using PVZBloonTest.Projectiles;
using PVZBloonTest.Particles;

namespace PVZBloonTest.Plants
{
    public class Wallnut : Plant
    {
        public override PlantType  PlantType    => PlantType.Wallnut;
        public override int        BaseCost     => 50;
        public override string[][] UpgradeNames => new[] {
            new[] { "Tough-nut",     "Tall-nut"     },
            new[] { "Endurian",      "The Shredder" },
            new[] { "Explode-o-nut", "Torchwood"    }
        };
        public override int[][] UpgradeCosts => new[] {
            new[] { 25, 75 }, new[] { 75, 250 }, new[] { 75, 175 }
        };

        private bool  _regen, _endurian, _superEndurian, _torchwood;
        private bool  _explode;
        public  bool  HasExplode => _explode;
        private float _regenTimer, _meleeTimer;

        // Tracks projectiles already upgraded by this Torchwood to prevent
        // multi-frame promotion (normal→fire→plasma in the same zone pass).
        private readonly System.Collections.Generic.HashSet<Projectile> _torchwoodDone = new();

        public Wallnut() { MaxHp = Hp = 100f; Block = 3; PlaceCooldown = 15f; }

        protected override void ApplyUpgrade(int path, int tier)
        {
            switch (path, tier)
            {
                case (0, 1): Block = 5; MaxHp = 150; Hp = Math.Min(Hp * 2, 150); break;
                case (0, 2): Block = 10; MaxHp = 300; Hp = Math.Min(Hp * 3, 300); _regen = true; break;
                case (1, 1): _endurian = true; break;
                case (1, 2): _superEndurian = true; break;
                case (2, 1): _explode = true; HasCamoDetection = true; HasLeadDetection = true; break;
                case (2, 2): _torchwood = true; break;
            }
        }

        public override void Update(float dt, GameContext ctx)
        {
            // Regen HP
            if (_regen)
            {
                _regenTimer += dt;
                if (_regenTimer >= 1f) { _regenTimer = 0f; Hp = Math.Min(Hp + 1f, MaxHp); }
            }

            // Endurian: fire Thorn projectiles toward approaching bloons
            if (_endurian)
            {
                _meleeTimer += dt;
                float thornInterval = _superEndurian ? 1f : 1.5f;
                if (_meleeTimer >= thornInterval)
                {
                    bool hasTarget = false;
                    float range = 1.5f * Constants.CELL_W;
                    foreach (var b in ctx.Bloons)
                    {
                        if (b.IsDead || b.Row != Row) continue;
                        if (b.X >= CenterX && b.X <= CenterX + range) { hasTarget = true; break; }
                    }
                    if (hasTarget)
                    {
                        _meleeTimer = 0f;
                        float dmg    = _superEndurian ? 5f : 2f;
                        int   pierce = _superEndurian ? 3 : 1;
                        ParticleManager.Instance.ProjectileHit(CenterX + 45f, CenterY);
                        ctx.Projectiles.Add(new Thorn(Row, CenterX + 20, dmg, pierce, 1.5f, _superEndurian, HasLeadDetection, HasCamoDetection));
                    }
                }
            }

            // Torchwood: upgrade peas and spikes that pass through — one stage per zone visit.
            if (_torchwood)
            {
                float cx = Grid.CellCenterX(Col);
                // Remove dead or out-of-zone projectiles from the "already done" set
                _torchwoodDone.RemoveWhere(proj =>
                    proj.IsDead || Math.Abs(proj.X - cx) >= Constants.CELL_W / 2f);

                foreach (var proj in ctx.Projectiles)
                {
                    if (proj.Row != Row || proj.IsDead) continue;
                    if (Math.Abs(proj.X - cx) >= Constants.CELL_W / 2f) continue;
                    if (_torchwoodDone.Contains(proj)) continue;  // already upgraded this pass

                    bool upgraded = false;
                    if (proj is Pea p)
                    {
                        // Normal pea → fire pea (one stage only)
                        upgraded = p.UpgradeToFire();
                        // Fire pea → plasma pea (only if already fire)
                        if (!upgraded) upgraded = p.UpgradeToPlasma();
                    }
                    else if (proj is Spike s)
                    {
                        // Normal spike → heat spike (one stage only)
                        upgraded = s.UpgradeToHeat();
                        // Heat spike → plasma spike (only if already heat)
                        if (!upgraded) upgraded = s.UpgradeToPlasma();
                    }

                    if (upgraded) _torchwoodDone.Add(proj);
                }
            }
        }

        public override void Draw()
        {
            float pct = MaxHp > 0 ? Hp / MaxHp : 0;
            // HP tint shifts white → orange → red — pass as baseTint so T2 sprite ignores it
            Color hpTint = pct > 0.6f ? Color.White
                         : pct > 0.3f ? new Color(255, 190, 80, 255)
                         :               new Color(255, 100, 80, 255);
            float tallnutH = _regen ? 120f : 100f;
            DrawUpgradeSprites(Sprites.Wallnut, Sprites.WallnutT1,
                               Sprites.WallnutT2, 100f, tallnutH, hpTint);
            DrawPhotoAura();
            DrawHpBar();
        }
    }
}
