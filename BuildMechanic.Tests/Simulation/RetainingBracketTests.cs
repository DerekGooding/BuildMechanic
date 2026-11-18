using System.Linq;
using BuildMechanic.Simulation;
using BuildMechanic.Tools;
using Microsoft.Xna.Framework;
using Xunit;

namespace BuildMechanic.Tests.Simulation
{
    public class RetainingBracketTests
    {
        [Fact]
        public void RetainingBracket_RequiresBoltsRemoved_BeforeToggle()
        {
            var chassis = new MechChassis();
            chassis.Initialize();
            chassis.ConfigureDamage(new[] { "plate_torso_left" });

            var bracket = chassis.Brackets.First(b => b.SectorId == "plate_torso_left");
            Assert.Equal(BracketState.Secured, bracket.State);
            Assert.True(bracket.IsFullySecured);
            Assert.False(bracket.CanRemove);
            Assert.False(bracket.TryToggle());

            foreach (var bolt in bracket.Bolts)
            {
                bolt.ThreadProgress = 0.0f;
            }

            Assert.True(bracket.CanRemove);
            Assert.True(bracket.TryToggle());
            Assert.Equal(BracketState.Removed, bracket.State);
            Assert.False(bracket.IsFullySecured);
        }

        [Fact]
        public void ToolRaycaster_ShieldsSubBayInternals_UntilBracketRemoved()
        {
            var chassis = new MechChassis();
            chassis.Initialize();
            chassis.ConfigureDamage(new[] { "plate_torso_left" });

            var plate = chassis.LeftTorsoPlate;
            foreach (var b in plate.Bolts) b.ThreadProgress = 0f;
            Assert.True(plate.ToggleMount());
            plate.Update(5.0f); // Complete transition to Detached

            var harness = chassis.Harnesses.First(h => h.SectorId == "plate_torso_left");
            var ray = new Ray(harness.MidPoint + new Vector3(0, 0, 1.0f), Vector3.Forward);

            // While bracket is Secured, harness cannot be targeted directly
            var hitBefore = ToolRaycaster.PerformRaycast(ray, chassis, ToolType.PryBar);
            Assert.Null(hitBefore.HoveredHarness);

            // Remove bracket bolts and toggle bracket
            var bracket = chassis.Brackets.First(b => b.SectorId == "plate_torso_left");
            foreach (var b in bracket.Bolts) b.ThreadProgress = 0f;
            Assert.True(bracket.TryToggle());

            // Now harness is accessible
            var hitAfter = ToolRaycaster.PerformRaycast(ray, chassis, ToolType.PryBar);
            Assert.Equal(harness, hitAfter.HoveredHarness);
        }
    }
}
