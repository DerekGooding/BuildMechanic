using System;
using Microsoft.Xna.Framework;
using BuildMechanic.Simulation;
using Xunit;

namespace BuildMechanic.Tests.Spatial
{
    public class WorkbenchCollisionTests
    {
        [Fact]
        public void DetachedPlate_MustHavePositiveClearance_AboveWorkbenchSurface()
        {
            var chassis = new MechChassis();
            chassis.Initialize();

            var plate = chassis.LeftTorsoPlate;

            // Remove all bolts to allow detaching
            foreach (var bolt in plate.Bolts)
            {
                bolt.Drive(-2.0f);
            }

            Assert.True(plate.CanDetach, "Plate should be detachable after all bolts are loosened.");

            bool detached = plate.ToggleMount();
            Assert.True(detached, "ToggleMount should transition plate to detached.");

            // Update to complete animation
            plate.Update(5.0f);
            Assert.Equal(PlateMountState.Detached, plate.State);

            // Table top surface is at Y = 1.245m (cabinet 1.20m + slab 0.045m)
            float tableSurfaceY = 1.245f;

            // Calculate lowest point of tilted plate (tilt = 0.25 radians around X)
            float tilt = 0.25f;
            float halfHeight = plate.Dimensions.Y / 2f;
            float halfThickness = plate.Dimensions.Z / 2f;

            // Bottom edge height considering tilt
            float lowestY = plate.DetachedPosition.Y - (halfHeight * MathF.Cos(tilt)) - (halfThickness * MathF.Sin(tilt));

            Assert.True(lowestY > tableSurfaceY + 0.20f,
                $"Detached plate penetrates or clips workbench! LowestY={lowestY}m, TableSurfaceY={tableSurfaceY}m");

            // Cradle pads sit at Y = 1.58m; plate lowest point should rest right on or slightly above the cradle
            float cradlePadY = 1.58f;
            Assert.True(lowestY >= cradlePadY - 0.02f && lowestY <= cradlePadY + 0.08f,
                $"Plate is not seated correctly in cradle! LowestY={lowestY}m, CradlePadY={cradlePadY}m");
        }

        [Fact]
        public void DetachedPlate_BoundingBox_DoesNotIntersectWorkbenchBase()
        {
            var chassis = new MechChassis();
            chassis.Initialize();
            var plate = chassis.LeftTorsoPlate;

            // Table base cabinet box: Center (-1.80, 0.60, 1.20), Size (1.40, 1.20, 0.90)
            Vector3 tableBasePos = new Vector3(-1.80f, 0.60f, 1.20f);
            Vector3 tableBaseSize = new Vector3(1.40f, 1.20f, 0.90f);
            BoundingBox tableBox = new BoundingBox(
                tableBasePos - tableBaseSize / 2f,
                tableBasePos + tableBaseSize / 2f
            );

            // Move plate to detached position
            foreach (var b in plate.Bolts) b.Drive(-2.0f);
            plate.ToggleMount();
            plate.Update(5.0f);

            BoundingBox plateBox = plate.GetBoundingBox();

            Assert.False(plateBox.Intersects(tableBox),
                "Detached armor plate bounding box collides with workbench base!");
        }
    }
}
