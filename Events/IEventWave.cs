namespace PVZBloonTest.Events
{
    public interface IEventWave
    {
        string Name { get; }
        void Trigger(GameContext ctx, Difficulty diff);
    }
}
