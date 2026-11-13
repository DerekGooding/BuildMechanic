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
            ModelCatalogToolAssemblies.RegisterToolAssemblies(catalog, device, box, cyl, metalTex, gripTex, gaugeTex);
        }

        private static void BuildFullTorsoAssembly(ModelCatalog catalog, GraphicsDevice device, Texture2D chassisTex, SimpleMesh box)
        {
            var cockpit = MechChassisMeshBuilder.CreateDetailedCockpit(device);
            var keel = MechChassisMeshBuilder.CreateSternalKeel(device);
            var ring = MechChassisMeshBuilder.CreateTurretRing(device);
            var pack = MechChassisMeshBuilder.CreateReactorBackpack(device);
            var pelvis = MechCoreMeshBuilder.CreatePelvisAssembly(device);
            var spine = MechCoreMeshBuilder.CreateSpineBulkhead(device);
            catalog.RegisterDisposable(cockpit);
            catalog.RegisterDisposable(keel);
            catalog.RegisterDisposable(ring);
            catalog.RegisterDisposable(pack);
            catalog.RegisterDisposable(pelvis);
            catalog.RegisterDisposable(spine);

            int totalVerts = cockpit.VertexCount + keel.VertexCount + ring.VertexCount + pack.VertexCount + pelvis.VertexCount + spine.VertexCount;
            int totalTris = cockpit.PrimitiveCount + keel.PrimitiveCount + ring.PrimitiveCount + pack.PrimitiveCount + pelvis.PrimitiveCount + spine.PrimitiveCount;

            catalog.RegisterModel(new InspectableModel(
                "Full Torso Chassis (Assembly)",
                ModelCategory.ChassisAndCore,
                "Integrated torso core: cockpit canopy, vertebral spine, sternal keel, pelvic turret ring, pelvis casting, and reactor backpack.",
                new BoundingBox(new Vector3(-0.7f, -0.7f, -0.6f), new Vector3(0.7f, 0.7f, 0.6f)),
                totalVerts, totalTris,
                (dev, eff, mode) =>
                {
                    eff.Texture = chassisTex;
                    eff.DiffuseColor = new Vector3(0.85f, 0.88f, 0.90f);
                    eff.World = Matrix.CreateTranslation(0, 0.25f, 0.1f);
                    cockpit.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.72f, 0.75f, 0.78f);
                    eff.World = Matrix.CreateTranslation(0, 0.10f, 0);
                    spine.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.82f, 0.85f, 0.88f);
                    eff.World = Matrix.CreateTranslation(0, 0.10f, 0.25f);
                    keel.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.70f, 0.72f, 0.76f);
                    eff.World = Matrix.CreateTranslation(0, -0.45f, 0);
                    pelvis.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.75f, 0.78f, 0.82f);
                    eff.World = Matrix.CreateTranslation(0, -0.35f, 0);
                    ring.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.72f, 0.75f, 0.78f);
                    eff.World = Matrix.CreateTranslation(0, 0.10f, -0.35f);
                    pack.Draw(dev, eff);
                }));
        }

        private static void BuildFullArmAssembly(ModelCatalog catalog, GraphicsDevice device, Texture2D chassisTex, SimpleMesh box, SimpleMesh cyl)
        {
            var forearm = MechLimbsMeshBuilder.CreateForearmAssembly(device);
            var claw = MechLimbsMeshBuilder.CreateManipulatorClaw(device);
            var knee = MechChassisMeshBuilder.CreateKneeJoint(device);
            var bicep = MechLimbsMeshBuilder.CreateArmoredBicep(device, 0.40f, 0.09f);
            catalog.RegisterDisposable(forearm);
            catalog.RegisterDisposable(claw);
            catalog.RegisterDisposable(knee);
            catalog.RegisterDisposable(bicep);

            int totalVerts = forearm.VertexCount + claw.VertexCount + knee.VertexCount + bicep.VertexCount;
            int totalTris = forearm.PrimitiveCount + claw.PrimitiveCount + knee.PrimitiveCount + bicep.PrimitiveCount;

            catalog.RegisterModel(new InspectableModel(
                "Full Arm Manipulator (Assembly)",
                ModelCategory.LimbsAndJoints,
                "Articulated arm: armored bicep spar, dual-shear elbow clevis, forearm beam, and 3-finger claw.",
                new BoundingBox(new Vector3(-0.25f, -0.85f, -0.25f), new Vector3(0.25f, 0.65f, 0.25f)),
                totalVerts, totalTris,
                (dev, eff, mode) =>
                {
                    eff.Texture = chassisTex;
                    // Bicep
                    eff.DiffuseColor = new Vector3(0.75f, 0.78f, 0.82f);
                    eff.World = Matrix.CreateTranslation(0, 0.45f, 0);
                    bicep.Draw(dev, eff);

                    // Elbow
                    eff.DiffuseColor = new Vector3(0.70f, 0.72f, 0.75f);
                    eff.World = Matrix.CreateTranslation(0, 0.05f, 0);
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
            var thigh = MechLimbsMeshBuilder.CreateArmoredThigh(device, 0.50f, 0.12f);
            catalog.RegisterDisposable(calf);
            catalog.RegisterDisposable(foot);
            catalog.RegisterDisposable(knee);
            catalog.RegisterDisposable(thigh);

            int totalVerts = calf.VertexCount + foot.VertexCount + knee.VertexCount + thigh.VertexCount;
            int totalTris = calf.PrimitiveCount + foot.PrimitiveCount + knee.PrimitiveCount + thigh.PrimitiveCount;

            catalog.RegisterModel(new InspectableModel(
                "Full Heavy Leg (Assembly)",
                ModelCategory.LimbsAndJoints,
                "Locomotion leg: armored thigh spar, dual-shear knee joint, calf column, twin shocks, and foot pad.",
                new BoundingBox(new Vector3(-0.35f, -1.15f, -0.35f), new Vector3(0.35f, 0.75f, 0.35f)),
                totalVerts, totalTris,
                (dev, eff, mode) =>
                {
                    eff.Texture = chassisTex;
                    // Thigh
                    eff.DiffuseColor = new Vector3(0.72f, 0.75f, 0.78f);
                    eff.World = Matrix.CreateTranslation(0, 0.55f, 0);
                    thigh.Draw(dev, eff);

                    // Knee
                    eff.DiffuseColor = new Vector3(0.70f, 0.72f, 0.75f);
                    eff.World = Matrix.CreateTranslation(0, 0.05f, 0);
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
    }
}
