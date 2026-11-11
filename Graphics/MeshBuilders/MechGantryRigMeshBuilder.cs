using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class MechGantryRigMeshBuilder
    {
        public static SimpleMesh CreateSpreaderBeam(GraphicsDevice device, float width = 3.6f, float height = 0.28f, float depth = 0.35f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            float hw = width * 0.5f, hh = height * 0.5f, hd = depth * 0.5f;

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

        public static SimpleMesh CreateSuspensionCable(GraphicsDevice device, float length = 2.4f, float radius = 0.022f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int segs = 10;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                float ca = MathF.Cos(a), sa = MathF.Sin(a);
                Vector3 n = new Vector3(ca, 0, sa);

                // Bottom (-length) followed by top (0) so AddCylinderWall matches standard winding
                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * radius, -length, sa * radius), n, new Vector2((float)i / segs, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * radius, 0, sa * radius), n, new Vector2((float)i / segs, 0)));
            }

            for (int i = 0; i < segs; i++)
            {
                ushort b = (ushort)(i * 2);
                MeshGeometryHelper.AddCylinderWall(indices, b);
            }

            // Top clevis cap (y = 0)
            ushort topCenter = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(Vector3.Zero, Vector3.Up, new Vector2(0.5f, 0.5f)));
            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(MathF.Cos(a) * radius, 0, MathF.Sin(a) * radius), Vector3.Up, Vector2.Zero));
            }
            for (int i = 0; i < segs; i++)
            {
                MeshGeometryHelper.AddTopCapTri(indices, topCenter, (ushort)i);
            }

            // Bottom clevis cap (y = -length)
            ushort botCenter = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(new Vector3(0, -length, 0), Vector3.Down, new Vector2(0.5f, 0.5f)));
            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(MathF.Cos(a) * radius, -length, MathF.Sin(a) * radius), Vector3.Down, Vector2.Zero));
            }
            for (int i = 0; i < segs; i++)
            {
                MeshGeometryHelper.AddBottomCapTri(indices, botCenter, (ushort)i);
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateUmbilicalBoom(GraphicsDevice device, float length = 1.8f, float radius = 0.045f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int segs = 12;

            Vector3 cLeft = Vector3.Zero;
            Vector3 cRight = new Vector3(length, 0, 0);

            for (int i = 0; i < segs; i++)
            {
                float a1 = (float)i / segs * MathF.PI * 2f;
                float a2 = (float)(i + 1) / segs * MathF.PI * 2f;
                float ca1 = MathF.Cos(a1), sa1 = MathF.Sin(a1);
                float ca2 = MathF.Cos(a2), sa2 = MathF.Sin(a2);

                Vector3 p0 = new Vector3(0, ca1 * radius, sa1 * radius);
                Vector3 p1 = new Vector3(length, ca1 * radius, sa1 * radius);
                Vector3 p2 = new Vector3(length, ca2 * radius, sa2 * radius);
                Vector3 p3 = new Vector3(0, ca2 * radius, sa2 * radius);

                Vector3 n = Vector3.Normalize(new Vector3(0, (ca1 + ca2) * 0.5f, (sa1 + sa2) * 0.5f));

                // Cylindrical boom tube
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, n);
                // Base mounting flange (x = 0)
                MeshGeometryHelper.AddTri(vertices, indices, cLeft, p0, p3, Vector3.Left, Vector2.Zero, Vector2.Zero, Vector2.Zero);
                // Distal connector head (x = length)
                MeshGeometryHelper.AddTri(vertices, indices, cRight, p2, p1, Vector3.Right, Vector2.Zero, Vector2.Zero, Vector2.Zero);
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
