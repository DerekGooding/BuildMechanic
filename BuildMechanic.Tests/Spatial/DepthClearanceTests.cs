using System;
using Microsoft.Xna.Framework;
using BuildMechanic.Simulation;
using Xunit;

namespace BuildMechanic.Tests.Spatial
{
    public class DepthClearanceTests
    {
        [Fact]
        public void TorsoInnerFrame_MustBeStrictlyBehind_MountingBossesAndArmorPlates()
        {
            // Torso Inner Frame: Center (0, 2.7, 0.05), Size (1.40, 1.40, 0.70)
            float torsoFrontZ = 0.05f + (0.70f / 2f); // 0.40m
            
            // Mounting Boss Blocks: Center (-0.52, 2.7, 0.43), Size (0.76, 0.96, 0.06)
            float bossBackZ = 0.43f - (0.06f / 2f);   // 0.40m
            float bossFrontZ = 0.43f + (0.06f / 2f);  // 0.46m

            // Armor Plate: Mounted Center Z = 0.50m, Thickness = 0.08m
            float plateCenterZ = 0.50f;
            float plateBackZ = plateCenterZ - (0.08f / 2f);  // 0.46m
            float plateFrontZ = plateCenterZ + (0.08f / 2f); // 0.54m

            Assert.True(torsoFrontZ <= bossBackZ + 0.001f, 
                $"Torso inner frame ({torsoFrontZ}m) protrudes into mounting boss ({bossBackZ}m)!");

            Assert.True(Math.Abs(bossFrontZ - plateBackZ) < 0.002f, 
                $"Mounting boss front ({bossFrontZ}m) does not flush dock with plate back ({plateBackZ}m)!");

            Assert.True(plateBackZ >= torsoFrontZ + 0.05f, 
                $"Armor plate is inside the torso chest box! PlateBack={plateBackZ}m, TorsoFront={torsoFrontZ}m");
        }

        [Fact]
        public void AllFasteners_MustBePositionedProudOfChestBox_AndVisible()
        {
            var chassis = new MechChassis();
            chassis.Initialize();

            float torsoFrontZ = 0.40f;

            foreach (var plate in chassis.Plates)
            {
                Matrix plateWorld = plate.GetWorldMatrix();

                Assert.True(plate.Bolts.Count >= 4, $"Plate {plate.Id} should have at least 4 bolts.");

                foreach (var bolt in plate.Bolts)
                {
                    Vector3 worldPos = bolt.GetWorldPosition(plateWorld);

                    // Bolt head must be strictly in front of the torso inner frame
                    Assert.True(worldPos.Z > torsoFrontZ + 0.10f,
                        $"Bolt {bolt.Id} on plate {plate.Id} is submerged or clipped by the chest box! Z={worldPos.Z}");

                    // Bolt head base sits on or proud of the plate front surface (Z = 0.54m)
                    Assert.True(worldPos.Z >= 0.535f,
                        $"Bolt {bolt.Id} head is recessed behind the front face of the plate! Z={worldPos.Z}");
                }
            }
        }

        [Fact]
        public void SternalKeel_MustFitBetweenLeftAndRightBreastplates_WithoutOverlap()
        {
            var chassis = new MechChassis();
            chassis.Initialize();

            // Left plate extends to X = -0.52 + 0.425 = -0.095m
            float leftPlateInnerX = chassis.LeftTorsoPlate.MountedPosition.X + (chassis.LeftTorsoPlate.Dimensions.X / 2f);
            // Right plate extends to X = +0.52 - 0.425 = +0.095m
            float rightPlateInnerX = chassis.RightTorsoPlate.MountedPosition.X - (chassis.RightTorsoPlate.Dimensions.X / 2f);

            // Sternal Keel width is 0.16m, centered at X = 0 (from -0.08m to +0.08m)
            float keelHalfWidth = 0.16f / 2f; // 0.08m
            float keelLeftX = -keelHalfWidth;
            float keelRightX = +keelHalfWidth;

            Assert.True(keelLeftX > leftPlateInnerX, 
                $"Keel left edge ({keelLeftX}) overlaps Left Breastplate ({leftPlateInnerX})!");

            Assert.True(keelRightX < rightPlateInnerX, 
                $"Keel right edge ({keelRightX}) overlaps Right Breastplate ({rightPlateInnerX})!");
        }
    }
}
