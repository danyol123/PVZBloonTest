namespace PVZBloonTest.Events
{
    public class ToughRubberEvent : IEventWave
    {
        public string Name => "Tough Rubber!";
        public void Trigger(GameContext ctx, Difficulty diff)
        {
            float bonus = diff switch { Difficulty.Hard => 2.5f, Difficulty.Normal => 2f, _ => 1.5f };
            ctx.HpEventMult = bonus;
            foreach (var b in ctx.Bloons) { b.Hp *= bonus; b.MaxHp *= bonus; }
        }
    }
}
