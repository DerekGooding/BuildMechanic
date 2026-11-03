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
    }
}
