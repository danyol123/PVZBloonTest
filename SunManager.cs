using System;
using PVZBloonTest.UI;
using PVZBloonTest.Particles;

namespace PVZBloonTest
{
    public class SunManager
    {
        public int Sun { get; private set; } = 50;
        private float _timer = 0f;
        private const float Interval = 4f;   // natural sun drop every 4s
        private readonly Random _rng = new();

        public void Reset() { Sun = 10000; _timer = 0f; } // set to 10000 for testing
        public void AddSun(int v) => Sun += v;
        public bool SpendSun(int v) { if (Sun < v) return false; Sun -= v; return true; }

        public void Update(float dt, GameContext ctx)
        {
            if (ctx.IsNightMode) return;
            _timer += dt;
            if (_timer >= Interval)
            {
                _timer -= Interval;
                int col = _rng.Next(0, Constants.GRID_COLS);
                int row = _rng.Next(0, Constants.GRID_ROWS);

                ParticleManager.Instance.SunProduce(Grid.CellCenterX(col), Grid.CellCenterY(row) - 25);
                ctx.SunDrops.Add(new SunDrop(Grid.CellCenterX(col), Grid.CellCenterY(row) - 25, 25, false));
            }
        }
    }
}
