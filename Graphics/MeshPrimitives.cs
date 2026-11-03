using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics
{
    public class SimpleMesh : IDisposable
    {
        public VertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }
        public int PrimitiveCount { get; set; }
        public int VertexCount { get; set; }

        public void Draw(GraphicsDevice device, BasicEffect effect)
        {
            if (PrimitiveCount <= 0 || VertexBuffer == null || IndexBuffer == null) return;
            device.SetVertexBuffer(VertexBuffer);
            device.Indices = IndexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, PrimitiveCount);
            }
        }

        public void Dispose()
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }
    }

    public static class MeshPrimitives
    {
        public static SimpleMesh CreateBox(GraphicsDevice device, Vector3 size)
        {
            float w = size.X * 0.5f;
            float h = size.Y * 0.5f;
            float d = size.Z * 0.5f;

            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            void AddFace(Vector3 normal, Vector3 up, Vector3 right, float wDist, float hDist, float dDist)
            {
                ushort startIndex = (ushort)vertices.Count;
                Vector3 c = normal * dDist;
                Vector3 p0 = c - right * wDist - up * hDist;
                Vector3 p1 = c + right * wDist - up * hDist;
                Vector3 p2 = c + right * wDist + up * hDist;
                Vector3 p3 = c - right * wDist + up * hDist;

                vertices.Add(new VertexPositionNormalTexture(p0, normal, new Vector2(0, 1)));
                vertices.Add(new VertexPositionNormalTexture(p1, normal, new Vector2(1, 1)));
                vertices.Add(new VertexPositionNormalTexture(p2, normal, new Vector2(1, 0)));
                vertices.Add(new VertexPositionNormalTexture(p3, normal, new Vector2(0, 0)));

                indices.Add(startIndex);
                indices.Add((ushort)(startIndex + 1));
                indices.Add((ushort)(startIndex + 2));

                indices.Add(startIndex);
                indices.Add((ushort)(startIndex + 2));
                indices.Add((ushort)(startIndex + 3));
            }

            // 6 faces
            AddFace(Vector3.Forward, Vector3.Up, Vector3.Right, w, h, d);
            AddFace(Vector3.Backward, Vector3.Up, Vector3.Left, w, h, d);
            AddFace(Vector3.Right, Vector3.Up, Vector3.Backward, d, h, w);
            AddFace(Vector3.Left, Vector3.Up, Vector3.Forward, d, h, w);
            AddFace(Vector3.Up, Vector3.Backward, Vector3.Right, w, d, h);
            AddFace(Vector3.Down, Vector3.Forward, Vector3.Right, w, d, h);

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

        public static SimpleMesh CreateCylinder(GraphicsDevice device, float radius, float height, int segments)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            float halfH = height * 0.5f;

            // Side wall
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * MathF.PI * 2f;
                float x = MathF.Cos(angle) * radius;
                float z = MathF.Sin(angle) * radius;
                Vector3 normal = Vector3.Normalize(new Vector3(x, 0, z));
                float u = (float)i / segments;

                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, -halfH, z), normal, new Vector2(u, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, halfH, z), normal, new Vector2(u, 0)));
            }

            for (int i = 0; i < segments; i++)
            {
                ushort b = (ushort)(i * 2);
                indices.Add(b);
                indices.Add((ushort)(b + 1));
                indices.Add((ushort)(b + 3));

                indices.Add(b);
                indices.Add((ushort)(b + 3));
                indices.Add((ushort)(b + 2));
            }

            // Caps
            ushort topCenterIdx = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(new Vector3(0, halfH, 0), Vector3.Up, new Vector2(0.5f, 0.5f)));

            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * MathF.PI * 2f;
                float x = MathF.Cos(angle) * radius;
                float z = MathF.Sin(angle) * radius;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, halfH, z), Vector3.Up, new Vector2(x / radius * 0.5f + 0.5f, z / radius * 0.5f + 0.5f)));
            }

            for (int i = 0; i < segments; i++)
            {
                indices.Add(topCenterIdx);
                indices.Add((ushort)(topCenterIdx + 1 + i));
                indices.Add((ushort)(topCenterIdx + 2 + i));
            }

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

        public static SimpleMesh CreateHexBolt(GraphicsDevice device, float headRadius = 0.08f, float headHeight = 0.04f, float shankRadius = 0.038f, float shankLength = 0.16f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            // 6-sided Hex Head
            int segments = 6;
            for (int i = 0; i < segments; i++)
            {
                float a1 = (float)i / segments * MathF.PI * 2f;
                float a2 = (float)(i + 1) / segments * MathF.PI * 2f;

                Vector3 p0 = new Vector3(MathF.Cos(a1) * headRadius, 0, MathF.Sin(a1) * headRadius);
                Vector3 p1 = new Vector3(MathF.Cos(a2) * headRadius, 0, MathF.Sin(a2) * headRadius);
                Vector3 p2 = new Vector3(MathF.Cos(a2) * headRadius, headHeight, MathF.Sin(a2) * headRadius);
                Vector3 p3 = new Vector3(MathF.Cos(a1) * headRadius, headHeight, MathF.Sin(a1) * headRadius);

                Vector3 normal = Vector3.Normalize(Vector3.Cross(p1 - p0, p3 - p0));

                ushort start = (ushort)vertices.Count;
                vertices.Add(new VertexPositionNormalTexture(p0, normal, new Vector2(0, 1)));
                vertices.Add(new VertexPositionNormalTexture(p1, normal, new Vector2(1, 1)));
                vertices.Add(new VertexPositionNormalTexture(p2, normal, new Vector2(1, 0)));
                vertices.Add(new VertexPositionNormalTexture(p3, normal, new Vector2(0, 0)));

                indices.Add(start);
                indices.Add((ushort)(start + 1));
                indices.Add((ushort)(start + 2));

                indices.Add(start);
                indices.Add((ushort)(start + 2));
                indices.Add((ushort)(start + 3));
            }

            // Top face of head
            ushort centerIdx = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(new Vector3(0, headHeight, 0), Vector3.Up, new Vector2(0.5f, 0.5f)));
            for (int i = 0; i <= segments; i++)
            {
                float a = (float)i / segments * MathF.PI * 2f;
                float x = MathF.Cos(a) * headRadius;
                float z = MathF.Sin(a) * headRadius;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, headHeight, z), Vector3.Up, new Vector2(x / headRadius * 0.5f + 0.5f, z / headRadius * 0.5f + 0.5f)));
            }
            for (int i = 0; i < segments; i++)
            {
                indices.Add(centerIdx);
                indices.Add((ushort)(centerIdx + 1 + i));
                indices.Add((ushort)(centerIdx + 2 + i));
            }

            // Threaded Shank extending downward below head (y: 0 to -shankLength)
            int shankSegs = 8;
            for (int i = 0; i < shankSegs; i++)
            {
                float a1 = (float)i / shankSegs * MathF.PI * 2f;
                float a2 = (float)(i + 1) / shankSegs * MathF.PI * 2f;

                Vector3 p0 = new Vector3(MathF.Cos(a1) * shankRadius, -shankLength, MathF.Sin(a1) * shankRadius);
                Vector3 p1 = new Vector3(MathF.Cos(a2) * shankRadius, -shankLength, MathF.Sin(a2) * shankRadius);
                Vector3 p2 = new Vector3(MathF.Cos(a2) * shankRadius, 0, MathF.Sin(a2) * shankRadius);
                Vector3 p3 = new Vector3(MathF.Cos(a1) * shankRadius, 0, MathF.Sin(a1) * shankRadius);

                Vector3 normal = Vector3.Normalize(new Vector3(MathF.Cos((a1 + a2) * 0.5f), 0, MathF.Sin((a1 + a2) * 0.5f)));

                ushort start = (ushort)vertices.Count;
                vertices.Add(new VertexPositionNormalTexture(p0, normal, new Vector2(0, 1)));
                vertices.Add(new VertexPositionNormalTexture(p1, normal, new Vector2(1, 1)));
                vertices.Add(new VertexPositionNormalTexture(p2, normal, new Vector2(1, 0)));
                vertices.Add(new VertexPositionNormalTexture(p3, normal, new Vector2(0, 0)));

                indices.Add(start);
                indices.Add((ushort)(start + 1));
                indices.Add((ushort)(start + 2));

                indices.Add(start);
                indices.Add((ushort)(start + 2));
                indices.Add((ushort)(start + 3));
            }

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
