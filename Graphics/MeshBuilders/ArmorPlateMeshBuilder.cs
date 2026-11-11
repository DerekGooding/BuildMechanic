using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class ArmorPlateMeshBuilder
    {
        public static SimpleMesh CreateBeveledPlate(
            GraphicsDevice device,
            Vector3 size,
            float bx,
            float by,
            float chamfer = 0.035f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            float hw = size.X * 0.5f;
            float hh = size.Y * 0.5f;
            float hz = size.Z * 0.5f;

            // Inner front face boundary inside chamfer
            float ihw = hw - chamfer;
            float ihh = hh - chamfer;

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
            {
                ushort s = (ushort)vertices.Count;
                vertices.Add(new VertexPositionNormalTexture(p0, normal, uv0));
                vertices.Add(new VertexPositionNormalTexture(p1, normal, uv1));
                vertices.Add(new VertexPositionNormalTexture(p2, normal, uv2));
                vertices.Add(new VertexPositionNormalTexture(p3, normal, uv3));

                indices.Add(s); indices.Add((ushort)(s + 1)); indices.Add((ushort)(s + 2));
                indices.Add(s); indices.Add((ushort)(s + 2)); indices.Add((ushort)(s + 3));
            }

            // 1. Front Flat Armor Face (+Z)
            Vector3 f0 = new Vector3(-ihw, -ihh, hz);
            Vector3 f1 = new Vector3(ihw, -ihh, hz);
            Vector3 f2 = new Vector3(ihw, ihh, hz);
            Vector3 f3 = new Vector3(-ihw, ihh, hz);
            AddQuad(f0, f1, f2, f3, Vector3.Backward,
                new Vector2(0.08f, 0.92f), new Vector2(0.92f, 0.92f), new Vector2(0.92f, 0.08f), new Vector2(0.08f, 0.08f));

            // 2. Beveled Chamfer Facets (Top, Bottom, Left, Right)
            Vector3 o0 = new Vector3(-hw, -hh, hz - chamfer);
            Vector3 o1 = new Vector3(hw, -hh, hz - chamfer);
            Vector3 o2 = new Vector3(hw, hh, hz - chamfer);
            Vector3 o3 = new Vector3(-hw, hh, hz - chamfer);

            // Top Chamfer
            Vector3 nTop = Vector3.Normalize(new Vector3(0, 1, 1));
            AddQuad(f3, f2, o2, o3, nTop, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.08f), new Vector2(1, 0), new Vector2(0, 0));

            // Bottom Chamfer
            Vector3 nBot = Vector3.Normalize(new Vector3(0, -1, 1));
            AddQuad(o0, o1, f1, f0, nBot, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.92f, 0.92f), new Vector2(0.08f, 0.92f));

            // Left Chamfer
            Vector3 nLeft = Vector3.Normalize(new Vector3(-1, 0, 1));
            AddQuad(o0, f0, f3, o3, nLeft, new Vector2(0, 1), new Vector2(0.08f, 0.92f), new Vector2(0.08f, 0.08f), new Vector2(0, 0));

            // Right Chamfer
            Vector3 nRight = Vector3.Normalize(new Vector3(1, 0, 1));
            AddQuad(f1, o1, o2, f2, nRight, new Vector2(0.92f, 0.92f), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0.92f, 0.08f));

            // 3. Side Walls
            Vector3 b0 = new Vector3(-hw, -hh, -hz);
            Vector3 b1 = new Vector3(hw, -hh, -hz);
            Vector3 b2 = new Vector3(hw, hh, -hz);
            Vector3 b3 = new Vector3(-hw, hh, -hz);

            AddQuad(o3, o2, b2, b3, Vector3.Up, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            AddQuad(b0, b1, o1, o0, Vector3.Down, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            AddQuad(b0, o0, o3, b3, Vector3.Left, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            AddQuad(o1, b1, b2, o2, Vector3.Right, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));

            // 4. Back Face (-Z) - Waffle Stiffener Grid
            AddQuad(b1, b0, b3, b2, Vector3.Forward, new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0));

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
