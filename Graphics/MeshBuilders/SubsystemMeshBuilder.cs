using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class SubsystemMeshBuilder
    {
        public static SimpleMesh CreateBanjoFitting(GraphicsDevice device, float ringRadius = 0.026f, float thickness = 0.016f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int segs = 16;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                float x = MathF.Cos(a) * ringRadius;
                float z = MathF.Sin(a) * ringRadius;
                Vector3 n = new Vector3(MathF.Cos(a), 0, MathF.Sin(a));

                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, 0, z), n, new Vector2((float)i / segs, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, thickness, z), n, new Vector2((float)i / segs, 0)));
            }

            for (int i = 0; i < segs; i++)
            {
                ushort b = (ushort)(i * 2);
                MeshGeometryHelper.AddCylinderWall(indices, b);
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateDinRail(GraphicsDevice device, float length = 0.20f, float width = 0.035f, float height = 0.008f)
        {
            return MeshPrimitives.CreateBox(device, new Vector3(width, length, height));
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
