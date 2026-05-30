using System;
using System.IO;
using System.Text.Json;

namespace PVZBloonTest
{
    public class UnlockManager
    {
        private readonly string _savePath;
        public bool EasyBeaten    { get; private set; }
        public bool NormalBeaten  { get; private set; }
        public bool HardBeaten    { get; private set; }
        public bool Tier1Unlocked { get; private set; }
        public bool Tier2Unlocked { get; private set; }

        public UnlockManager()
        {
            _savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "save.json");
            Load();
        }

        public bool IsDifficultyUnlocked(Difficulty d) => d switch
        {
            Difficulty.Easy   => true,
            Difficulty.Normal => EasyBeaten,
            Difficulty.Hard   => NormalBeaten,
            _                 => false
        };

        public bool IsUpgradeTierUnlocked(int tier) => tier switch
        {
            1 => Tier1Unlocked,
            2 => Tier2Unlocked,
            _ => false
        };

        public void UnlockAfterBeating(Difficulty d)
        {
            switch (d)
            {
                case Difficulty.Easy:   EasyBeaten   = true; Tier1Unlocked = true; break;
                case Difficulty.Normal: NormalBeaten = true; Tier2Unlocked = true; break;
                case Difficulty.Hard:   HardBeaten   = true; break;
            }
            Save();
        }

        private record SaveData(bool Easy, bool Normal, bool Hard, bool T1, bool T2);

        private void Load()
        {
            try
            {
                if (!File.Exists(_savePath)) return;
                var d = JsonSerializer.Deserialize<SaveData>(File.ReadAllText(_savePath));
                if (d == null) return;
                EasyBeaten = d.Easy; NormalBeaten = d.Normal; HardBeaten = d.Hard;
                Tier1Unlocked = d.T1; Tier2Unlocked = d.T2;
            }
            catch { }
        }

        private void Save()
        {
            File.WriteAllText(_savePath, JsonSerializer.Serialize(
                new SaveData(EasyBeaten, NormalBeaten, HardBeaten, Tier1Unlocked, Tier2Unlocked)));
        }
    }
}
