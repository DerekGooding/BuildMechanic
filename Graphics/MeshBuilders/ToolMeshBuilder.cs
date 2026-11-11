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
                indices.Add(b); indices.Add((ushort)(b + 1)); indices.Add((ushort)(b + 3));
                indices.Add(b); indices.Add((ushort)(b + 3)); indices.Add((ushort)(b + 2));
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateGrinderDisc(GraphicsDevice device, float outerRadius = 0.085f, float innerRadius = 0.015f, float thickness = 0.008f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int segs = 20;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                float ca = MathF.Cos(a), sa = MathF.Sin(a);
                float u = (float)i / segs;

                Vector3 n = new Vector3(ca, 0, sa);
                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * outerRadius, 0, sa * outerRadius), n, new Vector2(u, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * outerRadius, thickness, sa * outerRadius), n, new Vector2(u, 0)));
            }
            for (int i = 0; i < segs; i++)
            {
                ushort b = (ushort)(i * 2);
                indices.Add(b); indices.Add((ushort)(b + 1)); indices.Add((ushort)(b + 3));
                indices.Add(b); indices.Add((ushort)(b + 3)); indices.Add((ushort)(b + 2));
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
                indices.Add(b); indices.Add((ushort)(b + 1)); indices.Add((ushort)(b + 3));
                indices.Add(b); indices.Add((ushort)(b + 3)); indices.Add((ushort)(b + 2));
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
                indices.Add(b); indices.Add((ushort)(b + 3)); indices.Add((ushort)(b + 1));
                indices.Add(b); indices.Add((ushort)(b + 2)); indices.Add((ushort)(b + 3));
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
                indices.Add(b); indices.Add((ushort)(b + 3)); indices.Add((ushort)(b + 1));
                indices.Add(b); indices.Add((ushort)(b + 2)); indices.Add((ushort)(b + 3));
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
