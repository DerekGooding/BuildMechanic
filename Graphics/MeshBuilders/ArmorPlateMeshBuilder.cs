using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class ArmorPlateMeshBuilder
    {
        public static SimpleMesh CreateTorsoPlate(GraphicsDevice device, Vector3 size, float bx, float by)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            float hw = size.X * 0.5f;
            float hh = size.Y * 0.5f;
            float hz = size.Z * 0.5f;
            float chamfer = 0.04f;

            // Inner front face boundary
            float ihw = hw - chamfer;
            float ihh = hh - chamfer;

            // Clockwise quad helper
            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
            {
                ushort s = (ushort)vertices.Count;
                vertices.Add(new VertexPositionNormalTexture(p0, normal, uv0));
                vertices.Add(new VertexPositionNormalTexture(p1, normal, uv1));
                vertices.Add(new VertexPositionNormalTexture(p2, normal, uv2));
                vertices.Add(new VertexPositionNormalTexture(p3, normal, uv3));

                // Clockwise outward facing triangles
                indices.Add(s); indices.Add((ushort)(s + 2)); indices.Add((ushort)(s + 1));
                indices.Add(s); indices.Add((ushort)(s + 3)); indices.Add((ushort)(s + 2));
            }

            // 1. Faceted Front Ballistic Prow (Center peak raised slightly for ballistic slope)
            float prowPeakZ = hz + 0.012f;
            Vector3 f0 = new Vector3(-ihw, -ihh, hz);
            Vector3 f1 = new Vector3(ihw, -ihh, hz);
            Vector3 f2 = new Vector3(ihw, ihh, hz);
            Vector3 f3 = new Vector3(-ihw, ihh, hz);
            Vector3 fMid = new Vector3(0, 0, prowPeakZ);

            // 4 faceted deflection triangles around central prow ridge
            void AddTri(Vector3 a, Vector3 b, Vector3 c, Vector3 normal, Vector2 uva, Vector2 uvb, Vector2 uvc)
            {
                ushort s = (ushort)vertices.Count;
                vertices.Add(new VertexPositionNormalTexture(a, normal, uva));
                vertices.Add(new VertexPositionNormalTexture(b, normal, uvb));
                vertices.Add(new VertexPositionNormalTexture(c, normal, uvc));
                indices.Add(s); indices.Add((ushort)(s + 2)); indices.Add((ushort)(s + 1));
            }

            Vector3 nLower = Vector3.Normalize(new Vector3(0, -0.2f, 1));
            AddTri(f0, f1, fMid, nLower, new Vector2(0.1f, 0.9f), new Vector2(0.9f, 0.9f), new Vector2(0.5f, 0.5f));

            Vector3 nRight = Vector3.Normalize(new Vector3(0.2f, 0, 1));
            AddTri(f1, f2, fMid, nRight, new Vector2(0.9f, 0.9f), new Vector2(0.9f, 0.1f), new Vector2(0.5f, 0.5f));

            Vector3 nUpper = Vector3.Normalize(new Vector3(0, 0.2f, 1));
            AddTri(f2, f3, fMid, nUpper, new Vector2(0.9f, 0.1f), new Vector2(0.1f, 0.1f), new Vector2(0.5f, 0.5f));

            Vector3 nLeft = Vector3.Normalize(new Vector3(-0.2f, 0, 1));
            AddTri(f3, f0, fMid, nLeft, new Vector2(0.1f, 0.1f), new Vector2(0.1f, 0.9f), new Vector2(0.5f, 0.5f));

            // 2. Beveled Outer Perimeter Chamfers
            Vector3 o0 = new Vector3(-hw, -hh, hz - chamfer);
            Vector3 o1 = new Vector3(hw, -hh, hz - chamfer);
            Vector3 o2 = new Vector3(hw, hh, hz - chamfer);
            Vector3 o3 = new Vector3(-hw, hh, hz - chamfer);

            AddQuad(f3, f2, o2, o3, Vector3.Normalize(new Vector3(0, 1, 1)), new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.1f), new Vector2(1, 0), new Vector2(0, 0));
            AddQuad(o0, o1, f1, f0, Vector3.Normalize(new Vector3(0, -1, 1)), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.9f, 0.9f), new Vector2(0.1f, 0.9f));
            AddQuad(o0, f0, f3, o3, Vector3.Normalize(new Vector3(-1, 0, 1)), new Vector2(0, 1), new Vector2(0.1f, 0.9f), new Vector2(0.1f, 0.1f), new Vector2(0, 0));
            AddQuad(f1, o1, o2, f2, Vector3.Normalize(new Vector3(1, 0, 1)), new Vector2(0.9f, 0.9f), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0.9f, 0.1f));

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
            AddQuad(b0, b1, b2, b3, Vector3.Forward, new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0));

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateShoulderPlate(GraphicsDevice device, Vector3 size, float bx, float by)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            float hw = size.X * 0.5f, hh = size.Y * 0.5f, hz = size.Z * 0.5f;

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
            {
                ushort s = (ushort)vertices.Count;
                vertices.Add(new VertexPositionNormalTexture(p0, normal, uv0));
                vertices.Add(new VertexPositionNormalTexture(p1, normal, uv1));
                vertices.Add(new VertexPositionNormalTexture(p2, normal, uv2));
                vertices.Add(new VertexPositionNormalTexture(p3, normal, uv3));

                indices.Add(s); indices.Add((ushort)(s + 2)); indices.Add((ushort)(s + 1));
                indices.Add(s); indices.Add((ushort)(s + 3)); indices.Add((ushort)(s + 2));
            }

            // Arched clamshell pauldron geometry
            float archZ = hz + 0.015f;
            Vector3 f0 = new Vector3(-hw, -hh, hz - 0.01f);
            Vector3 f1 = new Vector3(hw, -hh, hz - 0.01f);
            Vector3 f2 = new Vector3(hw, hh, archZ);
            Vector3 f3 = new Vector3(-hw, hh, archZ);

            Vector3 nFront = Vector3.Normalize(new Vector3(0, 0.15f, 1f));
            AddQuad(f0, f1, f2, f3, nFront, new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0));

            // Top Deflector Ridge (Anti-shot trap baffle)
            Vector3 ridgeTop = new Vector3(0, hh + 0.035f, archZ - 0.02f);
            Vector3 r0 = new Vector3(-hw, hh + 0.035f, archZ - 0.02f);
            Vector3 r1 = new Vector3(hw, hh + 0.035f, archZ - 0.02f);
            AddQuad(f3, f2, r1, r0, Vector3.Up, new Vector2(0, 0.2f), new Vector2(1, 0.2f), new Vector2(1, 0), new Vector2(0, 0));

            // Back & sides
            Vector3 b0 = new Vector3(-hw, -hh, -hz);
            Vector3 b1 = new Vector3(hw, -hh, -hz);
            Vector3 b2 = new Vector3(hw, hh, -hz);
            Vector3 b3 = new Vector3(-hw, hh, -hz);

            AddQuad(r0, r1, b2, b3, Vector3.Up, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            AddQuad(b0, b1, f1, f0, Vector3.Down, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            AddQuad(b0, f0, f3, b3, Vector3.Left, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            AddQuad(f1, b1, b2, f2, Vector3.Right, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));

            // Back face
            AddQuad(b0, b1, b2, b3, Vector3.Forward, new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0));

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateLegPlate(GraphicsDevice device, Vector3 size, float bx, float by)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            float hw = size.X * 0.5f, hh = size.Y * 0.5f, hz = size.Z * 0.5f;

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
            {
                ushort s = (ushort)vertices.Count;
                vertices.Add(new VertexPositionNormalTexture(p0, normal, uv0));
                vertices.Add(new VertexPositionNormalTexture(p1, normal, uv1));
                vertices.Add(new VertexPositionNormalTexture(p2, normal, uv2));
                vertices.Add(new VertexPositionNormalTexture(p3, normal, uv3));

                indices.Add(s); indices.Add((ushort)(s + 2)); indices.Add((ushort)(s + 1));
                indices.Add(s); indices.Add((ushort)(s + 3)); indices.Add((ushort)(s + 2));
            }

            // Tapered angular shin guard with vertical prow crest
            float prowZ = hz + 0.016f;
            Vector3 fBotL = new Vector3(-hw * 0.85f, -hh, hz);
            Vector3 fBotR = new Vector3(hw * 0.85f, -hh, hz);
            Vector3 fTopL = new Vector3(-hw, hh, hz);
            Vector3 fTopR = new Vector3(hw, hh, hz);

            Vector3 fMidBot = new Vector3(0, -hh, prowZ);
            Vector3 fMidTop = new Vector3(0, hh, prowZ);

            Vector3 nLeft = Vector3.Normalize(new Vector3(-0.25f, 0, 1f));
            Vector3 nRight = Vector3.Normalize(new Vector3(0.25f, 0, 1f));

            AddQuad(fBotL, fMidBot, fMidTop, fTopL, nLeft, new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 0), new Vector2(0, 0));
            AddQuad(fMidBot, fBotR, fTopR, fMidTop, nRight, new Vector2(0.5f, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0.5f, 0));

            // Back & sides
            Vector3 b0 = new Vector3(-hw * 0.85f, -hh, -hz);
            Vector3 b1 = new Vector3(hw * 0.85f, -hh, -hz);
            Vector3 b2 = new Vector3(hw, hh, -hz);
            Vector3 b3 = new Vector3(-hw, hh, -hz);

            AddQuad(fTopL, fTopR, b2, b3, Vector3.Up, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            AddQuad(b0, b1, fBotR, fBotL, Vector3.Down, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            AddQuad(b0, fBotL, fTopL, b3, Vector3.Left, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            AddQuad(fBotR, b1, b2, fTopR, Vector3.Right, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));

            // Back face
            AddQuad(b0, b1, b2, b3, Vector3.Forward, new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0));

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
