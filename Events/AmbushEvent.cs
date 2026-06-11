using System;
using PVZBloonTest.Bloons;
namespace PVZBloonTest.Events
{
    public class AmbushEvent : IEventWave
    {
        private static readonly Random _rng = new();
        public string Name => "Ambush!";
        public void Trigger(GameContext ctx, Difficulty diff)
        {
            int count  = diff == Difficulty.Hard ? 6 : 4;
            int minCol = diff == Difficulty.Hard ? 4 : 6;
            for (int i = 0; i < count; i++)
            {
                int col = _rng.Next(minCol, minCol + 2);
                int row = _rng.Next(0, Constants.GRID_ROWS);
                float sx = Constants.GRID_X + col * Constants.CELL_W + Constants.CELL_W / 2f;
                ctx.BloonsToAdd.Add(new CamoBloon(row, sx));
            }
        }
    }
}
