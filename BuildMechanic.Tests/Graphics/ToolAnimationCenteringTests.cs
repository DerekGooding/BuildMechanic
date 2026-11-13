using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics.MeshBuilders;
using Xunit;

namespace BuildMechanic.Tests.Graphics
{
    public class ToolAnimationCenteringTests
    {
        private readonly Viewport _viewport = new Viewport(0, 0, 1280, 720);
        private readonly Matrix _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), 1280f / 720f, 0.01f, 10f);

        [Fact]
        public void ImpactDriver_SpindleAxisRotation_CenterlineDeadCenterForAllAngles()
        {
            Vector3 hit = new Vector3(0.52f, 2.70f, 0.50f);
            float recoil = 0.005f;
            Vector3 socketTipPos = hit + new Vector3(0, 0, recoil);

            // Test full 360-degree rotation in 15-degree increments
            for (int deg = 0; deg <= 360; deg += 15)
            {
                float spinAngle = MathHelper.ToRadians(deg);
                Matrix world = Matrix.CreateRotationY(spinAngle) *
                               Matrix.CreateRotationX(MathHelper.PiOver2) *
                               Matrix.CreateTranslation(socketTipPos);

                // 1. Tip Center (y = 0)
                Vector3 tipLocal = Vector3.Zero;
                Vector3 tipTransformed = Vector3.Transform(tipLocal, world);

                Assert.Equal(hit.X, tipTransformed.X, 5);
                Assert.Equal(hit.Y, tipTransformed.Y, 5);
                Assert.Equal(hit.Z + recoil, tipTransformed.Z, 5);

                // 2. Midpoint along bit axis (y = 0.045f)
                Vector3 midLocal = new Vector3(0, 0.045f, 0);
                Vector3 midTransformed = Vector3.Transform(midLocal, world);

                Assert.Equal(hit.X, midTransformed.X, 5);
                Assert.Equal(hit.Y, midTransformed.Y, 5);
                Assert.Equal(hit.Z + 0.045f + recoil, midTransformed.Z, 5);

                // 3. Base entering collar (y = 0.09f)
                Vector3 baseLocal = new Vector3(0, 0.09f, 0);
                Vector3 baseTransformed = Vector3.Transform(baseLocal, world);

                Assert.Equal(hit.X, baseTransformed.X, 5);
                Assert.Equal(hit.Y, baseTransformed.Y, 5);
                Assert.Equal(hit.Z + 0.09f + recoil, baseTransformed.Z, 5);
            }
        }

        [Fact]
        public void ImpactDriver_SpindleAxisRotation_PerimeterPointMaintainsConstantRadius()
        {
            Vector3 hit = new Vector3(1.0f, -0.5f, 0.25f);
            float radius = 0.024f;
            float recoil = 0.002f;
            Vector3 socketTipPos = hit + new Vector3(0, 0, recoil);

            for (int deg = 0; deg <= 360; deg += 30)
            {
                float spinAngle = MathHelper.ToRadians(deg);
                Matrix world = Matrix.CreateRotationY(spinAngle) *
                               Matrix.CreateRotationX(MathHelper.PiOver2) *
                               Matrix.CreateTranslation(socketTipPos);

                // Point on perimeter at angle 0
                Vector3 ptLocal = new Vector3(radius, 0.03f, 0);
                Vector3 ptWorld = Vector3.Transform(ptLocal, world);

                // Distance from spindle axis (hit.X, hit.Y) must strictly equal radius
                float distFromAxis = MathF.Sqrt(
                    MathF.Pow(ptWorld.X - hit.X, 2) +
                    MathF.Pow(ptWorld.Y - hit.Y, 2));

                Assert.Equal(radius, distFromAxis, 4);
                Assert.Equal(hit.Z + 0.03f + recoil, ptWorld.Z, 4);
            }
        }

        [Fact]
        public void ImpactDriver_FlailingBugRegressionTest_OldZRotationFlailsBy9cm()
        {
            Vector3 hit = new Vector3(0, 0, 0);
            Vector3 bitPos = hit + new Vector3(0, 0, 0.04f);

            // With the old buggy transform: CreateRotationZ(spinAngle) * CreateRotationX(1.57f) * CreateTranslation(bitPos)
            float spinAngle = MathHelper.PiOver2; // 90 degrees
            Matrix oldBuggyWorld = Matrix.CreateRotationZ(spinAngle) *
                                  Matrix.CreateRotationX(1.57f) *
                                  Matrix.CreateTranslation(bitPos);

            Vector3 basePoint = new Vector3(0, 0.09f, 0);
            Vector3 transformed = Vector3.Transform(basePoint, oldBuggyWorld);

            // The old buggy code caused a massive 0.09m (9 cm) off-axis flail!
            float flailOffset = MathF.Abs(transformed.X - hit.X);
            Assert.True(flailOffset > 0.089f, $"Expected old bug to flail by ~0.09m, was {flailOffset}");
        }

        [Fact]
        public void ImpactSocket_MouthLipAndCavityFloor_ScreenSpaceWindingClockwise()
        {
            float radius = 0.024f;
            float mouthR = radius * 0.65f;
            float recessDepth = 0.018f;

            // Camera looking directly down into the socket mouth from -Y
            Matrix view = Matrix.CreateLookAt(new Vector3(0, -0.5f, 0), Vector3.Zero, Vector3.Forward);

            // 1. Front Lip Quad at angle 0
            float a1 = 0f, a2 = MathF.PI / 8f;
            Vector3 o0 = new Vector3(MathF.Cos(a1) * radius, 0, MathF.Sin(a1) * radius);
            Vector3 o1 = new Vector3(MathF.Cos(a2) * radius, 0, MathF.Sin(a2) * radius);
            Vector3 i0 = new Vector3(MathF.Cos(a1) * mouthR, 0, MathF.Sin(a1) * mouthR);
            Vector3 i1 = new Vector3(MathF.Cos(a2) * mouthR, 0, MathF.Sin(a2) * mouthR);

            var lipVerts = new List<VertexPositionNormalTexture>();
            var lipIndices = new List<ushort>();
            MeshGeometryHelper.AddQuad(lipVerts, lipIndices, o0, i0, i1, o1, Vector3.Down);

            Vector3 s0 = _viewport.Project(lipVerts[lipIndices[0]].Position, _projection, view, Matrix.Identity);
            Vector3 s1 = _viewport.Project(lipVerts[lipIndices[1]].Position, _projection, view, Matrix.Identity);
            Vector3 s2 = _viewport.Project(lipVerts[lipIndices[2]].Position, _projection, view, Matrix.Identity);
            float crossLip = (s1.X - s0.X) * (s2.Y - s0.Y) - (s1.Y - s0.Y) * (s2.X - s0.X);
            Assert.True(crossLip > 0f, $"Socket mouth lip tri must be clockwise on screen, was {crossLip}");

            // 2. Cavity Floor Triangle at recessDepth
            var floorIndices = new List<ushort>();
            MeshGeometryHelper.AddBottomCapTri(floorIndices, 0, 0);
            var floorPts = new[]
            {
                new Vector3(0, recessDepth, 0),
                new Vector3(MathF.Cos(0) * mouthR, recessDepth, MathF.Sin(0) * mouthR),
                new Vector3(MathF.Cos(MathF.PI / 3f) * mouthR, recessDepth, MathF.Sin(MathF.PI / 3f) * mouthR)
            };

            Vector3 sf0 = _viewport.Project(floorPts[floorIndices[0]], _projection, view, Matrix.Identity);
            Vector3 sf1 = _viewport.Project(floorPts[floorIndices[1]], _projection, view, Matrix.Identity);
            Vector3 sf2 = _viewport.Project(floorPts[floorIndices[2]], _projection, view, Matrix.Identity);
            float crossFloor = (sf1.X - sf0.X) * (sf2.Y - sf0.Y) - (sf1.Y - sf0.Y) * (sf2.X - sf0.X);
            Assert.True(crossFloor > 0f, $"Socket cavity floor tri must be clockwise on screen, was {crossFloor}");
        }
    }
}
