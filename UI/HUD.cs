using System;
using Raylib_cs;
namespace PVZBloonTest.UI
{
    public class HUD
    {
        public int  SelectedCard   { get; private set; } = -1;
        public bool ShovelSelected => SelectedCard == 4;

        private static readonly (PlantType Type, string Label, int Cost, float Cooldown)[] Cards = new[]
        {
            (PlantType.Sunflower,  "Sunflower",  50,   0f),
            (PlantType.Peashooter, "Peashooter", 100,  0f),
            (PlantType.Wallnut,    "Wallnut",    50,  0f),
            (PlantType.Cactus,     "Cactus",     150,  0f),
        };

        private readonly float[] _cdTimers = new float[4];
        private const int CW = 72, CH = 80, CP = 6, CSX = 10, CSY = 6;

        public PlantType SelectedPlantType => SelectedCard >= 0 && SelectedCard < 4 ? Cards[SelectedCard].Type : PlantType.Sunflower;
        public int       SelectedPlantCost => SelectedCard >= 0 && SelectedCard < 4 ? Cards[SelectedCard].Cost : 0;

        public void UpdateCooldowns(float dt)
        {
            for (int i = 0; i < _cdTimers.Length; i++)
                if (_cdTimers[i] > 0f) _cdTimers[i] = Math.Max(0f, _cdTimers[i] - dt);
        }

        public void StartCooldown(int card)
        {
            if (card >= 0 && card < Cards.Length) _cdTimers[card] = Cards[card].Cooldown;
        }

        public bool IsOnCooldown(int card) => card >= 0 && card < _cdTimers.Length && _cdTimers[card] > 0f;

        public void Update(GameContext ctx)
        {
            if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return;
            var mp = Raylib.GetMousePosition();
            int mx = (int)mp.X, my = (int)mp.Y;

            for (int i = 0; i < Cards.Length; i++)
            {
                int cx = CSX + i * (CW + CP);
                if (mx >= cx && mx < cx + CW && my >= CSY && my < CSY + CH)
                {
                    if (_cdTimers[i] <= 0f)
                        SelectedCard = SelectedCard == i ? -1 : i;
                    return;
                }
            }
            // Shovel button
            int shx = CSX + Cards.Length * (CW + CP) + 10;
            if (mx >= shx && mx < shx + 60 && my >= CSY && my < CSY + CH)
                SelectedCard = SelectedCard == 4 ? -1 : 4;
        }

        public void Deselect() => SelectedCard = -1;

        public void Draw(GameContext ctx)
        {
            Raylib.DrawRectangle(0, 0, Constants.SCREEN_W, Constants.HUD_H, new Color(25, 25, 45, 245));
            Raylib.DrawRectangle(0, Constants.HUD_H - 3, Constants.SCREEN_W, 3, new Color(70, 70, 110, 255));

            for (int i = 0; i < Cards.Length; i++)
            {
                int cx = CSX + i * (CW + CP);
                bool sel      = SelectedCard == i;
                bool enough   = ctx.SunManager.Sun >= Cards[i].Cost;
                bool onCd     = _cdTimers[i] > 0f;
                float cdPct   = onCd ? _cdTimers[i] / Cards[i].Cooldown : 0f;

                Color bg = onCd    ? new Color(30, 30, 30, 200)
                         : sel     ? new Color(255, 220, 80, 255)
                         : enough  ? new Color(55, 80, 55, 240)
                         :           new Color(45, 45, 45, 200);

                Raylib.DrawRectangle(cx, CSY, CW, CH, bg);
                Raylib.DrawRectangleLines(cx, CSY, CW, CH,
                    sel ? Color.Yellow : onCd ? Color.DarkGray : new Color(90, 110, 90, 255));

                // Seed-packet icon sprite (greyed when on cooldown)
                Texture2D icon = Cards[i].Type switch {
                    PlantType.Sunflower  => Sprites.IconSunflower,
                    PlantType.Peashooter => Sprites.IconPeashooter,
                    PlantType.Wallnut    => Sprites.IconWallnut,
                    PlantType.Cactus     => Sprites.IconCactus,
                    _                    => Sprites.IconPeashooter
                };
                Color iconTint = onCd ? new Color(90, 90, 90, 200) : Color.White;
                Sprites.DrawCentered(icon, cx + CW / 2f, CSY + 26f, 64f, 64f, iconTint);

                // Label
                int tw = Fonts.Measure(Cards[i].Label, 12);
                Fonts.Draw(Cards[i].Label, cx + CW / 2 - tw / 2, CSY + 58, 12, onCd ? Color.Gray : Color.White);

                // Cost or cooldown
                if (onCd)
                {
                    // Cooldown overlay
                    int cdH = (int)(CH * cdPct);
                    Raylib.DrawRectangle(cx, CSY, CW, cdH, new Color(0, 0, 0, 120));
                    string cdStr = $"{_cdTimers[i]:F1}s";
                    int cdw = Fonts.Measure(cdStr, 12);
                    Fonts.Draw(cdStr, cx + CW / 2 - cdw / 2, CSY + 68, 12, Color.LightGray);
                }
                else
                {
                    string cost = $"[{Cards[i].Cost}]";
                    int cw2 = Fonts.Measure(cost, 12);
                    Fonts.Draw(cost, cx + CW / 2 - cw2 / 2, CSY + 68, 12, enough ? Color.Yellow : Color.Gray);
                }
            }

            // Shovel
            int shx2 = CSX + Cards.Length * (CW + CP) + 10;
            Raylib.DrawRectangle(shx2, CSY, 60, CH, ShovelSelected ? new Color(200, 100, 30, 255) : new Color(110, 75, 40, 220));
            Raylib.DrawRectangleLines(shx2, CSY, 60, CH, Color.Orange);
            Fonts.Draw("SHOVEL", shx2 + 10, CSY + 33, 10, Color.White);

            // Sun counter
            int sunX = Constants.SCREEN_W - 870;
            Raylib.DrawCircle(sunX + 16, 37, 14f, new Color(255, 210, 30, 255));
            Fonts.Draw($"Sun: {ctx.SunManager.Sun}", sunX + 34, 28, 20, Color.Yellow);
            Fonts.Draw($"Lives: {ctx.Lives}", sunX + 4, 52, 18,
                ctx.Lives > 10 ? new Color(255, 100, 100, 255) : new Color(255, 50, 50, 255));

            // Wave bar
            if (ctx.WaveManager != null)
            {
                int bx = 900, bw = 360, by = 70;
                Raylib.DrawRectangle(bx, by, bw, 13, new Color(35, 35, 55, 210));
                Raylib.DrawRectangle(bx, by, (int)(bw * ctx.WaveManager.WaveProgress), 13, new Color(90, 190, 255, 210));
                Raylib.DrawRectangleLines(bx, by, bw, 13, new Color(70, 70, 110, 255));
                string wl = ctx.WaveManager.WaveLabel;
                Fonts.Draw(wl, bx + bw / 2 - Fonts.Measure(wl, 11) / 2, by - 25, 20, Color.White);
            }

            // Night mode overlay
            if (ctx.IsNightMode)
            {
                Raylib.DrawRectangle(0, 0, Constants.SCREEN_W, Constants.SCREEN_H, new Color(0, 0, 40, 90));
                string nm = "** NIGHT MODE **";
                Fonts.Draw(nm, Constants.SCREEN_W / 2 - Fonts.Measure(nm, 18) / 2, 20, 18, new Color(140, 140, 255, 255));
            }
        }
    }
}
