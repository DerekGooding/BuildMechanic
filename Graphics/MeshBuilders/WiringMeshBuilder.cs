using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class WiringMeshBuilder
    {
        public static SimpleMesh CreateCableHarness(
            GraphicsDevice device,
            Vector3 start,
            Vector3 end,
            float sag = 0.045f,
            float radius = 0.011f,
            int pathSegments = 12,
            int radialSegments = 8)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            var centers = new Vector3[pathSegments + 1];
            var tangents = new Vector3[pathSegments + 1];

            for (int i = 0; i <= pathSegments; i++)
            {
                float t = (float)i / pathSegments;
                Vector3 p = Vector3.Lerp(start, end, t);
                float arc = MathF.Sin(t * MathF.PI);
                p.Y -= sag * arc;
                p.Z += 0.018f * arc;
                centers[i] = p;
            }

            for (int i = 0; i <= pathSegments; i++)
            {
                Vector3 dir = i == 0
                    ? centers[1] - centers[0]
                    : (i == pathSegments ? centers[i] - centers[i - 1] : centers[i + 1] - centers[i - 1]);
                tangents[i] = dir.LengthSquared() > 1e-6f ? Vector3.Normalize(dir) : Vector3.Right;
            }

            var ringPts = new Vector3[pathSegments + 1, radialSegments];
            var ringNormals = new Vector3[pathSegments + 1, radialSegments];

            for (int i = 0; i <= pathSegments; i++)
            {
                Vector3 fwd = tangents[i];
                Vector3 refUp = Math.Abs(Vector3.Dot(fwd, Vector3.Backward)) > 0.9f ? Vector3.Up : Vector3.Backward;
                Vector3 right = Vector3.Normalize(Vector3.Cross(refUp, fwd));
                Vector3 up = Vector3.Normalize(Vector3.Cross(fwd, right));

                for (int j = 0; j < radialSegments; j++)
                {
                    float angle = (float)j / radialSegments * MathF.PI * 2f;
                    Vector3 n = right * MathF.Cos(angle) + up * MathF.Sin(angle);
                    ringNormals[i, j] = n;
                    ringPts[i, j] = centers[i] + n * radius;
                }
            }

            for (int i = 0; i < pathSegments; i++)
            {
                for (int j = 0; j < radialSegments; j++)
                {
                    int nextJ = (j + 1) % radialSegments;
                    Vector3 p0 = ringPts[i, j];
                    Vector3 p1 = ringPts[i + 1, j];
                    Vector3 p2 = ringPts[i + 1, nextJ];
                    Vector3 p3 = ringPts[i, nextJ];
                    Vector3 avgN = Vector3.Normalize(ringNormals[i, j] + ringNormals[i, nextJ]);
                    MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, avgN);
                }
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateConnectorPlug(GraphicsDevice device)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            // Stepped MIL-spec plug housing + locking collar
            AddBox(vertices, indices, Vector3.Zero, new Vector3(0.028f, 0.028f, 0.032f));
            AddBox(vertices, indices, new Vector3(0, 0, 0.018f), new Vector3(0.034f, 0.034f, 0.010f));

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
