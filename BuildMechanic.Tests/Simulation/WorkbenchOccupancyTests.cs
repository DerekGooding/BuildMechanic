using BuildMechanic.Simulation;
using Xunit;

namespace BuildMechanic.Tests.Simulation
{
    public class WorkbenchOccupancyTests
    {
        [Fact]
        public void Workbench_AllowsOnlyOnePlatePerSide_AtAnyTime()
        {
            var chassis = new MechChassis();
            chassis.Initialize();

            // Unbolt both Left Breastplate and Left Pauldron
            foreach (var b in chassis.LeftTorsoPlate.Bolts) b.Drive(-2.0f);
            foreach (var b in chassis.LeftShoulderPlate.Bolts) b.Drive(-2.0f);

            // Initially both are mounted and can detach
            Assert.True(chassis.LeftTorsoPlate.CanDetach);
            Assert.True(chassis.LeftShoulderPlate.CanDetach);

            // Detach Left Breastplate to left workbench
            bool detachedTorso = chassis.LeftTorsoPlate.ToggleMount();
            Assert.True(detachedTorso);

            // Now Left Shoulder Plate's CanDetach MUST be false because workbench is occupied!
            Assert.False(chassis.LeftShoulderPlate.CanDetach, "Left Pauldron allowed detachment while Left Breastplate occupies workbench!");
            Assert.False(chassis.LeftShoulderPlate.ToggleMount(), "ToggleMount should fail when workbench is occupied!");

            // Right workbench is completely independent
            foreach (var b in chassis.RightTorsoPlate.Bolts) b.Drive(-2.0f);
            Assert.True(chassis.RightTorsoPlate.CanDetach, "Right Breastplate should be allowed on right workbench!");
            Assert.True(chassis.RightTorsoPlate.ToggleMount());

            // Remount Left Breastplate
            chassis.LeftTorsoPlate.Update(5.0f);
            chassis.LeftTorsoPlate.ToggleMount(); // Return to mounted
            chassis.LeftTorsoPlate.Update(5.0f);
            Assert.Equal(PlateMountState.Mounted, chassis.LeftTorsoPlate.State);

            // Left workbench is free again! Left Pauldron can now detach
            Assert.True(chassis.LeftShoulderPlate.CanDetach);
            Assert.True(chassis.LeftShoulderPlate.ToggleMount());
        }
    }
}
