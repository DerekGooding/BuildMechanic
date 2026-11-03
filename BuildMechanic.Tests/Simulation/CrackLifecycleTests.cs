using System.Collections.Generic;
using Microsoft.Xna.Framework;
using BuildMechanic.Simulation;
using Xunit;

namespace BuildMechanic.Tests.Simulation
{
    public class CrackLifecycleTests
    {
        [Fact]
        public void Crack_TransitionsThrough_AllFourStagesInSequence()
        {
            var crack = new CrackSegment
            {
                Id = "test_crack",
                LocalPoints = new List<Vector3>
                {
                    new Vector3(-0.2f, -0.2f, 0.041f),
                    new Vector3(0.2f, 0.2f, 0.041f)
                }
            };

            var plate = new ArmorPlate();
            plate.Cracks.Add(crack);

            // Stage 1: Fractured
            Assert.Equal(CrackStage.Fractured, crack.Stage);
            Assert.False(crack.IsRepaired);
            Assert.True(plate.Integrity <= 45f);

            // Welding does nothing while still in Fractured state
            Assert.False(crack.ApplyWelding(0.5f));
            Assert.Equal(CrackStage.Fractured, crack.Stage);

            // Step 1: Angle Grinding to prep groove
            Assert.False(crack.ApplyGrinding(0.5f));
            Assert.Equal(0.5f, crack.StageProgress);
            Assert.False(crack.IsRepaired);

            bool prepped = crack.ApplyGrinding(0.55f);
            Assert.True(prepped);
            Assert.Equal(CrackStage.PreppedGroove, crack.Stage);
            Assert.Equal(0.0f, crack.StageProgress);
            Assert.False(crack.IsRepaired);
            Assert.True(plate.Integrity >= 55f);

            // Grinding does not advance PreppedGroove directly to Welded
            Assert.False(crack.ApplyGrinding(0.5f));
            Assert.Equal(CrackStage.PreppedGroove, crack.Stage);

            // Step 2: Welding torch to lay bead
            Assert.False(crack.ApplyWelding(0.6f));
            Assert.Equal(0.6f, crack.StageProgress);

            bool welded = crack.ApplyWelding(0.45f);
            Assert.True(welded);
            Assert.Equal(CrackStage.WeldedBead, crack.Stage);
            Assert.Equal(0.0f, crack.StageProgress);
            Assert.False(crack.IsRepaired);
            Assert.True(plate.Integrity >= 75f);

            // Welding does not advance WeldedBead to Flush
            Assert.False(crack.ApplyWelding(0.5f));
            Assert.Equal(CrackStage.WeldedBead, crack.Stage);

            // Step 3: Second pass grinding to smooth flush
            Assert.False(crack.ApplyGrinding(0.7f));
            Assert.Equal(0.7f, crack.StageProgress);

            bool finished = crack.ApplyGrinding(0.35f);
            Assert.True(finished);
            Assert.Equal(CrackStage.FlushFinished, crack.Stage);
            Assert.True(crack.IsRepaired);
            Assert.Equal(100f, plate.Integrity);
            Assert.True(plate.IsCrackRepaired);
        }

        [Fact]
        public void Crack_RayIntersects_HitsAlongPolyline_AndIgnoresDistantRays()
        {
            var crack = new CrackSegment
            {
                LocalPoints = new List<Vector3>
                {
                    new Vector3(-0.2f, 0.0f, 0.041f),
                    new Vector3(0.2f, 0.0f, 0.041f)
                }
            };

            Matrix world = Matrix.Identity;

            // Direct hit at crack midpoint (0, 0, 0.041)
            Ray directRay = new Ray(new Vector3(0, 0, 2.0f), Vector3.Forward);
            Assert.True(crack.RayIntersects(directRay, world, out float dist, out Vector3 hitPoint));
            Assert.True(dist > 0f);
            Assert.True(MathF.Abs(hitPoint.Z - 0.041f) < 0.06f);

            // Distant ray aimed at top corner of plate (0.35, 0.45, 2.0)
            Ray distantRay = new Ray(new Vector3(0.35f, 0.45f, 2.0f), Vector3.Forward);
            Assert.False(crack.RayIntersects(distantRay, world, out _, out _));
        }
    }
}
