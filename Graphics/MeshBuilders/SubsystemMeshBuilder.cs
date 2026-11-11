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
            float innerR = ringRadius * 0.55f;

            for (int i = 0; i < segs; i++)
            {
                float a1 = (float)i / segs * MathF.PI * 2f;
                float a2 = (float)(i + 1) / segs * MathF.PI * 2f;
                float ca1 = MathF.Cos(a1), sa1 = MathF.Sin(a1);
                float ca2 = MathF.Cos(a2), sa2 = MathF.Sin(a2);

                Vector3 o0 = new Vector3(ca1 * ringRadius, 0, sa1 * ringRadius);
                Vector3 o1 = new Vector3(ca2 * ringRadius, 0, sa2 * ringRadius);
                Vector3 o2 = new Vector3(ca2 * ringRadius, thickness, sa2 * ringRadius);
                Vector3 o3 = new Vector3(ca1 * ringRadius, thickness, sa1 * ringRadius);

                Vector3 i0 = new Vector3(ca1 * innerR, 0, sa1 * innerR);
                Vector3 i1 = new Vector3(ca2 * innerR, 0, sa2 * innerR);
                Vector3 i2 = new Vector3(ca2 * innerR, thickness, sa2 * innerR);
                Vector3 i3 = new Vector3(ca1 * innerR, thickness, sa1 * innerR);

                Vector3 nOuter = Vector3.Normalize(new Vector3((ca1 + ca2) * 0.5f, 0, (sa1 + sa2) * 0.5f));

                // Outer cylindrical ring perimeter
                MeshGeometryHelper.AddQuad(vertices, indices, o0, o1, o2, o3, nOuter);
                // Inner hydraulic fluid port bore
                MeshGeometryHelper.AddQuad(vertices, indices, i1, i0, i3, i2, -nOuter);
                // Top sealing washer deck (+Y)
                MeshGeometryHelper.AddQuad(vertices, indices, i3, o3, o2, i2, Vector3.Up);
                // Bottom sealing washer deck (-Y)
                MeshGeometryHelper.AddQuad(vertices, indices, o0, i0, i1, o1, Vector3.Down);
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
