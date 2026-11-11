using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class MechChassisMeshBuilder
    {
        public static SimpleMesh CreateCockpitHead(GraphicsDevice device)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            // Sloped armored cockpit pod with angled facet visor
            Vector3 size = new Vector3(0.75f, 0.55f, 0.60f);
            float hw = size.X * 0.5f, hh = size.Y * 0.5f, hd = size.Z * 0.5f;

            // 8 sloped vertices for faceted ballistic cockpit
            Vector3 p0 = new Vector3(-hw * 0.85f, -hh, hd * 0.8f);
            Vector3 p1 = new Vector3(hw * 0.85f, -hh, hd * 0.8f);
            Vector3 p2 = new Vector3(hw * 0.65f, hh * 0.7f, hd * 0.3f);
            Vector3 p3 = new Vector3(-hw * 0.65f, hh * 0.7f, hd * 0.3f);
            Vector3 p4 = new Vector3(-hw, -hh, -hd);
            Vector3 p5 = new Vector3(hw, -hh, -hd);
            Vector3 p6 = new Vector3(hw * 0.8f, hh, -hd * 0.7f);
            Vector3 p7 = new Vector3(-hw * 0.8f, hh, -hd * 0.7f);

            void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                ushort s = (ushort)vertices.Count;
                Vector3 n = Vector3.Normalize(Vector3.Cross(c - a, b - a));
                vertices.Add(new VertexPositionNormalTexture(a, n, new Vector2(0, 1)));
                vertices.Add(new VertexPositionNormalTexture(b, n, new Vector2(1, 1)));
                vertices.Add(new VertexPositionNormalTexture(c, n, new Vector2(1, 0)));
                vertices.Add(new VertexPositionNormalTexture(d, n, new Vector2(0, 0)));

                indices.Add(s); indices.Add((ushort)(s + 1)); indices.Add((ushort)(s + 2));
                indices.Add(s); indices.Add((ushort)(s + 2)); indices.Add((ushort)(s + 3));
            }

            // Front sloped face
            AddQuad(p0, p1, p2, p3);
            // Left flank
            AddQuad(p4, p0, p3, p7);
            // Right flank
            AddQuad(p1, p5, p6, p2);
            // Top roof
            AddQuad(p3, p2, p6, p7);
            // Rear bulkhead
            AddQuad(p5, p4, p7, p6);
            // Bottom
            AddQuad(p4, p5, p1, p0);

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateTurretRing(GraphicsDevice device, float outerRadius = 0.55f, float innerRadius = 0.42f, float height = 0.12f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int segs = 24;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                float ca = MathF.Cos(a);
                float sa = MathF.Sin(a);
                float u = (float)i / segs;

                Vector3 nOut = new Vector3(ca, 0, sa);
                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * outerRadius, 0, sa * outerRadius), nOut, new Vector2(u, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * outerRadius, height, sa * outerRadius), nOut, new Vector2(u, 0)));
            }

            for (int i = 0; i < segs; i++)
            {
                ushort b = (ushort)(i * 2);
                indices.Add(b); indices.Add((ushort)(b + 1)); indices.Add((ushort)(b + 3));
                indices.Add(b); indices.Add((ushort)(b + 3)); indices.Add((ushort)(b + 2));
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateSubArmorBoss(GraphicsDevice device, Vector3 size)
        {
            // Mounting boss with recessed bolt anchor sockets
            return MeshPrimitives.CreateBox(device, size);
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
