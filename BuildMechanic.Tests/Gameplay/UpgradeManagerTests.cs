using BuildMechanic.Gameplay;
using Xunit;

namespace BuildMechanic.Tests.Gameplay
{
    public class UpgradeManagerTests
    {
        [Fact]
        public void UpgradeManager_InitialState_HasSeedCapitalAndTier1Tools()
        {
            var mgr = new UpgradeManager();
            Assert.Equal(1500, mgr.Credits);
            Assert.Equal(1, mgr.DriverLevel);
            Assert.Equal(1, mgr.GrinderLevel);
            Assert.Equal(1, mgr.WelderLevel);

            Assert.Equal(1.0f, mgr.GetDriverSpeed());
            Assert.Equal(1.0f, mgr.GetGrinderSpeed());
            Assert.Equal(1.0f, mgr.GetWelderSpeed());
        }

        [Fact]
        public void UpgradeManager_UpgradeDriver_CostsCredits_AndIncreasesSpeed()
        {
            var mgr = new UpgradeManager { Credits = 10000 };

            // Level 1 -> 2 costs 3,000
            int cost1 = mgr.GetUpgradeCost(1);
            Assert.Equal(3000, cost1);

            bool success1 = mgr.TryUpgradeDriver();
            Assert.True(success1);
            Assert.Equal(2, mgr.DriverLevel);
            Assert.Equal(7000, mgr.Credits);
            Assert.Equal(1.65f, mgr.GetDriverSpeed());

            // Level 2 -> 3 costs 6,500
            int cost2 = mgr.GetUpgradeCost(2);
            Assert.Equal(6500, cost2);

            bool success2 = mgr.TryUpgradeDriver();
            Assert.True(success2);
            Assert.Equal(3, mgr.DriverLevel);
            Assert.Equal(500, mgr.Credits);
            Assert.Equal(2.30f, mgr.GetDriverSpeed());

            // Level 3 is MAX, upgrade fails
            bool success3 = mgr.TryUpgradeDriver();
            Assert.False(success3);
            Assert.Equal(3, mgr.DriverLevel);
        }

        [Fact]
        public void UpgradeManager_InsufficientFunds_PreventsUpgrade()
        {
            var mgr = new UpgradeManager { Credits = 500 };

            Assert.False(mgr.TryUpgradeGrinder());
            Assert.Equal(1, mgr.GrinderLevel);
            Assert.Equal(500, mgr.Credits);

            // Add payout reward
            mgr.AddReward(3000);
            Assert.Equal(3500, mgr.Credits);

            Assert.True(mgr.TryUpgradeGrinder());
            Assert.Equal(2, mgr.GrinderLevel);
            Assert.Equal(500, mgr.Credits);
            Assert.Equal(1.60f, mgr.GetGrinderSpeed());
        }
    }
}
