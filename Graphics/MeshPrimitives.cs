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

            // 6 faces with proper outward normals
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
                // Clockwise outward facing triangles
                indices.Add(b);
                indices.Add((ushort)(b + 1));
                indices.Add((ushort)(b + 3));

                indices.Add(b);
                indices.Add((ushort)(b + 3));
                indices.Add((ushort)(b + 2));
            }

            // Top Cap (+Y)
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

            // Bottom Cap (-Y)
            ushort botCenterIdx = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(new Vector3(0, -halfH, 0), Vector3.Down, new Vector2(0.5f, 0.5f)));

            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * MathF.PI * 2f;
                float x = MathF.Cos(angle) * radius;
                float z = MathF.Sin(angle) * radius;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, -halfH, z), Vector3.Down, new Vector2(x / radius * 0.5f + 0.5f, z / radius * 0.5f + 0.5f)));
            }

            for (int i = 0; i < segments; i++)
            {
                indices.Add(botCenterIdx);
                indices.Add((ushort)(botCenterIdx + 2 + i));
                indices.Add((ushort)(botCenterIdx + 1 + i));
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
