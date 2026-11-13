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

        private float _thermalGlow = 0f;
        private float _socketSpin = 0f;
        private float _grinderSpin = 0f;

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

            // Update procedural animation states
            float dt = 0.016f; // Nominal frame time
            if (tools.IsOperating && tools.ActiveTool == ToolType.Welder)
                _thermalGlow = Math.Min(1.0f, _thermalGlow + dt * 4f);
            else
                _thermalGlow = Math.Max(0.0f, _thermalGlow - dt * 0.8f);

            if (tools.IsOperating && tools.ActiveTool == ToolType.ImpactDriver)
                _socketSpin += dt * 35f;

            if (tools.IsOperating && tools.ActiveTool == ToolType.AngleGrinder)
                _grinderSpin += dt * 65f;

            float ms = DateTime.Now.Millisecond;
            float recoil = tools.IsOperating && tools.ActiveTool == ToolType.ImpactDriver
                ? (float)Math.Sin(ms * 0.12) * 0.007f : 0f;
            float vibration = tools.IsOperating && tools.ActiveTool == ToolType.AngleGrinder
                ? (float)Math.Sin(ms * 0.25) * 0.003f : 0f;
            Vector3 tremor = tools.IsOperating && tools.ActiveTool == ToolType.Welder
                ? new Vector3((float)Math.Sin(ms * 0.3) * 0.0015f, (float)Math.Cos(ms * 0.3) * 0.0015f, 0) : Vector3.Zero;
            float triggerPull = tools.IsOperating && tools.ActiveTool == ToolType.PaintGun ? 1.0f : 0.0f;

            // Live tungsten electrode thermal heating/cooling color ramp
            Vector3 coldTip = new Vector3(0.6f, 0.6f, 0.65f);
            Vector3 cherryRed = new Vector3(1.4f, 0.35f, 0.15f);
            Vector3 incandWhite = new Vector3(2.8f, 2.8f, 3.2f);
            Vector3 tipColor = _thermalGlow > 0.5f
                ? Vector3.Lerp(cherryRed, incandWhite, (_thermalGlow - 0.5f) * 2f)
                : Vector3.Lerp(coldTip, cherryRed, _thermalGlow * 2f);

            switch (tools.ActiveTool)
            {
                case ToolType.ImpactDriver:
                    ToolInHandRenderer.DrawImpactDriver(dev, eff, box, cyl, _socketMesh, _metalTex, _gripTex, tools, hit, recoil, _socketSpin);
                    break;
                case ToolType.PryBar:
                    ToolInHandRenderer.DrawPryBar(dev, eff, box, _pryBarMesh, _metalTex, hit);
                    break;
                case ToolType.AngleGrinder:
                    ToolInHandRenderer.DrawAngleGrinder(dev, eff, box, cyl, _grinderDiscMesh, _metalTex, _gripTex, tools, hit, vibration, _grinderSpin);
                    break;
                case ToolType.Welder:
                    ToolInHandRenderer.DrawWelder(dev, eff, cyl, _welderShroudMesh, _metalTex, _gripTex, hit, tipColor, tremor);
                    break;
                case ToolType.Multimeter:
                    ToolInHandRenderer.DrawMultimeter(dev, eff, box, cyl, _metalTex, _gripTex, hit);
                    break;
                case ToolType.HydraulicBleeder:
                    ToolInHandRenderer.DrawHydraulicBleeder(dev, eff, box, cyl, _metalTex, _gripTex, _gaugeTex, hit);
                    break;
                case ToolType.PaintGun:
                    ToolInHandRenderer.DrawPaintGun(dev, eff, box, cyl, _sprayCupMesh, _metalTex, _gripTex, tools, hit, triggerPull);
                    break;
            }
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
