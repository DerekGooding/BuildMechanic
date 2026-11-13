using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics.MeshBuilders;
using BuildMechanic.Simulation;
using BuildMechanic.Tools;

namespace BuildMechanic.Graphics
{
    public static class ToolInHandRenderer
    {
        public static void DrawImpactDriver(
            GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl,
            SimpleMesh socketMesh, Texture2D metalTex, Texture2D gripTex,
            ToolManager tools, Vector3 hit, float recoil, float spinAngle)
        {
            Vector3 bitPos = hit + new Vector3(0, 0, 0.04f + recoil);
            Vector3 collarPos = hit + new Vector3(0, 0, 0.09f + recoil);
            Vector3 bodyPos = hit + new Vector3(0, 0, 0.17f + recoil);
            Vector3 gripPos = bodyPos + new Vector3(0, -0.13f, 0.04f);
            Vector3 battPos = gripPos + new Vector3(0, -0.09f, -0.02f);

            // Chrome impact socket sleeve with rotation
            eff.Texture = metalTex;
            eff.DiffuseColor = new Vector3(0.88f, 0.90f, 0.96f);
            eff.World = Matrix.CreateRotationZ(spinAngle) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(bitPos);
            socketMesh.Draw(dev, eff);

            // Machined aluminum anvil collar
            eff.DiffuseColor = new Vector3(0.72f, 0.75f, 0.80f);
            DrawCylinder(eff, cyl, dev, collarPos, 0.038f, 0.05f);

            // High-intensity LED work light
            eff.DiffuseColor = tools.IsOperating ? new Vector3(2.5f, 2.5f, 2.8f) : new Vector3(0.7f, 0.7f, 0.8f);
            DrawBox(eff, box, dev, bodyPos + new Vector3(0, 0.055f, -0.07f), new Vector3(0.015f, 0.015f, 0.015f));

            // High-torque industrial motor body (safety orange)
            eff.Texture = gripTex;
            eff.DiffuseColor = new Vector3(0.95f, 0.45f, 0.12f);
            DrawBox(eff, box, dev, bodyPos, new Vector3(0.11f, 0.12f, 0.16f));

            // Overmolded textured rubber grip
            eff.DiffuseColor = new Vector3(0.20f, 0.22f, 0.25f);
            DrawBox(eff, box, dev, gripPos, new Vector3(0.06f, 0.15f, 0.07f));

            // 18V Lithium battery pack
            eff.DiffuseColor = new Vector3(0.15f, 0.16f, 0.18f);
            DrawBox(eff, box, dev, battPos, new Vector3(0.10f, 0.055f, 0.14f));

            // Battery fuel gauge LED (active green)
            eff.DiffuseColor = new Vector3(0.2f, 1.6f, 0.3f);
            DrawBox(eff, box, dev, battPos + new Vector3(0, 0.032f, 0.05f), new Vector3(0.035f, 0.01f, 0.015f));
        }

        public static void DrawAngleGrinder(
            GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl,
            SimpleMesh discMesh, Texture2D metalTex, Texture2D gripTex,
            ToolManager tools, Vector3 hit, float vibration, float spinAngle)
        {
            Vector3 discPos = hit + new Vector3(0, 0.01f, 0.02f);
            Vector3 guardPos = hit + new Vector3(0, 0.025f, 0.05f);
            Vector3 gearPos = hit + new Vector3(0.06f, 0.03f, 0.08f) + new Vector3(vibration, 0, 0);
            Vector3 bodyPos = hit + new Vector3(0.17f, 0.04f, 0.17f) + new Vector3(vibration, 0, 0);

            // Abrasive cut-off disc
            eff.Texture = metalTex;
            eff.DiffuseColor = tools.IsOperating ? new Vector3(1.25f, 0.85f, 0.45f) : new Vector3(0.35f, 0.32f, 0.30f);
            eff.World = Matrix.CreateRotationY(spinAngle) * Matrix.CreateTranslation(discPos);
            discMesh.Draw(dev, eff);

            // Stamped steel spark deflection guard
            eff.DiffuseColor = new Vector3(0.22f, 0.24f, 0.26f);
            DrawBox(eff, box, dev, guardPos, new Vector3(0.14f, 0.025f, 0.10f));

            // Cast aluminum gearhead casing
            eff.DiffuseColor = new Vector3(0.72f, 0.75f, 0.80f);
            DrawBox(eff, box, dev, gearPos, new Vector3(0.09f, 0.07f, 0.09f));

            // Spindle lock button
            eff.DiffuseColor = new Vector3(0.15f, 0.16f, 0.18f);
            DrawBox(eff, box, dev, gearPos + new Vector3(0, 0.04f, 0), new Vector3(0.025f, 0.015f, 0.025f));

            // Auxiliary knurled side handle
            eff.Texture = gripTex;
            eff.DiffuseColor = new Vector3(0.20f, 0.22f, 0.25f);
            eff.World = Matrix.CreateRotationZ(1.57f) * Matrix.CreateTranslation(gearPos + new Vector3(-0.06f, 0, 0));
            cyl.Draw(dev, eff);

            // Industrial motor barrel
            eff.DiffuseColor = new Vector3(0.12f, 0.65f, 0.58f);
            eff.World = Matrix.CreateRotationX(0.78f) * Matrix.CreateTranslation(bodyPos);
            cyl.Draw(dev, eff);
        }

        public static void DrawWelder(
            GraphicsDevice dev, BasicEffect eff, SimpleMesh cyl,
            SimpleMesh shroudMesh, Texture2D metalTex, Texture2D gripTex,
            Vector3 hit, Vector3 tipColor, Vector3 tremor)
        {
            Vector3 tipPos = hit + new Vector3(0, 0.015f, 0.02f) + tremor;
            Vector3 nozzlePos = hit + new Vector3(0, 0.035f, 0.06f) + tremor;
            Vector3 handlePos = hit + new Vector3(0, 0.12f, 0.20f) + tremor;

            // Tungsten electrode with live thermal glow
            eff.Texture = metalTex;
            eff.DiffuseColor = tipColor;
            eff.World = Matrix.CreateScale(0.006f, 0.045f, 0.006f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(tipPos);
            cyl.Draw(dev, eff);

            // Pink alumina ceramic gas shroud cup
            eff.DiffuseColor = new Vector3(0.92f, 0.65f, 0.70f);
            eff.World = Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(nozzlePos);
            shroudMesh.Draw(dev, eff);

            // Brass collet body chuck
            eff.DiffuseColor = new Vector3(0.85f, 0.68f, 0.28f);
            eff.World = Matrix.CreateScale(0.032f, 0.04f, 0.032f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(nozzlePos + new Vector3(0, 0, 0.04f));
            cyl.Draw(dev, eff);

            // Ribbed torch pencil handle
            eff.Texture = gripTex;
            eff.DiffuseColor = new Vector3(0.18f, 0.19f, 0.22f);
            eff.World = Matrix.CreateScale(0.034f, 0.18f, 0.034f) * Matrix.CreateRotationX(0.45f) * Matrix.CreateTranslation(handlePos);
            cyl.Draw(dev, eff);

            // Rear electrode back-cap rod
            eff.DiffuseColor = new Vector3(0.10f, 0.10f, 0.12f);
            eff.World = Matrix.CreateScale(0.016f, 0.14f, 0.016f) * Matrix.CreateRotationX(-1.12f) * Matrix.CreateTranslation(handlePos + new Vector3(0, 0.06f, -0.04f));
            cyl.Draw(dev, eff);
        }

        public static void DrawMultimeter(
            GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl,
            Texture2D metalTex, Texture2D gripTex, Vector3 hit)
        {
            Vector3 probePos = hit + new Vector3(0, 0, 0.03f);
            Vector3 meterPos = hit + new Vector3(0.12f, 0.10f, 0.15f);

            // Red test probe wand
            eff.Texture = metalTex;
            eff.DiffuseColor = new Vector3(0.95f, 0.22f, 0.22f);
            eff.World = Matrix.CreateScale(0.010f, 0.09f, 0.010f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(probePos);
            cyl.Draw(dev, eff);

            // Black ground probe lead wand
            eff.DiffuseColor = new Vector3(0.15f, 0.16f, 0.18f);
            eff.World = Matrix.CreateScale(0.010f, 0.09f, 0.010f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(probePos + new Vector3(0.035f, -0.02f, 0.01f));
            cyl.Draw(dev, eff);

            // High-impact yellow protective holster boot
            eff.Texture = gripTex;
            eff.DiffuseColor = new Vector3(0.98f, 0.75f, 0.12f);
            DrawBox(eff, box, dev, meterPos, new Vector3(0.11f, 0.18f, 0.042f));

            // Dark meter body bezel
            eff.DiffuseColor = new Vector3(0.18f, 0.20f, 0.22f);
            DrawBox(eff, box, dev, meterPos, new Vector3(0.095f, 0.165f, 0.045f));

            // Backlit LCD readout display screen
            eff.Texture = metalTex;
            eff.DiffuseColor = new Vector3(0.25f, 0.75f, 0.65f);
            DrawBox(eff, box, dev, meterPos + new Vector3(0, 0.042f, 0.024f), new Vector3(0.075f, 0.042f, 0.005f));

            // Rotary function selector dial switch
            eff.DiffuseColor = new Vector3(0.28f, 0.30f, 0.32f);
            eff.World = Matrix.CreateScale(0.045f, 0.012f, 0.045f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(meterPos + new Vector3(0, -0.018f, 0.024f));
            cyl.Draw(dev, eff);
        }

        public static void DrawHydraulicBleeder(
            GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl,
            Texture2D metalTex, Texture2D gripTex, Texture2D gaugeTex, Vector3 hit)
        {
            Vector3 couplerPos = hit + new Vector3(0, 0, 0.02f);
            Vector3 gaugePos = hit + new Vector3(0.07f, 0.04f, 0.07f);
            Vector3 bottlePos = hit + new Vector3(0.08f, -0.08f, 0.12f);

            // High-pressure knurled brass coupler
            eff.Texture = metalTex;
            eff.DiffuseColor = new Vector3(0.88f, 0.72f, 0.25f);
            eff.World = Matrix.CreateScale(0.022f, 0.05f, 0.022f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(couplerPos);
            cyl.Draw(dev, eff);

            // 0-60 PSI Dial pressure gauge
            eff.Texture = gaugeTex;
            eff.DiffuseColor = Vector3.One;
            eff.World = Matrix.CreateScale(0.055f, 0.018f, 0.055f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(gaugePos);
            cyl.Draw(dev, eff);

            // Fluid reservoir canister bottle
            eff.Texture = gripTex;
            eff.DiffuseColor = new Vector3(0.88f, 0.90f, 0.92f);
            eff.World = Matrix.CreateScale(0.085f, 0.16f, 0.085f) * Matrix.CreateTranslation(bottlePos);
            cyl.Draw(dev, eff);

            // Blue anodized aluminum cap
            eff.Texture = metalTex;
            eff.DiffuseColor = new Vector3(0.20f, 0.45f, 0.85f);
            eff.World = Matrix.CreateScale(0.092f, 0.03f, 0.092f) * Matrix.CreateTranslation(bottlePos + new Vector3(0, 0.08f, 0));
            cyl.Draw(dev, eff);
        }

        public static void DrawPaintGun(
            GraphicsDevice dev, BasicEffect eff, SimpleMesh box, SimpleMesh cyl,
            SimpleMesh cupMesh, Texture2D metalTex, Texture2D gripTex,
            ToolManager tools, Vector3 hit, float triggerPull)
        {
            Vector3 nozzlePos = hit + new Vector3(0, 0, 0.04f);
            Vector3 cupPos = hit + new Vector3(0, 0.12f, 0.12f);
            Vector3 bodyPos = hit + new Vector3(0, 0.04f, 0.10f);
            Vector3 handlePos = hit + new Vector3(0, -0.07f, 0.10f);

            // Atomizing brass air cap & nozzle
            eff.Texture = metalTex;
            eff.DiffuseColor = new Vector3(0.88f, 0.72f, 0.25f);
            eff.World = Matrix.CreateScale(0.038f, 0.035f, 0.038f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(nozzlePos);
            cyl.Draw(dev, eff);

            // Forged aluminum gun body
            eff.DiffuseColor = new Vector3(0.85f, 0.88f, 0.95f);
            DrawBox(eff, box, dev, bodyPos, new Vector3(0.042f, 0.05f, 0.14f));

            // Translucent nylon paint reservoir cup tinted with active paint palette
            Color activeColor = ArmorDecal.PaintPalette[tools.ActiveColorIndex];
            eff.DiffuseColor = activeColor.ToVector3();
            eff.World = Matrix.CreateTranslation(cupPos);
            cupMesh.Draw(dev, eff);

            // Two-finger curved trigger lever with pull animation
            eff.DiffuseColor = new Vector3(0.95f, 0.96f, 1.0f);
            DrawBox(eff, box, dev, bodyPos + new Vector3(0, -0.04f, 0.02f - triggerPull * 0.015f), new Vector3(0.015f, 0.07f, 0.025f));

            // Ergonomic handle grip
            eff.Texture = gripTex;
            eff.DiffuseColor = new Vector3(0.22f, 0.24f, 0.26f);
            DrawBox(eff, box, dev, handlePos, new Vector3(0.038f, 0.13f, 0.055f));

            // Lower quick-disconnect air hose nipple
            eff.Texture = metalTex;
            eff.DiffuseColor = new Vector3(0.85f, 0.88f, 0.95f);
            eff.World = Matrix.CreateScale(0.016f, 0.04f, 0.016f) * Matrix.CreateTranslation(handlePos + new Vector3(0, -0.075f, 0));
            cyl.Draw(dev, eff);
        }

        public static void DrawPryBar(
            GraphicsDevice dev, BasicEffect eff, SimpleMesh box,
            SimpleMesh pryBarMesh, Texture2D metalTex, Vector3 hit)
        {
            Vector3 bladePos = hit + new Vector3(0.03f, 0.03f, 0.02f);
            Vector3 rockerPos = hit + new Vector3(0.06f, 0.06f, 0.05f);
            Vector3 handlePos = hit + new Vector3(0.16f, 0.16f, 0.16f);

            // Chisel ground wedge blade
            eff.Texture = metalTex;
            eff.DiffuseColor = new Vector3(0.85f, 0.88f, 0.92f);
            DrawBox(eff, box, dev, bladePos, new Vector3(0.035f, 0.07f, 0.015f));

            // Curved rocker fulcrum heel
            eff.DiffuseColor = new Vector3(0.30f, 0.32f, 0.36f);
            DrawBox(eff, box, dev, rockerPos, new Vector3(0.045f, 0.07f, 0.045f));

            // Cobalt blue forged hex alloy shaft
            eff.DiffuseColor = new Vector3(0.22f, 0.42f, 0.85f);
            eff.World = Matrix.CreateRotationX(0.78f) * Matrix.CreateRotationY(0.78f) * Matrix.CreateTranslation(handlePos);
            pryBarMesh.Draw(dev, eff);
        }

        private static void DrawBox(BasicEffect eff, SimpleMesh box, GraphicsDevice dev, Vector3 pos, Vector3 size)
        {
            eff.World = Matrix.CreateScale(size) * Matrix.CreateTranslation(pos);
            box.Draw(dev, eff);
        }

        private static void DrawCylinder(BasicEffect eff, SimpleMesh cyl, GraphicsDevice dev, Vector3 pos, float r, float h)
        {
            eff.World = Matrix.CreateScale(r * 2f, h, r * 2f) * Matrix.CreateTranslation(pos);
            cyl.Draw(dev, eff);
        }
    }
}
