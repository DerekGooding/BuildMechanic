using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class HangarEquipmentMeshBuilder
    {
        public static SimpleMesh CreateDetailedBenchVise(GraphicsDevice device)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal) =>
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);

            void AddBox(Vector3 center, Vector3 size, Vector3 normalScale)
            {
                float hw = size.X * 0.5f, hh = size.Y * 0.5f, hd = size.Z * 0.5f;
                Vector3 f0 = center + new Vector3(-hw, -hh, hd);
                Vector3 f1 = center + new Vector3(hw, -hh, hd);
                Vector3 f2 = center + new Vector3(hw, hh, hd);
                Vector3 f3 = center + new Vector3(-hw, hh, hd);

                Vector3 b0 = center + new Vector3(-hw, -hh, -hd);
                Vector3 b1 = center + new Vector3(hw, -hh, -hd);
                Vector3 b2 = center + new Vector3(hw, hh, -hd);
                Vector3 b3 = center + new Vector3(-hw, hh, -hd);

                AddQuad(f0, f1, f2, f3, Vector3.Backward);
                AddQuad(b1, b0, b3, b2, Vector3.Forward);
                AddQuad(f3, f2, b2, b3, Vector3.Up);
                AddQuad(b0, b1, f1, f0, Vector3.Down);
                AddQuad(b0, f0, f3, b3, Vector3.Left);
                AddQuad(f1, b1, b2, f2, Vector3.Right);
            }

            // 1. Heavy Cast-Iron Base Plate with Table Mounting Flange
            AddBox(new Vector3(0, 0.015f, 0), new Vector3(0.18f, 0.03f, 0.22f), Vector3.One);

            // 2. Fixed Rear Jaw Tower & Anvil Horn
            AddBox(new Vector3(0, 0.08f, -0.04f), new Vector3(0.14f, 0.10f, 0.10f), Vector3.One);
            // Rear Anvil Flat
            AddBox(new Vector3(0, 0.09f, -0.11f), new Vector3(0.08f, 0.06f, 0.07f), Vector3.One);

            // 3. Sliding Dynamic Front Jaw Carriage
            AddBox(new Vector3(0, 0.08f, 0.06f), new Vector3(0.14f, 0.10f, 0.10f), Vector3.One);
            // Sliding Guide Beam underneath
            AddBox(new Vector3(0, 0.04f, 0.02f), new Vector3(0.07f, 0.04f, 0.16f), Vector3.One);

            // 4. Hardened Serrated Jaw Clamping Inserts (Meeting at z = 0.01)
            AddBox(new Vector3(0, 0.11f, 0.008f), new Vector3(0.138f, 0.035f, 0.012f), Vector3.One);
            AddBox(new Vector3(0, 0.11f, 0.018f), new Vector3(0.138f, 0.035f, 0.012f), Vector3.One);

            // 5. Acme Threaded Lead Screw Spindle & Tommy Bar Handle
            int segs = 10;
            float spindleR = 0.014f, spindleLen = 0.06f;
            Vector3 spindleStart = new Vector3(0, 0.06f, 0.11f);
            for (int i = 0; i < segs; i++)
            {
                float a1 = (float)i / segs * MathF.PI * 2f;
                float a2 = (float)(i + 1) / segs * MathF.PI * 2f;
                float ca1 = MathF.Cos(a1), sa1 = MathF.Sin(a1);
                float ca2 = MathF.Cos(a2), sa2 = MathF.Sin(a2);

                Vector3 p0 = spindleStart + new Vector3(ca1 * spindleR, sa1 * spindleR, 0);
                Vector3 p1 = spindleStart + new Vector3(ca2 * spindleR, sa2 * spindleR, 0);
                Vector3 p2 = spindleStart + new Vector3(ca2 * spindleR, sa2 * spindleR, spindleLen);
                Vector3 p3 = spindleStart + new Vector3(ca1 * spindleR, sa1 * spindleR, spindleLen);
                Vector3 n = Vector3.Normalize(new Vector3((ca1 + ca2) * 0.5f, (sa1 + sa2) * 0.5f, 0));
                AddQuad(p0, p1, p2, p3, n);
            }

            // Tommy Bar Handle passing horizontally through spindle head
            AddBox(new Vector3(0, 0.06f, 0.165f), new Vector3(0.18f, 0.012f, 0.012f), Vector3.One);
            // Handle End Ball Knobs
            AddBox(new Vector3(-0.095f, 0.06f, 0.165f), new Vector3(0.018f, 0.018f, 0.018f), Vector3.One);
            AddBox(new Vector3(0.095f, 0.06f, 0.165f), new Vector3(0.018f, 0.018f, 0.018f), Vector3.One);

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateCRTTerminal(GraphicsDevice device)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, Vector2? uv0 = null, Vector2? uv1 = null, Vector2? uv2 = null, Vector2? uv3 = null) =>
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal,
                    uv0 ?? new Vector2(0, 1), uv1 ?? new Vector2(1, 1), uv2 ?? new Vector2(1, 0), uv3 ?? new Vector2(0, 0));

            // 1. Swivel Desktop Pedestal Base
            float pw = 0.14f, ph = 0.025f, pd = 0.14f;
            Vector3 pb0 = new Vector3(-pw * 0.5f, 0, pd * 0.5f);
            Vector3 pb1 = new Vector3(pw * 0.5f, 0, pd * 0.5f);
            Vector3 pb2 = new Vector3(pw * 0.5f, ph, pd * 0.5f);
            Vector3 pb3 = new Vector3(-pw * 0.5f, ph, pd * 0.5f);

            Vector3 pr0 = new Vector3(-pw * 0.45f, 0, -pd * 0.5f);
            Vector3 pr1 = new Vector3(pw * 0.45f, 0, -pd * 0.5f);
            Vector3 pr2 = new Vector3(pw * 0.45f, ph, -pd * 0.5f);
            Vector3 pr3 = new Vector3(-pw * 0.45f, ph, -pd * 0.5f);

            AddQuad(pb0, pb1, pb2, pb3, Vector3.Backward);
            AddQuad(pr1, pr0, pr3, pr2, Vector3.Forward);
            AddQuad(pb3, pb2, pr2, pr3, Vector3.Up);
            AddQuad(pr0, pr1, pb1, pb0, Vector3.Down);
            AddQuad(pr0, pb0, pb3, pr3, Vector3.Left);
            AddQuad(pb1, pr1, pr2, pb2, Vector3.Right);

            // 2. Main CRT Console Casing (Tapered toward rear with forward display hood)
            float cw = 0.28f, ch = 0.22f, cd = 0.22f, yOff = 0.035f;
            Vector3 f0 = new Vector3(-cw * 0.5f, yOff, cd * 0.5f);
            Vector3 f1 = new Vector3(cw * 0.5f, yOff, cd * 0.5f);
            Vector3 f2 = new Vector3(cw * 0.5f, yOff + ch, cd * 0.5f);
            Vector3 f3 = new Vector3(-cw * 0.5f, yOff + ch, cd * 0.5f);

            float tr = 0.78f; // rear taper ratio
            Vector3 b0 = new Vector3(-cw * 0.5f * tr, yOff + 0.02f, -cd * 0.5f);
            Vector3 b1 = new Vector3(cw * 0.5f * tr, yOff + 0.02f, -cd * 0.5f);
            Vector3 b2 = new Vector3(cw * 0.5f * tr, yOff + ch * 0.95f, -cd * 0.5f);
            Vector3 b3 = new Vector3(-cw * 0.5f * tr, yOff + ch * 0.95f, -cd * 0.5f);

            // Top, Bottom, Sides, Rear Casing
            AddQuad(f3, f2, b2, b3, Vector3.Up);
            AddQuad(b0, b1, f1, f0, Vector3.Down);
            AddQuad(b0, f0, f3, b3, Vector3.Left);
            AddQuad(f1, b1, b2, f2, Vector3.Right);
            AddQuad(b1, b0, b3, b2, Vector3.Forward);

            // 3. Recessed Cathode-Ray Tube Screen Bezel
            float bezel = 0.035f, screenRecess = 0.016f;
            Vector3 s0 = new Vector3(f0.X + bezel, f0.Y + bezel + 0.03f, f0.Z - screenRecess);
            Vector3 s1 = new Vector3(f1.X - bezel, f1.Y + bezel + 0.03f, f1.Z - screenRecess);
            Vector3 s2 = new Vector3(f2.X - bezel, f2.Y - bezel, f2.Z - screenRecess);
            Vector3 s3 = new Vector3(f3.X + bezel, f3.Y - bezel, f3.Z - screenRecess);

            // Anti-glare hooded bezel walls
            AddQuad(f0, f1, s1, s0, Vector3.Normalize(new Vector3(0, 0.4f, 1)));
            AddQuad(s3, s2, f2, f3, Vector3.Normalize(new Vector3(0, -0.4f, 1)));
            AddQuad(f0, s0, s3, f3, Vector3.Normalize(new Vector3(0.4f, 0, 1)));
            AddQuad(s1, f1, f2, s2, Vector3.Normalize(new Vector3(-0.4f, 0, 1)));

            // Phosphor CRT Display Face (UV-mapped for diagnostic screen texture)
            AddQuad(s0, s1, s2, s3, Vector3.Backward,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0));

            // 4. Lower Control Fascia with Rotary Dials & Power Switch
            Vector3 cFascia0 = new Vector3(f0.X + bezel, f0.Y + 0.008f, f0.Z);
            Vector3 cFascia1 = new Vector3(f1.X - bezel, f1.Y + 0.008f, f1.Z);
            Vector3 cFascia2 = new Vector3(f1.X - bezel, f0.Y + bezel + 0.02f, f0.Z);
            Vector3 cFascia3 = new Vector3(f0.X + bezel, f0.Y + bezel + 0.02f, f0.Z);
            AddQuad(cFascia0, cFascia1, cFascia2, cFascia3, Vector3.Backward);

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateArticulatedTaskLamp(GraphicsDevice device)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal) =>
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);

            void AddBox(Vector3 center, Vector3 size)
            {
                float hw = size.X * 0.5f, hh = size.Y * 0.5f, hd = size.Z * 0.5f;
                Vector3 f0 = center + new Vector3(-hw, -hh, hd);
                Vector3 f1 = center + new Vector3(hw, -hh, hd);
                Vector3 f2 = center + new Vector3(hw, hh, hd);
                Vector3 f3 = center + new Vector3(-hw, hh, hd);

                Vector3 b0 = center + new Vector3(-hw, -hh, -hd);
                Vector3 b1 = center + new Vector3(hw, -hh, -hd);
                Vector3 b2 = center + new Vector3(hw, hh, -hd);
                Vector3 b3 = center + new Vector3(-hw, hh, -hd);

                AddQuad(f0, f1, f2, f3, Vector3.Backward);
                AddQuad(b1, b0, b3, b2, Vector3.Forward);
                AddQuad(f3, f2, b2, b3, Vector3.Up);
                AddQuad(b0, b1, f1, f0, Vector3.Down);
                AddQuad(b0, f0, f3, b3, Vector3.Left);
                AddQuad(f1, b1, b2, f2, Vector3.Right);
            }

            // 1. Bench Edge C-Clamp Mounting Bracket
            AddBox(new Vector3(0, 0.04f, 0), new Vector3(0.045f, 0.08f, 0.045f));
            AddBox(new Vector3(0, 0.01f, 0.03f), new Vector3(0.040f, 0.02f, 0.05f));
            AddBox(new Vector3(0, 0.07f, 0.03f), new Vector3(0.040f, 0.02f, 0.05f));

            // 2. Lower Parallel Pantograph Arms (Swiveling upward from base to elbow)
            Vector3 basePivot = new Vector3(0, 0.09f, 0);
            Vector3 elbowPivot = new Vector3(0, 0.32f, 0.12f);
            AddBox((basePivot + elbowPivot) * 0.5f + new Vector3(-0.014f, 0, 0), new Vector3(0.010f, 0.24f, 0.010f));
            AddBox((basePivot + elbowPivot) * 0.5f + new Vector3(0.014f, 0, 0), new Vector3(0.010f, 0.24f, 0.010f));

            // 3. Articulated Elbow Knuckle Joint
            AddBox(elbowPivot, new Vector3(0.045f, 0.025f, 0.025f));

            // 4. Upper Pantograph Arm extending forward over workpiece
            Vector3 wristPivot = elbowPivot + new Vector3(0, 0.06f, 0.24f);
            AddBox((elbowPivot + wristPivot) * 0.5f + new Vector3(-0.012f, 0, 0), new Vector3(0.009f, 0.010f, 0.24f));
            AddBox((elbowPivot + wristPivot) * 0.5f + new Vector3(0.012f, 0, 0), new Vector3(0.009f, 0.010f, 0.24f));

            // 5. Lamp Shade Bell Cowl & Bulb (Angled down ~45 deg)
            int shadeSegs = 14;
            float topR = 0.030f, botR = 0.095f, shadeLen = 0.10f;
            for (int i = 0; i < shadeSegs; i++)
            {
                float a1 = (float)i / shadeSegs * MathF.PI * 2f;
                float a2 = (float)(i + 1) / shadeSegs * MathF.PI * 2f;
                float ca1 = MathF.Cos(a1), sa1 = MathF.Sin(a1);
                float ca2 = MathF.Cos(a2), sa2 = MathF.Sin(a2);

                Vector3 t0 = wristPivot + new Vector3(ca1 * topR, 0, sa1 * topR);
                Vector3 t1 = wristPivot + new Vector3(ca2 * topR, 0, sa2 * topR);
                Vector3 b0 = wristPivot + new Vector3(ca1 * botR, -shadeLen, sa1 * botR);
                Vector3 b1 = wristPivot + new Vector3(ca2 * botR, -shadeLen, sa2 * botR);

                Vector3 n = Vector3.Normalize(new Vector3((ca1 + ca2) * 0.5f, 0.35f, (sa1 + sa2) * 0.5f));
                // Double-sided cone so visible outside and inside
                AddQuad(b0, b1, t1, t0, n);
                AddQuad(t0, t1, b1, b0, -n);
            }

            // Top Lamp Switch Cap
            AddBox(wristPivot + new Vector3(0, 0.02f, 0), new Vector3(0.035f, 0.035f, 0.035f));
            // Glowing Halogen Bulb Core
            AddBox(wristPivot + new Vector3(0, -0.04f, 0), new Vector3(0.024f, 0.035f, 0.024f));

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
