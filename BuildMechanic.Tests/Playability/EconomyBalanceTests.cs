using System.Linq;
using BuildMechanic.Gameplay;
using BuildMechanic.Simulation;
using BuildMechanic.Simulation.Graph;
using Xunit;

namespace BuildMechanic.Tests.Playability
{
    public class EconomyBalanceTests
    {
        [Fact]
        public void ContractsOneThroughEight_NeverSoftlockOnReplacementParts()
        {
            var upgrades = new UpgradeManager { Credits = 1500 };
            var catalog = new PartsCatalogManager();
            var chassis = new MechChassis();
            var manager = new ContractManager();

            for (int c = 1; c <= 8; c++)
            {
                manager.LoadContract(c, chassis);

                var seizedParts = chassis.Components
                    .Where(comp => comp.IsFaulty || comp.WearState == ComponentWearState.Seized)
                    .ToList();

                foreach (var part in seizedParts)
                {
                    bool canReplace = catalog.TryConsumeForReplacement(part.Category, part.ReplacementCost);
                    Assert.True(canReplace, $"Economy softlock on Contract #{c}: cannot afford or stock {part.DisplayName} (Credits={upgrades.Credits}).");
                    catalog.ScrapPart(part.ScrapValue);
                }

                // Award contract completion payout
                upgrades.AddReward(manager.ActiveContract.Reward);
            }

            Assert.True(upgrades.Credits > 1500, "Player should net positive workshop credits across Contracts 1-8.");
        }
    }
}
