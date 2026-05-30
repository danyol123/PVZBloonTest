using System;
using Raylib_cs;

namespace PVZBloonTest.Particles
{
    /// <summary>One particle emitted when a bloon pops.</summary>
    public class BloonParticle
    {
        private float _x, _y;
        private float _vx, _vy;
        private float _life;        // seconds remaining
        private float _maxLife;
        private float _radius;
        private Color _color;

        public bool IsDead => _life <= 0f;

        public BloonParticle(float x, float y, float vx, float vy,
                             float life, float radius, Color color)
        {
            _x = x; _y = y;
            _vx = vx; _vy = vy;
            _life = _maxLife = life;
            _radius = radius;
            _color  = color;
        }

        public void Update(float dt)
        {
            if (IsDead) return;
            _x    += _vx * dt;
            _y    += _vy * dt;
            _vy   += 180f * dt;  // gentle gravity
            _vx   *= (1f - 3f * dt); // air drag
            _life -= dt;
        }

        public void Draw()
        {
            if (IsDead) return;
            float pct  = Math.Max(0f, _life / _maxLife);
            float r    = _radius * pct;
            byte  a    = (byte)(255 * pct);
            var   col  = new Color(_color.R, _color.G, _color.B, a);
            Raylib.DrawCircle((int)_x, (int)_y, r, col);
        }
    }
}
