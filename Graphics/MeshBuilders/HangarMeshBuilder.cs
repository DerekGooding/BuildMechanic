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
            float innerR = 0.10f;
            float outerR = 0.15f;
            float hz = 0.024f; // half thickness

            for (int i = 0; i < segs; i++)
            {
                float a1 = (float)i / segs * MathF.PI * 1.35f;
                float a2 = (float)(i + 1) / segs * MathF.PI * 1.35f;

                float ca1 = MathF.Cos(a1), sa1 = MathF.Sin(a1);
                float ca2 = MathF.Cos(a2), sa2 = MathF.Sin(a2);

                // Front cheek vertices (+Z)
                Vector3 if1 = new Vector3(ca1 * innerR, -sa1 * innerR, hz);
                Vector3 of1 = new Vector3(ca1 * outerR, -sa1 * outerR, hz);
                Vector3 if2 = new Vector3(ca2 * innerR, -sa2 * innerR, hz);
                Vector3 of2 = new Vector3(ca2 * outerR, -sa2 * outerR, hz);

                // Back cheek vertices (-Z)
                Vector3 ib1 = new Vector3(ca1 * innerR, -sa1 * innerR, -hz);
                Vector3 ob1 = new Vector3(ca1 * outerR, -sa1 * outerR, -hz);
                Vector3 ib2 = new Vector3(ca2 * innerR, -sa2 * innerR, -hz);
                Vector3 ob2 = new Vector3(ca2 * outerR, -sa2 * outerR, -hz);

                // Front cheek (+Z)
                MeshGeometryHelper.AddQuad(vertices, indices, if1, of1, of2, if2, Vector3.Backward);
                // Back cheek (-Z)
                MeshGeometryHelper.AddQuad(vertices, indices, ob1, ib1, ib2, ob2, Vector3.Forward);

                // Outer spine face
                Vector3 nOuter = Vector3.Normalize(new Vector3((ca1 + ca2) * 0.5f, -(sa1 + sa2) * 0.5f, 0));
                MeshGeometryHelper.AddQuad(vertices, indices, of1, ob1, ob2, of2, nOuter);

                // Inner throat face
                MeshGeometryHelper.AddQuad(vertices, indices, ib1, if1, if2, ib2, -nOuter);
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
                // Double-sided wall ensures visible both from above and from underneath
                MeshGeometryHelper.AddDoubleSidedCylinderWall(indices, b);
            }

            // Top socket cap (+Y)
            ushort topCenter = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(new Vector3(0, height * 0.5f, 0), Vector3.Up, new Vector2(0.5f, 0.5f)));
            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(MathF.Cos(a) * topR, height * 0.5f, MathF.Sin(a) * topR), Vector3.Up, Vector2.Zero));
            }
            for (int i = 0; i < segs; i++)
            {
                MeshGeometryHelper.AddTopCapTri(indices, topCenter, (ushort)i);
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
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);
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
