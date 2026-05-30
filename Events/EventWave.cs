namespace PVZBloonTest.Events
{
    public abstract class EventWave
    {
        public abstract string Name { get; }
        public abstract void Trigger(GameContext ctx, Difficulty diff);
    }
}
