using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics.MeshBuilders;
using BuildMechanic.Graphics.Textures;
using BuildMechanic.Tools;

namespace BuildMechanic.Graphics
{
    public class ToolRenderer : IDisposable
    {
        private SimpleMesh _socketMesh;
        private SimpleMesh _grinderDiscMesh;
        private SimpleMesh _pryBarMesh;
        private SimpleMesh _welderShroudMesh;
        private SimpleMesh _sprayCupMesh;
        private Texture2D _metalTex;
        private Texture2D _gripTex;
        private Texture2D _gaugeTex;

        public void Initialize(GraphicsDevice device)
        {
            _socketMesh = ToolMeshBuilder.CreateImpactSocket(device);
            _grinderDiscMesh = ToolMeshBuilder.CreateGrinderDisc(device);
            _pryBarMesh = ToolMeshBuilder.CreatePryBarShaft(device);
            _welderShroudMesh = ToolMeshBuilder.CreateWelderShroud(device);
            _sprayCupMesh = ToolMeshBuilder.CreateSprayCup(device);

            _metalTex = ToolTextureGenerator.CreateBrushedMetalTexture(device, 256);
            _gripTex = ToolTextureGenerator.CreateRubberGripTexture(device, 128);
            _gaugeTex = ToolTextureGenerator.CreatePressureGaugeTexture(device, 128);
        }

        public void DrawActiveTool(GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl, Texture2D chassisTex, ToolManager tools)
        {
            if (_socketMesh == null) Initialize(dev);
            if (tools.ActiveTool == ToolType.Scanner) return;
            if (tools.HoveredPlate == null && tools.HoveredBolt == null && tools.HoveredCrack == null &&
                tools.HoveredHydraulic == null && tools.HoveredRelay == null) return;

            Vector3 hit = tools.HitPoint;
            if (hit == Vector3.Zero) return;

            switch (tools.ActiveTool)
            {
                case ToolType.ImpactDriver:
                    DrawImpactDriver(dev, eff, box, cyl, tools, hit);
                    break;
                case ToolType.PryBar:
                    DrawPryBar(dev, eff, box, hit);
                    break;
                case ToolType.AngleGrinder:
                    DrawAngleGrinder(dev, eff, box, cyl, tools, hit);
                    break;
                case ToolType.Welder:
                    DrawWelder(dev, eff, cyl, tools, hit);
                    break;
                case ToolType.Multimeter:
                    DrawMultimeter(dev, eff, box, cyl, hit);
                    break;
                case ToolType.HydraulicBleeder:
                    DrawHydraulicBleeder(dev, eff, box, cyl, hit);
                    break;
                case ToolType.PaintGun:
                    DrawPaintGun(dev, eff, box, cyl, tools, hit);
                    break;
            }
        }

        private void DrawImpactDriver(GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl, ToolManager tools, Vector3 hit)
        {
            float recoil = tools.IsOperating ? (float)Math.Sin(DateTime.Now.Millisecond * 0.12) * 0.007f : 0f;
            Vector3 bitPos = hit + new Vector3(0, 0, 0.04f + recoil);
            Vector3 bodyPos = hit + new Vector3(0, 0, 0.17f + recoil);
            Vector3 gripPos = bodyPos + new Vector3(0, -0.13f, 0.04f);
            Vector3 battPos = gripPos + new Vector3(0, -0.09f, -0.02f);

            // Chrome impact socket sleeve
            eff.Texture = _metalTex;
            eff.DiffuseColor = new Vector3(0.85f, 0.88f, 0.95f);
            eff.World = Matrix.CreateScale(1.1f, 1f, 1.1f) * Matrix.CreateTranslation(bitPos);
            _socketMesh.Draw(dev, eff);

            // LED Worklight shining on fastener
            eff.DiffuseColor = tools.IsOperating ? new Vector3(2.4f, 2.4f, 2.6f) : new Vector3(0.7f, 0.7f, 0.8f);
            DrawBox(eff, box, dev, bodyPos + new Vector3(0, 0.055f, -0.07f), new Vector3(0.015f, 0.015f, 0.015f));

            // High-torque industrial motor body (high-vis orange)
            eff.Texture = _gripTex;
            eff.DiffuseColor = new Vector3(0.95f, 0.45f, 0.12f);
            DrawBox(eff, box, dev, bodyPos, new Vector3(0.11f, 0.12f, 0.17f));

            // Overmolded rubber grip
            eff.DiffuseColor = new Vector3(0.20f, 0.22f, 0.25f);
            DrawBox(eff, box, dev, gripPos, new Vector3(0.06f, 0.15f, 0.07f));

            // Lithium battery pack base + fuel gauge LED
            eff.DiffuseColor = new Vector3(0.15f, 0.16f, 0.18f);
            DrawBox(eff, box, dev, battPos, new Vector3(0.10f, 0.06f, 0.13f));
            eff.DiffuseColor = new Vector3(0.2f, 1.4f, 0.3f);
            DrawBox(eff, box, dev, battPos + new Vector3(0, 0.035f, 0.05f), new Vector3(0.04f, 0.01f, 0.02f));
        }

        private void DrawPryBar(GraphicsDevice dev, BasicEffect eff, SimpleMesh box, Vector3 hit)
        {
            Vector3 bladePos = hit + new Vector3(0.04f, 0.04f, 0.03f);
            Vector3 rockerPos = hit + new Vector3(0.08f, 0.08f, 0.07f);
            Vector3 handlePos = hit + new Vector3(0.18f, 0.18f, 0.18f);

            eff.Texture = _metalTex;
            eff.DiffuseColor = new Vector3(0.85f, 0.88f, 0.92f);
            DrawBox(eff, box, dev, bladePos, new Vector3(0.04f, 0.08f, 0.02f));

            eff.DiffuseColor = new Vector3(0.35f, 0.55f, 0.85f);
            DrawBox(eff, box, dev, rockerPos, new Vector3(0.05f, 0.08f, 0.05f));

            eff.World = Matrix.CreateRotationX(0.78f) * Matrix.CreateRotationY(0.78f) * Matrix.CreateTranslation(handlePos);
            _pryBarMesh.Draw(dev, eff);
        }

        private void DrawAngleGrinder(GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl, ToolManager tools, Vector3 hit)
        {
            Vector3 discPos = hit + new Vector3(0, 0.01f, 0.02f);
            Vector3 guardPos = hit + new Vector3(0, 0.025f, 0.05f);
            Vector3 gearPos = hit + new Vector3(0.06f, 0.03f, 0.08f);
            Vector3 bodyPos = hit + new Vector3(0.17f, 0.04f, 0.17f);

            // Abrasive grinding disc
            eff.Texture = _metalTex;
            eff.DiffuseColor = tools.IsOperating ? new Vector3(1.2f, 0.8f, 0.4f) : new Vector3(0.35f, 0.32f, 0.30f);
            eff.World = Matrix.CreateTranslation(discPos);
            _grinderDiscMesh.Draw(dev, eff);

            // Steel safety spark guard
            eff.DiffuseColor = new Vector3(0.25f, 0.26f, 0.28f);
            DrawBox(eff, box, dev, guardPos, new Vector3(0.12f, 0.03f, 0.09f));

            // Metal gearhead casing
            eff.DiffuseColor = new Vector3(0.72f, 0.75f, 0.80f);
            DrawBox(eff, box, dev, gearPos, new Vector3(0.09f, 0.07f, 0.09f));

            // Auxiliary side handle
            eff.Texture = _gripTex;
            eff.DiffuseColor = new Vector3(0.20f, 0.22f, 0.25f);
            DrawCylinder(eff, cyl, dev, gearPos + new Vector3(-0.06f, 0, 0), 0.020f, 0.10f);

            // Ergonomic teal grinder body
            eff.DiffuseColor = new Vector3(0.12f, 0.65f, 0.58f);
            DrawCylinder(eff, cyl, dev, bodyPos, 0.042f, 0.22f);
        }

        private void DrawWelder(GraphicsDevice dev, BasicEffect eff, SimpleMesh cyl, ToolManager tools, Vector3 hit)
        {
            Vector3 tipPos = hit + new Vector3(0, 0.015f, 0.02f);
            Vector3 nozzlePos = hit + new Vector3(0, 0.035f, 0.06f);
            Vector3 handlePos = hit + new Vector3(0, 0.12f, 0.20f);

            // Tungsten electrode (white-hot when operating)
            eff.Texture = _metalTex;
            eff.DiffuseColor = tools.IsOperating ? new Vector3(2.5f, 2.5f, 2.8f) : new Vector3(0.6f, 0.6f, 0.65f);
            DrawCylinder(eff, cyl, dev, tipPos, 0.005f, 0.035f);

            // Ceramic pink gas shroud nozzle cup
            eff.DiffuseColor = new Vector3(0.92f, 0.65f, 0.70f);
            eff.World = Matrix.CreateTranslation(nozzlePos);
            _welderShroudMesh.Draw(dev, eff);

            // Ribbed torch handle
            eff.Texture = _gripTex;
            eff.DiffuseColor = new Vector3(0.18f, 0.19f, 0.22f);
            DrawCylinder(eff, cyl, dev, handlePos, 0.032f, 0.18f);
        }

        private void DrawMultimeter(GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl, Vector3 hit)
        {
            Vector3 probePos = hit + new Vector3(0, 0, 0.03f);
            Vector3 meterPos = hit + new Vector3(0.12f, 0.10f, 0.15f);

            eff.Texture = _metalTex;
            eff.DiffuseColor = new Vector3(0.95f, 0.25f, 0.25f);
            DrawCylinder(eff, cyl, dev, probePos, 0.007f, 0.07f);

            eff.Texture = _gripTex;
            eff.DiffuseColor = new Vector3(0.96f, 0.78f, 0.12f);
            DrawBox(eff, box, dev, meterPos, new Vector3(0.09f, 0.15f, 0.045f));

            eff.DiffuseColor = new Vector3(0.2f, 1.3f, 0.4f);
            DrawBox(eff, box, dev, meterPos + new Vector3(0, 0.035f, 0.024f), new Vector3(0.065f, 0.045f, 0.01f));
        }

        private void DrawHydraulicBleeder(GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl, Vector3 hit)
        {
            Vector3 couplerPos = hit + new Vector3(0, 0, 0.02f);
            Vector3 gaugePos = hit + new Vector3(0.07f, 0.04f, 0.07f);
            Vector3 bottlePos = hit + new Vector3(0.08f, -0.08f, 0.12f);

            eff.Texture = _metalTex;
            eff.DiffuseColor = new Vector3(0.25f, 0.55f, 0.95f);
            DrawCylinder(eff, cyl, dev, couplerPos, 0.018f, 0.05f);

            eff.Texture = _gaugeTex;
            eff.DiffuseColor = Vector3.One;
            DrawCylinder(eff, cyl, dev, gaugePos, 0.035f, 0.02f);

            eff.Texture = _metalTex;
            eff.DiffuseColor = new Vector3(0.70f, 0.45f, 0.20f);
            DrawCylinder(eff, cyl, dev, bottlePos, 0.038f, 0.14f);
        }

        private void DrawPaintGun(GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl, ToolManager tools, Vector3 hit)
        {
            Vector3 nozzlePos = hit + new Vector3(0, 0, 0.04f);
            Vector3 cupPos = hit + new Vector3(0, 0.12f, 0.12f);
            Vector3 handlePos = hit + new Vector3(0, -0.08f, 0.12f);

            eff.Texture = _metalTex;
            eff.DiffuseColor = new Vector3(0.85f, 0.88f, 0.92f);
            DrawCylinder(eff, cyl, dev, nozzlePos, 0.016f, 0.06f);

            // Translucent nylon paint cup
            eff.DiffuseColor = ArmorDecal.PaintPalette[tools.ActiveColorIndex].ToVector3();
            eff.World = Matrix.CreateTranslation(cupPos);
            _sprayCupMesh.Draw(dev, eff);

            eff.Texture = _gripTex;
            eff.DiffuseColor = new Vector3(0.22f, 0.24f, 0.26f);
            DrawBox(eff, box, dev, handlePos, new Vector3(0.04f, 0.12f, 0.06f));
        }

        private void DrawBox(BasicEffect eff, SimpleMesh box, GraphicsDevice dev, Vector3 pos, Vector3 size)
        {
            eff.World = Matrix.CreateScale(size) * Matrix.CreateTranslation(pos);
            box.Draw(dev, eff);
        }

        private void DrawCylinder(BasicEffect eff, SimpleMesh cyl, GraphicsDevice dev, Vector3 pos, float r, float h)
        {
            eff.World = Matrix.CreateScale(r * 2f, h, r * 2f) * Matrix.CreateTranslation(pos);
            cyl.Draw(dev, eff);
        }

        public void Dispose()
        {
            _socketMesh?.Dispose();
            _grinderDiscMesh?.Dispose();
            _pryBarMesh?.Dispose();
            _welderShroudMesh?.Dispose();
            _sprayCupMesh?.Dispose();
            _metalTex?.Dispose();
            _gripTex?.Dispose();
            _gaugeTex?.Dispose();
        }
    }
}
