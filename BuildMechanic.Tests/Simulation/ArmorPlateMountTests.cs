using Microsoft.Xna.Framework;
using BuildMechanic.Simulation;
using Xunit;

namespace BuildMechanic.Tests.Simulation
{
    public class ArmorPlateMountTests
    {
        [Fact]
        public void Plate_CannotDetach_IfAnyBoltIsStillFastened()
        {
            var plate = new ArmorPlate();
            plate.Bolts.Add(new Bolt { Id = "b1" });
            plate.Bolts.Add(new Bolt { Id = "b2" });
            plate.Bolts.Add(new Bolt { Id = "b3" });
            plate.Bolts.Add(new Bolt { Id = "b4" });

            Assert.False(plate.CanDetach);
            Assert.False(plate.ToggleMount());

            // Loosen 3 of 4
            plate.Bolts[0].Drive(-2.0f);
            plate.Bolts[1].Drive(-2.0f);
            plate.Bolts[2].Drive(-2.0f);

            Assert.False(plate.CanDetach);
            Assert.False(plate.ToggleMount());

            // Loosen last bolt
            plate.Bolts[3].Drive(-2.0f);

            Assert.True(plate.CanDetach);
            Assert.True(plate.ToggleMount());
            Assert.Equal(PlateMountState.MovingToDetached, plate.State);
        }

        [Fact]
        public void Plate_StateTransitions_BetweenMountedAndDetached()
        {
            var plate = new ArmorPlate
            {
                MountedPosition = new Vector3(0, 2.70f, 0.50f),
                DetachedPosition = new Vector3(-1.80f, 2.10f, 1.20f)
            };

            // Detach plate
            plate.ToggleMount();
            Assert.Equal(PlateMountState.MovingToDetached, plate.State);

            // Animate halfway
            plate.Update(0.25f);
            Assert.Equal(PlateMountState.MovingToDetached, plate.State);
            Assert.True(plate.CurrentPosition.X < 0.0f);

            // Finish animation
            plate.Update(5.0f);
            Assert.Equal(PlateMountState.Detached, plate.State);
            Assert.Equal(plate.DetachedPosition, plate.CurrentPosition);
            Assert.True(plate.CanMount);

            // Remount plate
            Assert.True(plate.ToggleMount());
            Assert.Equal(PlateMountState.MovingToMounted, plate.State);

            plate.Update(5.0f);
            Assert.Equal(PlateMountState.Mounted, plate.State);
            Assert.Equal(plate.MountedPosition, plate.CurrentPosition);
        }

        [Fact]
        public void Plate_RayIntersects_HitsAccurately_WhenMountedAndDetached()
        {
            var plate = new ArmorPlate
            {
                MountedPosition = new Vector3(0, 2.70f, 0.50f),
                DetachedPosition = new Vector3(-1.80f, 2.10f, 1.20f),
                Dimensions = new Vector3(0.9f, 1.1f, 0.08f)
            };

            // 1. Ray aiming at center of mounted plate
            Ray rayMounted = new Ray(plate.MountedPosition + new Vector3(0, 0, 2.0f), Vector3.Forward);
            Assert.True(plate.RayIntersects(rayMounted, out float distMounted));
            Assert.True(distMounted > 0f);

            // 2. Detach to workbench (plate tilts 0.25 rad)
            plate.ToggleMount();
            plate.Update(5.0f); // fully detached
            Assert.Equal(PlateMountState.Detached, plate.State);

            // Ray aiming at top and bottom of tilted plate
            Matrix world = plate.GetWorldMatrix();
            Vector3 localTop = new Vector3(0, 0.45f, 0.04f); // near top edge
            Vector3 worldTop = Vector3.Transform(localTop, world);

            Ray rayTiltedTop = new Ray(worldTop + new Vector3(0, 0, 1.0f), Vector3.Forward);
            Assert.True(plate.RayIntersects(rayTiltedTop, out float distTop));
            Assert.True(distTop > 0f);

            Vector3 localBot = new Vector3(0, -0.45f, 0.04f); // near bottom edge
            Vector3 worldBot = Vector3.Transform(localBot, world);

            Ray rayTiltedBot = new Ray(worldBot + new Vector3(0, 0, 1.0f), Vector3.Forward);
            Assert.True(plate.RayIntersects(rayTiltedBot, out float distBot));
            Assert.True(distBot > 0f);
        }

        [Fact]
        public void Bolt_IsRemoved_TolerantThreshold()
        {
            var bolt = new Bolt();
            Assert.Equal(1.0f, bolt.ThreadProgress);
            Assert.False(bolt.IsRemoved);

            // Drive to 0.08f
            bolt.ThreadProgress = 0.08f;
            Assert.True(bolt.IsRemoved);

            bolt.ThreadProgress = 0.081f;
            Assert.False(bolt.IsRemoved);

            bolt.ThreadProgress = 0.05f;
            Assert.True(bolt.IsRemoved);
        }
    }
}
