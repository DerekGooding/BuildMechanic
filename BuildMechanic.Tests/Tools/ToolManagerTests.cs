using Microsoft.Xna.Framework;
using BuildMechanic.Simulation;
using BuildMechanic.Tools;
using Xunit;

namespace BuildMechanic.Tests.Tools
{
    public class ToolManagerTests
    {
        [Fact]
        public void ToolManager_ToolSelection_AndDirectionToggle()
        {
            var manager = new ToolManager();
            Assert.Equal(ToolType.Scanner, manager.ActiveTool);
            Assert.Equal(DriverDirection.Reverse, manager.DriverDir);

            manager.SelectTool(ToolType.ImpactDriver);
            Assert.Equal(ToolType.ImpactDriver, manager.ActiveTool);

            manager.ToggleDriverDirection();
            Assert.Equal(DriverDirection.Forward, manager.DriverDir);

            manager.ToggleDriverDirection();
            Assert.Equal(DriverDirection.Reverse, manager.DriverDir);

            manager.SelectTool(ToolType.PryBar);
            Assert.Equal(ToolType.PryBar, manager.ActiveTool);

            manager.SelectTool(ToolType.AngleGrinder);
            Assert.Equal(ToolType.AngleGrinder, manager.ActiveTool);

            manager.SelectTool(ToolType.Welder);
            Assert.Equal(ToolType.Welder, manager.ActiveTool);
        }

        [Fact]
        public void ToolManager_PryBar_PrioritizesPlateRaycast()
        {
            var chassis = new MechChassis();
            chassis.Initialize();
            chassis.ConfigureDamage(new[] { "plate_torso_left" });

            var manager = new ToolManager();
            manager.SelectTool(ToolType.PryBar);

            // Ray aimed directly through the crack on the Left Breastplate
            Vector3 crackPoint = chassis.LeftTorsoPlate.Cracks[0].LocalPoints[0];
            Vector3 worldTarget = Vector3.Transform(crackPoint, chassis.LeftTorsoPlate.GetWorldMatrix());
            Ray ray = new Ray(worldTarget + new Vector3(0, 0, 1.5f), Vector3.Forward);

            var gameTime = new GameTime();
            manager.Update(gameTime, ray, chassis, null);

            // With Pry Bar selected, HoveredPlate must be chosen directly
            Assert.NotNull(manager.HoveredPlate);
            Assert.Equal("plate_torso_left", manager.HoveredPlate.Id);
            Assert.Null(manager.HoveredCrack);
            Assert.Null(manager.HoveredBolt);
        }

        [Fact]
        public void ToolManager_ImpactDriver_SelectsBolt_AndPreventsDetachedDriving()
        {
            var chassis = new MechChassis();
            chassis.Initialize();

            var manager = new ToolManager();
            manager.SelectTool(ToolType.ImpactDriver);

            // Ray aimed at bolt 1
            Bolt bolt = chassis.LeftTorsoPlate.Bolts[0];
            Vector3 boltWorld = bolt.GetWorldPosition(chassis.LeftTorsoPlate.GetWorldMatrix());
            Ray ray = new Ray(boltWorld + new Vector3(0, 0, 1.5f), Vector3.Forward);

            var gameTime = new GameTime();
            manager.Update(gameTime, ray, chassis, null);

            Assert.NotNull(manager.HoveredBolt);
            Assert.Equal(bolt.Id, manager.HoveredBolt.Id);
            Assert.NotNull(manager.HoveredPlate);
        }

        [Fact]
        public void ToolManager_GrindAndWeld_RequiresDetachedPlate()
        {
            var manager = new ToolManager();
            var plate = new ArmorPlate();
            var crack = new CrackSegment
            {
                Stage = CrackStage.Fractured
            };

            // 1. Mounted plate -> Grinding and welding MUST be prohibited
            Assert.Equal(PlateMountState.Mounted, plate.State);
            Assert.False(manager.CanRepairCrack(crack, plate, ToolType.AngleGrinder));
            Assert.False(manager.CanRepairCrack(crack, plate, ToolType.Welder));

            // 2. Detached plate -> Grinding fractured crack is allowed, welding is not yet allowed
            plate.Bolts.Clear(); // no bolts locking it
            plate.ToggleMount();
            plate.Update(5.0f);
            Assert.Equal(PlateMountState.Detached, plate.State);

            Assert.True(manager.CanRepairCrack(crack, plate, ToolType.AngleGrinder));
            Assert.False(manager.CanRepairCrack(crack, plate, ToolType.Welder));

            // 3. Prep the groove -> Welding allowed, grinding paused
            crack.Stage = CrackStage.PreppedGroove;
            Assert.False(manager.CanRepairCrack(crack, plate, ToolType.AngleGrinder));
            Assert.True(manager.CanRepairCrack(crack, plate, ToolType.Welder));

            // 4. Bead deposited -> Flush grinding allowed
            crack.Stage = CrackStage.WeldedBead;
            Assert.True(manager.CanRepairCrack(crack, plate, ToolType.AngleGrinder));
            Assert.False(manager.CanRepairCrack(crack, plate, ToolType.Welder));

            // 5. Finished -> Neither allowed
            crack.Stage = CrackStage.FlushFinished;
            Assert.False(manager.CanRepairCrack(crack, plate, ToolType.AngleGrinder));
            Assert.False(manager.CanRepairCrack(crack, plate, ToolType.Welder));
        }
    }
}
