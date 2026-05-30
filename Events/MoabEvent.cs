using PVZBloonTest.Bloons;
namespace PVZBloonTest.Events
{
    public class MoabEvent : EventWave
    {
        public override string Name => "M.O.A.B.!";
        public override void Trigger(GameContext ctx, Difficulty diff)
        {
            float bx = Constants.GRID_X + Constants.GRID_COLS * Constants.CELL_W + 30f;
            ctx.BloonsToAdd.Add(new MoabBloon(2, bx));
            for (int row = 0; row < Constants.GRID_ROWS; row++)
                for (int i = 0; i < 4; i++)
                {
                    float ox = bx + 80f + i * 55f;
                    ctx.BloonsToAdd.Add(i % 2 == 0 ? new YellowBloon(row, ox) : (Bloon)new PinkBloon(row, ox));
                }
        }
    }
}
