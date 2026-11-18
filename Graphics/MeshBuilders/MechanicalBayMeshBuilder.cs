using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class MechanicalBayMeshBuilder
    {
        public static SimpleMesh CreateRetainingBracket(GraphicsDevice device)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            // Normalized 1x1x1 U-channel retaining shroud with top/bottom stiffener flanges and central rib
            AddBox(vertices, indices, Vector3.Zero, new Vector3(1.0f, 0.56f, 0.45f));
            AddBox(vertices, indices, new Vector3(0, 0.34f, -0.15f), new Vector3(0.92f, 0.14f, 0.70f));
            AddBox(vertices, indices, new Vector3(0, -0.34f, -0.15f), new Vector3(0.92f, 0.14f, 0.70f));
            // Raised center reinforcement spine
            AddBox(vertices, indices, new Vector3(0, 0, 0.32f), new Vector3(0.45f, 0.24f, 0.28f));

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateHydraulicManifold(GraphicsDevice device)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            // Main machined valve block
            AddBox(vertices, indices, Vector3.Zero, new Vector3(0.085f, 0.095f, 0.065f));
            // Top pressure-relief hex valve cap
            AddBox(vertices, indices, new Vector3(0, 0.06f, 0), new Vector3(0.036f, 0.032f, 0.036f));
            // Left & right braided hose port bosses
            AddBox(vertices, indices, new Vector3(-0.052f, 0.015f, 0.01f), new Vector3(0.025f, 0.028f, 0.028f));
            AddBox(vertices, indices, new Vector3(0.052f, -0.015f, 0.01f), new Vector3(0.025f, 0.028f, 0.028f));
            // Front pressure gauge bezel
            AddBox(vertices, indices, new Vector3(0, -0.01f, 0.038f), new Vector3(0.042f, 0.042f, 0.018f));

            return BuildMesh(device, vertices, indices);
        }

        private static void AddBox(List<VertexPositionNormalTexture> v, List<ushort> idx, Vector3 center, Vector3 size)
        {
            Vector3 h = size * 0.5f;
            Vector3 p000 = center + new Vector3(-h.X, -h.Y, -h.Z);
            Vector3 p100 = center + new Vector3(h.X, -h.Y, -h.Z);
            Vector3 p110 = center + new Vector3(h.X, h.Y, -h.Z);
            Vector3 p010 = center + new Vector3(-h.X, h.Y, -h.Z);
            Vector3 p001 = center + new Vector3(-h.X, -h.Y, h.Z);
            Vector3 p101 = center + new Vector3(h.X, -h.Y, h.Z);
            Vector3 p111 = center + new Vector3(h.X, h.Y, h.Z);
            Vector3 p011 = center + new Vector3(-h.X, h.Y, h.Z);

            MeshGeometryHelper.AddQuad(v, idx, p001, p101, p111, p011, Vector3.Backward);
            MeshGeometryHelper.AddQuad(v, idx, p100, p000, p010, p110, Vector3.Forward);
            MeshGeometryHelper.AddQuad(v, idx, p000, p001, p011, p010, Vector3.Left);
            MeshGeometryHelper.AddQuad(v, idx, p101, p100, p110, p111, Vector3.Right);
            MeshGeometryHelper.AddQuad(v, idx, p011, p111, p110, p010, Vector3.Up);
            MeshGeometryHelper.AddQuad(v, idx, p000, p100, p101, p001, Vector3.Down);
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
