namespace PVZBloonTest.Events
{
    public class SweetDreamsEvent : IEventWave
    {
        public string Name => "Sweet Dreams!";
        public void Trigger(GameContext ctx, Difficulty diff) => ctx.IsNightMode = true;
    }
}
