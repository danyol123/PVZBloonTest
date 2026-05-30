using System;
using System.IO;
using System.Numerics;
using Raylib_cs;

namespace PVZBloonTest
{
    public static class Sprites
    {
        // ── Plants — base sprites ────────────────────────────────────────
        public static Texture2D Peashooter;
        public static Texture2D Sunflower;
        public static Texture2D Wallnut;
        public static Texture2D Cactus;

        // ── Plants — T1 overlay per path [0..2] ─────────────────────────
        // Drawn on top of the base sprite when UpgradeLevels[path] == 1
        public static Texture2D[] PeashooterT1 = new Texture2D[3];
        public static Texture2D[] SunflowerT1  = new Texture2D[3];
        public static Texture2D[] WallnutT1    = new Texture2D[3];
        public static Texture2D[] CactusT1     = new Texture2D[3];

        // ── Plants — T2 full crosspath sprite [t2Path 0..2][crossIdx 0..3]
        // crossIdx: 0 = no crosspath, 1 = crossed with path0,
        //           2 = crossed with path1, 3 = crossed with path2
        // File names:  {plant}_t2_{t2_path}.png   (no cross)
        //              {plant}_t2_{t2_path}_x{crosspath}.png
        public static Texture2D[,] PeashooterT2 = new Texture2D[3, 4];
        public static Texture2D[,] SunflowerT2  = new Texture2D[3, 4];
        public static Texture2D[,] WallnutT2    = new Texture2D[3, 4];
        public static Texture2D[,] CactusT2     = new Texture2D[3, 4];

        // ── Seed packet icons (HUD cards) ────────────────────────────────
        public static Texture2D IconPeashooter;
        public static Texture2D IconSunflower;
        public static Texture2D IconWallnut;
        public static Texture2D IconCactus;

        // ── Bloons ────────────────────────────────────────────────────────
        public static Texture2D BloonRed;
        public static Texture2D BloonBlue;
        public static Texture2D BloonYellow;
        public static Texture2D BloonPink;
        public static Texture2D BloonLead;
        public static Texture2D BloonLeadShield;  // shielded variant — slightly bigger draw
        public static Texture2D BloonCeramic;
        public static Texture2D BloonMoab;
        public static Texture2D BloonCamo;

        // ── Projectiles ───────────────────────────────────────────────────
        public static Texture2D Pea;
        public static Texture2D PeaFire;
        public static Texture2D PeaPlasma;
        public static Texture2D SpikeNormal;
        public static Texture2D SpikeHeat;
        public static Texture2D SpikePlasma;
        public static Texture2D ThornNormal;
        public static Texture2D ThornPlasma;
        public static Texture2D SunBlast;
        public static Texture2D SunLaser;

        // ── Sun drop ─────────────────────────────────────────────────────
        public static Texture2D SunDropTex;

        // ── Lawn area backdrops ──────────────────────────────────────────
        // Drop-in custom PNGs for the house sidebar and bloon spawn strip.
        // See sprites/README.md for exact pixel dimensions.
        public static Texture2D HouseArea;   // left sidebar
        public static Texture2D SpawnArea;   // right spawn strip

        // ── T1 placeholder tints per path ────────────────────────────────
        private static readonly Color[] _t1Colors = {
            new Color(255, 220, 80,  200),   // path 0 — gold   (rate / damage)
            new Color(255, 100, 60,  200),   // path 1 — orange (fire / damage type)
            new Color(80,  200, 255, 200),   // path 2 — cyan   (range / utility)
        };
        private static readonly Color[] _t2Colors = {
            new Color(255, 200, 40,  255),   // T2 path 0
            new Color(220, 80,  30,  255),   // T2 path 1
            new Color(60,  180, 255, 255),   // T2 path 2
        };

        // ─────────────────────────────────────────────────────────────────

        public static void LoadAll()
        {
            // Base plant sprites
            Peashooter = Load("sprites/peashooter.png", new Color(60,  180, 60,  255));
            Sunflower  = Load("sprites/sunflower.png",  new Color(255, 220, 40,  255));
            Wallnut    = Load("sprites/wallnut.png",    new Color(160, 200, 80,  255));
            Cactus     = Load("sprites/cactus.png",     new Color(50,  160, 50,  255));

            // Upgrade sprites for each plant
            LoadUpgradeSprites("peashooter", PeashooterT1, PeashooterT2);
            LoadUpgradeSprites("sunflower",  SunflowerT1,  SunflowerT2);
            LoadUpgradeSprites("wallnut",    WallnutT1,    WallnutT2);
            LoadUpgradeSprites("cactus",     CactusT1,     CactusT2);

            // Icons
            IconPeashooter = Load("sprites/icon_peashooter.png", new Color(60,  180, 60,  255));
            IconSunflower  = Load("sprites/icon_sunflower.png",  new Color(255, 220, 40,  255));
            IconWallnut    = Load("sprites/icon_wallnut.png",    new Color(160, 200, 80,  255));
            IconCactus     = Load("sprites/icon_cactus.png",     new Color(50,  160, 50,  255));

            // Bloons
            BloonRed     = Load("sprites/bloon_red.png",     new Color(220, 50,  50,  255));
            BloonBlue    = Load("sprites/bloon_blue.png",    new Color(60,  100, 220, 255));
            BloonYellow  = Load("sprites/bloon_yellow.png",  new Color(255, 230, 40,  255));
            BloonPink    = Load("sprites/bloon_pink.png",    new Color(255, 140, 200, 255));
            BloonLead    = Load("sprites/bloon_lead.png",    new Color(160, 160, 180, 255));
            BloonLeadShield = Load("sprites/bloon_lead_shield.png", new Color(210, 215, 235, 255));
            BloonCeramic = Load("sprites/bloon_ceramic.png", new Color(180, 130, 60,  255));
            BloonMoab    = Load("sprites/bloon_moab.png",    new Color(20,  40,  120, 255));
            BloonCamo    = Load("sprites/bloon_camo.png",    new Color(120, 200, 120, 200));

            // Projectiles
            Pea         = Load("sprites/pea.png",          new Color(80,  200, 60,  255));
            PeaFire     = Load("sprites/pea_fire.png",     new Color(255, 140, 30,  255));
            PeaPlasma   = Load("sprites/pea_plasma.png",   new Color(160, 50,  255, 255));
            SpikeNormal = Load("sprites/spike_normal.png", new Color(160, 160, 160, 255));
            SpikeHeat   = Load("sprites/spike_heat.png",   new Color(220, 80,  30,  255));
            SpikePlasma = Load("sprites/spike_plasma.png", new Color(160, 60,  220, 255));
            ThornNormal = Load("sprites/thorn_normal.png", new Color(130, 90,  40,  200));
            ThornPlasma = Load("sprites/thorn_plasma.png", new Color(140, 60,  200, 200));
            SunBlast    = Load("sprites/sun_blast.png",    new Color(255, 210, 30,  255));
            SunLaser    = Load("sprites/sun_laser.png",    new Color(255, 210, 30,  255));
            SunDropTex  = Load("sprites/sun_drop.png",     new Color(255, 210, 30,  255));

            // Lawn area backdrops
            HouseArea = Load("sprites/house_area.png", new Color(160, 120, 60, 255));
            SpawnArea = Load("sprites/spawn_area.png", new Color(110, 85,  45, 255));
        }

        private static void LoadUpgradeSprites(string plant, Texture2D[] t1, Texture2D[,] t2)
        {
            // T1 overlays
            for (int p = 0; p < 3; p++)
                t1[p] = Load($"sprites/{plant}_t1_{p}.png", _t1Colors[p]);

            // T2 full sprites
            for (int t2Path = 0; t2Path < 3; t2Path++)
            {
                // No crosspath
                t2[t2Path, 0] = Load($"sprites/{plant}_t2_{t2Path}.png", _t2Colors[t2Path]);

                // With a crosspath
                for (int x = 0; x < 3; x++)
                {
                    if (x == t2Path) continue;   // invalid combination
                    t2[t2Path, x + 1] = Load($"sprites/{plant}_t2_{t2Path}_x{x}.png", _t2Colors[t2Path]);
                }
            }
        }

        public static void UnloadAll()
        {
            Raylib.UnloadTexture(Peashooter);
            Raylib.UnloadTexture(Sunflower);
            Raylib.UnloadTexture(Wallnut);
            Raylib.UnloadTexture(Cactus);

            UnloadArray(PeashooterT1); UnloadArray2D(PeashooterT2);
            UnloadArray(SunflowerT1);  UnloadArray2D(SunflowerT2);
            UnloadArray(WallnutT1);    UnloadArray2D(WallnutT2);
            UnloadArray(CactusT1);     UnloadArray2D(CactusT2);

            Raylib.UnloadTexture(IconPeashooter);
            Raylib.UnloadTexture(IconSunflower);
            Raylib.UnloadTexture(IconWallnut);
            Raylib.UnloadTexture(IconCactus);

            Raylib.UnloadTexture(BloonRed);   Raylib.UnloadTexture(BloonBlue);
            Raylib.UnloadTexture(BloonYellow);Raylib.UnloadTexture(BloonPink);
            Raylib.UnloadTexture(BloonLead);  Raylib.UnloadTexture(BloonLeadShield);
            Raylib.UnloadTexture(BloonCeramic);
            Raylib.UnloadTexture(BloonMoab);  Raylib.UnloadTexture(BloonCamo);

            Raylib.UnloadTexture(Pea);        Raylib.UnloadTexture(PeaFire);
            Raylib.UnloadTexture(PeaPlasma);  Raylib.UnloadTexture(SpikeNormal);
            Raylib.UnloadTexture(SpikeHeat);  Raylib.UnloadTexture(SpikePlasma);
            Raylib.UnloadTexture(ThornNormal);Raylib.UnloadTexture(ThornPlasma);
            Raylib.UnloadTexture(SunBlast);   Raylib.UnloadTexture(SunLaser);
            Raylib.UnloadTexture(SunDropTex);
            Raylib.UnloadTexture(HouseArea);  Raylib.UnloadTexture(SpawnArea);
        }

        private static void UnloadArray(Texture2D[] arr)
        {
            foreach (var t in arr) if (t.Id != 0) Raylib.UnloadTexture(t);
        }
        private static void UnloadArray2D(Texture2D[,] arr)
        {
            foreach (var t in arr) if (t.Id != 0) Raylib.UnloadTexture(t);
        }

        // ── Draw helpers ─────────────────────────────────────────────────

        /// <summary>Draw a texture centred at (cx,cy) scaled to w×h, with optional tint and rotation.</summary>
        public static void DrawCentered(Texture2D tex, float cx, float cy,
                                        float w, float h,
                                        Color tint = default,
                                        float rotation = 0f)
        {
            if (tint.Equals(default(Color))) tint = Color.White;
            var src    = new Rectangle(0, 0, tex.Width, tex.Height);
            var dest   = new Rectangle(cx, cy, w, h);
            var origin = new Vector2(w / 2f, h / 2f);
            Raylib.DrawTexturePro(tex, src, dest, origin, rotation, tint);
        }

        /// <summary>
        /// Returns the crosspath index for a T2 lookup into [t2Path, crossIdx].
        /// crossPath = -1 means no crosspath → index 0.
        /// crossPath = 0..2 → index crossPath+1.
        /// </summary>
        public static int CrossIndex(int crossPath) => crossPath < 0 ? 0 : crossPath + 1;

        // ── File loader ──────────────────────────────────────────────────

        private static Texture2D Load(string path, Color placeholder)
        {
            if (File.Exists(path)) return Raylib.LoadTexture(path);

            // Generate a coloured placeholder with an X so missing files are obvious
            var img = Raylib.GenImageColor(64, 64, placeholder);
            Raylib.ImageDrawLine(ref img,  0,  0, 63, 63, new Color(0, 0, 0, 200));
            Raylib.ImageDrawLine(ref img, 63,  0,  0, 63, new Color(0, 0, 0, 200));
            Raylib.ImageDrawRectangleLines(ref img, new Rectangle(0, 0, 64, 64), 2,
                                           new Color(255, 255, 255, 180));
            var tex = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);
            return tex;
        }
    }
}
