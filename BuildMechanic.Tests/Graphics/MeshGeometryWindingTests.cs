using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics.MeshBuilders;
using Xunit;

namespace BuildMechanic.Tests.Graphics
{
    public class MeshGeometryWindingTests
    {
        private readonly Viewport _viewport = new Viewport(0, 0, 1280, 720);
        private readonly Matrix _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), 1280f / 720f, 0.1f, 100f);

        [Fact]
        public void AddQuad_ScreenSpaceWinding_AlwaysClockwiseFacingCamera()
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            Vector3 normal = Vector3.Backward; // +Z
            // Bottom-left, bottom-right, top-right, top-left
            Vector3 p0 = new Vector3(-1, -1, 0);
            Vector3 p1 = new Vector3(1, -1, 0);
            Vector3 p2 = new Vector3(1, 1, 0);
            Vector3 p3 = new Vector3(-1, 1, 0);

            MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);

            Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);

            // Verify both triangles are screen-space CLOCKWISE (cross2D > 0)
            Vector3 s0 = _viewport.Project(vertices[indices[0]].Position, _projection, view, Matrix.Identity);
            Vector3 s1 = _viewport.Project(vertices[indices[1]].Position, _projection, view, Matrix.Identity);
            Vector3 s2 = _viewport.Project(vertices[indices[2]].Position, _projection, view, Matrix.Identity);
            float cross1 = (s1.X - s0.X) * (s2.Y - s0.Y) - (s1.Y - s0.Y) * (s2.X - s0.X);
            Assert.True(cross1 > 0f, $"Quad Triangle 1 must be clockwise on screen (cross > 0), but was {cross1}");

            Vector3 s3 = _viewport.Project(vertices[indices[3]].Position, _projection, view, Matrix.Identity);
            Vector3 s4 = _viewport.Project(vertices[indices[4]].Position, _projection, view, Matrix.Identity);
            Vector3 s5 = _viewport.Project(vertices[indices[5]].Position, _projection, view, Matrix.Identity);
            float cross2 = (s4.X - s3.X) * (s5.Y - s3.Y) - (s4.Y - s3.Y) * (s5.X - s3.X);
            Assert.True(cross2 > 0f, $"Quad Triangle 2 must be clockwise on screen (cross > 0), but was {cross2}");
        }

        [Fact]
        public void CylinderWallsAndCaps_ScreenSpaceWinding_AlwaysClockwiseFacingCamera()
        {
            // Cylinder Wall at angle 0 facing +X
            var indices = new List<ushort>();
            MeshGeometryHelper.AddCylinderWall(indices, 0);

            Matrix view = Matrix.CreateLookAt(new Vector3(5, 0, 0), Vector3.Zero, Vector3.Up);

            float r = 1.0f, h = 2.0f, a0 = -0.1f, a1 = 0.1f;
            Vector3 b0 = new Vector3(MathF.Cos(a0) * r, -h * 0.5f, MathF.Sin(a0) * r);
            Vector3 b1 = new Vector3(MathF.Cos(a0) * r, h * 0.5f, MathF.Sin(a0) * r);
            Vector3 b2 = new Vector3(MathF.Cos(a1) * r, -h * 0.5f, MathF.Sin(a1) * r);
            Vector3 b3 = new Vector3(MathF.Cos(a1) * r, h * 0.5f, MathF.Sin(a1) * r);
            var pts = new[] { b0, b1, b2, b3 };

            Vector3 s0 = _viewport.Project(pts[indices[0]], _projection, view, Matrix.Identity);
            Vector3 s1 = _viewport.Project(pts[indices[1]], _projection, view, Matrix.Identity);
            Vector3 s2 = _viewport.Project(pts[indices[2]], _projection, view, Matrix.Identity);
            float cross1 = (s1.X - s0.X) * (s2.Y - s0.Y) - (s1.Y - s0.Y) * (s2.X - s0.X);
            Assert.True(cross1 > 0f, $"Cylinder Wall Tri 1 must be clockwise on screen, was {cross1}");

            Vector3 s3 = _viewport.Project(pts[indices[3]], _projection, view, Matrix.Identity);
            Vector3 s4 = _viewport.Project(pts[indices[4]], _projection, view, Matrix.Identity);
            Vector3 s5 = _viewport.Project(pts[indices[5]], _projection, view, Matrix.Identity);
            float cross2 = (s4.X - s3.X) * (s5.Y - s3.Y) - (s4.Y - s3.Y) * (s5.X - s3.X);
            Assert.True(cross2 > 0f, $"Cylinder Wall Tri 2 must be clockwise on screen, was {cross2}");

            // Top Cap viewed from above (+Y)
            var topIndices = new List<ushort>();
            MeshGeometryHelper.AddTopCapTri(topIndices, 0, 0);
            Matrix viewTop = Matrix.CreateLookAt(new Vector3(0, 5, 0), Vector3.Zero, Vector3.Backward);
            Vector3 cTop = new Vector3(0, 1, 0);
            Vector3 t1 = new Vector3(MathF.Cos(0) * r, 1, MathF.Sin(0) * r);
            Vector3 t2 = new Vector3(MathF.Cos(0.2f) * r, 1, MathF.Sin(0.2f) * r);
            var topPts = new[] { cTop, t1, t2 };

            Vector3 st0 = _viewport.Project(topPts[topIndices[0]], _projection, viewTop, Matrix.Identity);
            Vector3 st1 = _viewport.Project(topPts[topIndices[1]], _projection, viewTop, Matrix.Identity);
            Vector3 st2 = _viewport.Project(topPts[topIndices[2]], _projection, viewTop, Matrix.Identity);
            float crossTop = (st1.X - st0.X) * (st2.Y - st0.Y) - (st1.Y - st0.Y) * (st2.X - st0.X);
            Assert.True(crossTop > 0f, $"Top Cap Tri must be clockwise on screen, was {crossTop}");

            // Bottom Cap viewed from below (-Y)
            var botIndices = new List<ushort>();
            MeshGeometryHelper.AddBottomCapTri(botIndices, 0, 0);
            Matrix viewBot = Matrix.CreateLookAt(new Vector3(0, -5, 0), Vector3.Zero, Vector3.Forward);
            Vector3 cBot = new Vector3(0, -1, 0);
            Vector3 bt1 = new Vector3(MathF.Cos(0) * r, -1, MathF.Sin(0) * r);
            Vector3 bt2 = new Vector3(MathF.Cos(0.2f) * r, -1, MathF.Sin(0.2f) * r);
            var botPts = new[] { cBot, bt1, bt2 };

            Vector3 sb0 = _viewport.Project(botPts[botIndices[0]], _projection, viewBot, Matrix.Identity);
            Vector3 sb1 = _viewport.Project(botPts[botIndices[1]], _projection, viewBot, Matrix.Identity);
            Vector3 sb2 = _viewport.Project(botPts[botIndices[2]], _projection, viewBot, Matrix.Identity);
            float crossBot = (sb1.X - sb0.X) * (sb2.Y - sb0.Y) - (sb1.Y - sb0.Y) * (sb2.X - sb0.X);
            Assert.True(crossBot > 0f, $"Bottom Cap Tri must be clockwise on screen, was {crossBot}");
        }

        [Fact]
        public void AddTri_ScreenSpaceWinding_AlwaysClockwiseFacingCamera()
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            Vector3 normal = Vector3.Up; // +Y
            Vector3 p0 = new Vector3(-1, 0, -1);
            Vector3 p1 = new Vector3(1, 0, -1);
            Vector3 p2 = new Vector3(0, 0, 1);

            MeshGeometryHelper.AddTri(vertices, indices, p0, p1, p2, normal, Vector2.Zero, Vector2.Zero, Vector2.Zero);

            Matrix view = Matrix.CreateLookAt(new Vector3(0, 5, 0), Vector3.Zero, Vector3.Backward);

            Vector3 s0 = _viewport.Project(vertices[indices[0]].Position, _projection, view, Matrix.Identity);
            Vector3 s1 = _viewport.Project(vertices[indices[1]].Position, _projection, view, Matrix.Identity);
            Vector3 s2 = _viewport.Project(vertices[indices[2]].Position, _projection, view, Matrix.Identity);
            float cross = (s1.X - s0.X) * (s2.Y - s0.Y) - (s1.Y - s0.Y) * (s2.X - s0.X);
            Assert.True(cross > 0f, $"Tri must be clockwise on screen, was {cross}");
        }

        [Fact]
        public void AddQuad_OnAllSixCardinalFaces_AlwaysClockwiseFacingCamera()
        {
            var testCases = new (Vector3 normal, Vector3 camPos, Vector3 up)[]
            {
                (Vector3.Backward, new Vector3(0, 0, 5), Vector3.Up),
                (Vector3.Forward, new Vector3(0, 0, -5), Vector3.Up),
                (Vector3.Right, new Vector3(5, 0, 0), Vector3.Up),
                (Vector3.Left, new Vector3(-5, 0, 0), Vector3.Up),
                (Vector3.Up, new Vector3(0, 5, 0), Vector3.Backward),
                (Vector3.Down, new Vector3(0, -5, 0), Vector3.Forward),
            };

            foreach (var (normal, camPos, up) in testCases)
            {
                var vertices = new List<VertexPositionNormalTexture>();
                var indices = new List<ushort>();

                // Build a quad perpendicular to normal
                Vector3 right = Vector3.Normalize(Vector3.Cross(up, normal));
                Vector3 quadUp = Vector3.Normalize(Vector3.Cross(normal, right));
                Vector3 c = normal * 0.5f;

                Vector3 p0 = c - right - quadUp;
                Vector3 p1 = c + right - quadUp;
                Vector3 p2 = c + right + quadUp;
                Vector3 p3 = c - right + quadUp;

                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);

                Matrix view = Matrix.CreateLookAt(camPos, Vector3.Zero, up);

                Vector3 s0 = _viewport.Project(vertices[indices[0]].Position, _projection, view, Matrix.Identity);
                Vector3 s1 = _viewport.Project(vertices[indices[1]].Position, _projection, view, Matrix.Identity);
                Vector3 s2 = _viewport.Project(vertices[indices[2]].Position, _projection, view, Matrix.Identity);
                float cross1 = (s1.X - s0.X) * (s2.Y - s0.Y) - (s1.Y - s0.Y) * (s2.X - s0.X);
                Assert.True(cross1 > 0f, $"Quad for {normal} Tri 1 must be clockwise on screen, was {cross1}");

                Vector3 s3 = _viewport.Project(vertices[indices[3]].Position, _projection, view, Matrix.Identity);
                Vector3 s4 = _viewport.Project(vertices[indices[4]].Position, _projection, view, Matrix.Identity);
                Vector3 s5 = _viewport.Project(vertices[indices[5]].Position, _projection, view, Matrix.Identity);
                float cross2 = (s4.X - s3.X) * (s5.Y - s3.Y) - (s4.Y - s3.Y) * (s5.X - s3.X);
                Assert.True(cross2 > 0f, $"Quad for {normal} Tri 2 must be clockwise on screen, was {cross2}");
            }
        }

        [Fact]
        public void BoltShankAndWeldBead_ScreenSpaceWinding_AlwaysClockwiseFacingCamera()
        {
            // 1. Bolt Shank Quad at angle 0 facing +X
            float r = 0.024f, yTop = -0.01f, yBot = -0.02f, a0 = -0.1f, a1 = 0.1f;
            Vector3 b0 = new Vector3(MathF.Cos(a0) * r, yTop, MathF.Sin(a0) * r);
            Vector3 b1 = new Vector3(MathF.Cos(a1) * r, yTop, MathF.Sin(a1) * r);
            Vector3 b2 = new Vector3(MathF.Cos(a0) * r, yBot, MathF.Sin(a0) * r);
            Vector3 b3 = new Vector3(MathF.Cos(a1) * r, yBot, MathF.Sin(a1) * r);

            var shankPts = new[] { b0, b1, b2, b3 };
            Matrix viewShank = Matrix.CreateLookAt(new Vector3(1, -0.015f, 0), new Vector3(0, -0.015f, 0), Vector3.Up);

            // Shank indices: (b0, b2, b1) and (b1, b2, b3)
            Vector3 ss0 = _viewport.Project(shankPts[0], _projection, viewShank, Matrix.Identity);
            Vector3 ss1 = _viewport.Project(shankPts[2], _projection, viewShank, Matrix.Identity);
            Vector3 ss2 = _viewport.Project(shankPts[1], _projection, viewShank, Matrix.Identity);
            float crossS1 = (ss1.X - ss0.X) * (ss2.Y - ss0.Y) - (ss1.Y - ss0.Y) * (ss2.X - ss0.X);
            Assert.True(crossS1 > 0f, $"Shank Tri 1 must be clockwise on screen, was {crossS1}");

            Vector3 ss3 = _viewport.Project(shankPts[1], _projection, viewShank, Matrix.Identity);
            Vector3 ss4 = _viewport.Project(shankPts[2], _projection, viewShank, Matrix.Identity);
            Vector3 ss5 = _viewport.Project(shankPts[3], _projection, viewShank, Matrix.Identity);
            float crossS2 = (ss4.X - ss3.X) * (ss5.Y - ss3.Y) - (ss4.Y - ss3.Y) * (ss5.X - ss3.X);
            Assert.True(crossS2 > 0f, $"Shank Tri 2 must be clockwise on screen, was {crossS2}");

            // 2. Weld Bead Left Flank Triangles facing left (-X)
            float z1 = -0.1f, z2 = 0.1f, h = 0.022f, hw = 0.019f;
            Vector3 r1 = new Vector3(0, h, z1);
            Vector3 r2 = new Vector3(0, h, z2);
            Vector3 r1Toe = new Vector3(-hw, 0, z1);
            Vector3 r2Toe = new Vector3(-hw, 0, z2);

            Matrix viewWeld = Matrix.CreateLookAt(new Vector3(-1, 0.01f, 0), new Vector3(0, 0.01f, 0), Vector3.Up);

            // Left flank: (r1, r2, r1Toe) and (r1Toe, r2, r2Toe)
            Vector3 sw0 = _viewport.Project(r1, _projection, viewWeld, Matrix.Identity);
            Vector3 sw1 = _viewport.Project(r2, _projection, viewWeld, Matrix.Identity);
            Vector3 sw2 = _viewport.Project(r1Toe, _projection, viewWeld, Matrix.Identity);
            float crossW1 = (sw1.X - sw0.X) * (sw2.Y - sw0.Y) - (sw1.Y - sw0.Y) * (sw2.X - sw0.X);
            Assert.True(crossW1 > 0f, $"Weld Tri 1 must be clockwise on screen, was {crossW1}");

            Vector3 sw3 = _viewport.Project(r1Toe, _projection, viewWeld, Matrix.Identity);
            Vector3 sw4 = _viewport.Project(r2, _projection, viewWeld, Matrix.Identity);
            Vector3 sw5 = _viewport.Project(r2Toe, _projection, viewWeld, Matrix.Identity);
            float crossW2 = (sw4.X - sw3.X) * (sw5.Y - sw3.Y) - (sw4.Y - sw3.Y) * (sw5.X - sw3.X);
            Assert.True(crossW2 > 0f, $"Weld Tri 2 must be clockwise on screen, was {crossW2}");
        }
    }
}
