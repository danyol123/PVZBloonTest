namespace PVZBloonTest.Events
{
    public class SpeedyWindsEvent : IEventWave
    {
        public string Name => "Speedy Winds!";
        public void Trigger(GameContext ctx, Difficulty diff)
        {
            float bonus = diff switch { Difficulty.Hard => 1.75f, Difficulty.Normal => 1.50f, _ => 1.25f };
            ctx.SpeedEventMult = bonus;
            foreach (var b in ctx.Bloons) b.SpeedMult *= bonus;
        }
    }
}
