using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class MechChassisMeshBuilder
    {
        public static SimpleMesh CreateDetailedCockpit(GraphicsDevice device)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            Vector3 size = new Vector3(0.75f, 0.55f, 0.60f);
            float hw = size.X * 0.5f, hh = size.Y * 0.5f, hd = size.Z * 0.5f;

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
            {
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal, uv0, uv1, uv2, uv3);
            }

            Vector3 fBotL = new Vector3(-hw * 0.85f, -hh, hd * 0.8f);
            Vector3 fBotR = new Vector3(hw * 0.85f, -hh, hd * 0.8f);
            Vector3 fMidL = new Vector3(-hw * 0.70f, 0, hd * 0.75f);
            Vector3 fMidR = new Vector3(hw * 0.70f, 0, hd * 0.75f);
            Vector3 fTopL = new Vector3(-hw * 0.60f, hh * 0.7f, hd * 0.3f);
            Vector3 fTopR = new Vector3(hw * 0.60f, hh * 0.7f, hd * 0.3f);

            Vector3 bBotL = new Vector3(-hw, -hh, -hd);
            Vector3 bBotR = new Vector3(hw, -hh, -hd);
            Vector3 bTopL = new Vector3(-hw * 0.75f, hh, -hd * 0.7f);
            Vector3 bTopR = new Vector3(hw * 0.75f, hh, -hd * 0.7f);

            AddQuad(fBotL, fBotR, fMidR, fMidL, Vector3.Normalize(new Vector3(0, 0.4f, 1f)), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(0, 0.5f));
            AddQuad(fMidL, fMidR, fTopR, fTopL, Vector3.Normalize(new Vector3(0, 0.8f, 0.6f)), new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0), new Vector2(0, 0));
            AddQuad(fTopL, fTopR, bTopR, bTopL, Vector3.Up, new Vector2(0, 0.3f), new Vector2(1, 0.3f), new Vector2(1, 0), new Vector2(0, 0));
            AddQuad(bBotL, fBotL, fTopL, bTopL, Vector3.Left, new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0));
            AddQuad(fBotR, bBotR, bTopR, fTopR, Vector3.Right, new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0));
            AddQuad(bBotR, bBotL, bTopL, bTopR, Vector3.Forward, new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0));
            AddQuad(bBotL, bBotR, fBotR, fBotL, Vector3.Down, new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0));

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateSternalKeel(GraphicsDevice device, float width = 0.16f, float height = 1.10f, float depth = 0.08f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            float hw = width * 0.5f, hh = height * 0.5f, hd = depth * 0.5f;

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal)
            {
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);
            }

            Vector3 fBotMid = new Vector3(0, -hh, hd);
            Vector3 fTopMid = new Vector3(0, hh, hd);
            Vector3 bBotL = new Vector3(-hw, -hh, -hd);
            Vector3 bBotR = new Vector3(hw, -hh, -hd);
            Vector3 bTopL = new Vector3(-hw, hh, -hd);
            Vector3 bTopR = new Vector3(hw, hh, -hd);

            Vector3 nL = Vector3.Normalize(new Vector3(-hd, 0, hw));
            AddQuad(bBotL, fBotMid, fTopMid, bTopL, nL);
            Vector3 nR = Vector3.Normalize(new Vector3(hd, 0, hw));
            AddQuad(fBotMid, bBotR, bTopR, fTopMid, nR);
            AddQuad(bBotR, bBotL, bTopL, bTopR, Vector3.Forward);

            MeshGeometryHelper.AddTri(vertices, indices, bTopL, bTopR, fTopMid, Vector3.Up, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 1));
            MeshGeometryHelper.AddTri(vertices, indices, bBotL, fBotMid, bBotR, Vector3.Down, new Vector2(0, 0), new Vector2(0.5f, 1), new Vector2(1, 0));

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateReactorBackpack(GraphicsDevice device, float width = 0.95f, float height = 0.85f, float depth = 0.28f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            float hw = width * 0.5f, hh = height * 0.5f, hd = depth * 0.5f;

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal)
            {
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);
            }

            Vector3 f0 = new Vector3(-hw, -hh, hd);
            Vector3 f1 = new Vector3(hw, -hh, hd);
            Vector3 f2 = new Vector3(hw, hh, hd);
            Vector3 f3 = new Vector3(-hw, hh, hd);

            Vector3 b0 = new Vector3(-hw * 0.85f, -hh, -hd);
            Vector3 b1 = new Vector3(hw * 0.85f, -hh, -hd);
            Vector3 b2 = new Vector3(hw * 0.85f, hh, -hd);
            Vector3 b3 = new Vector3(-hw * 0.85f, hh, -hd);

            AddQuad(f0, f1, f2, f3, Vector3.Backward);
            AddQuad(b1, b0, b3, b2, Vector3.Forward);
            AddQuad(f3, f2, b2, b3, Vector3.Up);
            AddQuad(b0, b1, f1, f0, Vector3.Down);
            AddQuad(b0, f0, f3, b3, Vector3.Left);
            AddQuad(f1, b1, b2, f2, Vector3.Right);

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateTurretRing(GraphicsDevice device, float outerR = 0.55f, float innerR = 0.42f, float h = 0.12f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int teeth = 32;

            for (int i = 0; i < teeth; i++)
            {
                float a1 = (float)i / teeth * MathF.PI * 2f;
                float a2 = (float)(i + 1) / teeth * MathF.PI * 2f;
                float r1 = (i % 2 == 0) ? outerR : outerR - 0.035f;
                float r2 = ((i + 1) % 2 == 0) ? outerR : outerR - 0.035f;

                float ca1 = MathF.Cos(a1), sa1 = MathF.Sin(a1);
                float ca2 = MathF.Cos(a2), sa2 = MathF.Sin(a2);

                Vector3 o0 = new Vector3(ca1 * r1, 0, sa1 * r1);
                Vector3 o1 = new Vector3(ca2 * r2, 0, sa2 * r2);
                Vector3 o2 = new Vector3(ca2 * r2, h, sa2 * r2);
                Vector3 o3 = new Vector3(ca1 * r1, h, sa1 * r1);

                Vector3 i0 = new Vector3(ca1 * innerR, 0, sa1 * innerR);
                Vector3 i1 = new Vector3(ca2 * innerR, 0, sa2 * innerR);
                Vector3 i2 = new Vector3(ca2 * innerR, h, sa2 * innerR);
                Vector3 i3 = new Vector3(ca1 * innerR, h, sa1 * innerR);

                Vector3 nOuter = Vector3.Normalize(new Vector3((ca1 + ca2) * 0.5f, 0, (sa1 + sa2) * 0.5f));

                // Outer toothed perimeter
                MeshGeometryHelper.AddQuad(vertices, indices, o0, o1, o2, o3, nOuter);
                // Inner race bore
                MeshGeometryHelper.AddQuad(vertices, indices, i1, i0, i3, i2, -nOuter);
                // Top flange ring
                MeshGeometryHelper.AddQuad(vertices, indices, i3, o3, o2, i2, Vector3.Up);
                // Bottom mounting deck
                MeshGeometryHelper.AddQuad(vertices, indices, o0, i0, i1, o1, Vector3.Down);
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateKneeJoint(GraphicsDevice device, float radius = 0.16f, float thickness = 0.22f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int segs = 16;
            float ht = thickness * 0.5f;

            Vector3 cLeft = new Vector3(-ht, 0, 0);
            Vector3 cRight = new Vector3(ht, 0, 0);

            for (int i = 0; i < segs; i++)
            {
                float a1 = (float)i / segs * MathF.PI * 2f;
                float a2 = (float)(i + 1) / segs * MathF.PI * 2f;
                float ca1 = MathF.Cos(a1), sa1 = MathF.Sin(a1);
                float ca2 = MathF.Cos(a2), sa2 = MathF.Sin(a2);

                Vector3 p0 = new Vector3(-ht, sa1 * radius, ca1 * radius);
                Vector3 p1 = new Vector3(ht, sa1 * radius, ca1 * radius);
                Vector3 p2 = new Vector3(ht, sa2 * radius, ca2 * radius);
                Vector3 p3 = new Vector3(-ht, sa2 * radius, ca2 * radius);

                Vector3 n = Vector3.Normalize(new Vector3(0, (sa1 + sa2) * 0.5f, (ca1 + ca2) * 0.5f));

                // Cylindrical bearing drum
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, n);
                // Left bearing pivot cap
                MeshGeometryHelper.AddTri(vertices, indices, cLeft, p0, p3, Vector3.Left, Vector2.Zero, Vector2.Zero, Vector2.Zero);
                // Right bearing pivot cap
                MeshGeometryHelper.AddTri(vertices, indices, cRight, p2, p1, Vector3.Right, Vector2.Zero, Vector2.Zero, Vector2.Zero);
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
