using PVZBloonTest.Bloons;
namespace PVZBloonTest.Events
{
    public class BloonSwarmEvent : EventWave
    {
        public override string Name => "Bloon Swarm!";
        public override void Trigger(GameContext ctx, Difficulty diff)
        {
            float bx = Constants.GRID_X + Constants.GRID_COLS * Constants.CELL_W + 20f;
            for (int row = 0; row < Constants.GRID_ROWS; row++)
                for (int i = 0; i < 6; i++)
                {
                    float ox = bx + i * 60f;
                    Bloon b = diff switch
                    {
                        Difficulty.Hard   => i % 3 == 0 ? new PinkBloon(row, ox) : i % 2 == 0 ? new YellowBloon(row, ox) : (Bloon)new BlueBloon(row, ox),
                        Difficulty.Normal => i % 3 == 0 ? new YellowBloon(row, ox) : i % 2 == 0 ? new BlueBloon(row, ox) : (Bloon)new RedBloon(row, ox),
                        _                 => i % 2 == 0 ? new BlueBloon(row, ox) : (Bloon)new RedBloon(row, ox)
                    };
                    ctx.BloonsToAdd.Add(b);
                }
        }
    }
}
