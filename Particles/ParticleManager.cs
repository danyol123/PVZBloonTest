using System;
using System.Collections.Generic;
using Raylib_cs;

namespace PVZBloonTest.Particles
{
    /// <summary>
    /// Manages all active bloon-pop particles.
    /// Call ParticleManager.Instance.Burst() from Bloon.OnDeath to spawn particles.
    /// Call Update and Draw once per frame from Game.
    /// </summary>
    public class ParticleManager
    {
        public static readonly ParticleManager Instance = new();
        private ParticleManager() { }

        private readonly List<BloonParticle> _particles = new();
        private static readonly Random _rng = new();

        /// <summary>Spawn a burst of coloured particles at (x, y).</summary>
        public void Burst(float x, float y, Color color, int count = 14, float baseRadius = 7f)
        {
            BurstEx(x, y, color, count, baseRadius, speed: 60f, speedRange: 160f, life: 0.35f, lifeRange: 0.35f);
        }

        // ── Plant event bursts ──────────────────────────────────────────────

        /// <summary>Green sparkle when a plant is placed.</summary>
        public void PlantPlace(float x, float y)
        {
            BurstEx(x, y, new Color(80, 220, 80,  255), 18, 6f, 120f, 160f, 0.45f, 0.35f, upBias: 80f);
            BurstEx(x, y, new Color(70, 100, 50, 255), 8, 5f, 240f, 120f, 0.30f, 0.25f, upBias: 60f);
        }

        /// <summary>Gold starburst when a plant is upgraded.</summary>
        public void PlantUpgrade(float x, float y)
        {
            BurstEx(x, y, new Color(255, 210, 40,  255), 24, 7f, 160f, 180f, 0.55f, 0.45f, upBias: 90f);
            BurstEx(x, y, new Color(255, 255, 180, 255), 12, 4f, 260f, 140f, 0.35f, 0.30f, upBias: 110f);
        }

        /// <summary>Brown/grey dust when a plant dies or is shoveled.</summary>
        public void PlantDeath(float x, float y)
        {
            BurstEx(x, y, new Color(140, 100, 60,  255), 16, 8f,  80f, 90f, 0.50f, 0.30f, upBias: 30f);
            BurstEx(x, y, new Color(200, 200, 200, 255), 10, 5f,  50f, 60f, 0.40f, 0.25f, upBias: 20f);
        }

        /// <summary>Yellow/gold sparkle when a sun drop is collected.</summary>
        public void SunCollect(float x, float y)
        {
            BurstEx(x, y, new Color(255, 220, 40,  255), 12, 4f, 120f, 160f, 0.60f, 0.40f, upBias: 160f);
            BurstEx(x, y, new Color(255, 255, 255, 255),  6, 4f, 400f,  300f, 0.35f, 0.30f, upBias: 120f);
        }

        /// <summary>Yellow/gold sparkle when a sun drop is produced.</summary>
        public void SunProduce(float x, float y)
        {
            BurstEx(x, y, new Color(255, 220, 40,  255), 16, 5f, 100f, 140f, 0.40f, 0.30f, upBias: 100f);
            BurstEx(x, y, new Color(255, 255, 160, 255),  8, 3f, 180f,  80f, 0.25f, 0.20f, upBias: 120f);
        }

        /// <summary>Silver/grey metallic shards when lead shielding is stripped.</summary>
        public void LeadShatter(float x, float y)
        {
            BurstEx(x, y, new Color(200, 200, 220, 255), 16, 6f, 340f, 280f, 0.45f, 0.30f, upBias: 60f);
            BurstEx(x, y, new Color(80, 80, 80, 255), 8, 10f,  200f, 130f, 0.30f, 0.20f, upBias: 80f);
        }

        /// <summary>Electric yellow/white sparks for the lightning event.</summary>
        public void Lightning(float x, float y)
        {
            BurstEx(x, y, new Color(255, 240, 80,  255), 28, 5f, 200f, 250f, 0.40f, 0.35f, upBias: 60f);
            BurstEx(x, y, new Color(255, 255, 255, 255), 14, 3f, 350f, 200f, 0.25f, 0.25f, upBias: 80f);
        }

        /// <summary>Tiny red sparks when a bloon gnaws on a plant.</summary>
        public void PlantHit(float x, float y)
        {
            BurstEx(x, y, new Color(255, 80,  40,  255),  6, 4f,  60f,  80f, 0.25f, 0.15f, upBias: 30f);
        }

        /// <summary>Tiny white/cyan sparks when a projectile hits a bloon.</summary>
        public void ProjectileHit(float x, float y)
        {
            BurstEx(x, y, new Color(220, 255, 255, 255),  6, 5f,  80f,  80f, 0.30f, 0.15f, upBias: 20f);
        }

        /// <summary>Large fiery burst for explosions (Explode-o-nut, explosive peas).</summary>
        public void Explosion(float x, float y)
        {
            BurstEx(x, y, new Color(255, 140, 20,  255), 30, 10f, 160f, 220f, 0.55f, 0.35f, upBias: 60f);
            BurstEx(x, y, new Color(255, 60,  20,  255), 16,  7f, 220f, 180f, 0.40f, 0.30f, upBias: 40f);
            BurstEx(x, y, new Color(255, 255, 200, 255), 12,  5f, 300f, 150f, 0.30f, 0.25f, upBias: 80f);
        }

        /// <summary>
        /// Continuous purple/yellow smoke aura for Photosynthesis-buffed plants.
        /// Call once per Draw() frame per buffed plant — emits 1-2 drifting puffs.
        /// </summary>
        public void PhotoAura(float x, float y)
        {
            BurstEx(x, y, new Color(180, 80, 255, 255), 2, 6f, 50f, 40f, 0.4f, 0.3f, upBias: 50f);
            BurstEx(x, y, new Color(255, 220, 60, 255), 1, 5f, 40f, 30f, 0.5f, 0.4f, upBias: 25f);
        }

        // ── Core helper ────────────────────────────────────────────────────

        private void BurstEx(float x, float y, Color color,
                             int count, float baseRadius,
                             float speed, float speedRange,
                             float life, float lifeRange,
                             float upBias = 40f)
        {
            for (int i = 0; i < count; i++)
            {
                double angle = _rng.NextDouble() * Math.PI * 2;
                float  spd   = speed + (float)_rng.NextDouble() * speedRange;
                float  vx    = (float)Math.Cos(angle) * spd;
                float  vy    = (float)Math.Sin(angle) * spd - upBias;
                float  lt    = life + (float)_rng.NextDouble() * lifeRange;
                float  rad   = baseRadius * (0.5f + (float)_rng.NextDouble() * 0.8f);
                _particles.Add(new BloonParticle(x, y, vx, vy, lt, rad, color));
            }
        }

        public void Update(float dt)
        {
            foreach (var p in _particles) p.Update(dt);
            _particles.RemoveAll(p => p.IsDead);
        }

        public void Draw()
        {
            foreach (var p in _particles) p.Draw();
        }

        public void Clear() => _particles.Clear();
    }
}
