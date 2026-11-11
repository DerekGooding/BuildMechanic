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

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal) =>
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);

            // 1. Top Swivel Shackle Eye (y: 0.14 to 0.22)
            int eyeSegs = 12;
            float eyeOuterR = 0.038f, eyeInnerR = 0.018f, eyeHz = 0.015f;
            Vector3 eyeCenter = new Vector3(0, 0.175f, 0);

            for (int i = 0; i < eyeSegs; i++)
            {
                float a1 = (float)i / eyeSegs * MathF.PI * 2f;
                float a2 = (float)(i + 1) / eyeSegs * MathF.PI * 2f;
                float ca1 = MathF.Cos(a1), sa1 = MathF.Sin(a1);
                float ca2 = MathF.Cos(a2), sa2 = MathF.Sin(a2);

                Vector3 of1 = eyeCenter + new Vector3(ca1 * eyeOuterR, sa1 * eyeOuterR, eyeHz);
                Vector3 of2 = eyeCenter + new Vector3(ca2 * eyeOuterR, sa2 * eyeOuterR, eyeHz);
                Vector3 if1 = eyeCenter + new Vector3(ca1 * eyeInnerR, sa1 * eyeInnerR, eyeHz);
                Vector3 if2 = eyeCenter + new Vector3(ca2 * eyeInnerR, sa2 * eyeInnerR, eyeHz);

                Vector3 ob1 = eyeCenter + new Vector3(ca1 * eyeOuterR, sa1 * eyeOuterR, -eyeHz);
                Vector3 ob2 = eyeCenter + new Vector3(ca2 * eyeOuterR, sa2 * eyeOuterR, -eyeHz);
                Vector3 ib1 = eyeCenter + new Vector3(ca1 * eyeInnerR, sa1 * eyeInnerR, -eyeHz);
                Vector3 ib2 = eyeCenter + new Vector3(ca2 * eyeInnerR, sa2 * eyeInnerR, -eyeHz);

                AddQuad(if1, of1, of2, if2, Vector3.Backward);
                AddQuad(ob1, ib1, ib2, ob2, Vector3.Forward);

                Vector3 nOuter = Vector3.Normalize(new Vector3((ca1 + ca2) * 0.5f, (sa1 + sa2) * 0.5f, 0));
                AddQuad(of1, ob1, ob2, of2, nOuter);
                AddQuad(ib1, if1, if2, ib2, -nOuter);
            }

            // 2. Swivel Thrust Bearing Collar (y: 0.08 to 0.14)
            int collarSegs = 16;
            float collarR = 0.032f, collarY0 = 0.08f, collarY1 = 0.14f;
            for (int i = 0; i < collarSegs; i++)
            {
                float a1 = (float)i / collarSegs * MathF.PI * 2f;
                float a2 = (float)(i + 1) / collarSegs * MathF.PI * 2f;
                float ca1 = MathF.Cos(a1), sa1 = MathF.Sin(a1);
                float ca2 = MathF.Cos(a2), sa2 = MathF.Sin(a2);

                Vector3 c0 = new Vector3(ca1 * collarR, collarY0, sa1 * collarR);
                Vector3 c1 = new Vector3(ca2 * collarR, collarY0, sa2 * collarR);
                Vector3 c2 = new Vector3(ca2 * collarR, collarY1, sa2 * collarR);
                Vector3 c3 = new Vector3(ca1 * collarR, collarY1, sa1 * collarR);
                Vector3 nC = Vector3.Normalize(new Vector3((ca1 + ca2) * 0.5f, 0, (sa1 + sa2) * 0.5f));
                AddQuad(c0, c1, c2, c3, nC);

                Vector3 topIn1 = new Vector3(ca1 * eyeInnerR, collarY1, sa1 * eyeInnerR);
                Vector3 topIn2 = new Vector3(ca2 * eyeInnerR, collarY1, sa2 * eyeInnerR);
                AddQuad(topIn1, c3, c2, topIn2, Vector3.Up);

                Vector3 botIn1 = new Vector3(ca1 * 0.018f, collarY0, sa1 * 0.018f);
                Vector3 botIn2 = new Vector3(ca2 * 0.018f, collarY0, sa2 * 0.018f);
                AddQuad(c0, botIn1, botIn2, c1, Vector3.Down);
            }

            // 3. Heavy Forged Anatomical Hook Body with Load Saddle & Taper
            int hookSegs = 16;
            float hookArc = MathF.PI * 1.38f;

            Vector3 rootIf = Vector3.Zero, rootOf = Vector3.Zero, rootOb = Vector3.Zero, rootIb = Vector3.Zero;
            Vector3 tipIf = Vector3.Zero, tipOf = Vector3.Zero, tipOb = Vector3.Zero, tipIb = Vector3.Zero;

            for (int i = 0; i < hookSegs; i++)
            {
                float t1 = (float)i / hookSegs;
                float t2 = (float)(i + 1) / hookSegs;
                float a1 = t1 * hookArc;
                float a2 = t2 * hookArc;

                float saddleWeight1 = MathF.Sin(t1 * MathF.PI);
                float saddleWeight2 = MathF.Sin(t2 * MathF.PI);

                float rIn1 = 0.090f - saddleWeight1 * 0.010f;
                float rOut1 = 0.142f + saddleWeight1 * 0.024f;
                float hz1 = 0.018f + saddleWeight1 * 0.014f;

                float rIn2 = 0.090f - saddleWeight2 * 0.010f;
                float rOut2 = 0.142f + saddleWeight2 * 0.024f;
                float hz2 = 0.018f + saddleWeight2 * 0.014f;

                float ca1 = MathF.Cos(a1), sa1 = MathF.Sin(a1);
                float ca2 = MathF.Cos(a2), sa2 = MathF.Sin(a2);

                Vector3 if1 = new Vector3(ca1 * rIn1, -sa1 * rIn1, hz1);
                Vector3 of1 = new Vector3(ca1 * rOut1, -sa1 * rOut1, hz1);
                Vector3 if2 = new Vector3(ca2 * rIn2, -sa2 * rIn2, hz2);
                Vector3 of2 = new Vector3(ca2 * rOut2, -sa2 * rOut2, hz2);

                Vector3 ib1 = new Vector3(ca1 * rIn1, -sa1 * rIn1, -hz1);
                Vector3 ob1 = new Vector3(ca1 * rOut1, -sa1 * rOut1, -hz1);
                Vector3 ib2 = new Vector3(ca2 * rIn2, -sa2 * rIn2, -hz2);
                Vector3 ob2 = new Vector3(ca2 * rOut2, -sa2 * rOut2, -hz2);

                if (i == 0) { rootIf = if1; rootOf = of1; rootOb = ob1; rootIb = ib1; }
                if (i == hookSegs - 1) { tipIf = if2; tipOf = of2; tipOb = ob2; tipIb = ib2; }

                AddQuad(if1, of1, of2, if2, Vector3.Backward);
                AddQuad(ob1, ib1, ib2, ob2, Vector3.Forward);

                Vector3 nOuter = Vector3.Normalize(new Vector3((ca1 + ca2) * 0.5f, -(sa1 + sa2) * 0.5f, 0));
                AddQuad(of1, ob1, ob2, of2, nOuter);
                MeshGeometryHelper.AddQuad(vertices, indices, ib1, if1, if2, ib2, -nOuter);
            }

            // Closed hook root cap (connecting to shank)
            AddQuad(rootIb, rootOb, rootOf, rootIf, Vector3.Up);

            // Closed hook tip horn nose cap
            Vector3 tipNormal = Vector3.Normalize(new Vector3(tipOf.X - tipIf.X, tipOf.Y - tipIf.Y, 0));
            Vector3 tipDir = Vector3.Normalize(Vector3.Cross(tipNormal, Vector3.Backward));
            AddQuad(tipIf, tipOf, tipOb, tipIb, tipDir);

            // 4. Spring-Loaded Stamped Steel Safety Latch (Clevis Tongue)
            Vector3 latchBase = new Vector3(0.065f, 0.020f, 0);
            Vector3 latchTip = new Vector3(tipIf.X + 0.010f, tipIf.Y + 0.005f, 0);
            float latchW = 0.005f, latchHz = 0.006f;
            Vector3 latchUp = Vector3.Normalize(new Vector3(-(latchTip.Y - latchBase.Y), latchTip.X - latchBase.X, 0)) * latchW;

            Vector3 l0 = latchBase - latchUp + new Vector3(0, 0, latchHz);
            Vector3 l1 = latchTip - latchUp + new Vector3(0, 0, latchHz);
            Vector3 l2 = latchTip + latchUp + new Vector3(0, 0, latchHz);
            Vector3 l3 = latchBase + latchUp + new Vector3(0, 0, latchHz);

            Vector3 lb0 = latchBase - latchUp - new Vector3(0, 0, latchHz);
            Vector3 lb1 = latchTip - latchUp - new Vector3(0, 0, latchHz);
            Vector3 lb2 = latchTip + latchUp - new Vector3(0, 0, latchHz);
            Vector3 lb3 = latchBase + latchUp - new Vector3(0, 0, latchHz);

            AddQuad(l0, l1, l2, l3, Vector3.Backward);
            AddQuad(lb3, lb2, lb1, lb0, Vector3.Forward);
            AddQuad(l3, l2, lb2, lb3, Vector3.Normalize(latchUp));
            AddQuad(l1, l0, lb0, lb1, -Vector3.Normalize(latchUp));

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateLampShade(GraphicsDevice device, float topR = 0.03f, float botR = 0.11f, float height = 0.12f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            int segs = 16;

            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                float ca = MathF.Cos(a), sa = MathF.Sin(a);
                Vector3 n = new Vector3(ca, 0.4f, sa);

                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * botR, -height * 0.5f, sa * botR), n, new Vector2((float)i / segs, 1)));
                vertices.Add(new VertexPositionNormalTexture(new Vector3(ca * topR, height * 0.5f, sa * topR), n, new Vector2((float)i / segs, 0)));
            }

            for (int i = 0; i < segs; i++)
            {
                ushort b = (ushort)(i * 2);
                // Double-sided wall ensures visible both from above and from underneath
                MeshGeometryHelper.AddDoubleSidedCylinderWall(indices, b);
            }

            // Top socket cap (+Y)
            ushort topCenter = (ushort)vertices.Count;
            vertices.Add(new VertexPositionNormalTexture(new Vector3(0, height * 0.5f, 0), Vector3.Up, new Vector2(0.5f, 0.5f)));
            for (int i = 0; i <= segs; i++)
            {
                float a = (float)i / segs * MathF.PI * 2f;
                vertices.Add(new VertexPositionNormalTexture(new Vector3(MathF.Cos(a) * topR, height * 0.5f, MathF.Sin(a) * topR), Vector3.Up, Vector2.Zero));
            }
            for (int i = 0; i < segs; i++)
            {
                MeshGeometryHelper.AddTopCapTri(indices, topCenter, (ushort)i);
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateBenchVise(GraphicsDevice device)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            // Heavy duty cast iron workshop bench vise with serrated jaws
            Vector3 size = new Vector3(0.18f, 0.14f, 0.24f);
            float hw = size.X * 0.5f, hh = size.Y * 0.5f, hd = size.Z * 0.5f;

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal)
            {
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);
            }

            Vector3 f0 = new Vector3(-hw, -hh, hd);
            Vector3 f1 = new Vector3(hw, -hh, hd);
            Vector3 f2 = new Vector3(hw, hh, hd);
            Vector3 f3 = new Vector3(-hw, hh, hd);

            Vector3 b0 = new Vector3(-hw, -hh, -hd);
            Vector3 b1 = new Vector3(hw, -hh, -hd);
            Vector3 b2 = new Vector3(hw, hh, -hd);
            Vector3 b3 = new Vector3(-hw, hh, -hd);

            AddQuad(f0, f1, f2, f3, Vector3.Backward);
            AddQuad(b1, b0, b3, b2, Vector3.Forward);
            AddQuad(f3, f2, b2, b3, Vector3.Up);
            AddQuad(b0, b1, f1, f0, Vector3.Down);
            AddQuad(b0, f0, f3, b3, Vector3.Left);
            AddQuad(f1, b1, b2, f2, Vector3.Right);

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
