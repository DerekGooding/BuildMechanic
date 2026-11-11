using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics;
using BuildMechanic.Graphics.MeshBuilders;

namespace BuildMechanic.Inspector
{
    public static class ModelCatalogAssemblies
    {
        public static void RegisterAssemblies(
            ModelCatalog catalog,
            GraphicsDevice device,
            Texture2D chassisTex,
            Texture2D armorTex,
            Texture2D metalTex,
            Texture2D gripTex,
            Texture2D gaugeTex)
        {
            var box = MeshPrimitives.CreateBox(device, Vector3.One);
            var cyl = MeshPrimitives.CreateCylinder(device, 0.5f, 1.0f, 16);
            catalog.RegisterDisposable(box);
            catalog.RegisterDisposable(cyl);

            // 1. Full Mech Torso Chassis Assembly
            BuildFullTorsoAssembly(catalog, device, chassisTex, box);

            // 2. Full Arm Assembly
            BuildFullArmAssembly(catalog, device, chassisTex, box, cyl);

            // 3. Full Leg Assembly
            BuildFullLegAssembly(catalog, device, chassisTex, box, cyl);

            // 4. Complete Gantry Suspension Rig
            BuildFullRigAssembly(catalog, device, chassisTex);

            // 5. Full Assembled Tools
            BuildAssembledTools(catalog, device, box, cyl, metalTex, gripTex, gaugeTex);
        }

        private static void BuildFullTorsoAssembly(ModelCatalog catalog, GraphicsDevice device, Texture2D chassisTex, SimpleMesh box)
        {
            var cockpit = MechChassisMeshBuilder.CreateDetailedCockpit(device);
            var keel = MechChassisMeshBuilder.CreateSternalKeel(device);
            var ring = MechChassisMeshBuilder.CreateTurretRing(device);
            var pack = MechChassisMeshBuilder.CreateReactorBackpack(device);
            catalog.RegisterDisposable(cockpit);
            catalog.RegisterDisposable(keel);
            catalog.RegisterDisposable(ring);
            catalog.RegisterDisposable(pack);

            int totalVerts = cockpit.VertexCount + keel.VertexCount + ring.VertexCount + pack.VertexCount;
            int totalTris = cockpit.PrimitiveCount + keel.PrimitiveCount + ring.PrimitiveCount + pack.PrimitiveCount;

            catalog.RegisterModel(new InspectableModel(
                "Full Torso Chassis (Assembly)",
                ModelCategory.ChassisAndCore,
                "Integrated torso core: cockpit canopy, sternal keel, pelvic turret ring, and fusion reactor backpack.",
                new BoundingBox(new Vector3(-0.6f, -0.6f, -0.6f), new Vector3(0.6f, 0.6f, 0.6f)),
                totalVerts, totalTris,
                (dev, eff, mode) =>
                {
                    eff.Texture = chassisTex;
                    eff.DiffuseColor = new Vector3(0.85f, 0.88f, 0.90f);
                    eff.World = Matrix.CreateTranslation(0, 0.15f, 0.1f);
                    cockpit.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.82f, 0.85f, 0.88f);
                    eff.World = Matrix.CreateTranslation(0, -0.2f, 0.25f);
                    keel.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.75f, 0.78f, 0.82f);
                    eff.World = Matrix.CreateTranslation(0, -0.55f, 0);
                    ring.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.72f, 0.75f, 0.78f);
                    eff.World = Matrix.CreateTranslation(0, 0, -0.4f);
                    pack.Draw(dev, eff);
                }));
        }

        private static void BuildFullArmAssembly(ModelCatalog catalog, GraphicsDevice device, Texture2D chassisTex, SimpleMesh box, SimpleMesh cyl)
        {
            var forearm = MechLimbsMeshBuilder.CreateForearmAssembly(device);
            var claw = MechLimbsMeshBuilder.CreateManipulatorClaw(device);
            var knee = MechChassisMeshBuilder.CreateKneeJoint(device);
            catalog.RegisterDisposable(forearm);
            catalog.RegisterDisposable(claw);
            catalog.RegisterDisposable(knee);

            int totalVerts = forearm.VertexCount + claw.VertexCount + knee.VertexCount + 120;
            int totalTris = forearm.PrimitiveCount + claw.PrimitiveCount + knee.PrimitiveCount + 80;

            catalog.RegisterModel(new InspectableModel(
                "Full Arm Manipulator (Assembly)",
                ModelCategory.LimbsAndJoints,
                "Articulated arm: bicep column, hydraulic servo, dual-shear elbow clevis, forearm, and 3-finger claw.",
                new BoundingBox(new Vector3(-0.25f, -0.85f, -0.25f), new Vector3(0.25f, 0.65f, 0.25f)),
                totalVerts, totalTris,
                (dev, eff, mode) =>
                {
                    eff.Texture = chassisTex;
                    // Bicep
                    eff.DiffuseColor = new Vector3(0.75f, 0.78f, 0.82f);
                    eff.World = Matrix.CreateScale(0.12f, 0.40f, 0.12f) * Matrix.CreateTranslation(0, 0.35f, 0);
                    cyl.Draw(dev, eff);

                    // Elbow
                    eff.DiffuseColor = new Vector3(0.70f, 0.72f, 0.75f);
                    eff.World = Matrix.CreateTranslation(0, 0.10f, 0);
                    knee.Draw(dev, eff);

                    // Forearm
                    eff.DiffuseColor = new Vector3(0.80f, 0.82f, 0.85f);
                    eff.World = Matrix.CreateTranslation(0, -0.25f, 0);
                    forearm.Draw(dev, eff);

                    // Claw
                    eff.DiffuseColor = new Vector3(0.88f, 0.88f, 0.90f);
                    eff.World = Matrix.CreateTranslation(0, -0.65f, 0);
                    claw.Draw(dev, eff);
                }));
        }

        private static void BuildFullLegAssembly(ModelCatalog catalog, GraphicsDevice device, Texture2D chassisTex, SimpleMesh box, SimpleMesh cyl)
        {
            var calf = MechLimbsMeshBuilder.CreateLowerLegCalf(device);
            var foot = MechLimbsMeshBuilder.CreateFootPad(device);
            var knee = MechChassisMeshBuilder.CreateKneeJoint(device);
            catalog.RegisterDisposable(calf);
            catalog.RegisterDisposable(foot);
            catalog.RegisterDisposable(knee);

            int totalVerts = calf.VertexCount + foot.VertexCount + knee.VertexCount + 150;
            int totalTris = calf.PrimitiveCount + foot.PrimitiveCount + knee.PrimitiveCount + 90;

            catalog.RegisterModel(new InspectableModel(
                "Full Heavy Leg (Assembly)",
                ModelCategory.LimbsAndJoints,
                "Locomotion leg: thigh stanchion, dual-shear knee joint, calf column, twin shocks, and foot pad.",
                new BoundingBox(new Vector3(-0.35f, -1.15f, -0.35f), new Vector3(0.35f, 0.75f, 0.35f)),
                totalVerts, totalTris,
                (dev, eff, mode) =>
                {
                    eff.Texture = chassisTex;
                    // Thigh
                    eff.DiffuseColor = new Vector3(0.72f, 0.75f, 0.78f);
                    eff.World = Matrix.CreateScale(0.18f, 0.55f, 0.18f) * Matrix.CreateTranslation(0, 0.45f, 0);
                    cyl.Draw(dev, eff);

                    // Knee
                    eff.DiffuseColor = new Vector3(0.70f, 0.72f, 0.75f);
                    eff.World = Matrix.CreateTranslation(0, 0.15f, 0);
                    knee.Draw(dev, eff);

                    // Calf
                    eff.DiffuseColor = new Vector3(0.78f, 0.80f, 0.84f);
                    eff.World = Matrix.CreateTranslation(0, -0.40f, 0);
                    calf.Draw(dev, eff);

                    // Foot
                    eff.DiffuseColor = new Vector3(0.68f, 0.70f, 0.72f);
                    eff.World = Matrix.CreateTranslation(0, -0.90f, 0.05f);
                    foot.Draw(dev, eff);
                }));
        }

        private static void BuildFullRigAssembly(ModelCatalog catalog, GraphicsDevice device, Texture2D chassisTex)
        {
            var beam = MechGantryRigMeshBuilder.CreateSpreaderBeam(device);
            var cable = MechGantryRigMeshBuilder.CreateSuspensionCable(device);
            var umbilical = MechGantryRigMeshBuilder.CreateUmbilicalBoom(device);
            catalog.RegisterDisposable(beam);
            catalog.RegisterDisposable(cable);
            catalog.RegisterDisposable(umbilical);

            int totalVerts = beam.VertexCount + cable.VertexCount + umbilical.VertexCount;
            int totalTris = beam.PrimitiveCount + cable.PrimitiveCount + umbilical.PrimitiveCount;

            catalog.RegisterModel(new InspectableModel(
                "Complete Gantry Rig (Assembly)",
                ModelCategory.GantryRig,
                "Integrated gantry crane suspension: spreader I-beam, wire rope slings, and umbilical boom.",
                new BoundingBox(new Vector3(-1.8f, -2.4f, -0.5f), new Vector3(1.8f, 0.2f, 0.5f)),
                totalVerts, totalTris,
                (dev, eff, mode) =>
                {
                    eff.Texture = chassisTex;
                    eff.DiffuseColor = new Vector3(0.95f, 0.85f, 0.15f);
                    eff.World = Matrix.Identity;
                    beam.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.70f, 0.72f, 0.75f);
                    eff.World = Matrix.CreateTranslation(-1.2f, 0, 0);
                    cable.Draw(dev, eff);
                    eff.World = Matrix.CreateTranslation(1.2f, 0, 0);
                    cable.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.85f, 0.50f, 0.20f);
                    eff.World = Matrix.CreateTranslation(0, 0, -0.3f);
                    umbilical.Draw(dev, eff);
                }));
        }

        private static void BuildAssembledTools(
            ModelCatalog catalog, GraphicsDevice device, SimpleMesh box, SimpleMesh cyl,
            Texture2D metalTex, Texture2D gripTex, Texture2D gaugeTex)
        {
            var socket = ToolMeshBuilder.CreateImpactSocket(device);
            var disc = ToolMeshBuilder.CreateGrinderDisc(device);
            catalog.RegisterDisposable(socket);
            catalog.RegisterDisposable(disc);

            // Impact Driver
            catalog.RegisterModel(new InspectableModel(
                "Impact Driver (Assembly)",
                ModelCategory.ShopTools,
                "Cordless brushless impact driver with overmolded rubber grip, battery gauge, and 19mm socket.",
                new BoundingBox(new Vector3(-0.08f, -0.22f, -0.12f), new Vector3(0.08f, 0.12f, 0.12f)),
                socket.VertexCount + 150, socket.PrimitiveCount + 80,
                (dev, eff, mode) =>
                {
                    eff.Texture = metalTex;
                    eff.DiffuseColor = new Vector3(0.85f, 0.88f, 0.95f);
                    eff.World = Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(0, 0, 0.08f);
                    socket.Draw(dev, eff);

                    eff.Texture = gripTex;
                    eff.DiffuseColor = new Vector3(0.95f, 0.45f, 0.12f);
                    eff.World = Matrix.CreateScale(0.11f, 0.12f, 0.17f);
                    box.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.20f, 0.22f, 0.25f);
                    eff.World = Matrix.CreateScale(0.06f, 0.15f, 0.07f) * Matrix.CreateTranslation(0, -0.12f, -0.04f);
                    box.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.15f, 0.16f, 0.18f);
                    eff.World = Matrix.CreateScale(0.10f, 0.06f, 0.13f) * Matrix.CreateTranslation(0, -0.21f, -0.04f);
                    box.Draw(dev, eff);
                }));

            // Angle Grinder
            catalog.RegisterModel(new InspectableModel(
                "Angle Grinder (Assembly)",
                ModelCategory.ShopTools,
                "Industrial angle grinder with cast aluminum gear head, side handle, guard, and abrasive disc.",
                new BoundingBox(new Vector3(-0.15f, -0.08f, -0.18f), new Vector3(0.15f, 0.08f, 0.18f)),
                disc.VertexCount + 160, disc.PrimitiveCount + 90,
                (dev, eff, mode) =>
                {
                    eff.Texture = metalTex;
                    eff.DiffuseColor = new Vector3(0.35f, 0.32f, 0.30f);
                    eff.World = Matrix.CreateTranslation(0, -0.02f, 0.08f);
                    disc.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.72f, 0.75f, 0.80f);
                    eff.World = Matrix.CreateScale(0.09f, 0.07f, 0.09f) * Matrix.CreateTranslation(0, 0, 0.08f);
                    box.Draw(dev, eff);

                    eff.Texture = gripTex;
                    eff.DiffuseColor = new Vector3(0.12f, 0.65f, 0.58f);
                    eff.World = Matrix.CreateScale(0.045f, 0.22f, 0.045f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(0, 0, -0.08f);
                    cyl.Draw(dev, eff);
                }));
        }
    }
}
