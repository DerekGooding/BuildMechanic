using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class DeepBayMeshBuilder
    {
        public static SimpleMesh CreateThermalCowling(GraphicsDevice device)
        {
            var v = new List<VertexPositionNormalTexture>();
            var idx = new List<ushort>();

            // Base frame + 4 angled cooling louver slats (normalized 1x1x1)
            AddBox(v, idx, new Vector3(0, 0.38f, 0), new Vector3(1.0f, 0.22f, 0.7f));
            AddBox(v, idx, new Vector3(0, -0.38f, 0), new Vector3(1.0f, 0.22f, 0.7f));
            for (int i = -1; i <= 1; i++)
            {
                AddBox(v, idx, new Vector3(0, i * 0.18f, 0.15f), new Vector3(0.86f, 0.08f, 0.5f));
            }
            return BuildMesh(device, v, idx);
        }

        public static SimpleMesh CreateStructuralRib(GraphicsDevice device)
        {
            var v = new List<VertexPositionNormalTexture>();
            var idx = new List<ushort>();

            // Heavy I-beam titanium rib with center gusset
            AddBox(v, idx, new Vector3(0, 0, 0), new Vector3(1.0f, 0.34f, 0.85f));
            AddBox(v, idx, new Vector3(-0.35f, 0, 0.1f), new Vector3(0.24f, 0.95f, 0.9f));
            AddBox(v, idx, new Vector3(0.35f, 0, 0.1f), new Vector3(0.24f, 0.95f, 0.9f));
            return BuildMesh(device, v, idx);
        }

        public static SimpleMesh CreatePumpCarrier(GraphicsDevice device)
        {
            var v = new List<VertexPositionNormalTexture>();
            var idx = new List<ushort>();

            // Saddle bracket with dual mounting ears
            AddBox(v, idx, new Vector3(-0.36f, 0, 0), new Vector3(0.26f, 0.92f, 0.8f));
            AddBox(v, idx, new Vector3(0.36f, 0, 0), new Vector3(0.26f, 0.92f, 0.8f));
            AddBox(v, idx, new Vector3(0, 0.36f, 0.1f), new Vector3(0.82f, 0.22f, 0.65f));
            AddBox(v, idx, new Vector3(0, -0.36f, 0.1f), new Vector3(0.82f, 0.22f, 0.65f));
            return BuildMesh(device, v, idx);
        }

        public static SimpleMesh CreateImpellerPump(GraphicsDevice device)
        {
            var v = new List<VertexPositionNormalTexture>();
            var idx = new List<ushort>();

            // Central hub + 6 radial impeller blades
            AddBox(v, idx, Vector3.Zero, new Vector3(0.38f, 0.38f, 0.85f));
            for (int i = 0; i < 6; i++)
            {
                float a = i * MathF.PI / 3f;
                Vector3 offset = new Vector3(MathF.Cos(a) * 0.28f, MathF.Sin(a) * 0.28f, 0.12f);
                AddBox(v, idx, offset, new Vector3(0.20f, 0.20f, 0.45f));
            }
            return BuildMesh(device, v, idx);
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
