using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Tools;

namespace BuildMechanic.Graphics
{
    public class ToolRenderer
    {
        public void DrawActiveTool(GraphicsDevice device, BasicEffect effect, SimpleMesh boxMesh, SimpleMesh cylinderMesh, Texture2D chassisTex, ToolManager tools)
        {
            if (tools.ActiveTool == ToolType.Scanner) return;
            if (tools.HoveredPlate == null && tools.HoveredBolt == null && tools.HoveredCrack == null &&
                tools.HoveredHydraulic == null && tools.HoveredRelay == null) return;

            Vector3 hit = tools.HitPoint;
            if (hit == Vector3.Zero) return;

            effect.Texture = chassisTex;

            switch (tools.ActiveTool)
            {
                case ToolType.ImpactDriver:
                    DrawImpactDriver(device, effect, boxMesh, cylinderMesh, tools, hit);
                    break;
                case ToolType.PryBar:
                    DrawPryBar(device, effect, boxMesh, hit);
                    break;
                case ToolType.AngleGrinder:
                    DrawAngleGrinder(device, effect, boxMesh, cylinderMesh, hit);
                    break;
                case ToolType.Welder:
                    DrawWelder(device, effect, cylinderMesh, hit);
                    break;
                case ToolType.Multimeter:
                    DrawMultimeter(device, effect, boxMesh, cylinderMesh, hit);
                    break;
                case ToolType.HydraulicBleeder:
                    DrawHydraulicBleeder(device, effect, boxMesh, cylinderMesh, hit);
                    break;
                case ToolType.PaintGun:
                    DrawPaintGun(device, effect, boxMesh, cylinderMesh, tools, hit);
                    break;
            }
        }

        private void DrawImpactDriver(GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl, ToolManager tools, Vector3 hit)
        {
            float recoil = tools.IsOperating ? (float)Math.Sin(DateTime.Now.Millisecond * 0.1) * 0.008f : 0f;
            Vector3 bitPos = hit + new Vector3(0, 0, 0.04f + recoil);
            Vector3 bodyPos = hit + new Vector3(0, 0, 0.16f + recoil);
            Vector3 gripPos = bodyPos + new Vector3(0, -0.12f, 0.04f);

            eff.DiffuseColor = new Vector3(0.3f, 0.3f, 0.35f);
            DrawCylinderMesh(dev, eff, cyl, bitPos, 0.022f, 0.08f);

            eff.DiffuseColor = new Vector3(0.9f, 0.45f, 0.1f);
            DrawBoxMesh(dev, eff, box, bodyPos, new Vector3(0.10f, 0.11f, 0.18f));

            eff.DiffuseColor = new Vector3(0.2f, 0.2f, 0.2f);
            DrawBoxMesh(dev, eff, box, gripPos, new Vector3(0.06f, 0.14f, 0.07f));
        }

        private void DrawPryBar(GraphicsDevice dev, BasicEffect eff, SimpleMesh box, Vector3 hit)
        {
            Vector3 bladePos = hit + new Vector3(0.05f, 0.05f, 0.04f);
            Vector3 handlePos = hit + new Vector3(0.18f, 0.18f, 0.18f);

            eff.DiffuseColor = new Vector3(0.4f, 0.6f, 0.85f);
            DrawBoxMesh(dev, eff, box, bladePos, new Vector3(0.04f, 0.12f, 0.03f));
            DrawBoxMesh(dev, eff, box, handlePos, new Vector3(0.05f, 0.28f, 0.05f));
        }

        private void DrawAngleGrinder(GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl, Vector3 hit)
        {
            Vector3 spindlePos = hit + new Vector3(0, 0.02f, 0.06f);
            Vector3 discPos = hit + new Vector3(0, 0.01f, 0.02f);
            Vector3 bodyPos = hit + new Vector3(0.14f, 0.04f, 0.16f);

            eff.DiffuseColor = new Vector3(0.35f, 0.32f, 0.30f);
            DrawCylinderMesh(dev, eff, cyl, discPos, 0.08f, 0.012f);

            eff.DiffuseColor = new Vector3(0.2f, 0.2f, 0.25f);
            DrawBoxMesh(dev, eff, box, spindlePos, new Vector3(0.18f, 0.04f, 0.10f));

            eff.DiffuseColor = new Vector3(0.1f, 0.65f, 0.3f);
            DrawCylinderMesh(dev, eff, cyl, bodyPos, 0.045f, 0.22f);
        }

        private void DrawWelder(GraphicsDevice dev, BasicEffect eff, SimpleMesh cyl, Vector3 hit)
        {
            Vector3 tipPos = hit + new Vector3(0, 0.02f, 0.03f);
            Vector3 neckPos = hit + new Vector3(0, 0.06f, 0.10f);
            Vector3 handlePos = hit + new Vector3(0, 0.14f, 0.22f);

            eff.DiffuseColor = new Vector3(0.85f, 0.55f, 0.25f);
            DrawCylinderMesh(dev, eff, cyl, tipPos, 0.015f, 0.05f);

            eff.DiffuseColor = new Vector3(0.7f, 0.7f, 0.75f);
            DrawCylinderMesh(dev, eff, cyl, neckPos, 0.018f, 0.10f);

            eff.DiffuseColor = new Vector3(0.15f, 0.15f, 0.18f);
            DrawCylinderMesh(dev, eff, cyl, handlePos, 0.032f, 0.18f);
        }

        private void DrawMultimeter(GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl, Vector3 hit)
        {
            Vector3 probePos = hit + new Vector3(0, 0, 0.03f);
            Vector3 meterPos = hit + new Vector3(0.12f, 0.10f, 0.15f);

            eff.DiffuseColor = new Vector3(0.9f, 0.2f, 0.2f); // Red probe needle
            DrawCylinderMesh(dev, eff, cyl, probePos, 0.008f, 0.06f);

            eff.DiffuseColor = new Vector3(0.95f, 0.75f, 0.1f); // Fluke yellow handheld multimeter
            DrawBoxMesh(dev, eff, box, meterPos, new Vector3(0.08f, 0.14f, 0.04f));
        }

        private void DrawHydraulicBleeder(GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl, Vector3 hit)
        {
            Vector3 couplerPos = hit + new Vector3(0, 0, 0.02f);
            Vector3 hosePos = hit + new Vector3(0.06f, -0.06f, 0.08f);

            eff.DiffuseColor = new Vector3(0.3f, 0.5f, 0.85f); // Anodized blue quick-connect coupler
            DrawCylinderMesh(dev, eff, cyl, couplerPos, 0.02f, 0.05f);

            eff.DiffuseColor = new Vector3(0.1f, 0.1f, 0.1f); // Braided high-pressure line
            DrawCylinderMesh(dev, eff, cyl, hosePos, 0.012f, 0.15f);
        }

        private void DrawPaintGun(GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl, ToolManager tools, Vector3 hit)
        {
            Vector3 nozzlePos = hit + new Vector3(0, 0, 0.04f);
            Vector3 cupPos = hit + new Vector3(0, 0.12f, 0.12f);
            Vector3 handlePos = hit + new Vector3(0, -0.08f, 0.12f);

            eff.DiffuseColor = new Vector3(0.7f, 0.75f, 0.8f); // Chrome spray nozzle
            DrawCylinderMesh(dev, eff, cyl, nozzlePos, 0.015f, 0.06f);

            eff.DiffuseColor = ArmorDecal.PaintPalette[tools.ActiveColorIndex].ToVector3(); // Paint reservoir cup
            DrawCylinderMesh(dev, eff, cyl, cupPos, 0.045f, 0.10f);

            eff.DiffuseColor = new Vector3(0.25f, 0.25f, 0.28f); // Pistol grip
            DrawBoxMesh(dev, eff, box, handlePos, new Vector3(0.04f, 0.12f, 0.06f));
        }

        private void DrawBoxMesh(GraphicsDevice dev, BasicEffect eff, SimpleMesh mesh, Vector3 pos, Vector3 size)
        {
            eff.World = Matrix.CreateScale(size) * Matrix.CreateTranslation(pos);
            mesh.Draw(dev, eff);
        }

        private void DrawCylinderMesh(GraphicsDevice dev, BasicEffect eff, SimpleMesh mesh, Vector3 pos, float radius, float height)
        {
            eff.World = Matrix.CreateScale(radius * 2f, height, radius * 2f) * Matrix.CreateTranslation(pos);
            mesh.Draw(dev, eff);
        }
    }
}
