using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class HangarMeshBuilder
    {
        public static SimpleMesh CreateCraneHook(GraphicsDevice device)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            int segs = 14;
            float hookRadius = 0.14f;
            float thickness = 0.035f;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 1.4f;
                float x = MathF.Cos(a) * hookRadius;
                float y = -MathF.Sin(a) * hookRadius;

                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, y, -thickness), Vector3.Backward, new Vector2((float)i / segs, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, y, thickness), Vector3.Forward, new Vector2((float)i / segs, 0)));
            }

            for (int i = 0; i < segs; i++)
            {
                ushort b = (ushort)(i * 2);
                indices.Add(b); indices.Add((ushort)(b + 3)); indices.Add((ushort)(b + 1));
                indices.Add(b); indices.Add((ushort)(b + 2)); indices.Add((ushort)(b + 3));
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateLampShade(GraphicsDevice device, float topR = 0.03f, float botR = 0.11f, float height = 0.12f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int segs = 16;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                float ca = MathF.Cos(a), sa = MathF.Sin(a);
                Vector3 n = new Vector3(ca, 0.4f, sa);

                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * botR, -height * 0.5f, sa * botR), n, new Vector2((float)i / segs, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * topR, height * 0.5f, sa * topR), n, new Vector2((float)i / segs, 0)));
            }

            for (int i = 0; i < segs; i++)
            {
                ushort b = (ushort)(i * 2);
                indices.Add(b); indices.Add((ushort)(b + 3)); indices.Add((ushort)(b + 1));
                indices.Add(b); indices.Add((ushort)(b + 2)); indices.Add((ushort)(b + 3));
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateBenchVise(GraphicsDevice device)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            // Heavy duty cast iron workshop bench vise with serrated jaws
            Vector3 size = new Vector3(0.18f, 0.14f, 0.24f);
            float hw = size.X * 0.5f, hh = size.Y * 0.5f, hd = size.Z * 0.5f;

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal)
            {
                ushort s = (ushort)vertices.Count;
                vertices.Add(new VertexPositionNormalTexture(p0, normal, new Vector2(0, 1)));
                vertices.Add(new VertexPositionNormalTexture(p1, normal, new Vector2(1, 1)));
                vertices.Add(new VertexPositionNormalTexture(p2, normal, new Vector2(1, 0)));
                vertices.Add(new VertexPositionNormalTexture(p3, normal, new Vector2(0, 0)));

                indices.Add(s); indices.Add((ushort)(s + 2)); indices.Add((ushort)(s + 1));
                indices.Add(s); indices.Add((ushort)(s + 3)); indices.Add((ushort)(s + 2));
            }

            Vector3 f0 = new Vector3(-hw, -hh, hd);
            Vector3 f1 = new Vector3(hw, -hh, hd);
            Vector3 f2 = new Vector3(hw, hh, hd);
            Vector3 f3 = new Vector3(-hw, hh, hd);

            Vector3 b0 = new Vector3(-hw, -hh, -hd);
            Vector3 b1 = new Vector3(hw, -hh, -hd);
            Vector3 b2 = new Vector3(hw, hh, -hd);
            Vector3 b3 = new Vector3(-hw, hh, -hd);

            AddQuad(f0, f1, f2, f3, Vector3.Backward);
            AddQuad(b1, b0, b3, b2, Vector3.Forward);
            AddQuad(f3, f2, b2, b3, Vector3.Up);
            AddQuad(b0, b1, f1, f0, Vector3.Down);
            AddQuad(b0, f0, f3, b3, Vector3.Left);
            AddQuad(f1, b1, b2, f2, Vector3.Right);

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
