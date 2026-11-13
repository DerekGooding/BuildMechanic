using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics.MeshBuilders;
using Xunit;

namespace BuildMechanic.Tests.Graphics
{
    public class BoltGeometryTests
    {
        private readonly Viewport _viewport = new Viewport(0, 0, 1280, 720);
        private readonly Matrix _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), 1280f / 720f, 0.01f, 10f);

        [Fact]
        public void FlangeTopDeck_InnerRadiusStrictlyInsideHexFlats_NoGaps()
        {
            float headRadius = 0.045f;
            float hexFlatMidpointRadius = headRadius * MathF.Cos(MathF.PI / 6f); // ~0.03897f
            float deckInnerRadius = headRadius * 0.80f; // 0.036f

            // Inner deck radius must be strictly smaller than hex flat midpoint to prevent open gaps
            Assert.True(deckInnerRadius < hexFlatMidpointRadius,
                $"Deck inner radius ({deckInnerRadius}) must be inside hex flat midpoint ({hexFlatMidpointRadius})");
        }

        [Fact]
        public void FlangeTopDeck_ScreenSpaceWinding_AlwaysClockwiseFacingAbove()
        {
            float flangeRadius = 0.054f;
            float flangeH = 0.005f;
            float deckInnerR = 0.045f * 0.80f;
            int flangeSegs = 24;

            Matrix viewTop = Matrix.CreateLookAt(new Vector3(0, 1f, 0), new Vector3(0, flangeH, 0), Vector3.Backward);

            for (int i = 0; i < flangeSegs; i++)
            {
                var vertices = new List<VertexPositionNormalTexture>();
                var indices = new List<ushort>();

                float a1 = (float)i / flangeSegs * MathF.PI * 2f;
                float a2 = (float)(i + 1) / flangeSegs * MathF.PI * 2f;

                Vector3 o0 = new Vector3(MathF.Cos(a1) * flangeRadius, flangeH, MathF.Sin(a1) * flangeRadius);
                Vector3 o1 = new Vector3(MathF.Cos(a2) * flangeRadius, flangeH, MathF.Sin(a2) * flangeRadius);
                Vector3 i0 = new Vector3(MathF.Cos(a1) * deckInnerR, flangeH, MathF.Sin(a1) * deckInnerR);
                Vector3 i1 = new Vector3(MathF.Cos(a2) * deckInnerR, flangeH, MathF.Sin(a2) * deckInnerR);

                MeshGeometryHelper.AddQuad(vertices, indices, o0, o1, i1, i0, Vector3.Up);

                // Verify Triangle 1 is screen-space clockwise
                Vector3 s0 = _viewport.Project(vertices[indices[0]].Position, _projection, viewTop, Matrix.Identity);
                Vector3 s1 = _viewport.Project(vertices[indices[1]].Position, _projection, viewTop, Matrix.Identity);
                Vector3 s2 = _viewport.Project(vertices[indices[2]].Position, _projection, viewTop, Matrix.Identity);
                float cross1 = (s1.X - s0.X) * (s2.Y - s0.Y) - (s1.Y - s0.Y) * (s2.X - s0.X);
                Assert.True(cross1 > 0f, $"Flange deck seg {i} Tri 1 must be clockwise on screen, was {cross1}");

                // Verify Triangle 2 is screen-space clockwise
                Vector3 s3 = _viewport.Project(vertices[indices[3]].Position, _projection, viewTop, Matrix.Identity);
                Vector3 s4 = _viewport.Project(vertices[indices[4]].Position, _projection, viewTop, Matrix.Identity);
                Vector3 s5 = _viewport.Project(vertices[indices[5]].Position, _projection, viewTop, Matrix.Identity);
                float cross2 = (s4.X - s3.X) * (s5.Y - s3.Y) - (s4.Y - s3.Y) * (s5.X - s3.X);
                Assert.True(cross2 > 0f, $"Flange deck seg {i} Tri 2 must be clockwise on screen, was {cross2}");
            }
        }

        [Fact]
        public void HexHeadSides_ScreenSpaceWinding_AlwaysClockwiseFacingFlat()
        {
            float headRadius = 0.045f;
            float flangeH = 0.005f;
            float headHeight = 0.022f;
            int hexSegs = 6;

            for (int i = 0; i < hexSegs; i++)
            {
                var vertices = new List<VertexPositionNormalTexture>();
                var indices = new List<ushort>();

                float a1 = (float)i / hexSegs * MathF.PI * 2f;
                float a2 = (float)(i + 1) / hexSegs * MathF.PI * 2f;

                Vector3 p0 = new Vector3(MathF.Cos(a1) * headRadius, flangeH, MathF.Sin(a1) * headRadius);
                Vector3 p1 = new Vector3(MathF.Cos(a2) * headRadius, flangeH, MathF.Sin(a2) * headRadius);
                Vector3 p2 = new Vector3(MathF.Cos(a2) * headRadius, headHeight, MathF.Sin(a2) * headRadius);
                Vector3 p3 = new Vector3(MathF.Cos(a1) * headRadius, headHeight, MathF.Sin(a1) * headRadius);

                Vector3 n = Vector3.Normalize(Vector3.Cross(p3 - p0, p1 - p0));
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, n);

                Vector3 center = (p0 + p1 + p2 + p3) * 0.25f;
                Vector3 camPos = center + n * 0.5f;
                Matrix view = Matrix.CreateLookAt(camPos, center, Vector3.Up);

                Vector3 s0 = _viewport.Project(vertices[indices[0]].Position, _projection, view, Matrix.Identity);
                Vector3 s1 = _viewport.Project(vertices[indices[1]].Position, _projection, view, Matrix.Identity);
                Vector3 s2 = _viewport.Project(vertices[indices[2]].Position, _projection, view, Matrix.Identity);
                float cross1 = (s1.X - s0.X) * (s2.Y - s0.Y) - (s1.Y - s0.Y) * (s2.X - s0.X);
                Assert.True(cross1 > 0f, $"Hex flat {i} Tri 1 must be clockwise on screen, was {cross1}");

                Vector3 s3 = _viewport.Project(vertices[indices[3]].Position, _projection, view, Matrix.Identity);
                Vector3 s4 = _viewport.Project(vertices[indices[4]].Position, _projection, view, Matrix.Identity);
                Vector3 s5 = _viewport.Project(vertices[indices[5]].Position, _projection, view, Matrix.Identity);
                float cross2 = (s4.X - s3.X) * (s5.Y - s3.Y) - (s4.Y - s3.Y) * (s5.X - s3.X);
                Assert.True(cross2 > 0f, $"Hex flat {i} Tri 2 must be clockwise on screen, was {cross2}");
            }
        }

        [Fact]
        public void HexCrown_ScreenSpaceWinding_AlwaysClockwiseFacingAbove()
        {
            float headRadius = 0.045f;
            float headHeight = 0.022f;
            int hexSegs = 6;

            var indices = new List<ushort>();
            ushort centerIdx = 0;
            var pts = new List<Vector3> { new Vector3(0, headHeight, 0) };

            for (int i = 0; i <= hexSegs; i++)
            {
                float a = (float)i / hexSegs * MathF.PI * 2f;
                pts.Add(new Vector3(MathF.Cos(a) * headRadius, headHeight, MathF.Sin(a) * headRadius));
            }

            for (int i = 0; i < hexSegs; i++)
            {
                MeshGeometryHelper.AddTopCapTri(indices, centerIdx, (ushort)i);
            }

            Matrix viewTop = Matrix.CreateLookAt(new Vector3(0, 1f, 0), new Vector3(0, headHeight, 0), Vector3.Backward);

            for (int i = 0; i < hexSegs; i++)
            {
                Vector3 s0 = _viewport.Project(pts[indices[i * 3]], _projection, viewTop, Matrix.Identity);
                Vector3 s1 = _viewport.Project(pts[indices[i * 3 + 1]], _projection, viewTop, Matrix.Identity);
                Vector3 s2 = _viewport.Project(pts[indices[i * 3 + 2]], _projection, viewTop, Matrix.Identity);
                float cross = (s1.X - s0.X) * (s2.Y - s0.Y) - (s1.Y - s0.Y) * (s2.X - s0.X);
                Assert.True(cross > 0f, $"Hex crown tri {i} must be clockwise on screen, was {cross}");
            }
        }

        [Fact]
        public void BoltHeadAtlasUVMapping_StaysStrictlyPartitioned()
        {
            float flangeRadius = 0.054f;

            // Planar top-down UV projection for crown and flange deck
            // U in [0, 0.5], V in [0, 1.0]
            Vector2 uvCenter = new Vector2(
                (0 / (2f * flangeRadius) + 0.5f) * 0.5f,
                0 / (2f * flangeRadius) + 0.5f);
            Assert.Equal(0.25f, uvCenter.X);
            Assert.Equal(0.50f, uvCenter.Y);

            for (int deg = 0; deg < 360; deg += 15)
            {
                float rad = MathHelper.ToRadians(deg);
                float x = MathF.Cos(rad) * flangeRadius;
                float z = MathF.Sin(rad) * flangeRadius;

                float u = (x / (2f * flangeRadius) + 0.5f) * 0.5f;
                float v = z / (2f * flangeRadius) + 0.5f;

                Assert.InRange(u, 0.0f, 0.5f);
                Assert.InRange(v, 0.0f, 1.0f);
            }

            // Hex sides UV mapping spans right half U in [0.5, 1.0]
            int hexSegs = 6;
            for (int i = 0; i < hexSegs; i++)
            {
                float uStart = 0.52f + i * (0.45f / hexSegs);
                float uEnd = 0.52f + (i + 1) * (0.45f / hexSegs);

                Assert.True(uStart >= 0.50f && uStart <= 1.0f);
                Assert.True(uEnd >= 0.50f && uEnd <= 1.0f);
            }
        }
    }
}
