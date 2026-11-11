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
        [Fact]
        public void AddQuad_WithClockwiseInput_GeneratesOutwardFacingTriangles()
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            Vector3 normal = Vector3.Backward; // +Z
            // Clockwise around quad: top-left, top-right, bottom-right, bottom-left
            Vector3 p0 = new Vector3(-1, 1, 0);
            Vector3 p1 = new Vector3(1, 1, 0);
            Vector3 p2 = new Vector3(1, -1, 0);
            Vector3 p3 = new Vector3(-1, -1, 0);

            MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);

            Assert.Equal(4, vertices.Count);
            Assert.Equal(6, indices.Count);

            // Triangle 1
            Vector3 a0 = vertices[indices[0]].Position;
            Vector3 a1 = vertices[indices[1]].Position;
            Vector3 a2 = vertices[indices[2]].Position;
            Vector3 crossA = Vector3.Cross(a1 - a0, a2 - a0);
            Assert.True(Vector3.Dot(crossA, normal) > 0f, "Triangle 1 must point in outward normal direction");

            // Triangle 2
            Vector3 b0 = vertices[indices[3]].Position;
            Vector3 b1 = vertices[indices[4]].Position;
            Vector3 b2 = vertices[indices[5]].Position;
            Vector3 crossB = Vector3.Cross(b1 - b0, b2 - b0);
            Assert.True(Vector3.Dot(crossB, normal) > 0f, "Triangle 2 must point in outward normal direction");
        }

        [Fact]
        public void AddQuad_WithCounterClockwiseInput_AutomaticallyInvertsWindingToOutward()
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            Vector3 normal = Vector3.Backward; // +Z
            // Counter-clockwise around quad: bottom-left, bottom-right, top-right, top-left
            Vector3 p0 = new Vector3(-1, -1, 0);
            Vector3 p1 = new Vector3(1, -1, 0);
            Vector3 p2 = new Vector3(1, 1, 0);
            Vector3 p3 = new Vector3(-1, 1, 0);

            MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);

            Assert.Equal(4, vertices.Count);
            Assert.Equal(6, indices.Count);

            Vector3 a0 = vertices[indices[0]].Position;
            Vector3 a1 = vertices[indices[1]].Position;
            Vector3 a2 = vertices[indices[2]].Position;
            Vector3 crossA = Vector3.Cross(a1 - a0, a2 - a0);
            Assert.True(Vector3.Dot(crossA, normal) > 0f, "Auto-aligned Triangle 1 must point in outward normal direction");

            Vector3 b0 = vertices[indices[3]].Position;
            Vector3 b1 = vertices[indices[4]].Position;
            Vector3 b2 = vertices[indices[5]].Position;
            Vector3 crossB = Vector3.Cross(b1 - b0, b2 - b0);
            Assert.True(Vector3.Dot(crossB, normal) > 0f, "Auto-aligned Triangle 2 must point in outward normal direction");
        }

        [Fact]
        public void CylinderCaps_GenerateOutwardFacingNormals()
        {
            var indicesTop = new List<ushort>();
            MeshGeometryHelper.AddTopCapTri(indicesTop, 0, 0);
            // Center is (0, 1, 0), v1 is (1, 1, 0), v2 is (1, 1, 0.1)
            Vector3 c = new Vector3(0, 1, 0);
            Vector3 v1 = new Vector3(1, 1, 0);
            Vector3 v2 = new Vector3(1, 1, 0.1f);
            var vertMap = new Dictionary<ushort, Vector3> { { 0, c }, { 1, v1 }, { 2, v2 } };

            Vector3 topA = vertMap[indicesTop[0]];
            Vector3 topB = vertMap[indicesTop[1]];
            Vector3 topC = vertMap[indicesTop[2]];
            Vector3 topCross = Vector3.Cross(topB - topA, topC - topA);
            Assert.True(Vector3.Dot(topCross, Vector3.Up) > 0f, "Top cap triangle must point UP (+Y)");

            var indicesBot = new List<ushort>();
            MeshGeometryHelper.AddBottomCapTri(indicesBot, 0, 0);
            Vector3 bc = new Vector3(0, -1, 0);
            Vector3 bv1 = new Vector3(1, -1, 0);
            Vector3 bv2 = new Vector3(1, -1, 0.1f);
            var botMap = new Dictionary<ushort, Vector3> { { 0, bc }, { 1, bv1 }, { 2, bv2 } };

            Vector3 botA = botMap[indicesBot[0]];
            Vector3 botB = botMap[indicesBot[1]];
            Vector3 botC = botMap[indicesBot[2]];
            Vector3 botCross = Vector3.Cross(botB - botA, botC - botA);
            Assert.True(Vector3.Dot(botCross, Vector3.Down) > 0f, "Bottom cap triangle must point DOWN (-Y)");
        }
    }
}
