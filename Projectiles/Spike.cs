using System.Numerics;
using Raylib_cs;
using PVZBloonTest.Bloons;
using PVZBloonTest.UI;

namespace PVZBloonTest.Projectiles
{
    public class Spike : Projectile
    {
        public bool IsHeatTipped { get; private set; }
        public bool IsHeadshot   { get; }
        public bool IsPlasma     { get; private set; }

        public Spike(int row, float x, float dmg, int pierce, bool heat, bool head, bool camo, bool plasma = false)
            : base(row, x, dmg, pierce)
        {
            IsHeatTipped = heat || head || plasma;
            IsHeadshot   = head;
            IsPlasma     = plasma;
            HasLead = heat || head || plasma;
            HasCamo = camo;
            Speed   = 520f;
        }

        /// <summary>Called by Torchwood: upgrades a normal spike to heat spike in-place.</summary>
        public bool UpgradeToHeat()
        {
            if (IsHeatTipped || IsPlasma) return false;
            IsHeatTipped = true;
            HasLead      = true;
            Damage      += 3f;
            return true;
        }

        /// <summary>Called by Torchwood: upgrades a heat spike to plasma spike in-place.</summary>
        public bool UpgradeToPlasma()
        {
            if (IsPlasma) return false;
            if (!IsHeatTipped) UpgradeToHeat();
            IsPlasma = true;
            HasLead  = true;
            Pierce  += 1;
            return true;
        }

        public override float DamageForBloon(Bloon b)
        {
            float d = Damage;
            if (b.IsLead && IsHeatTipped) d += 5f;
            if (IsPlasma) d += 8f;
            if (IsHeadshot && (b is CeramicBloon || b is MoabBloon)) d += 40f;
            return d;
        }

        protected override void OnHit(Bloon b, GameContext ctx)
        {
            if (b.IsDead && b.PhotoBuff)
                ctx.SunDrops.Add(new SunDrop(b.X, b.Y, 25, false));
        }

        public override void Draw()
        {
            Texture2D tex = IsPlasma     ? Sprites.SpikePlasma
                          : IsHeatTipped ? Sprites.SpikeHeat
                          :                Sprites.SpikeNormal;
            Sprites.DrawCentered(tex, X, Y, 32f, 16f);
        }
    }
}
