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
    }
}
