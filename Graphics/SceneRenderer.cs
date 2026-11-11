using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics.Renderers;
using BuildMechanic.Simulation;
using BuildMechanic.Tools;

namespace BuildMechanic.Graphics
{
    public class SceneRenderer : IDisposable
    {
        private readonly GraphicsDevice _device;
        private readonly BasicEffect _effect;

        // Shared Primitives & Base Textures
        private readonly SimpleMesh _boxMesh;
        private readonly SimpleMesh _cylinderMesh;
        private readonly Texture2D _pixelTex;
        private readonly Texture2D _glowTex;
        private readonly Texture2D _chassisTex;

        // Subsystem & Specialized Component Renderers
        private readonly LightingManager _lighting;
        private readonly HangarRenderer _hangarRenderer;
        private readonly ChassisRenderer _chassisRenderer;
        private readonly ArmorPlateRenderer _plateRenderer;
        private readonly BoltRenderer _boltRenderer;
        private readonly CrackRenderer _crackRenderer;
        private readonly SubsystemRenderer _subsystemRenderer;
        private readonly ToolRenderer _toolRenderer;

        public Texture2D PixelTexture => _pixelTex;
        public Texture2D GlowTexture => _glowTex;

        public SceneRenderer(GraphicsDevice device)
        {
            _device = device;
            _effect = new BasicEffect(device)
            {
                TextureEnabled = true,
                LightingEnabled = true,
                PreferPerPixelLighting = true
            };

            // Base procedural textures
            _pixelTex = TextureGenerator.CreateWhitePixel(device);
            _glowTex = TextureGenerator.CreateRadialGlow(device, 64);
            _chassisTex = TextureGenerator.CreateChassisTexture(device, 256);

            // Base geometric primitives
            _boxMesh = MeshPrimitives.CreateBox(device, Vector3.One);
            _cylinderMesh = MeshPrimitives.CreateCylinder(device, 0.5f, 1.0f, 20);

            // Initialize modular sub-renderers
            _lighting = new LightingManager();
            _hangarRenderer = new HangarRenderer(device, _boxMesh, _cylinderMesh, _chassisTex);
            _chassisRenderer = new ChassisRenderer(device, _chassisTex, _boxMesh, _cylinderMesh);
            _plateRenderer = new ArmorPlateRenderer(device);
            _boltRenderer = new BoltRenderer(device);
            _crackRenderer = new CrackRenderer(device, _boxMesh, _pixelTex);
            _subsystemRenderer = new SubsystemRenderer();
            _toolRenderer = new ToolRenderer();
            _toolRenderer.Initialize(device);
        }

        public void DrawScene(Matrix view, Matrix proj, MechChassis chassis, ToolManager tools, float servoOffset = 0f)
        {
            _device.DepthStencilState = DepthStencilState.Default;
            _device.RasterizerState = RasterizerState.CullCounterClockwise;
            _device.SamplerStates[0] = SamplerState.LinearWrap;

            _effect.View = view;
            _effect.Projection = proj;

            // 1. Dynamic 3-point studio lighting & tool arc/spark flashes
            _lighting.ApplyLighting(_effect, tools);

            // 2. Industrial Hangar Bay & Workbenches
            _hangarRenderer.Draw(_device, _effect);

            // 3. Heavy Mech Chassis, Gimbals & Sub-Armor Bulkheads
            _chassisRenderer.Draw(_device, _effect, chassis, servoOffset);

            // 4. Subsystem Internals (Hydraulic Cylinders & Avionics Relays)
            _subsystemRenderer.DrawSubsystems(_device, _effect, _boxMesh, _cylinderMesh, _chassisTex, chassis, tools);

            // 5. Beveled Composite Ballistic Armor Plates
            _plateRenderer.Draw(_device, _effect, chassis, tools);

            // 6. Fasteners & Screw Threads (with ISO metric thread profiles & dynamic spin)
            _boltRenderer.Draw(_device, _effect, chassis, tools);

            // 7. Structural Cracks & Scalloped Weld Beads with HAZ Temper Colors
            _crackRenderer.Draw(_device, _effect, chassis, tools);

            // 8. 3D Shop Tools (Impact Driver, Grinder, Welder, Multimeter, etc.)
            _toolRenderer.DrawActiveTool(_device, _effect, _boxMesh, _cylinderMesh, _chassisTex, tools);
        }

        public void Dispose()
        {
            _effect?.Dispose();
            _boxMesh?.Dispose();
            _cylinderMesh?.Dispose();
            _pixelTex?.Dispose();
            _glowTex?.Dispose();
            _chassisTex?.Dispose();

            _hangarRenderer?.Dispose();
            _chassisRenderer?.Dispose();
            _plateRenderer?.Dispose();
            _boltRenderer?.Dispose();
            _crackRenderer?.Dispose();
            _subsystemRenderer?.Dispose();
            _toolRenderer?.Dispose();
        }
    }
}
