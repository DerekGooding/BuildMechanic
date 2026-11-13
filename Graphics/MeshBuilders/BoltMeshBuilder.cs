using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public class DetailedBoltMesh : IDisposable
    {
        public SimpleMesh HeadMesh { get; set; }
        public SimpleMesh ShankMesh { get; set; }

        public void Dispose()
        {
            HeadMesh?.Dispose();
            ShankMesh?.Dispose();
        }
    }

    public static class BoltMeshBuilder
    {
        public static DetailedBoltMesh CreateDetailedBolt(
            GraphicsDevice device,
            float headRadius = 0.045f,
            float headHeight = 0.022f,
            float flangeRadius = 0.054f,
            float shankRadius = 0.024f,
            float shankLength = 0.08f)
        {
            return new DetailedBoltMesh
            {
                HeadMesh = CreateHeadMesh(device, headRadius, headHeight, flangeRadius, shankRadius),
                ShankMesh = CreateThreadedShankMesh(device, shankRadius, shankLength)
            };
        }

        private static SimpleMesh CreateHeadMesh(
            GraphicsDevice device,
            float headRadius,
            float headHeight,
            float flangeRadius,
            float shankRadius)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            float flangeH = 0.005f;
            int flangeSegs = 24;

            // 1. Integrated Washer Flange Outer Rim (y: 0 to flangeH)
            for (int i = 0; i <= flangeSegs; i++)
            {
                float a = (float)i / flangeSegs * MathF.PI * 2f;
                float x = MathF.Cos(a) * flangeRadius;
                float z = MathF.Sin(a) * flangeRadius;
                Vector3 n = new Vector3(MathF.Cos(a), 0, MathF.Sin(a));
                float u = (float)i / flangeSegs;

                vertices.Add(new VertexPositionNormalTexture(
                    new Vector3(x, 0, z), n, new Vector2(0.5f + u * 0.48f, 0.98f)));
                vertices.Add(new VertexPositionNormalTexture(
                    new Vector3(x, flangeH, z), n, new Vector2(0.5f + u * 0.48f, 0.88f)));
            }
            for (int i = 0; i < flangeSegs; i++)
            {
                ushort b = (ushort)(i * 2);
                MeshGeometryHelper.AddCylinderWall(indices, b);
            }

            // 2. Bottom Washer Seating Deck (y = 0, facing -Y)
            for (int i = 0; i < flangeSegs; i++)
            {
                float a1 = (float)i / flangeSegs * MathF.PI * 2f;
                float a2 = (float)(i + 1) / flangeSegs * MathF.PI * 2f;
                float ca1 = MathF.Cos(a1), sa1 = MathF.Sin(a1);
                float ca2 = MathF.Cos(a2), sa2 = MathF.Sin(a2);

                Vector3 o0 = new Vector3(ca1 * flangeRadius, 0, sa1 * flangeRadius);
                Vector3 o1 = new Vector3(ca2 * flangeRadius, 0, sa2 * flangeRadius);
                Vector3 i0 = new Vector3(ca1 * shankRadius, 0, sa1 * shankRadius);
                Vector3 i1 = new Vector3(ca2 * shankRadius, 0, sa2 * shankRadius);

                float u0 = (float)i / flangeSegs;
                float u1 = (float)(i + 1) / flangeSegs;
                Vector2 uv0 = new Vector2(0.5f + u0 * 0.48f, 0.12f);
                Vector2 uv1 = new Vector2(0.5f + u0 * 0.48f, 0.02f);
                Vector2 uv2 = new Vector2(0.5f + u1 * 0.48f, 0.02f);
                Vector2 uv3 = new Vector2(0.5f + u1 * 0.48f, 0.12f);

                MeshGeometryHelper.AddQuad(vertices, indices, o0, i0, i1, o1, Vector3.Down, uv0, uv1, uv2, uv3);
            }

            // 3. Upper Flange Deck (y = flangeH, facing +Y)
            // Bridges outer flange rim into hex head base, eliminating see-through gaps
            float deckInnerR = headRadius * 0.80f;
            for (int i = 0; i < flangeSegs; i++)
            {
                float a1 = (float)i / flangeSegs * MathF.PI * 2f;
                float a2 = (float)(i + 1) / flangeSegs * MathF.PI * 2f;
                float ca1 = MathF.Cos(a1), sa1 = MathF.Sin(a1);
                float ca2 = MathF.Cos(a2), sa2 = MathF.Sin(a2);

                Vector3 o0 = new Vector3(ca1 * flangeRadius, flangeH, sa1 * flangeRadius);
                Vector3 o1 = new Vector3(ca2 * flangeRadius, flangeH, sa2 * flangeRadius);
                Vector3 i0 = new Vector3(ca1 * deckInnerR, flangeH, sa1 * deckInnerR);
                Vector3 i1 = new Vector3(ca2 * deckInnerR, flangeH, sa2 * deckInnerR);

                Vector2 uvO0 = GetTopPlanarUV(o0.X, o0.Z, flangeRadius);
                Vector2 uvO1 = GetTopPlanarUV(o1.X, o1.Z, flangeRadius);
                Vector2 uvI1 = GetTopPlanarUV(i1.X, i1.Z, flangeRadius);
                Vector2 uvI0 = GetTopPlanarUV(i0.X, i0.Z, flangeRadius);

                MeshGeometryHelper.AddQuad(vertices, indices, o0, o1, i1, i0, Vector3.Up, uvO0, uvO1, uvI1, uvI0);
            }

            // 4. 6-sided Hex Head Sides (y: flangeH to headHeight)
            int hexSegs = 6;
            for (int i = 0; i < hexSegs; i++)
            {
                float a1 = (float)i / hexSegs * MathF.PI * 2f;
                float a2 = (float)(i + 1) / hexSegs * MathF.PI * 2f;

                Vector3 p0 = new Vector3(MathF.Cos(a1) * headRadius, flangeH, MathF.Sin(a1) * headRadius);
                Vector3 p1 = new Vector3(MathF.Cos(a2) * headRadius, flangeH, MathF.Sin(a2) * headRadius);
                Vector3 p2 = new Vector3(MathF.Cos(a2) * headRadius, headHeight, MathF.Sin(a2) * headRadius);
                Vector3 p3 = new Vector3(MathF.Cos(a1) * headRadius, headHeight, MathF.Sin(a1) * headRadius);

                Vector3 n = Vector3.Normalize(Vector3.Cross(p3 - p0, p1 - p0));

                float uStart = 0.52f + i * (0.45f / hexSegs);
                float uEnd = 0.52f + (i + 1) * (0.45f / hexSegs);

                Vector2 uv0 = new Vector2(uStart, 0.85f);
                Vector2 uv1 = new Vector2(uEnd, 0.85f);
                Vector2 uv2 = new Vector2(uEnd, 0.15f);
                Vector2 uv3 = new Vector2(uStart, 0.15f);

                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, n, uv0, uv1, uv2, uv3);
            }

            // 5. Solid Flat Hex Crown (+Y) with stamped markings & socket
            ushort topCenterIdx = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(
                new Vector3(0, headHeight, 0), Vector3.Up, new Vector2(0.25f, 0.5f)));

            for (int i = 0; i <= hexSegs; i++)
            {
                float a = (float)i / hexSegs * MathF.PI * 2f;
                float x = MathF.Cos(a) * headRadius;
                float z = MathF.Sin(a) * headRadius;
                Vector2 uv = GetTopPlanarUV(x, z, flangeRadius);
                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, headHeight, z), Vector3.Up, uv));
            }

            for (int i = 0; i < hexSegs; i++)
            {
                MeshGeometryHelper.AddTopCapTri(indices, topCenterIdx, (ushort)i);
            }

            return BuildMesh(device, vertices, indices);
        }

        private static Vector2 GetTopPlanarUV(float x, float z, float flangeRadius)
        {
            return new Vector2(
                (x / (2f * flangeRadius) + 0.5f) * 0.5f,
                z / (2f * flangeRadius) + 0.5f);
        }

        private static SimpleMesh CreateThreadedShankMesh(GraphicsDevice device, float radius, float length)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            int radialSegments = 16;
            int lengthSegments = 8;
            float threadPitchRepeats = 4.0f; // repeats texture vertically

            for (int j = 0; j <= lengthSegments; j++)
            {
                float vNorm = (float)j / lengthSegments;
                float y = -vNorm * length;

                // Taper lead-in chamfer at tip (bottom 15% of shank)
                float currentRadius = (vNorm > 0.85f) ?
                    MathHelper.Lerp(radius, radius * 0.75f, (vNorm - 0.85f) / 0.15f) : radius;

                for (int i = 0; i <= radialSegments; i++)
                {
                    float uNorm = (float)i / radialSegments;
                    float angle = uNorm * MathF.PI * 2f;

                    float x = MathF.Cos(angle) * currentRadius;
                    float z = MathF.Sin(angle) * currentRadius;
                    Vector3 normal = Vector3.Normalize(new Vector3(MathF.Cos(angle), 0, MathF.Sin(angle)));

                    // Continuous cylindrical helical UV mapping
                    Vector2 uv = new Vector2(uNorm, vNorm * threadPitchRepeats);
                    vertices.Add(new VertexPositionNormalTexture(new Vector3(x, y, z), normal, uv));
                }
            }

            for (int j = 0; j < lengthSegments; j++)
            {
                int row1 = j * (radialSegments + 1);
                int row2 = (j + 1) * (radialSegments + 1);

                for (int i = 0; i < radialSegments; i++)
                {
                    ushort b0 = (ushort)(row1 + i);
                    ushort b1 = (ushort)(row1 + i + 1);
                    ushort b2 = (ushort)(row2 + i);
                    ushort b3 = (ushort)(row2 + i + 1);

                    indices.Add(b0); indices.Add(b2); indices.Add(b1);
                    indices.Add(b1); indices.Add(b2); indices.Add(b3);
                }
            }

            // Bottom tip end cap (-Y)
            ushort tipCenter = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(new Vector3(0, -length, 0), Vector3.Down, new Vector2(0.5f, 0.5f)));
            int tipRow = lengthSegments * (radialSegments + 1);
            for (int i = 0; i < radialSegments; i++)
            {
                indices.Add(tipCenter);
                indices.Add((ushort)(tipRow + i + 1));
                indices.Add((ushort)(tipRow + i));
            }

            return BuildMesh(device, vertices, indices);
        }

        private static SimpleMesh BuildMesh(GraphicsDevice device, List<VertexPositionNormalTexture> vertices, List<ushort> indices)
        {
            var vb = new VertexBuffer(device, typeof(VertexPositionNormalTexture), vertices.Count, BufferUsage.WriteOnly);
            vb.SetData(vertices.ToArray());

            var ib = new IndexBuffer(device, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
            ib.SetData(indices.ToArray());

            return new SimpleMesh
            {
                VertexBuffer = vb,
                IndexBuffer = ib,
                VertexCount = vertices.Count,
                PrimitiveCount = indices.Count / 3
            };
        }
    }
}
