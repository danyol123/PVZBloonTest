using System;
using Raylib_cs;
using PVZBloonTest.Projectiles;
using PVZBloonTest.UI;
using PVZBloonTest.Particles;

namespace PVZBloonTest.Plants
{
    public class Sunflower : Plant
    {
        public override PlantType  PlantType    => PlantType.Sunflower;
        public override int        BaseCost     => 50;
        public override string[][] UpgradeNames => new[] {
            new[] { "Faster Sun",       "Twin Sunflowers"  },
            new[] { "Supporting Growth","Photosynthesis"   },
            new[] { "Solar Power",      "Magnifying Grass" }
        };
        public override int[][] UpgradeCosts => new[] {
            new[] { 25,  200 }, new[] { 75, 250 }, new[] { 125, 350 }
        };

        private float _sunTimer    = 2f;    // first drop after 2s
        private float _sunInterval = 6f;    // then every 6s
        private int   _sunValue    = 25;
        private bool  _hasSolar    = false;
        private bool  _hasMagGrass = false;
        private bool  _asleep      = false;
        private readonly Random _rng = new();

        public Sunflower() { MaxHp = Hp = 10f; Block = 1; PlaceCooldown = 5f; }

        protected override void ApplyUpgrade(int path, int tier)
        {
            switch (path, tier)
            {
                case (0, 1): _sunInterval = 4f; break;  // Faster Sun: every 4s
                case (2, 1): _hasSolar    = true; break;
                case (2, 2): _hasMagGrass = true; break;
            }
        }

        public override void Update(float dt, GameContext ctx)
        {
            _asleep = ctx.IsNightMode;
            if (_asleep) return;

            if (UpgradeLevels[1] >= 2) ApplyPhotosynthesis(ctx);

            _sunTimer -= dt;
            if (_sunTimer <= 0f)
            {
                _sunTimer = _sunInterval;
                int val   = _sunValue;
                if (UpgradeLevels[1] >= 1 && HasNearbySG(ctx)) val = 50;
                int cnt   = UpgradeLevels[0] >= 2 ? 2 : 1;
                ParticleManager.Instance.SunProduce(CenterX, CenterY);
                for (int i = 0; i < cnt; i++)
                {
                    float ox = CenterX + (float)(_rng.NextDouble() - 0.5) * 50;
                    ctx.SunDrops.Add(new SunDrop(ox, CenterY - 60, val, false));
                    if (_hasSolar){
                        float dmg = _hasMagGrass ? (_sunValue / 25f) * 3f : (_sunValue / 25f) * 3f;
                        int   prc = _hasMagGrass ? 9 : 1;
                        ctx.Projectiles.Add(new SunBlast(Row, CenterX + i * 30f, dmg, prc, _hasMagGrass));
                    }
                }
            }
        }

        private bool HasNearbySG(GameContext ctx)
        {
            for (int dc = -1; dc <= 1; dc++)
                for (int dr = -1; dr <= 1; dr++)
                {
                    if (dc == 0 && dr == 0) continue;
                    var p = ctx.Grid.GetPlant(Col + dc, Row + dr);
                    if (p is Sunflower sf && sf != this && sf.UpgradeLevels[1] >= 1) return true;
                }
            return false;
        }

        private void ApplyPhotosynthesis(GameContext ctx)
        {
            // Reset last frame's buff so plants no longer adjacent stop glowing
            for (int dc = -1; dc <= 1; dc++)
                for (int dr = -1; dr <= 1; dr++)
                {
                    var p = ctx.Grid.GetPlant(Col + dc, Row + dr);
                    if (p != null && p != this) p.PhotosynthesisBuff = false;
                }

            // Apply buff to current neighbors and mark camo/lead detection
            for (int dc = -1; dc <= 1; dc++)
                for (int dr = -1; dr <= 1; dr++)
                {
                    var p = ctx.Grid.GetPlant(Col + dc, Row + dr);
                    if (p != null && p != this)
                    {
                        p.HasCamoDetection  = true;
                        p.HasLeadDetection  = true;
                        p.PhotosynthesisBuff = true;
                    }
                }

            // Sunflower itself also shows the aura
            PhotosynthesisBuff = true;

            foreach (var b in ctx.Bloons) b.PhotoBuff = true;
        }

        public override void Draw()
        {
            Color nightTint = _asleep ? new Color(150, 130, 80, 180) : Color.White;
            DrawUpgradeSprites(Sprites.Sunflower, Sprites.SunflowerT1,
                               Sprites.SunflowerT2, 100f, 100f, nightTint);
            if (_asleep) Fonts.Draw("zzz", (int)CenterX - 10, (int)CenterY - 34, 12, Color.White);
            DrawPhotoAura();
            DrawHpBar();
        }
    }
}
