using System.Collections.Generic;
using PVZBloonTest.Bloons;
using PVZBloonTest.Plants;
using PVZBloonTest.Projectiles;
using PVZBloonTest.UI;

namespace PVZBloonTest
{
    public class GameContext
    {
        public Difficulty    Difficulty       { get; set; }
        public int           Lives            { get; set; } = 20;
        public bool          IsNightMode      { get; set; } = false;
        public float         SpeedEventMult   { get; set; } = 1f;
        public float         HpEventMult      { get; set; } = 1f;

        public Grid          Grid             { get; } = new Grid();
        public SunManager    SunManager       { get; } = new SunManager();
        public UnlockManager UnlockManager    { get; set; } = null!;
        public WaveManager   WaveManager      { get; set; } = null!;

        public List<Bloon>      Bloons        { get; } = new();
        public List<Bloon>      BloonsToAdd   { get; } = new();
        public List<Projectile> Projectiles   { get; } = new();
        public List<SunDrop>    SunDrops      { get; } = new();

        // Event notification banner
        public string EventNotification      { get; set; } = "";
        public float  EventNotificationTimer { get; set; } = 0f;

        public bool GameOver  { get; set; } = false;
        public bool Victory   { get; set; } = false;

        public void Reset(Difficulty diff)
        {
            Difficulty      = diff;
            Lives           = 20;
            IsNightMode     = false;
            SpeedEventMult  = 1f;
            HpEventMult     = 1f;
            GameOver        = false;
            Victory         = false;
            EventNotification      = "";
            EventNotificationTimer = 0f;
            Bloons.Clear();
            BloonsToAdd.Clear();
            Projectiles.Clear();
            SunDrops.Clear();
            Grid.Clear();
            SunManager.Reset();
            Particles.ParticleManager.Instance.Clear();
        }
    }
}
