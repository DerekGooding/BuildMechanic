using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class MechLimbsMeshBuilder
    {
        public static SimpleMesh CreateForearmAssembly(GraphicsDevice device, float length = 0.80f, float width = 0.22f, float depth = 0.22f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            float hw = width * 0.5f, hd = depth * 0.5f;

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
            {
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal, uv0, uv1, uv2, uv3);
            }

            // Tapered hexagonal armored forearm from elbow (y=0) to wrist (y=-length)
            Vector3 e0 = new Vector3(-hw, 0, hd);
            Vector3 e1 = new Vector3(hw, 0, hd);
            Vector3 e2 = new Vector3(hw * 1.1f, 0, -hd);
            Vector3 e3 = new Vector3(-hw * 1.1f, 0, -hd);

            float tw = hw * 0.75f, td = hd * 0.75f;
            Vector3 w0 = new Vector3(-tw, -length, td);
            Vector3 w1 = new Vector3(tw, -length, td);
            Vector3 w2 = new Vector3(tw, -length, -td);
            Vector3 w3 = new Vector3(-tw, -length, -td);

            AddQuad(e0, e1, w1, w0, Vector3.Backward, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            AddQuad(e1, e2, w2, w1, Vector3.Right, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            AddQuad(e2, e3, w3, w2, Vector3.Forward, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            AddQuad(e3, e0, w0, w3, Vector3.Left, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));

            // Elbow cap
            AddQuad(e3, e2, e1, e0, Vector3.Up, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            // Wrist cuff
            AddQuad(w0, w1, w2, w3, Vector3.Down, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateManipulatorClaw(GraphicsDevice device, float palmRadius = 0.12f, float fingerLength = 0.28f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal)
            {
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);
            }

            // 3 Heavy Industrial Claw Fingers angled inwards with solid 3D geometry
            for (int i = 0; i < 3; i++)
            {
                float angle = (float)i / 3f * MathF.PI * 2f;
                float ca = MathF.Cos(angle), sa = MathF.Sin(angle);

                Vector3 basePos = new Vector3(ca * palmRadius, 0, sa * palmRadius);
                Vector3 tipPos = new Vector3(ca * (palmRadius * 0.4f), -fingerLength, sa * (palmRadius * 0.4f));

                float fw = 0.035f;
                Vector3 r = new Vector3(-sa * fw, 0, ca * fw);
                Vector3 depthOffset = new Vector3(-ca * 0.025f, 0, -sa * 0.025f);

                Vector3 b0 = basePos - r;
                Vector3 b1 = basePos + r;
                Vector3 t0 = tipPos - r * 0.5f;
                Vector3 t1 = tipPos + r * 0.5f;

                Vector3 n = new Vector3(ca, -0.4f, sa);
                n.Normalize();

                // Front gripping face & back armor face
                AddQuad(b0, b1, t1, t0, n);
                AddQuad(b1 + depthOffset, b0 + depthOffset, t0 + depthOffset, t1 + depthOffset, -n);

                // Side flanks
                Vector3 rNorm = Vector3.Normalize(r);
                AddQuad(b1, b1 + depthOffset, t1 + depthOffset, t1, rNorm);
                AddQuad(b0 + depthOffset, b0, t0, t0 + depthOffset, -rNorm);

                // Fingertip cap & base mounting cap
                Vector3 tipNorm = Vector3.Normalize(tipPos - basePos);
                AddQuad(t0, t1, t1 + depthOffset, t0 + depthOffset, tipNorm);
                AddQuad(b0 + depthOffset, b1 + depthOffset, b1, b0, -tipNorm);
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateLowerLegCalf(GraphicsDevice device, float length = 0.90f, float width = 0.24f, float depth = 0.28f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            float hw = width * 0.5f, hd = depth * 0.5f;

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal)
            {
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);
            }

            // Armored calf stanchion column with shin spine
            Vector3 k0 = new Vector3(-hw, 0, hd);
            Vector3 k1 = new Vector3(hw, 0, hd);
            Vector3 k2 = new Vector3(hw * 0.9f, 0, -hd);
            Vector3 k3 = new Vector3(-hw * 0.9f, 0, -hd);

            float aw = hw * 0.85f, ad = hd * 0.9f;
            Vector3 a0 = new Vector3(-aw, -length, ad);
            Vector3 a1 = new Vector3(aw, -length, ad);
            Vector3 a2 = new Vector3(aw * 0.85f, -length, -ad);
            Vector3 a3 = new Vector3(-aw * 0.85f, -length, -ad);

            AddQuad(k0, k1, a1, a0, Vector3.Backward);
            AddQuad(k1, k2, a2, a1, Vector3.Right);
            AddQuad(k2, k3, a3, a2, Vector3.Forward);
            AddQuad(k3, k0, a0, a3, Vector3.Left);
            AddQuad(k3, k2, k1, k0, Vector3.Up);
            AddQuad(a0, a1, a2, a3, Vector3.Down);

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateFootPad(GraphicsDevice device, float width = 0.38f, float length = 0.54f, float height = 0.12f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            float hw = width * 0.5f, hl = length * 0.5f;

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal)
            {
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);
            }

            // Flared hexagonal heavy foot pad
            Vector3 b0 = new Vector3(-hw, 0, hl);
            Vector3 b1 = new Vector3(hw, 0, hl);
            Vector3 b2 = new Vector3(hw * 1.15f, 0, -hl);
            Vector3 b3 = new Vector3(-hw * 1.15f, 0, -hl);

            Vector3 t0 = new Vector3(-hw * 0.7f, height, hl * 0.6f);
            Vector3 t1 = new Vector3(hw * 0.7f, height, hl * 0.6f);
            Vector3 t2 = new Vector3(hw * 0.8f, height, -hl * 0.7f);
            Vector3 t3 = new Vector3(-hw * 0.8f, height, -hl * 0.7f);

            // Front toe bevel
            AddQuad(b0, b1, t1, t0, Vector3.Normalize(new Vector3(0, 0.4f, 1f)));
            // Right flank
            AddQuad(b1, b2, t2, t1, Vector3.Right);
            // Rear heel
            AddQuad(b2, b3, t3, t2, Vector3.Forward);
            // Left flank
            AddQuad(b3, b0, t0, t3, Vector3.Left);
            // Top ankle deck
            AddQuad(t3, t2, t1, t0, Vector3.Up);
            // Bottom tread (contacts floor)
            AddQuad(b0, b1, b2, b3, Vector3.Down);

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateArmoredBicep(GraphicsDevice device, float length = 0.85f, float radius = 0.17f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int segs = 8;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                float ca = MathF.Cos(a), sa = MathF.Sin(a);
                Vector3 n = new Vector3(ca, 0, sa);

                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * radius, -length, sa * radius), n, new Vector2((float)i / segs, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * radius, 0, sa * radius), n, new Vector2((float)i / segs, 0)));
            }

            for (int i = 0; i < segs; i++)
            {
                ushort b = (ushort)(i * 2);
                MeshGeometryHelper.AddCylinderWall(indices, b);
            }

            // Top shoulder trunnion cap (y = 0)
            ushort topCenter = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(Vector3.Zero, Vector3.Up, new Vector2(0.5f, 0.5f)));
            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(MathF.Cos(a) * radius, 0, MathF.Sin(a) * radius), Vector3.Up, Vector2.Zero));
            }
            for (int i = 0; i < segs; i++)
            {
                MeshGeometryHelper.AddTopCapTri(indices, topCenter, (ushort)i);
            }

            // Bottom elbow clevis cap (y = -length)
            ushort botCenter = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(new Vector3(0, -length, 0), Vector3.Down, new Vector2(0.5f, 0.5f)));
            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(MathF.Cos(a) * radius, -length, MathF.Sin(a) * radius), Vector3.Down, Vector2.Zero));
            }
            for (int i = 0; i < segs; i++)
            {
                MeshGeometryHelper.AddBottomCapTri(indices, botCenter, (ushort)i);
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateArmoredThigh(GraphicsDevice device, float length = 0.65f, float radius = 0.18f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int segs = 8;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                float ca = MathF.Cos(a), sa = MathF.Sin(a);
                Vector3 n = new Vector3(ca, 0, sa);

                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * radius, -length, sa * radius), n, new Vector2((float)i / segs, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * radius, 0, sa * radius), n, new Vector2((float)i / segs, 0)));
            }

            for (int i = 0; i < segs; i++)
            {
                ushort b = (ushort)(i * 2);
                MeshGeometryHelper.AddCylinderWall(indices, b);
            }

            // Top hip gimbal cap (y = 0)
            ushort topCenter = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(Vector3.Zero, Vector3.Up, new Vector2(0.5f, 0.5f)));
            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(MathF.Cos(a) * radius, 0, MathF.Sin(a) * radius), Vector3.Up, Vector2.Zero));
            }
            for (int i = 0; i < segs; i++)
            {
                MeshGeometryHelper.AddTopCapTri(indices, topCenter, (ushort)i);
            }

            // Bottom knee clevis cap (y = -length)
            ushort botCenter = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(new Vector3(0, -length, 0), Vector3.Down, new Vector2(0.5f, 0.5f)));
            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(MathF.Cos(a) * radius, -length, MathF.Sin(a) * radius), Vector3.Down, Vector2.Zero));
            }
            for (int i = 0; i < segs; i++)
            {
                MeshGeometryHelper.AddBottomCapTri(indices, botCenter, (ushort)i);
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
