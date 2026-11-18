using System.Linq;
using BuildMechanic.Gameplay;
using BuildMechanic.Simulation;
using BuildMechanic.Simulation.Archetypes;
using BuildMechanic.Simulation.Graph;
using Xunit;

namespace BuildMechanic.Tests.Simulation
{
    public class RoadmapPhasesTests
    {
        [Fact]
        public void AllSixSectors_HaveMultiLayerComponentStacks()
        {
            var chassis = new MechChassis();
            chassis.Initialize();

            string[] sectors =
            {
                "plate_torso_left", "plate_torso_right",
                "plate_shoulder_left", "plate_shoulder_right",
                "plate_leg_left", "plate_leg_right"
            };

            foreach (var sector in sectors)
            {
                var comps = chassis.Components.Where(c => c.SectorId == sector).ToList();
                Assert.True(comps.Count >= 3, $"Sector {sector} should have at least 3 nested component layers.");
            }
        }

        [Fact]
        public void PartsLifecycle_CleanRust_AndCatalogOrderConsumeScrap()
        {
            var upgrades = new UpgradeManager { Credits = 1000 };
            var catalog = new PartsCatalogManager();
            Assert.Equal(1, catalog.GetStock(ComponentCategory.PowerPCB));

            Assert.True(catalog.OrderPart(ComponentCategory.PowerPCB, 420));
            Assert.Equal(2, catalog.GetStock(ComponentCategory.PowerPCB));
            Assert.Equal(580, upgrades.Credits);

            Assert.True(catalog.TryConsumeForReplacement(ComponentCategory.PowerPCB, 420));
            Assert.Equal(1, catalog.GetStock(ComponentCategory.PowerPCB));

            catalog.ScrapPart(80);
            Assert.Equal(1, catalog.TotalScrapped);
            Assert.Equal(660, upgrades.Credits);

            var comp = new MechComponent { Category = ComponentCategory.StructuralRib };
            comp.Configure(ComponentMountState.Mounted, faulty: false, fastenersTorqued: true, wear: ComponentWearState.Rusty);
            Assert.False(comp.IsNominal);
            Assert.True(comp.CleanRust(2.0f));
            Assert.Equal(ComponentWearState.Clean, comp.WearState);
            Assert.True(comp.IsNominal);
        }

        [Fact]
        public void MechArchetypes_CycleAcrossContracts()
        {
            var chassis = new MechChassis();
            var mgr = new ContractManager();

            mgr.LoadContract(1, chassis);
            Assert.Equal(MechClass.VanguardTitan, chassis.ActiveArchetype.Class);

            mgr.LoadContract(2, chassis);
            Assert.Equal(MechClass.SpectreRecon, chassis.ActiveArchetype.Class);

            mgr.LoadContract(3, chassis);
            Assert.Equal(MechClass.ColossusSiege, chassis.ActiveArchetype.Class);

            mgr.LoadContract(4, chassis);
            Assert.Equal(MechClass.ArachnidStrider, chassis.ActiveArchetype.Class);
        }
    }
}
