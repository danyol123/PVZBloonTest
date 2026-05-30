namespace PVZBloonTest.Events
{
    public class SweetDreamsEvent : EventWave
    {
        public override string Name => "Sweet Dreams!";
        public override void Trigger(GameContext ctx, Difficulty diff) => ctx.IsNightMode = true;
    }
}
