using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class CoreInternalsMeshBuilder
    {
        public static SimpleMesh CreateGyroCage(GraphicsDevice device)
        {
            var v = new List<VertexPositionNormalTexture>();
            var idx = new List<ushort>();
            AddBox(v, idx, new Vector3(0, 0.36f, 0), new Vector3(0.95f, 0.18f, 0.75f));
            AddBox(v, idx, new Vector3(0, -0.36f, 0), new Vector3(0.95f, 0.18f, 0.75f));
            AddBox(v, idx, new Vector3(0, 0, 0), new Vector3(0.42f, 0.56f, 0.85f));
            return BuildMesh(device, v, idx);
        }

        public static SimpleMesh CreatePowerPCB(GraphicsDevice device)
        {
            var v = new List<VertexPositionNormalTexture>();
            var idx = new List<ushort>();
            AddBox(v, idx, Vector3.Zero, new Vector3(0.95f, 0.90f, 0.30f));
            for (int i = -1; i <= 1; i++)
                AddBox(v, idx, new Vector3(i * 0.28f, 0.15f, 0.28f), new Vector3(0.14f, 0.36f, 0.35f));
            AddBox(v, idx, new Vector3(0, -0.26f, 0.25f), new Vector3(0.75f, 0.16f, 0.25f));
            return BuildMesh(device, v, idx);
        }

        public static SimpleMesh CreateGearRing(GraphicsDevice device)
        {
            var v = new List<VertexPositionNormalTexture>();
            var idx = new List<ushort>();
            AddBox(v, idx, Vector3.Zero, new Vector3(0.66f, 0.66f, 0.65f));
            for (int i = 0; i < 8; i++)
            {
                float a = i * MathF.PI / 4f;
                AddBox(v, idx, new Vector3(MathF.Cos(a) * 0.38f, MathF.Sin(a) * 0.38f, 0.05f), new Vector3(0.16f, 0.16f, 0.55f));
            }
            return BuildMesh(device, v, idx);
        }

        public static SimpleMesh CreateBearingAssembly(GraphicsDevice device)
        {
            var v = new List<VertexPositionNormalTexture>();
            var idx = new List<ushort>();
            AddBox(v, idx, Vector3.Zero, new Vector3(0.45f, 0.45f, 0.75f));
            for (int i = 0; i < 6; i++)
            {
                float a = i * MathF.PI / 3f;
                AddBox(v, idx, new Vector3(MathF.Cos(a) * 0.30f, MathF.Sin(a) * 0.30f, 0.08f), new Vector3(0.14f, 0.14f, 0.50f));
            }
            return BuildMesh(device, v, idx);
        }

        public static SimpleMesh CreateShockStrut(GraphicsDevice device)
        {
            var v = new List<VertexPositionNormalTexture>();
            var idx = new List<ushort>();
            AddBox(v, idx, Vector3.Zero, new Vector3(0.32f, 0.96f, 0.55f));
            for (int i = -2; i <= 2; i++)
                AddBox(v, idx, new Vector3(0, i * 0.16f, 0.08f), new Vector3(0.56f, 0.08f, 0.65f));
            return BuildMesh(device, v, idx);
        }

        public static SimpleMesh CreateAccumulator(GraphicsDevice device)
        {
            var v = new List<VertexPositionNormalTexture>();
            var idx = new List<ushort>();
            AddBox(v, idx, Vector3.Zero, new Vector3(0.68f, 0.85f, 0.75f));
            AddBox(v, idx, new Vector3(0, 0.46f, 0.05f), new Vector3(0.30f, 0.16f, 0.30f));
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
