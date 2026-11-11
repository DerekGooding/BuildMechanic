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

            // Curved heavy forged steel gantry crane hook
            int segs = 12;
            float hookRadius = 0.14f;
            float thickness = 0.035f;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 1.4f; // 250 degree curve
                float x = MathF.Cos(a) * hookRadius;
                float y = -MathF.Sin(a) * hookRadius;

                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, y, -thickness), Vector3.Backward, new Vector2((float)i / segs, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(x, y, thickness), Vector3.Forward, new Vector2((float)i / segs, 0)));
            }

            for (int i = 0; i < segs; i++)
            {
                ushort b = (ushort)(i * 2);
                indices.Add(b); indices.Add((ushort)(b + 1)); indices.Add((ushort)(b + 3));
                indices.Add(b); indices.Add((ushort)(b + 3)); indices.Add((ushort)(b + 2));
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
