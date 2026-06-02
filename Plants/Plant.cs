using Raylib_cs;
using PVZBloonTest.Bloons;
using PVZBloonTest.Projectiles;

namespace PVZBloonTest.Plants
{
    public abstract class Plant
    {
        public int   Col               { get; set; }
        public int   Row               { get; set; }
        public float Hp                { get; set; }
        public float MaxHp             { get; protected set; }
        public int   Block             { get; protected set; } = 1;
        public int   CurrentlyBlocking { get; set; } = 0;
        public bool  IsDead            { get; private set; } = false;
        public bool  HasCamoDetection  { get; set; } = false;
        public bool  HasLeadDetection  { get; set; } = false;
        public bool  PhotosynthesisBuff { get; set; } = false; // Set each frame by Photosynthesis sunflower
        public int[] UpgradeLevels     { get; } = new int[3];

        public abstract string[][] UpgradeNames { get; }
        public abstract int[][]    UpgradeCosts  { get; }
        public abstract PlantType  PlantType     { get; }
        public abstract int        BaseCost      { get; }

        public float PlaceCooldown      { get; protected set; } = 7.5f;
        public float PlaceCooldownTimer { get; set; } = 0f;

        public void TakeDamage(float dmg)
        {
            Hp -= dmg;
            if (Hp <= 0) { Hp = 0; IsDead = true; }
        }

        // Crosspath upgrade rules
        /// <summary>Number of paths that have at least 1 upgrade.</summary>
        public int UpgradedPathCount()
        {
            int c = 0;
            for (int i = 0; i < 3; i++) if (UpgradeLevels[i] > 0) c++;
            return c;
        }

        /// <summary>Number of paths at tier 2.</summary>
        public int Tier2Count()
        {
            int c = 0;
            for (int i = 0; i < 3; i++) if (UpgradeLevels[i] >= 2) c++;
            return c;
        }

        /// <summary>True if this path is fully locked (cannot buy even tier 1).</summary>
        public bool IsPathLocked(int path)
        {
            if (UpgradeLevels[path] > 0) return false; // already invested → not locked
            return UpgradedPathCount() >= 2;            // 3rd path is locked
        }

        /// <summary>True if a specific tier purchase is available (ignores sun cost).</summary>
        public bool CanBuyTier(int path, int tier)
        {
            if (UpgradeLevels[path] != tier - 1) return false; // must go in order
            if (IsPathLocked(path)) return false;
            if (tier == 2 && Tier2Count() >= 1) return false;  // only one T2 allowed
            return true;
        }

        public virtual bool BuyUpgrade(int path, int tier, SunManager sun)
        {
            if (!CanBuyTier(path, tier)) return false;
            if (!sun.SpendSun(UpgradeCosts[path][tier - 1])) return false;
            UpgradeLevels[path] = tier;
            ApplyUpgrade(path, tier);
            Particles.ParticleManager.Instance.PlantUpgrade(CenterX, CenterY);
            return true;
        }

        protected virtual void ApplyUpgrade(int path, int tier) { }
        public virtual void Update(float dt, GameContext ctx) { }
        public abstract void Draw();

        protected float CenterX => Grid.CellCenterX(Col);
        protected float CenterY => Grid.CellCenterY(Row);

        /// <summary>Emit purple/yellow aura particles if this plant has the Photosynthesis buff.</summary>
        protected void DrawPhotoAura()
        {
            if (PhotosynthesisBuff)
                Particles.ParticleManager.Instance.PhotoAura(CenterX, CenterY - 50);
        }

        protected void DrawHpBar()
        {
            if (Hp >= MaxHp) return;  // only visible when damaged
            int bx = (int)CenterX - 20, by = (int)CenterY - 50;
            float pct = MaxHp > 0 ? Hp / MaxHp : 0;
            Raylib.DrawRectangle(bx, by, 40, 6, Color.DarkGray);
            Color hc = pct > 0.5f ? Color.Lime : pct > 0.25f ? Color.Orange : Color.Red;
            Raylib.DrawRectangle(bx, by, (int)(40 * pct), 6, hc);
        }

        /// <summary>
        /// Draws the upgrade-aware sprite for a plant.
        /// • No T2 active  → base sprite + T1 overlay(s) stacked on top.
        /// • T2 active      → T2 crosspath sprite only (overlays removed).
        /// Call DrawHpBar() yourself after this if needed.
        /// </summary>
        protected void DrawUpgradeSprites(
            Raylib_cs.Texture2D   baseSprite,
            Raylib_cs.Texture2D[] t1Overlays,
            Raylib_cs.Texture2D[,] t2Sprites,
            float w, float h,
            Raylib_cs.Color baseTint = default)
        {
            if (baseTint.Equals(default(Raylib_cs.Color))) baseTint = Raylib_cs.Color.White;

            // Find T2 path (if any) and its crosspath
            int t2Path = -1, crossPath = -1;
            for (int i = 0; i < 3; i++) if (UpgradeLevels[i] >= 2) { t2Path = i; break; }
            if (t2Path >= 0)
                for (int i = 0; i < 3; i++)
                    if (i != t2Path && UpgradeLevels[i] >= 1) { crossPath = i; break; }

            if (t2Path >= 0)
            {
                // T2 mode: one complete crosspath sprite, no overlays
                int xIdx = Sprites.CrossIndex(crossPath);
                Sprites.DrawCentered(t2Sprites[t2Path, xIdx], CenterX, CenterY, w, h);
            }
            else
            {
                // Base + T1 overlays
                Sprites.DrawCentered(baseSprite, CenterX, CenterY, w, h, baseTint);
                for (int i = 0; i < 3; i++)
                    if (UpgradeLevels[i] >= 1)
                        Sprites.DrawCentered(t1Overlays[i], CenterX, CenterY, w, h);
            }
        }

        protected Bloon? FindTarget(GameContext ctx, bool camo, bool lead)
        {
            Bloon? t = null; float closest = float.MaxValue;
            foreach (var b in ctx.Bloons)
            {
                if (b.IsDead || b.Row != Row) continue;
                if (b.IsCamo && !camo && !HasCamoDetection) continue;
                if (b.IsLead && !lead && !HasLeadDetection) continue;
                if (b.X <= CenterX) continue;
                float dist = b.X - CenterX;
                if (dist < closest) { closest = dist; t = b; }
            }
            return t;
        }
    }
}
