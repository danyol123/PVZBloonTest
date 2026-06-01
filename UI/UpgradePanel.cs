using Raylib_cs;
using PVZBloonTest.Plants;

namespace PVZBloonTest.UI
{
    public class UpgradePanel
    {
        public Plant? TargetPlant { get; set; } = null;
        public bool   IsOpen      => TargetPlant != null;
        public void   Close()     => TargetPlant = null;

        // PH increased to 452 to fit all upgrade buttons without clipping
        private const int PW = 290, PH = 452, BW = 246, BH = 38;
        private static int PanelX => Constants.SCREEN_W - PW - 10;
        private static int PanelY => Constants.HUD_H + 10;

        private int _hoveredPath = -1;
        private int _hoveredTier = -1;

        public bool Update(GameContext ctx)
        {
            if (!IsOpen) return false;
            var mp = Raylib.GetMousePosition();
            int mx = (int)mp.X, my = (int)mp.Y;
            int px = PanelX, py = PanelY;

            // Track hover — Y must match Draw(): py + 92 + path*118 + (tier-1)*50 + 18
            _hoveredPath = -1; _hoveredTier = -1;
            for (int path = 0; path < 3; path++)
                for (int tier = 1; tier <= 2; tier++)
                {
                    int bx = px + (PW - BW) / 2;
                    int by = py + 92 + path * 118 + (tier - 1) * 50 + 18;
                    if (mx >= bx && mx < bx + BW && my >= by && my < by + BH)
                    { _hoveredPath = path; _hoveredTier = tier; }
                }

            if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return false;

            // Close button
            if (mx >= px + PW - 28 && mx <= px + PW - 4 && my >= py + 4 && my <= py + 24)
            { Close(); return true; }

            if (TargetPlant == null) return false;

            // Upgrade buttons — same Y formula as Draw()
            for (int path = 0; path < 3; path++)
                for (int tier = 1; tier <= 2; tier++)
                {
                    int bx = px + (PW - BW) / 2;
                    int by = py + 92 + path * 118 + (tier - 1) * 50 + 18;
                    if (mx >= bx && mx < bx + BW && my >= by && my < by + BH)
                    {
                        if (ctx.UnlockManager.IsUpgradeTierUnlocked(tier) && TargetPlant.CanBuyTier(path, tier))
                            TargetPlant.BuyUpgrade(path, tier, ctx.SunManager);
                        return true;
                    }
                }

            // Click inside panel — consume
            if (mx >= px && mx < px + PW && my >= py && my < py + PH) return true;
            return false;
        }

        public void Draw(GameContext ctx)
        {
            if (!IsOpen || TargetPlant == null) return;
            int px = PanelX, py = PanelY;

            Raylib.DrawRectangle(px, py, PW, PH, new Color(18, 18, 38, 235));
            Raylib.DrawRectangleLines(px, py, PW, PH, new Color(120, 100, 200, 255));

            // ── Header ───────────────────────────────────────────────
            Fonts.Draw(TargetPlant.PlantType.ToString(), px + 10, py + 8, 20, Color.White);
            Fonts.Draw($"HP: {(int)TargetPlant.Hp}/{(int)TargetPlant.MaxHp}", px + 10, py + 32, 15, Color.Lime);
            string xp = $"Paths: {TargetPlant.UpgradedPathCount()}/2  T2: {TargetPlant.Tier2Count()}/1";
            Fonts.Draw(xp, px + 10, py + 52, 13, new Color(180, 180, 255, 255));

            // Close button
            Raylib.DrawRectangle(px + PW - 28, py + 4, 24, 20, new Color(180, 50, 50, 255));
            Fonts.Draw("X", px + PW - 22, py + 5, 14, Color.White);

            // ── Camo / Lead detection badges ─────────────────────────
            int badgeY = py + 70;
            DrawDetectionBadge(px + 10, badgeY, "CAMO", TargetPlant.HasCamoDetection,
                new Color(100, 200, 100, 220), new Color(30, 160, 30, 255), new Color(10, 100, 10, 255));
            DrawDetectionBadge(px + 82, badgeY, "LEAD", TargetPlant.HasLeadDetection,
                new Color(100, 200, 100, 220), new Color(30, 160, 30, 255), new Color(10, 100, 10, 255));

            // ── Upgrade paths ─────────────────────────────────────────
            Color[] pc = { new Color(255, 180, 30, 255), new Color(80, 200, 100, 255), new Color(100, 180, 255, 255) };

            for (int path = 0; path < 3; path++)
            {
                int pathY = py + 92 + path * 118;
                bool pathLocked = TargetPlant.IsPathLocked(path);
                Color hdrCol = pathLocked ? Color.DarkGray : pc[path];
                Raylib.DrawRectangle(px + 6, pathY, PW - 12, 16,
                    new Color(hdrCol.R / 4, hdrCol.G / 4, hdrCol.B / 4, 200));
                Fonts.Draw(pathLocked ? $"Path {path + 1} [LOCKED]" : $"Path {path + 1}",
                    px + 8, pathY + 1, 16, hdrCol);

                for (int tier = 1; tier <= 2; tier++)
                {
                    int bx = px + (PW - BW) / 2, by = py + 92 + path * 118 + (tier - 1) * 50 + 18;
                    bool bought   = TargetPlant.UpgradeLevels[path] >= tier;
                    bool unlocked = ctx.UnlockManager.IsUpgradeTierUnlocked(tier);
                    bool canBuy   = !bought && TargetPlant.CanBuyTier(path, tier) && unlocked
                                    && ctx.SunManager.Sun >= TargetPlant.UpgradeCosts[path][tier - 1];
                    bool isLocked = pathLocked || (!bought && !TargetPlant.CanBuyTier(path, tier));
                    bool xpLocked = !bought && tier == 2 && TargetPlant.Tier2Count() >= 1 && TargetPlant.UpgradeLevels[path] < 2;

                    Color bg = bought   ? new Color(50, 130, 50, 220)
                             : canBuy   ? new Color(55, 55, 110, 220)
                             : isLocked ? new Color(30, 30, 30, 180)
                             :            new Color(45, 45, 75, 180);
                    Color border = bought   ? Color.Lime
                                 : canBuy   ? pc[path]
                                 : isLocked ? new Color(60, 60, 60, 255)
                                 :            Color.DarkGray;

                    Raylib.DrawRectangle(bx, by, BW, BH, bg);
                    Raylib.DrawRectangleLines(bx, by, BW, BH, border);

                    string label = bought      ? $"[OK] {TargetPlant.UpgradeNames[path][tier - 1]}"
                                 : !unlocked   ? $"[Locked T{tier}]"
                                 : pathLocked  ? "[Path locked]"
                                 : xpLocked    ? "[T2 crosspath locked]"
                                 :               $"[{TargetPlant.UpgradeCosts[path][tier - 1]}] {TargetPlant.UpgradeNames[path][tier - 1]}";
                    Fonts.Draw(label, bx + 5, by + 10, 15, bought ? Color.Lime : canBuy ? Color.White : Color.Gray);
                }
            }

            // ── Upgrade tooltip on hover ──────────────────────────────
            if (_hoveredPath >= 0 && TargetPlant != null)
            {
                string desc = GetDescription(TargetPlant.PlantType, _hoveredPath, _hoveredTier);
                DrawTooltip(px - 10, py + 92 + _hoveredPath * 118 + (_hoveredTier - 1) * 50, desc);
            }
        }

        private static void DrawDetectionBadge(int x, int y, string label, bool active,
            Color bgActive, Color borderActive, Color textActive)
        {
            Color bg     = active ? bgActive                        : new Color(30, 30, 30, 180);
            Color border = active ? borderActive                    : new Color(70, 70, 70, 255);
            Color text   = active ? textActive                      : new Color(80, 80, 80, 255);
            Raylib.DrawRectangle(x, y, 66, 18, bg);
            Raylib.DrawRectangleLines(x, y, 66, 18, border);
            // Small dot indicator
            Raylib.DrawCircle(x + 10, y + 9, 4f, active ? borderActive : new Color(60, 60, 60, 255));
            Fonts.Draw(label, x + 18, y + 2, 13, text);
        }

        private static void DrawTooltip(int anchorX, int anchorY, string desc)
        {
            const int TW = 210, PAD = 8, FS = 18;
            var words = desc.Split(' ');
            var lines = new System.Collections.Generic.List<string>();
            string cur = "";
            foreach (var w in words)
            {
                string next = cur.Length == 0 ? w : cur + " " + w;
                if (Fonts.Measure(next, FS) > TW - PAD * 2) { if (cur.Length > 0) lines.Add(cur); cur = w; }
                else cur = next;
            }
            if (cur.Length > 0) lines.Add(cur);

            int th = lines.Count * (FS + 5) + PAD * 2;
            int tx = anchorX - TW - 4;
            int ty = System.Math.Clamp(anchorY, Constants.HUD_H + 10, Constants.SCREEN_H - th - 10);

            Raylib.DrawRectangle(tx, ty, TW, th, new Color(10, 10, 30, 230));
            Raylib.DrawRectangleLines(tx, ty, TW, th, new Color(180, 140, 255, 200));
            for (int i = 0; i < lines.Count; i++)
                Fonts.Draw(lines[i], tx + PAD, ty + PAD + i * (FS + 5), FS, Color.White);
        }

        private static string GetDescription(PlantType t, int path, int tier) => (t, path, tier) switch
        {
            (PlantType.Sunflower,  0, 1) => "Faster Sun: produces sun 33% faster.",
            (PlantType.Sunflower,  0, 2) => "Twin Sunflowers: drops 2 suns each time.",
            (PlantType.Sunflower,  1, 1) => "Fertilizer: +25 sun if nearby sunflowers are also fertilized.",
            (PlantType.Sunflower,  1, 2) => "Photosynthesis: buffs nearby plants with camo detection and bloons they pop also drop sun.",
            (PlantType.Sunflower,  2, 1) => "Solar Blast: fires a sun blast that can pop lead bloons.",
            (PlantType.Sunflower,  2, 2) => "Magnifying Grass: solar blast turns into a piercing laser.",

            (PlantType.Wallnut,    0, 1) => "Tough-nut: increased HP and blocking capacity.",
            (PlantType.Wallnut,    0, 2) => "Tall-nut: even more HP and block, slowly regenerates HP over time.",
            (PlantType.Wallnut,    1, 1) => "Endurian: fires thorns at bloons in front of it.",
            (PlantType.Wallnut,    1, 2) => "The Shredder: thorns become even more deadly with extra pierce and damage.",
            (PlantType.Wallnut,    2, 1) => "Explode-o-nut: explodes on death dealing AoE damage to all types of bloons.",
            (PlantType.Wallnut,    2, 2) => "Torchwood: upgrades peas and spikes to their fire counterpart.",

            (PlantType.Peashooter, 0, 1) => "Repeater: fires 2 peas per shot.",
            (PlantType.Peashooter, 0, 2) => "Gatling Pea: fires 4 peas per shot.",
            (PlantType.Peashooter, 1, 1) => "Fire Pea: peas become fire peas that pop lead bloons.",
            (PlantType.Peashooter, 1, 2) => "Explosive Peas: peas explode on impact, dealing AoE damage to nearby bloons.",
            (PlantType.Peashooter, 2, 1) => "Enhanced Sight: detects and targets camo bloons.",
            (PlantType.Peashooter, 2, 2) => "Threepeater: fires at 3 lanes simultaneously.",

            (PlantType.Cactus,     0, 1) => "Heat-tipped Spikes: spikes pop lead bloons and deal bonus lead damage.",
            (PlantType.Cactus,     0, 2) => "Headshot!: spikes deal insane damage with additional damage against Ceramic and MOAB bloons.",
            (PlantType.Cactus,     1, 1) => "Thorns: gain camo detection and fires thorn projectiles at bloons within 2 tiles.",
            (PlantType.Cactus,     1, 2) => "Garden of Thorns: thorns fire in 3 lanes with greatly increased damage and pierce.",
            (PlantType.Cactus,     2, 1) => "Razor Sharp Spikes: spikes now pierce through bloons.",
            (PlantType.Cactus,     2, 2) => "Super-Charged Spikes: spikes pierce even more bloons.",

            _ => "No description available."
        };
    }
}
