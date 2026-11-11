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
        private readonly SimpleMesh _backpackMesh;
        private readonly SimpleMesh _forearmMesh;
        private readonly SimpleMesh _clawMesh;
        private readonly SimpleMesh _calfMesh;
        private readonly SimpleMesh _footMesh;
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
            _backpackMesh = MechChassisMeshBuilder.CreateReactorBackpack(device, 0.95f, 0.85f, 0.28f);

            _forearmMesh = MechLimbsMeshBuilder.CreateForearmAssembly(device, 0.80f, 0.22f, 0.22f);
            _clawMesh = MechLimbsMeshBuilder.CreateManipulatorClaw(device, 0.12f, 0.28f);
            _calfMesh = MechLimbsMeshBuilder.CreateLowerLegCalf(device, 0.65f, 0.24f, 0.28f);
            _footMesh = MechLimbsMeshBuilder.CreateFootPad(device, 0.38f, 0.54f, 0.12f);
        }

        public void Draw(GraphicsDevice device, BasicEffect effect, MechChassis chassis, float servoOffset)
        {
            effect.Texture = _chassisTex;

            // 1. Lower Pelvis & Turret Traversing Gear Ring
            effect.DiffuseColor = new Vector3(0.52f, 0.55f, 0.62f);
            DrawBox(effect, new Vector3(0, 1.4f, 0), new Vector3(1.1f, 1.0f, 0.8f));
            effect.DiffuseColor = new Vector3(0.85f, 0.78f, 0.42f);
            effect.World = Matrix.CreateTranslation(new Vector3(0, 1.95f, 0));
            _turretRingMesh.Draw(device, effect);

            // Hip Gimbals
            effect.DiffuseColor = new Vector3(0.38f, 0.40f, 0.45f);
            DrawCylinder(effect, new Vector3(-0.55f, 1.35f, 0), 0.16f, 0.35f);
            DrawCylinder(effect, new Vector3(0.55f, 1.35f, 0), 0.16f, 0.35f);

            // 2. Heavy Upper Torso Core Frame & Fusion Reactor Backpack
            effect.DiffuseColor = new Vector3(0.65f, 0.68f, 0.74f);
            DrawBox(effect, new Vector3(0, 2.7f, 0.05f), new Vector3(1.40f, 1.40f, 0.70f));

            effect.DiffuseColor = new Vector3(0.35f, 0.38f, 0.44f);
            effect.World = Matrix.CreateTranslation(new Vector3(0, 2.80f, -0.42f));
            _backpackMesh.Draw(device, effect);

            // Dual Vertical Reactor Exhaust Stacks
            effect.DiffuseColor = new Vector3(0.48f, 0.50f, 0.56f);
            DrawCylinder(effect, new Vector3(-0.35f, 3.55f, -0.38f), 0.07f, 0.50f);
            DrawCylinder(effect, new Vector3(0.35f, 3.55f, -0.38f), 0.07f, 0.50f);

            // Sternal Keel Center Reinforcement Ridge
            effect.DiffuseColor = new Vector3(0.88f, 0.90f, 0.94f);
            effect.World = Matrix.CreateTranslation(new Vector3(0, 2.7f, 0.44f));
            _keelMesh.Draw(device, effect);

            // 3. Sub-Armor Mounting Bosses (Rendered ONLY when plates are detached)
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

            // 4. Faceted Cockpit Pod & Tri-Optic Visor Array
            effect.DiffuseColor = new Vector3(0.60f, 0.64f, 0.70f);
            effect.World = Matrix.CreateTranslation(new Vector3(0, 3.65f, 0.20f));
            _cockpitMesh.Draw(device, effect);

            effect.DiffuseColor = new Vector3(0.1f, 1.5f, 1.8f);
            DrawCylinder(effect, new Vector3(0, 3.62f, 0.46f), 0.045f, 0.03f);
            effect.DiffuseColor = new Vector3(1.8f, 0.3f, 0.2f);
            DrawCylinder(effect, new Vector3(-0.12f, 3.62f, 0.45f), 0.03f, 0.03f);
            DrawCylinder(effect, new Vector3(0.12f, 3.62f, 0.45f), 0.03f, 0.03f);
            effect.DiffuseColor = new Vector3(0.85f, 0.88f, 0.92f);
            DrawCylinder(effect, new Vector3(-0.25f, 4.10f, 0.05f), 0.012f, 0.35f);

            // 5. Full Articulated Arm Assemblies (Biceps + Forearms + Claws)
            DrawArm(device, effect, -1.45f, 3.35f + servoOffset, isLeft: true);
            DrawArm(device, effect, 1.45f, 3.35f - servoOffset, isLeft: false);

            // 6. Full Articulated Leg Assemblies (Thighs + Knees + Calves + Foot Pads)
            DrawLeg(device, effect, -0.55f);
            DrawLeg(device, effect, 0.55f);
        }

        private void DrawArm(GraphicsDevice device, BasicEffect effect, float shoulderX, float shoulderY, bool isLeft)
        {
            float sideSign = isLeft ? -1f : 1f;

            // Shoulder Gimbal & Upper Bicep
            effect.DiffuseColor = new Vector3(0.50f, 0.53f, 0.58f);
            DrawBox(effect, new Vector3(shoulderX, shoulderY, 0.15f), new Vector3(0.9f, 0.5f, 0.6f));
            effect.DiffuseColor = new Vector3(0.68f, 0.70f, 0.76f);
            float bicepX = shoulderX + (sideSign * 0.55f);
            DrawCylinder(effect, new Vector3(bicepX, shoulderY - 0.75f, 0.15f), 0.18f, 0.90f);

            // Chrome Hydraulic Servo Rod
            effect.DiffuseColor = new Vector3(0.92f, 0.95f, 1.0f);
            DrawCylinder(effect, new Vector3(bicepX + (sideSign * 0.12f), shoulderY - 0.70f, 0.28f), 0.032f, 0.75f);

            // Forearm Assembly extending downward
            effect.DiffuseColor = new Vector3(0.55f, 0.58f, 0.64f);
            Vector3 elbowPos = new Vector3(bicepX, shoulderY - 1.25f, 0.15f);
            effect.World = Matrix.CreateTranslation(elbowPos);
            _forearmMesh.Draw(device, effect);

            // Industrial 3-Finger Hydraulic Manipulator Claw
            effect.DiffuseColor = new Vector3(0.35f, 0.38f, 0.44f);
            Vector3 wristPos = elbowPos + new Vector3(0, -0.80f, 0);
            effect.World = Matrix.CreateTranslation(wristPos);
            _clawMesh.Draw(device, effect);
        }

        private void DrawLeg(GraphicsDevice device, BasicEffect effect, float legX)
        {
            // Upper Thigh
            effect.DiffuseColor = new Vector3(0.60f, 0.63f, 0.70f);
            DrawCylinder(effect, new Vector3(legX, 1.05f, 0), 0.18f, 0.65f);

            // Heavy Dual-Shear Knee Joint
            effect.DiffuseColor = new Vector3(0.38f, 0.40f, 0.46f);
            effect.World = Matrix.CreateTranslation(new Vector3(legX, 0.70f, 0.12f));
            _kneeMesh.Draw(device, effect);

            // Lower Leg Calf Stanchion
            effect.DiffuseColor = new Vector3(0.52f, 0.55f, 0.62f);
            effect.World = Matrix.CreateTranslation(new Vector3(legX, 0.70f, 0));
            _calfMesh.Draw(device, effect);

            // Twin External Hydraulic Suspension Shock Absorber Rods
            effect.DiffuseColor = new Vector3(0.92f, 0.95f, 1.0f);
            DrawCylinder(effect, new Vector3(legX - 0.12f, 0.38f, 0.15f), 0.022f, 0.50f);
            DrawCylinder(effect, new Vector3(legX + 0.12f, 0.38f, 0.15f), 0.022f, 0.50f);

            // Heavy Articulated Foot Pad resting firmly on floor
            effect.DiffuseColor = new Vector3(0.32f, 0.34f, 0.38f);
            effect.World = Matrix.CreateTranslation(new Vector3(legX, 0.05f, 0.04f));
            _footMesh.Draw(device, effect);
        }

        private void DrawSubArmorBay(BasicEffect effect, Vector3 pos, Vector3 size, float bx, float by)
        {
            effect.DiffuseColor = new Vector3(0.32f, 0.34f, 0.38f);
            DrawBox(effect, pos, size);

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
            _backpackMesh?.Dispose();
            _forearmMesh?.Dispose();
            _clawMesh?.Dispose();
            _calfMesh?.Dispose();
            _footMesh?.Dispose();
        }
    }
}
