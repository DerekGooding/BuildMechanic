using System;

namespace BuildMechanic.Gameplay
{
    public class UpgradeManager
    {
        public static UpgradeManager Instance { get; private set; }

        public int Credits { get; set; } = 1500; // Starting workshop seed capital

        public int DriverLevel { get; private set; } = 1;
        public int GrinderLevel { get; private set; } = 1;
        public int WelderLevel { get; private set; } = 1;

        public const int MaxLevel = 3;

        public UpgradeManager()
        {
            Instance = this;
        }

        public int GetUpgradeCost(int currentLevel) => currentLevel switch
        {
            1 => 3000,
            2 => 6500,
            _ => 0
        };

        public float GetDriverSpeed() => 1.0f + (DriverLevel - 1) * 0.65f;
        public float GetGrinderSpeed() => 1.0f + (GrinderLevel - 1) * 0.60f;
        public float GetWelderSpeed() => 1.0f + (WelderLevel - 1) * 0.60f;

        public bool TryUpgradeDriver()
        {
            if (DriverLevel >= MaxLevel) return false;
            int cost = GetUpgradeCost(DriverLevel);
            if (Credits < cost) return false;

            Credits -= cost;
            DriverLevel++;
            return true;
        }

        public bool TryUpgradeGrinder()
        {
            if (GrinderLevel >= MaxLevel) return false;
            int cost = GetUpgradeCost(GrinderLevel);
            if (Credits < cost) return false;

            Credits -= cost;
            GrinderLevel++;
            return true;
        }

        public bool TryUpgradeWelder()
        {
            if (WelderLevel >= MaxLevel) return false;
            int cost = GetUpgradeCost(WelderLevel);
            if (Credits < cost) return false;

            Credits -= cost;
            WelderLevel++;
            return true;
        }

        public void AddReward(int amount)
        {
            Credits += Math.Max(0, amount);
        }
    }
}
