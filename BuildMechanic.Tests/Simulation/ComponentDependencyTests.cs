using System.Linq;
using BuildMechanic.Simulation;
using BuildMechanic.Simulation.Graph;
using Xunit;

namespace BuildMechanic.Tests.Simulation
{
    public class ComponentDependencyTests
    {
        [Fact]
        public void DeepTorsoStack_EnforcesStrictFiveLayerDisassemblyAndReassembly()
        {
            var chassis = new MechChassis();
            chassis.Initialize();
            chassis.ConfigureDamage(new[] { "plate_torso_left" });

            var cowling = chassis.Components.First(c => c.Id == "comp_tl_cowling");
            var rib = chassis.Components.First(c => c.Id == "comp_tl_rib");
            var carrier = chassis.Components.First(c => c.Id == "comp_tl_carrier");
            var impeller = chassis.Components.First(c => c.Id == "comp_tl_impeller");

            Assert.True(impeller.IsFaulty);

            // L1 plate mounted -> blocks L2 cowling
            Assert.False(ComponentDependencySolver.CanUnmount(cowling, chassis, out string reason));
            Assert.Contains("BLOCKED BY", reason);

            // Remove L1 plate
            foreach (var b in chassis.LeftTorsoPlate.Bolts) b.ThreadProgress = 0f;
            Assert.True(chassis.LeftTorsoPlate.ToggleMount());
            chassis.LeftTorsoPlate.Update(5f);

            // L2 cowling still blocked by its own bolts, L3 rib blocked by L2 cowling
            Assert.False(ComponentDependencySolver.CanUnmount(rib, chassis, out _));
            Assert.False(ComponentDependencySolver.CanUnmount(cowling, chassis, out _));

            // Unbolt & remove L2 -> L3 -> L4 in order
            foreach (var b in cowling.Fasteners) b.ThreadProgress = 0f;
            Assert.True(ComponentDependencySolver.CanUnmount(cowling, chassis, out _));
            cowling.SetMountedState(ComponentMountState.Removed);

            foreach (var b in rib.Fasteners) b.ThreadProgress = 0f;
            Assert.True(ComponentDependencySolver.CanUnmount(rib, chassis, out _));
            rib.SetMountedState(ComponentMountState.Removed);

            foreach (var b in carrier.Fasteners) b.ThreadProgress = 0f;
            Assert.True(ComponentDependencySolver.CanUnmount(carrier, chassis, out _));
            carrier.SetMountedState(ComponentMountState.Removed);

            // Now L5 impeller is unblocked and can be serviced
            Assert.Empty(ComponentDependencySolver.GetBlockingNames(impeller, chassis));
            Assert.True(impeller.ServiceOrReplace());
            Assert.False(impeller.IsFaulty);

            // Cannot re-install L2 cowling before L3 rib and L4 carrier
            Assert.False(ComponentDependencySolver.CanMount(cowling, chassis, out _));
            Assert.True(ComponentDependencySolver.CanMount(carrier, chassis, out _));
            carrier.SetMountedState(ComponentMountState.Mounted);
            foreach (var b in carrier.Fasteners) b.ThreadProgress = 1f;

            Assert.True(ComponentDependencySolver.CanMount(rib, chassis, out _));
            rib.SetMountedState(ComponentMountState.Mounted);
            foreach (var b in rib.Fasteners) b.ThreadProgress = 1f;

            Assert.True(ComponentDependencySolver.CanMount(cowling, chassis, out _));
            cowling.SetMountedState(ComponentMountState.Mounted);
            foreach (var b in cowling.Fasteners) b.ThreadProgress = 1f;

            Assert.All(chassis.Components, c => Assert.True(c.IsNominal));
        }
    }
}
