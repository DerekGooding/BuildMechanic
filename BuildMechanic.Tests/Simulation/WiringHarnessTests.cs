using BuildMechanic.Simulation;
using Microsoft.Xna.Framework;
using Xunit;

namespace BuildMechanic.Tests.Simulation
{
    public class WiringHarnessTests
    {
        [Fact]
        public void WiringHarness_Charred_UnclipsAndSnapsToNominal()
        {
            var harness = new WiringHarness
            {
                Id = "harness_test",
                SectorId = "plate_torso_left",
                StartPos = new Vector3(-0.6f, 2.8f, 0.35f),
                EndPos = new Vector3(-0.3f, 2.8f, 0.35f)
            };

            harness.ConfigureFault(charred: true);
            Assert.Equal(HarnessState.Charred, harness.State);
            Assert.False(harness.IsNominal);
            Assert.False(harness.ProbeContinuity());
            Assert.True(harness.IsProbed);

            // Cannot snap replacement directly before unclipping charred bundle
            Assert.False(harness.SnapReplacement());

            Assert.True(harness.UnclipCharred());
            Assert.Equal(HarnessState.Unclipped, harness.State);
            Assert.False(harness.IsNominal);

            Assert.True(harness.SnapReplacement());
            Assert.Equal(HarnessState.Nominal, harness.State);
            Assert.True(harness.IsNominal);
            Assert.True(harness.ProbeContinuity());
        }

        [Fact]
        public void MechChassis_ConfigureDamage_FaultsSectorHarnessAndSecuresBracket()
        {
            var chassis = new MechChassis();
            chassis.Initialize();
            chassis.ConfigureDamage(new[] { "plate_torso_left" });

            Assert.Contains(chassis.Harnesses, h => h.SectorId == "plate_torso_left" && h.State == HarnessState.Charred);
            Assert.All(chassis.Harnesses.FindAll(h => h.SectorId == "plate_torso_right"), h => Assert.True(h.IsNominal));
            Assert.False(chassis.IsSectorSubBayNominal("plate_torso_left"));
        }
    }
}
