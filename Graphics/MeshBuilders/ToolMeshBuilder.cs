using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class ToolMeshBuilder
    {
        public static SimpleMesh CreateImpactSocket(GraphicsDevice device, float radius = 0.024f, float length = 0.09f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int segs = 16;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                float x = MathF.Cos(a) * radius;
                float z = MathF.Sin(a) * radius;
                Vector3 n = new Vector3(MathF.Cos(a), 0, MathF.Sin(a));
                float u = (float)i / segs;

                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, 0, z), n, new Vector2(u, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, length, z), n, new Vector2(u, 0)));
            }
            for (int i = 0; i < segs; i++)
            {
                ushort b = (ushort)(i * 2);
                MeshGeometryHelper.AddCylinderWall(indices, b);
            }

            // Top drive cap (y = length)
            ushort topCenter = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(new Vector3(0, length, 0), Vector3.Up, new Vector2(0.5f, 0.5f)));
            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(MathF.Cos(a) * radius, length, MathF.Sin(a) * radius), Vector3.Up, Vector2.Zero));
            }
            for (int i = 0; i < segs; i++)
            {
                MeshGeometryHelper.AddTopCapTri(indices, topCenter, (ushort)i);
            }

            // Bottom drive rim (y = 0)
            ushort botCenter = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(Vector3.Zero, Vector3.Down, new Vector2(0.5f, 0.5f)));
            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(MathF.Cos(a) * radius, 0, MathF.Sin(a) * radius), Vector3.Down, Vector2.Zero));
            }
            for (int i = 0; i < segs; i++)
            {
                MeshGeometryHelper.AddBottomCapTri(indices, botCenter, (ushort)i);
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateGrinderDisc(GraphicsDevice device, float outerRadius = 0.085f, float innerRadius = 0.015f, float thickness = 0.008f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int segs = 20;

            for (int i = 0; i < segs; i++)
            {
                float a1 = (float)i / segs * MathF.PI * 2f;
                float a2 = (float)(i + 1) / segs * MathF.PI * 2f;
                float ca1 = MathF.Cos(a1), sa1 = MathF.Sin(a1);
                float ca2 = MathF.Cos(a2), sa2 = MathF.Sin(a2);

                Vector3 o0 = new Vector3(ca1 * outerRadius, 0, sa1 * outerRadius);
                Vector3 o1 = new Vector3(ca2 * outerRadius, 0, sa2 * outerRadius);
                Vector3 o2 = new Vector3(ca2 * outerRadius, thickness, sa2 * outerRadius);
                Vector3 o3 = new Vector3(ca1 * outerRadius, thickness, sa1 * outerRadius);

                Vector3 i0 = new Vector3(ca1 * innerRadius, 0, sa1 * innerRadius);
                Vector3 i1 = new Vector3(ca2 * innerRadius, 0, sa2 * innerRadius);
                Vector3 i2 = new Vector3(ca2 * innerRadius, thickness, sa2 * innerRadius);
                Vector3 i3 = new Vector3(ca1 * innerRadius, thickness, sa1 * innerRadius);

                Vector3 nOuter = Vector3.Normalize(new Vector3((ca1 + ca2) * 0.5f, 0, (sa1 + sa2) * 0.5f));

                // Outer cutting edge perimeter
                MeshGeometryHelper.AddQuad(vertices, indices, o0, o1, o2, o3, nOuter);
                // Inner arbor hole bore
                MeshGeometryHelper.AddQuad(vertices, indices, i1, i0, i3, i2, -nOuter);
                // Top abrasive face (+Y)
                MeshGeometryHelper.AddQuad(vertices, indices, i3, o3, o2, i2, Vector3.Up);
                // Bottom abrasive face (-Y)
                MeshGeometryHelper.AddQuad(vertices, indices, o0, i0, i1, o1, Vector3.Down);
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreatePryBarShaft(GraphicsDevice device, float length = 0.38f, float radius = 0.016f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int segs = 6;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                float x = MathF.Cos(a) * radius;
                float z = MathF.Sin(a) * radius;
                Vector3 n = new Vector3(MathF.Cos(a), 0, MathF.Sin(a));

                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, 0, z), n, new Vector2((float)i / segs, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, length, z), n, new Vector2((float)i / segs, 0)));
            }
            for (int i = 0; i < segs; i++)
            {
                ushort b = (ushort)(i * 2);
                MeshGeometryHelper.AddCylinderWall(indices, b);
            }

            // Bottom heel cap
            ushort botCenter = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(Vector3.Zero, Vector3.Down, new Vector2(0.5f, 0.5f)));
            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(MathF.Cos(a) * radius, 0, MathF.Sin(a) * radius), Vector3.Down, Vector2.Zero));
            }
            for (int i = 0; i < segs; i++)
            {
                MeshGeometryHelper.AddBottomCapTri(indices, botCenter, (ushort)i);
            }

            // Top chisel cap
            ushort topCenter = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(new Vector3(0, length, 0), Vector3.Up, new Vector2(0.5f, 0.5f)));
            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(MathF.Cos(a) * radius, length, MathF.Sin(a) * radius), Vector3.Up, Vector2.Zero));
            }
            for (int i = 0; i < segs; i++)
            {
                MeshGeometryHelper.AddTopCapTri(indices, topCenter, (ushort)i);
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateWelderShroud(GraphicsDevice device, float innerR = 0.014f, float outerR = 0.024f, float length = 0.07f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int segs = 14;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                float ca = MathF.Cos(a), sa = MathF.Sin(a);
                Vector3 n = new Vector3(ca, 0.2f, sa);

                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * outerR, 0, sa * outerR), n, new Vector2((float)i / segs, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * innerR, length, sa * innerR), n, new Vector2((float)i / segs, 0)));
            }
            for (int i = 0; i < segs; i++)
            {
                ushort b = (ushort)(i * 2);
                MeshGeometryHelper.AddDoubleSidedCylinderWall(indices, b);
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateSprayCup(GraphicsDevice device, float baseR = 0.038f, float topR = 0.048f, float height = 0.12f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int segs = 16;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                float ca = MathF.Cos(a), sa = MathF.Sin(a);
                Vector3 n = new Vector3(ca, 0, sa);

                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * baseR, 0, sa * baseR), n, new Vector2((float)i / segs, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * topR, height, sa * topR), n, new Vector2((float)i / segs, 0)));
            }
            for (int i = 0; i < segs; i++)
            {
                ushort b = (ushort)(i * 2);
                MeshGeometryHelper.AddDoubleSidedCylinderWall(indices, b);
            }

            // Bottom cup base (y = 0)
            ushort botCenter = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(Vector3.Zero, Vector3.Down, new Vector2(0.5f, 0.5f)));
            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(MathF.Cos(a) * baseR, 0, MathF.Sin(a) * baseR), Vector3.Down, Vector2.Zero));
            }
            for (int i = 0; i < segs; i++)
            {
                MeshGeometryHelper.AddBottomCapTri(indices, botCenter, (ushort)i);
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
