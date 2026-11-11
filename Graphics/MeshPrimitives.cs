using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics.MeshBuilders;

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
                Vector3 c = normal * dDist;
                Vector3 p0 = c - right * wDist - up * hDist;
                Vector3 p1 = c + right * wDist - up * hDist;
                Vector3 p2 = c + right * wDist + up * hDist;
                Vector3 p3 = c - right * wDist + up * hDist;

                MeshGeometryHelper.AddQuad(
                    vertices, indices, p0, p1, p2, p3, normal,
                    new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0));
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
                MeshGeometryHelper.AddCylinderWall(indices, b);
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
                MeshGeometryHelper.AddTopCapTri(indices, topCenterIdx, (ushort)i);
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
                MeshGeometryHelper.AddBottomCapTri(indices, botCenterIdx, (ushort)i);
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
