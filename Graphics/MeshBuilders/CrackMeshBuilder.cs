using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class CrackMeshBuilder
    {
        public static SimpleMesh CreateWeldBead(GraphicsDevice device, float length = 1.0f, float width = 0.038f, float height = 0.022f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int scallops = 10;

            // Overlapping rounded weld scallops ("stack of dimes")
            for (int i = 0; i <= scallops; i++)
            {
                float z = ((float)i / scallops - 0.5f) * length;
                float u = (float)i / scallops;

                // Crest vertex
                vertices.Add(new VertexPositionNormalTexture(new Vector3(0, height, z), Vector3.Up, new Vector2(0.5f, u)));
                // Left toe vertex
                vertices.Add(new VertexPositionNormalTexture(new Vector3(-width * 0.5f, 0, z), Vector3.Left, new Vector2(0f, u)));
                // Right toe vertex
                vertices.Add(new VertexPositionNormalTexture(new Vector3(width * 0.5f, 0, z), Vector3.Right, new Vector2(1f, u)));
            }

            for (int i = 0; i < scallops; i++)
            {
                ushort row1 = (ushort)(i * 3);
                ushort row2 = (ushort)((i + 1) * 3);

                // Left flank triangle
                indices.Add(row1); indices.Add(row2); indices.Add((ushort)(row1 + 1));
                indices.Add((ushort)(row1 + 1)); indices.Add(row2); indices.Add((ushort)(row2 + 1));

                // Right flank triangle
                indices.Add(row1); indices.Add((ushort)(row1 + 2)); indices.Add(row2);
                indices.Add((ushort)(row1 + 2)); indices.Add((ushort)(row2 + 2)); indices.Add(row2);
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
