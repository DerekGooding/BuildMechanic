using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics.MeshBuilders;
using BuildMechanic.Graphics.Textures;

namespace BuildMechanic.Graphics.Renderers
{
    public class HangarRenderer : IDisposable
    {
        private readonly SimpleMesh _boxMesh;
        private readonly SimpleMesh _cylinderMesh;
        private readonly SimpleMesh _hookMesh;
        private readonly SimpleMesh _viseMesh;
        private readonly SimpleMesh _lampMesh;
        private readonly SimpleMesh _spreaderMesh;
        private readonly SimpleMesh _cableMesh;
        private readonly SimpleMesh _umbilicalMesh;
        private readonly Texture2D _floorTex;
        private readonly Texture2D _matTex;
        private readonly Texture2D _screenTex;
        private readonly Texture2D _chassisTex;

        public Texture2D FloorTexture => _floorTex;

        public HangarRenderer(GraphicsDevice device, SimpleMesh boxMesh, SimpleMesh cylinderMesh, Texture2D chassisTex)
        {
            _boxMesh = boxMesh;
            _cylinderMesh = cylinderMesh;
            _chassisTex = chassisTex;

            _floorTex = HangarTextureGenerator.CreateDiamondPlateFloor(device, 512);
            _matTex = HangarTextureGenerator.CreateWorkstationMat(device, 256);
            _screenTex = HangarTextureGenerator.CreateDiagnosticScreen(device, 256);

            _hookMesh = HangarMeshBuilder.CreateCraneHook(device);
            _viseMesh = HangarMeshBuilder.CreateBenchVise(device);
            _lampMesh = HangarMeshBuilder.CreateLampShade(device);

            _spreaderMesh = MechGantryRigMeshBuilder.CreateSpreaderBeam(device, 3.6f, 0.28f, 0.35f);
            _cableMesh = MechGantryRigMeshBuilder.CreateSuspensionCable(device, 2.4f, 0.022f);
            _umbilicalMesh = MechGantryRigMeshBuilder.CreateUmbilicalBoom(device, 2.2f, 0.045f);
        }

        public void Draw(GraphicsDevice device, BasicEffect effect)
        {
            // 1. Diamond Plate Steel Hangar Floor
            effect.Texture = _floorTex;
            effect.DiffuseColor = Vector3.One;
            DrawBox(device, effect, new Vector3(0, -0.05f, 0), new Vector3(16f, 0.1f, 16f));

            // 2. Rear Industrial Wall & Structural Gantry Columns
            effect.Texture = _chassisTex;
            effect.DiffuseColor = new Vector3(0.55f, 0.58f, 0.65f);
            DrawBox(device, effect, new Vector3(0, 5.0f, -4.5f), new Vector3(16f, 10f, 0.5f));

            // Heavy I-Beam Support Columns
            effect.DiffuseColor = new Vector3(0.70f, 0.72f, 0.76f);
            DrawBox(device, effect, new Vector3(-3.2f, 4.0f, -0.5f), new Vector3(0.4f, 8.0f, 0.4f));
            DrawBox(device, effect, new Vector3(3.2f, 4.0f, -0.5f), new Vector3(0.4f, 8.0f, 0.4f));

            // Yellow Electrical Conduits running along columns
            effect.DiffuseColor = new Vector3(0.95f, 0.75f, 0.15f);
            DrawBox(device, effect, new Vector3(-3.05f, 4.0f, -0.35f), new Vector3(0.06f, 7.8f, 0.06f));
            DrawBox(device, effect, new Vector3(3.05f, 4.0f, -0.35f), new Vector3(0.06f, 7.8f, 0.06f));

            // 3. Overhead Heavy-Lift Gantry Crane Beam & Trolley
            effect.DiffuseColor = new Vector3(0.92f, 0.72f, 0.18f);
            DrawBox(device, effect, new Vector3(0, 7.8f, -0.5f), new Vector3(7.0f, 0.5f, 0.6f));

            effect.DiffuseColor = new Vector3(0.28f, 0.30f, 0.35f);
            DrawBox(device, effect, new Vector3(0, 7.45f, -0.5f), new Vector3(0.8f, 0.35f, 0.5f));

            // Crane Hook hanging from trolley
            effect.DiffuseColor = new Vector3(0.85f, 0.88f, 0.95f);
            effect.World = Matrix.CreateTranslation(new Vector3(0, 7.15f, -0.5f));
            _hookMesh.Draw(device, effect);

            // 4. Gantry Maintenance Suspension Rig
            // Yellow Heavy Lifting Spreader Beam
            effect.DiffuseColor = new Vector3(0.95f, 0.75f, 0.15f);
            effect.World = Matrix.CreateTranslation(new Vector3(0, 6.20f, 0.15f));
            _spreaderMesh.Draw(device, effect);

            // High-Tensile Wire Rope Suspension Slings to Mech Shoulder Lift Eyes
            effect.DiffuseColor = new Vector3(0.80f, 0.82f, 0.88f);
            effect.World = Matrix.CreateTranslation(new Vector3(-1.45f, 6.20f, 0.15f));
            _cableMesh.Draw(device, effect);
            effect.World = Matrix.CreateTranslation(new Vector3(1.45f, 6.20f, 0.15f));
            _cableMesh.Draw(device, effect);

            // Articulated Hydraulic Umbilical Boom from Gantry Column to Mech Reactor Core
            effect.DiffuseColor = new Vector3(0.32f, 0.35f, 0.40f);
            effect.World = Matrix.CreateTranslation(new Vector3(-3.0f, 3.20f, -0.35f));
            _umbilicalMesh.Draw(device, effect);
            // Glowing Umbilical Connector Port
            effect.DiffuseColor = new Vector3(0.2f, 1.4f, 0.5f);
            DrawBox(device, effect, new Vector3(-0.80f, 3.20f, -0.35f), new Vector3(0.12f, 0.12f, 0.12f));

            // 5. Mechanic Workbenches (Left & Right)
            DrawWorkstation(device, effect, new Vector3(-1.80f, 0, 1.20f));
            DrawWorkstation(device, effect, new Vector3(1.80f, 0, 1.20f));
        }

        private void DrawWorkstation(GraphicsDevice device, BasicEffect effect, Vector3 basePos)
        {
            // Base Cabinet (Tool Storage Drawers)
            effect.Texture = _chassisTex;
            effect.DiffuseColor = new Vector3(0.68f, 0.70f, 0.74f);
            DrawBox(device, effect, basePos + new Vector3(0, 0.60f, 0), new Vector3(1.40f, 1.20f, 0.90f));

            // Heavy Steel Tabletop Slab
            effect.DiffuseColor = new Vector3(0.50f, 0.52f, 0.56f);
            DrawBox(device, effect, basePos + new Vector3(0, 1.22f, 0), new Vector3(1.48f, 0.05f, 0.96f));

            // Rubber Non-Slip Inspection Mat
            effect.Texture = _matTex;
            effect.DiffuseColor = Vector3.One;
            DrawBox(device, effect, basePos + new Vector3(0, 1.248f, 0), new Vector3(1.36f, 0.008f, 0.84f));

            // Plate Cradle Upright Struts
            effect.Texture = _chassisTex;
            effect.DiffuseColor = new Vector3(0.85f, 0.85f, 0.90f);
            DrawBox(device, effect, basePos + new Vector3(-0.35f, 1.42f, -0.04f), new Vector3(0.08f, 0.35f, 0.12f));
            DrawBox(device, effect, basePos + new Vector3(0.35f, 1.42f, -0.04f), new Vector3(0.08f, 0.35f, 0.12f));

            // Cradle Horizontal Support Beam
            DrawBox(device, effect, basePos + new Vector3(0, 1.54f, -0.02f), new Vector3(0.80f, 0.06f, 0.14f));

            // Non-Marring Rubber Cradle Pads
            effect.DiffuseColor = new Vector3(0.22f, 0.22f, 0.25f);
            DrawBox(device, effect, basePos + new Vector3(-0.25f, 1.58f, -0.01f), new Vector3(0.12f, 0.03f, 0.16f));
            DrawBox(device, effect, basePos + new Vector3(0.25f, 1.58f, -0.01f), new Vector3(0.12f, 0.03f, 0.16f));

            // Pneumatic Heavy Bench Vise on Table Corner
            effect.DiffuseColor = new Vector3(0.28f, 0.52f, 0.82f);
            effect.World = Matrix.CreateTranslation(basePos + new Vector3(-0.62f, 1.32f, 0.32f));
            _viseMesh.Draw(device, effect);

            // Articulated Task Lamp shining down on plate cradle
            effect.DiffuseColor = new Vector3(0.92f, 0.78f, 0.15f);
            effect.World = Matrix.CreateRotationX(0.4f) * Matrix.CreateTranslation(basePos + new Vector3(-0.50f, 1.82f, -0.15f));
            _lampMesh.Draw(device, effect);

            // CRT Diagnostic Telemetry Monitor
            effect.Texture = _screenTex;
            effect.DiffuseColor = Vector3.One;
            DrawBox(device, effect, basePos + new Vector3(0.55f, 1.42f, -0.28f), new Vector3(0.24f, 0.20f, 0.04f));
        }

        private void DrawBox(GraphicsDevice device, BasicEffect effect, Vector3 position, Vector3 size)
        {
            effect.World = Matrix.CreateScale(size) * Matrix.CreateTranslation(position);
            _boxMesh.Draw(device, effect);
        }

        public void Dispose()
        {
            _hookMesh?.Dispose();
            _viseMesh?.Dispose();
            _lampMesh?.Dispose();
            _spreaderMesh?.Dispose();
            _cableMesh?.Dispose();
            _umbilicalMesh?.Dispose();
            _floorTex?.Dispose();
            _matTex?.Dispose();
            _screenTex?.Dispose();
        }
    }
}
