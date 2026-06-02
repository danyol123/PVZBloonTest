using System;
using Raylib_cs;
namespace PVZBloonTest.Bloons
{
    public class CeramicBloon : Bloon
    {
        private static readonly Random _rng = new();
        public override int   Cost     => 15;
        public override Color PopColor => new Color(180, 130, 60, 255);
        public CeramicBloon(int row, float x) : base(row, x) { MaxHp = Hp = 120f; BaseSpeed = 1f; Block = 3; Radius = 36f; }

        public override void Draw()
        {
            DrawBase(new Color(180, 130, 60, 255), new Color(120, 80, 30, 255), Sprites.BloonCeramic);
        }

        public override void OnDeath(GameContext ctx)
        {
            Bloon spawn = _rng.Next(5) switch
            {
                0 => new RedBloon(Row, X),
                1 => new BlueBloon(Row, X),
                2 => new YellowBloon(Row, X),
                3 => new CamoBloon(Row, X),
                _ => new PinkBloon(Row, X)
            };
            spawn.SpeedMult = SpeedMult;
            ctx.BloonsToAdd.Add(spawn);
        }
    }
}
