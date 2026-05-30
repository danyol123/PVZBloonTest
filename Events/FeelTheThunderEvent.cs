using System.Linq;
namespace PVZBloonTest.Events
{
    public class FeelTheThunderEvent : EventWave
    {
        public override string Name => "Feel the Thunder!";
        public override void Trigger(GameContext ctx, Difficulty diff)
        {
            int   strikes = diff == Difficulty.Hard ? 5 : 3;
            float damage  = diff == Difficulty.Hard ? 100f : 50f;
            var targets = ctx.Grid.AllPlants()
                .Where(p => !p.IsDead)
                .OrderByDescending(p => p.Hp)
                .Take(strikes);
            foreach (var p in targets)
            {
                p.TakeDamage(damage);
                PVZBloonTest.Particles.ParticleManager.Instance.Lightning(
                    PVZBloonTest.Grid.CellCenterX(p.Col),
                    PVZBloonTest.Grid.CellCenterY(p.Row));
            }
        }
    }
}
