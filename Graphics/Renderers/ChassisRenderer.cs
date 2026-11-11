using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics.MeshBuilders;
using BuildMechanic.Simulation;

namespace BuildMechanic.Graphics.Renderers
{
    public class ChassisRenderer : IDisposable
    {
        private readonly GraphicsDevice _device;
        private readonly SimpleMesh _boxMesh;
        private readonly SimpleMesh _cylinderMesh;
        private readonly SimpleMesh _cockpitMesh;
        private readonly SimpleMesh _keelMesh;
        private readonly SimpleMesh _turretRingMesh;
        private readonly SimpleMesh _kneeMesh;
        private readonly Texture2D _chassisTex;

        public ChassisRenderer(GraphicsDevice device, Texture2D chassisTex, SimpleMesh boxMesh, SimpleMesh cylinderMesh)
        {
            _device = device;
            _chassisTex = chassisTex;
            _boxMesh = boxMesh;
            _cylinderMesh = cylinderMesh;

            _cockpitMesh = MechChassisMeshBuilder.CreateDetailedCockpit(device);
            _keelMesh = MechChassisMeshBuilder.CreateSternalKeel(device, 0.16f, 1.10f, 0.08f);
            _turretRingMesh = MechChassisMeshBuilder.CreateTurretRing(device);
            _kneeMesh = MechChassisMeshBuilder.CreateKneeJoint(device, 0.16f, 0.22f);
        }

        public void Draw(GraphicsDevice device, BasicEffect effect, MechChassis chassis, float servoOffset)
        {
            effect.Texture = _chassisTex;

            // 1. Lower Pelvis / Waist & Turret Traversing Gear Ring
            effect.DiffuseColor = new Vector3(0.52f, 0.55f, 0.62f);
            DrawBox(effect, new Vector3(0, 1.4f, 0), new Vector3(1.1f, 1.0f, 0.8f));

            effect.DiffuseColor = new Vector3(0.85f, 0.78f, 0.42f); // Bronzed gear teeth ring
            effect.World = Matrix.CreateTranslation(new Vector3(0, 1.95f, 0));
            _turretRingMesh.Draw(device, effect);

            // Hip Gimbals
            effect.DiffuseColor = new Vector3(0.38f, 0.40f, 0.45f);
            DrawCylinder(effect, new Vector3(-0.55f, 1.35f, 0), 0.16f, 0.35f);
            DrawCylinder(effect, new Vector3(0.55f, 1.35f, 0), 0.16f, 0.35f);

            // 2. Heavy Upper Torso Core Frame & Rear Heat Exchanger
            effect.DiffuseColor = new Vector3(0.65f, 0.68f, 0.74f);
            DrawBox(effect, new Vector3(0, 2.7f, 0.05f), new Vector3(1.40f, 1.40f, 0.70f));

            // Rear Radiator Louvers & Twin Reactor Exhaust Stacks
            effect.DiffuseColor = new Vector3(0.24f, 0.26f, 0.30f);
            DrawBox(effect, new Vector3(0, 2.85f, -0.32f), new Vector3(1.05f, 0.90f, 0.08f));
            effect.DiffuseColor = new Vector3(0.45f, 0.48f, 0.55f);
            DrawCylinder(effect, new Vector3(-0.35f, 3.55f, -0.28f), 0.07f, 0.45f);
            DrawCylinder(effect, new Vector3(0.35f, 3.55f, -0.28f), 0.07f, 0.45f);

            // Sternal Keel Center Reinforcement Ridge
            effect.DiffuseColor = new Vector3(0.88f, 0.90f, 0.94f);
            effect.World = Matrix.CreateTranslation(new Vector3(0, 2.7f, 0.44f));
            _keelMesh.Draw(device, effect);

            // 3. Sub-Armor Mounting Bosses (Rendered ONLY when plates are detached)
            effect.DiffuseColor = new Vector3(0.35f, 0.37f, 0.42f);
            if (chassis.LeftTorsoPlate?.State != PlateMountState.Mounted)
                DrawSubArmorBay(effect, new Vector3(-0.52f, 2.7f, 0.428f), new Vector3(0.76f, 0.96f, 0.056f), 0.30f, 0.38f);
            if (chassis.RightTorsoPlate?.State != PlateMountState.Mounted)
                DrawSubArmorBay(effect, new Vector3(0.52f, 2.7f, 0.428f), new Vector3(0.76f, 0.96f, 0.056f), 0.30f, 0.38f);

            if (chassis.LeftShoulderPlate?.State != PlateMountState.Mounted)
                DrawSubArmorBay(effect, new Vector3(-1.45f, 3.35f + servoOffset, 0.428f), new Vector3(0.80f, 0.50f, 0.056f), 0.26f, 0.17f);
            if (chassis.RightShoulderPlate?.State != PlateMountState.Mounted)
                DrawSubArmorBay(effect, new Vector3(1.45f, 3.35f - servoOffset, 0.428f), new Vector3(0.80f, 0.50f, 0.056f), 0.26f, 0.17f);

            if (chassis.LeftLegPlate?.State != PlateMountState.Mounted)
                DrawSubArmorBay(effect, new Vector3(-0.55f, 0.85f, 0.428f), new Vector3(0.50f, 0.85f, 0.056f), 0.16f, 0.28f);
            if (chassis.RightLegPlate?.State != PlateMountState.Mounted)
                DrawSubArmorBay(effect, new Vector3(0.55f, 0.85f, 0.428f), new Vector3(0.50f, 0.85f, 0.056f), 0.16f, 0.28f);

            // 4. Faceted Armored Cockpit Pod & Glowing Sensor Optics
            effect.DiffuseColor = new Vector3(0.60f, 0.64f, 0.70f);
            effect.World = Matrix.CreateTranslation(new Vector3(0, 3.65f, 0.20f));
            _cockpitMesh.Draw(device, effect);

            // Multi-lens Sensor Optic Visor (Central LiDAR + Dual infrared cameras)
            effect.DiffuseColor = new Vector3(0.1f, 1.5f, 1.8f);
            DrawCylinder(effect, new Vector3(0, 3.62f, 0.46f), 0.045f, 0.03f);
            effect.DiffuseColor = new Vector3(1.8f, 0.3f, 0.2f);
            DrawCylinder(effect, new Vector3(-0.12f, 3.62f, 0.45f), 0.03f, 0.03f);
            DrawCylinder(effect, new Vector3(0.12f, 3.62f, 0.45f), 0.03f, 0.03f);

            // Antenna Mast
            effect.DiffuseColor = new Vector3(0.85f, 0.88f, 0.92f);
            DrawCylinder(effect, new Vector3(-0.25f, 4.10f, 0.05f), 0.012f, 0.35f);

            // 5. Shoulder Assemblies & Dynamic Servo Actuators
            effect.DiffuseColor = new Vector3(0.50f, 0.53f, 0.58f);
            DrawBox(effect, new Vector3(-1.45f, 3.35f + servoOffset, 0.15f), new Vector3(0.9f, 0.5f, 0.6f));
            DrawBox(effect, new Vector3(1.45f, 3.35f - servoOffset, 0.15f), new Vector3(0.9f, 0.5f, 0.6f));

            // Upper Arm Structural Bicep & Servo Rods
            effect.DiffuseColor = new Vector3(0.68f, 0.70f, 0.76f);
            DrawCylinder(effect, new Vector3(-2.0f, 2.5f + servoOffset, 0.15f), 0.22f, 1.4f);
            DrawCylinder(effect, new Vector3(2.0f, 2.5f - servoOffset, 0.15f), 0.22f, 1.4f);

            // Twin Hydraulic Stabilizer Servo Rods
            effect.DiffuseColor = new Vector3(0.90f, 0.93f, 0.98f); // Chrome rod
            DrawCylinder(effect, new Vector3(-1.75f, 2.6f + servoOffset * 0.5f, 0.30f), 0.035f, 0.80f);
            DrawCylinder(effect, new Vector3(1.75f, 2.6f - servoOffset * 0.5f, 0.30f), 0.035f, 0.80f);

            // 6. Leg & Knee Joint Assemblies
            effect.DiffuseColor = new Vector3(0.60f, 0.63f, 0.70f);
            DrawCylinder(effect, new Vector3(-0.55f, 0.7f, 0), 0.20f, 1.4f);
            DrawCylinder(effect, new Vector3(0.55f, 0.7f, 0), 0.20f, 1.4f);

            // Heavy Dual-Shear Knee Hinge Joints
            effect.DiffuseColor = new Vector3(0.38f, 0.40f, 0.46f);
            effect.World = Matrix.CreateTranslation(new Vector3(-0.55f, 0.30f, 0.15f));
            _kneeMesh.Draw(device, effect);
            effect.World = Matrix.CreateTranslation(new Vector3(0.55f, 0.30f, 0.15f));
            _kneeMesh.Draw(device, effect);
        }

        private void DrawSubArmorBay(BasicEffect effect, Vector3 pos, Vector3 size, float bx, float by)
        {
            effect.DiffuseColor = new Vector3(0.32f, 0.34f, 0.38f);
            DrawBox(effect, pos, size);

            // 4 Recessed Threaded Receiver Bushings (where bolts anchor)
            effect.DiffuseColor = new Vector3(0.85f, 0.88f, 0.95f);
            float bz = pos.Z + size.Z * 0.5f;
            DrawCylinder(effect, new Vector3(pos.X - bx, pos.Y + by, bz), 0.035f, 0.015f);
            DrawCylinder(effect, new Vector3(pos.X + bx, pos.Y + by, bz), 0.035f, 0.015f);
            DrawCylinder(effect, new Vector3(pos.X - bx, pos.Y - by, bz), 0.035f, 0.015f);
            DrawCylinder(effect, new Vector3(pos.X + bx, pos.Y - by, bz), 0.035f, 0.015f);
        }

        private void DrawBox(BasicEffect effect, Vector3 position, Vector3 size)
        {
            effect.World = Matrix.CreateScale(size) * Matrix.CreateTranslation(position);
            _boxMesh.Draw(_device, effect);
        }

        private void DrawCylinder(BasicEffect effect, Vector3 position, float radius, float height)
        {
            effect.World = Matrix.CreateScale(radius * 2f, height, radius * 2f) * Matrix.CreateTranslation(position);
            _cylinderMesh.Draw(_device, effect);
        }

        public void Dispose()
        {
            _cockpitMesh?.Dispose();
            _keelMesh?.Dispose();
            _turretRingMesh?.Dispose();
            _kneeMesh?.Dispose();
        }
    }
}
