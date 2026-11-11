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
                HeadMesh = CreateHeadMesh(device, headRadius, headHeight, flangeRadius),
                ShankMesh = CreateThreadedShankMesh(device, shankRadius, shankLength)
            };
        }

        private static SimpleMesh CreateHeadMesh(GraphicsDevice device, float headRadius, float headHeight, float flangeRadius)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            // 1. Integrated Washer Flange Ring (y: 0 to 0.005)
            float flangeH = 0.005f;
            int flangeSegs = 16;
            for (int i = 0; i <= flangeSegs; i++)
            {
                float a = (float)i / flangeSegs * MathF.PI * 2f;
                float x = MathF.Cos(a) * flangeRadius;
                float z = MathF.Sin(a) * flangeRadius;
                Vector3 n = new Vector3(MathF.Cos(a), 0, MathF.Sin(a));
                float u = (float)i / flangeSegs;

                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, 0, z), n, new Vector2(u, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, flangeH, z), n, new Vector2(u, 0.85f)));
            }
            for (int i = 0; i < flangeSegs; i++)
            {
                ushort b = (ushort)(i * 2);
                indices.Add(b); indices.Add((ushort)(b + 1)); indices.Add((ushort)(b + 3));
                indices.Add(b); indices.Add((ushort)(b + 3)); indices.Add((ushort)(b + 2));
            }

            // 2. 6-sided Hex Head Sides (y: flangeH to headHeight)
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
                ushort start = (ushort)vertices.Count;

                vertices.Add(new VertexPositionNormalTexture(p0, n, new Vector2(0, 0.85f)));
                vertices.Add(new VertexPositionNormalTexture(p1, n, new Vector2(1, 0.85f)));
                vertices.Add(new VertexPositionNormalTexture(p2, n, new Vector2(1, 0.3f)));
                vertices.Add(new VertexPositionNormalTexture(p3, n, new Vector2(0, 0.3f)));

                indices.Add(start); indices.Add((ushort)(start + 2)); indices.Add((ushort)(start + 1));
                indices.Add(start); indices.Add((ushort)(start + 3)); indices.Add((ushort)(start + 2));
            }

            // 3. Top Face (+Y) with central socket bevel
            ushort topCenterIdx = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(new Vector3(0, headHeight - 0.006f, 0), Vector3.Up, new Vector2(0.5f, 0.5f)));

            for (int i = 0; i <= hexSegs; i++)
            {
                float a = (float)i / hexSegs * MathF.PI * 2f;
                float x = MathF.Cos(a) * headRadius;
                float z = MathF.Sin(a) * headRadius;
                Vector2 uv = new Vector2(x / flangeRadius * 0.45f + 0.5f, z / flangeRadius * 0.45f + 0.5f);
                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, headHeight, z), Vector3.Up, uv));
            }

            for (int i = 0; i < hexSegs; i++)
            {
                indices.Add(topCenterIdx);
                indices.Add((ushort)(topCenterIdx + 1 + i));
                indices.Add((ushort)(topCenterIdx + 2 + i));
            }

            return BuildMesh(device, vertices, indices);
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
