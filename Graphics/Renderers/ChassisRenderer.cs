using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics.MeshBuilders;
using BuildMechanic.Simulation;

namespace BuildMechanic.Graphics.Renderers
{
    public class ChassisRenderer : IDisposable
    {
        private readonly SimpleMesh _boxMesh;
        private readonly SimpleMesh _cylinderMesh;
        private readonly SimpleMesh _cockpitMesh;
        private readonly SimpleMesh _turretRingMesh;
        private readonly Texture2D _chassisTex;

        public ChassisRenderer(GraphicsDevice device, Texture2D chassisTex, SimpleMesh boxMesh, SimpleMesh cylinderMesh)
        {
            _device = device;
            _chassisTex = chassisTex;
            _boxMesh = boxMesh;
            _cylinderMesh = cylinderMesh;

            _cockpitMesh = MechChassisMeshBuilder.CreateCockpitHead(device);
            _turretRingMesh = MechChassisMeshBuilder.CreateTurretRing(device);
        }

        private readonly GraphicsDevice _device;

        public void Draw(GraphicsDevice device, BasicEffect effect, MechChassis chassis, float servoOffset)
        {
            effect.Texture = _chassisTex;

            // 1. Lower Pelvis / Waist & Turret Traversing Ring
            effect.DiffuseColor = new Vector3(0.55f, 0.58f, 0.65f);
            DrawBox(effect, new Vector3(0, 1.4f, 0), new Vector3(1.1f, 1.0f, 0.8f));

            effect.DiffuseColor = new Vector3(0.80f, 0.75f, 0.45f); // Bronzed gear teeth ring
            effect.World = Matrix.CreateTranslation(new Vector3(0, 1.95f, 0));
            _turretRingMesh.Draw(device, effect);

            // 2. Heavy Upper Torso Core Frame
            effect.DiffuseColor = new Vector3(0.68f, 0.70f, 0.75f);
            DrawBox(effect, new Vector3(0, 2.7f, 0.05f), new Vector3(1.40f, 1.40f, 0.70f));

            // Rear Heat Exchanger Exhaust Radiator Louvers
            effect.DiffuseColor = new Vector3(0.25f, 0.28f, 0.32f);
            DrawBox(effect, new Vector3(0, 2.85f, -0.32f), new Vector3(1.05f, 0.90f, 0.08f));

            // 3. Sub-Armor Torso Mounting Bosses (Recessed bolt anchors)
            effect.DiffuseColor = new Vector3(0.40f, 0.42f, 0.48f);
            DrawBox(effect, new Vector3(-0.52f, 2.7f, 0.43f), new Vector3(0.76f, 0.96f, 0.06f));
            DrawBox(effect, new Vector3(0.52f, 2.7f, 0.43f), new Vector3(0.76f, 0.96f, 0.06f));

            // Sternal Keel Center Reinforcement Ridge
            effect.DiffuseColor = new Vector3(0.85f, 0.88f, 0.92f);
            DrawBox(effect, new Vector3(0, 2.7f, 0.44f), new Vector3(0.16f, 1.10f, 0.08f));

            // 4. Sloped Armored Cockpit Pod
            effect.DiffuseColor = new Vector3(0.62f, 0.65f, 0.70f);
            effect.World = Matrix.CreateTranslation(new Vector3(0, 3.65f, 0.20f));
            _cockpitMesh.Draw(device, effect);

            // Glowing Sensor Optic Visor
            effect.DiffuseColor = new Vector3(0.1f, 1.4f, 1.6f); // Emerald/cyan glowing optic array
            DrawBox(effect, new Vector3(0, 3.60f, 0.45f), new Vector3(0.42f, 0.09f, 0.06f));

            // 5. Shoulder Mounting Bosses & Arm Actuator Gimbals
            effect.DiffuseColor = new Vector3(0.45f, 0.48f, 0.54f);
            DrawBox(effect, new Vector3(-1.45f, 3.35f + servoOffset, 0.43f), new Vector3(0.80f, 0.50f, 0.06f));
            DrawBox(effect, new Vector3(1.45f, 3.35f - servoOffset, 0.43f), new Vector3(0.80f, 0.50f, 0.06f));

            // Shoulder Structural Frame
            effect.DiffuseColor = new Vector3(0.55f, 0.58f, 0.64f);
            DrawBox(effect, new Vector3(-1.45f, 3.35f + servoOffset, 0.15f), new Vector3(0.9f, 0.5f, 0.6f));
            DrawBox(effect, new Vector3(1.45f, 3.35f - servoOffset, 0.15f), new Vector3(0.9f, 0.5f, 0.6f));

            // Upper Arm Hydraulic Actuator Cylinders
            effect.DiffuseColor = new Vector3(0.70f, 0.72f, 0.78f);
            DrawCylinder(effect, new Vector3(-2.0f, 2.5f + servoOffset, 0.15f), 0.25f, 1.4f);
            DrawCylinder(effect, new Vector3(2.0f, 2.5f - servoOffset, 0.15f), 0.25f, 1.4f);

            // 6. Leg Mounting Bosses & Stanchions
            effect.DiffuseColor = new Vector3(0.40f, 0.42f, 0.48f);
            DrawBox(effect, new Vector3(-0.55f, 0.85f, 0.43f), new Vector3(0.50f, 0.85f, 0.06f));
            DrawBox(effect, new Vector3(0.55f, 0.85f, 0.43f), new Vector3(0.50f, 0.85f, 0.06f));

            effect.DiffuseColor = new Vector3(0.65f, 0.68f, 0.74f);
            DrawCylinder(effect, new Vector3(-0.55f, 0.7f, 0), 0.22f, 1.4f);
            DrawCylinder(effect, new Vector3(0.55f, 0.7f, 0), 0.22f, 1.4f);
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
            _turretRingMesh?.Dispose();
        }
    }
}
