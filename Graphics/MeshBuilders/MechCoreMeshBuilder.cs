using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.MeshBuilders
{
    public static class MechCoreMeshBuilder
    {
        public static SimpleMesh CreatePelvisAssembly(GraphicsDevice device, float width = 1.10f, float height = 0.95f, float depth = 0.80f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal) =>
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);

            float hw = width * 0.5f, hh = height * 0.5f, hd = depth * 0.5f;

            // 1. Faceted Angular Groin Prow (Center prow peak angled forward)
            float prowZ = hd + 0.08f;
            Vector3 fBotMid = new Vector3(0, -hh, prowZ);
            Vector3 fTopMid = new Vector3(0, hh, hd * 0.9f);
            Vector3 fBotL = new Vector3(-hw * 0.85f, -hh, hd * 0.6f);
            Vector3 fBotR = new Vector3(hw * 0.85f, -hh, hd * 0.6f);
            Vector3 fTopL = new Vector3(-hw * 0.95f, hh, hd * 0.85f);
            Vector3 fTopR = new Vector3(hw * 0.95f, hh, hd * 0.85f);

            // Left & Right groin prow deflection facets
            Vector3 nGroinL = Vector3.Normalize(new Vector3(-0.35f, 0.1f, 1f));
            Vector3 nGroinR = Vector3.Normalize(new Vector3(0.35f, 0.1f, 1f));
            AddQuad(fBotL, fBotMid, fTopMid, fTopL, nGroinL);
            AddQuad(fBotMid, fBotR, fTopR, fTopMid, nGroinR);

            // 2. Rear Sacral Bulkhead
            Vector3 bBotL = new Vector3(-hw * 0.80f, -hh, -hd);
            Vector3 bBotR = new Vector3(hw * 0.80f, -hh, -hd);
            Vector3 bTopL = new Vector3(-hw * 0.90f, hh, -hd);
            Vector3 bTopR = new Vector3(hw * 0.90f, hh, -hd);

            AddQuad(bBotR, bBotL, bTopL, bTopR, Vector3.Forward);

            // 3. Left & Right Femoral Gimbal Housing Flanks
            AddQuad(bBotL, fBotL, fTopL, bTopL, Vector3.Left);
            AddQuad(fBotR, bBotR, bTopR, fTopR, Vector3.Right);

            // 4. Top Traversing Deck & Bottom Ventral Plate
            AddQuad(fTopL, fTopR, bTopR, bTopL, Vector3.Up);
            AddQuad(bBotL, bBotR, fBotR, fBotL, Vector3.Down);

            // 5. Left & Right Cylindrical Femoral Socket Hubs
            int hubSegs = 12;
            float hubR = 0.16f, hubLen = 0.12f;
            for (int s = 0; s < 2; s++)
            {
                float sideSign = (s == 0) ? -1f : 1f;
                Vector3 hubCenter = new Vector3(sideSign * hw, -hh * 0.2f, 0);

                for (int i = 0; i < hubSegs; i++)
                {
                    float a1 = (float)i / hubSegs * MathF.PI * 2f;
                    float a2 = (float)(i + 1) / hubSegs * MathF.PI * 2f;
                    float ca1 = MathF.Cos(a1), sa1 = MathF.Sin(a1);
                    float ca2 = MathF.Cos(a2), sa2 = MathF.Sin(a2);

                    Vector3 p0 = hubCenter + new Vector3(0, ca1 * hubR, sa1 * hubR);
                    Vector3 p1 = hubCenter + new Vector3(sideSign * hubLen, ca1 * hubR, sa1 * hubR);
                    Vector3 p2 = hubCenter + new Vector3(sideSign * hubLen, ca2 * hubR, sa2 * hubR);
                    Vector3 p3 = hubCenter + new Vector3(0, ca2 * hubR, sa2 * hubR);

                    Vector3 nHub = Vector3.Normalize(new Vector3(0, (ca1 + ca2) * 0.5f, (sa1 + sa2) * 0.5f));
                    AddQuad(p0, p1, p2, p3, nHub);

                    // Outer hub cap
                    Vector3 capCenter = hubCenter + new Vector3(sideSign * hubLen, 0, 0);
                    Vector3 nCap = new Vector3(sideSign, 0, 0);
                    if (sideSign > 0)
                        MeshGeometryHelper.AddTri(vertices, indices, capCenter, p1, p2, nCap);
                    else
                        MeshGeometryHelper.AddTri(vertices, indices, capCenter, p2, p1, nCap);
                }
            }

            return BuildMesh(device, vertices, indices);
        }

        public static SimpleMesh CreateSpineBulkhead(GraphicsDevice device, float width = 1.35f, float height = 1.35f, float depth = 0.65f)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal) =>
                MeshGeometryHelper.AddQuad(vertices, indices, p0, p1, p2, p3, normal);

            float hw = width * 0.5f, hh = height * 0.5f, hd = depth * 0.5f;

            // 1. Reinforced Spinal Column Frame
            Vector3 f0 = new Vector3(-hw * 0.85f, -hh, hd * 0.8f);
            Vector3 f1 = new Vector3(hw * 0.85f, -hh, hd * 0.8f);
            Vector3 f2 = new Vector3(hw, hh, hd * 0.6f);
            Vector3 f3 = new Vector3(-hw, hh, hd * 0.6f);

            Vector3 b0 = new Vector3(-hw * 0.80f, -hh, -hd);
            Vector3 b1 = new Vector3(hw * 0.80f, -hh, -hd);
            Vector3 b2 = new Vector3(hw * 0.90f, hh, -hd * 0.9f);
            Vector3 b3 = new Vector3(-hw * 0.90f, hh, -hd * 0.9f);

            AddQuad(f0, f1, f2, f3, Vector3.Backward);
            AddQuad(b1, b0, b3, b2, Vector3.Forward);
            AddQuad(f3, f2, b2, b3, Vector3.Up);
            AddQuad(b0, b1, f1, f0, Vector3.Down);
            AddQuad(b0, f0, f3, b3, Vector3.Left);
            AddQuad(f1, b1, b2, f2, Vector3.Right);

            // 2. Dual Clavicle Heavy Lifting Lugs (Top Left & Right for Gantry Slings)
            for (int s = 0; s < 2; s++)
            {
                float sideSign = (s == 0) ? -1f : 1f;
                Vector3 lugPos = new Vector3(sideSign * hw * 0.75f, hh + 0.08f, 0.05f);
                float lugR = 0.045f, lugThick = 0.028f;

                int segs = 10;
                for (int i = 0; i < segs; i++)
                {
                    float a1 = (float)i / segs * MathF.PI * 2f;
                    float a2 = (float)(i + 1) / segs * MathF.PI * 2f;
                    float ca1 = MathF.Cos(a1), sa1 = MathF.Sin(a1);
                    float ca2 = MathF.Cos(a2), sa2 = MathF.Sin(a2);

                    Vector3 p0 = lugPos + new Vector3(-lugThick * 0.5f, ca1 * lugR, sa1 * lugR);
                    Vector3 p1 = lugPos + new Vector3(lugThick * 0.5f, ca1 * lugR, sa1 * lugR);
                    Vector3 p2 = lugPos + new Vector3(lugThick * 0.5f, ca2 * lugR, sa2 * lugR);
                    Vector3 p3 = lugPos + new Vector3(-lugThick * 0.5f, ca2 * lugR, sa2 * lugR);

                    Vector3 nLug = Vector3.Normalize(new Vector3(0, (ca1 + ca2) * 0.5f, (sa1 + sa2) * 0.5f));
                    AddQuad(p0, p1, p2, p3, nLug);

                    // Outer cheek & inner cheek
                    MeshGeometryHelper.AddTri(vertices, indices, lugPos + new Vector3(lugThick * 0.5f, 0, 0), p1, p2, Vector3.Right);
                    MeshGeometryHelper.AddTri(vertices, indices, lugPos + new Vector3(-lugThick * 0.5f, 0, 0), p3, p0, Vector3.Left);
                }
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
