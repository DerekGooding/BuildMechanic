using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics;
using BuildMechanic.Graphics.MeshBuilders;
using BuildMechanic.Graphics.Textures;

namespace BuildMechanic.Inspector
{
    public static class ModelCatalogBuilder
    {
        public static ModelCatalog BuildCatalog(GraphicsDevice device)
        {
            var catalog = new ModelCatalog();

            // Textures
            var chassisTex = TextureGenerator.CreateChassisTexture(device, 256);
            var armorTex = ArmorTextureGenerator.CreateDetailedArmorTexture(device, 256, 256, true);
            var boltTex = ThreadTextureGenerator.CreateScrewThreadTexture(device, 256, 128);
            var floorTex = HangarTextureGenerator.CreateDiamondPlateFloor(device, 256);
            var metalTex = ToolTextureGenerator.CreateBrushedMetalTexture(device, 128);
            var gripTex = ToolTextureGenerator.CreateRubberGripTexture(device, 128);
            var gaugeTex = ToolTextureGenerator.CreatePressureGaugeTexture(device, 128);
            var pixelTex = TextureGenerator.CreateWhitePixel(device);

            catalog.RegisterDisposable(chassisTex);
            catalog.RegisterDisposable(armorTex);
            catalog.RegisterDisposable(boltTex);
            catalog.RegisterDisposable(floorTex);
            catalog.RegisterDisposable(metalTex);
            catalog.RegisterDisposable(gripTex);
            catalog.RegisterDisposable(gaugeTex);
            catalog.RegisterDisposable(pixelTex);

            // 1. Chassis & Core
            BuildChassisModels(catalog, device, chassisTex);

            // 2. Limbs & Joints
            BuildLimbModels(catalog, device, chassisTex);

            // 3. Armor & Fasteners
            BuildArmorAndFastenerModels(catalog, device, armorTex, boltTex);

            // 4. Gantry Suspension Rig
            BuildGantryRigModels(catalog, device, chassisTex);

            // 5-7. Tools, Subsystems, Hangar Environment
            ModelCatalogToolBuilder.BuildToolsAndEnvironment(
                catalog, device, chassisTex, metalTex, gripTex, gaugeTex, pixelTex);

            // Composite Assemblies
            ModelCatalogAssemblies.RegisterAssemblies(
                catalog, device, chassisTex, armorTex, metalTex, gripTex, gaugeTex);

            return catalog;
        }

        private static void BuildChassisModels(ModelCatalog catalog, GraphicsDevice device, Texture2D chassisTex)
        {
            // Cockpit Canopy
            var cockpit = MechChassisMeshBuilder.CreateDetailedCockpit(device);
            catalog.RegisterDisposable(cockpit);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Cockpit Canopy & Sensors",
                ModelCategory.ChassisAndCore,
                "Faceted ballistic glacis canopy with tri-optic LiDAR visor and FLIR sensor housings.",
                cockpit, chassisTex, new Vector3(0.85f, 0.88f, 0.90f),
                new BoundingBox(new Vector3(-0.45f, -0.35f, -0.4f), new Vector3(0.45f, 0.35f, 0.4f))));

            // Sternal Keel
            var keel = MechChassisMeshBuilder.CreateSternalKeel(device);
            catalog.RegisterDisposable(keel);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Sternal Keel Wedge",
                ModelCategory.ChassisAndCore,
                "Triangular spine keel deflecting frontal ballistic impacts along center line.",
                keel, chassisTex, new Vector3(0.82f, 0.85f, 0.88f),
                new BoundingBox(new Vector3(-0.15f, -0.55f, -0.1f), new Vector3(0.15f, 0.55f, 0.1f))));

            // Pelvic Turret Ring
            var ring = MechChassisMeshBuilder.CreateTurretRing(device);
            catalog.RegisterDisposable(ring);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Pelvic Turret Gear Ring",
                ModelCategory.ChassisAndCore,
                "32-tooth hardened spur gear traverse ring enabling 360° torso rotation.",
                ring, chassisTex, new Vector3(0.75f, 0.78f, 0.82f),
                new BoundingBox(new Vector3(-0.55f, 0, -0.55f), new Vector3(0.55f, 0.12f, 0.55f))));

            // Pelvic Girdle
            var pelvis = MechCoreMeshBuilder.CreatePelvisAssembly(device);
            catalog.RegisterDisposable(pelvis);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Pelvic Girdle & Femoral Hubs",
                ModelCategory.ChassisAndCore,
                "Heavy faceted pelvic casting with twin femoral socket hub gimbals and sacral bulkhead.",
                pelvis, chassisTex, new Vector3(0.70f, 0.72f, 0.76f),
                new BoundingBox(new Vector3(-0.70f, -0.25f, -0.35f), new Vector3(0.70f, 0.25f, 0.35f))));

            // Vertebral Spine
            var spine = MechCoreMeshBuilder.CreateSpineBulkhead(device);
            catalog.RegisterDisposable(spine);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Vertebral Spine & Rib Cage",
                ModelCategory.ChassisAndCore,
                "Reinforced spinal column with lateral armor ribs and dual clavicle crane lifting lugs.",
                spine, chassisTex, new Vector3(0.72f, 0.75f, 0.78f),
                new BoundingBox(new Vector3(-0.55f, -0.65f, -0.30f), new Vector3(0.55f, 0.65f, 0.30f))));

            // Dual-Shear Knee Joint
            var knee = MechChassisMeshBuilder.CreateKneeJoint(device);
            catalog.RegisterDisposable(knee);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Dual-Shear Knee Hinge",
                ModelCategory.ChassisAndCore,
                "Double-shear clevis joint bracket with integrated knee-cap shield plate.",
                knee, chassisTex, new Vector3(0.70f, 0.72f, 0.75f),
                new BoundingBox(new Vector3(-0.15f, -0.16f, -0.16f), new Vector3(0.15f, 0.16f, 0.16f))));

            // Fusion Backpack
            var backpack = MechChassisMeshBuilder.CreateReactorBackpack(device);
            catalog.RegisterDisposable(backpack);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Fusion Backpack & Exhausts",
                ModelCategory.ChassisAndCore,
                "Heavy rear reactor housing with twin angled exhaust stacks and coolant manifolds.",
                backpack, chassisTex, new Vector3(0.72f, 0.75f, 0.78f),
                new BoundingBox(new Vector3(-0.50f, -0.45f, -0.20f), new Vector3(0.50f, 0.45f, 0.20f))));
        }

        private static void BuildLimbModels(ModelCatalog catalog, GraphicsDevice device, Texture2D chassisTex)
        {
            // Armored Bicep
            var bicep = MechLimbsMeshBuilder.CreateArmoredBicep(device);
            catalog.RegisterDisposable(bicep);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Armored Bicep Spar",
                ModelCategory.LimbsAndJoints,
                "Faceted 8-sided upper arm bone spar with shoulder trunnion and elbow clevis caps.",
                bicep, chassisTex, new Vector3(0.75f, 0.78f, 0.82f),
                new BoundingBox(new Vector3(-0.18f, -0.85f, -0.18f), new Vector3(0.18f, 0, 0.18f))));

            // Arm Forearm
            var forearm = MechLimbsMeshBuilder.CreateForearmAssembly(device);
            catalog.RegisterDisposable(forearm);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Forearm Beam Assembly",
                ModelCategory.LimbsAndJoints,
                "Reinforced box beam forearm with hydraulic line routing channels.",
                forearm, chassisTex, new Vector3(0.80f, 0.82f, 0.85f),
                new BoundingBox(new Vector3(-0.15f, -0.80f, -0.15f), new Vector3(0.15f, 0, 0.15f))));

            // 3-Finger Claw
            var claw = MechLimbsMeshBuilder.CreateManipulatorClaw(device);
            catalog.RegisterDisposable(claw);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "3-Finger Industrial Manipulator",
                ModelCategory.LimbsAndJoints,
                "Articulated hydraulic end-effector claw with knurled rubber gripping pads.",
                claw, chassisTex, new Vector3(0.88f, 0.88f, 0.90f),
                new BoundingBox(new Vector3(-0.15f, -0.30f, -0.15f), new Vector3(0.15f, 0, 0.15f))));

            // Armored Thigh
            var thigh = MechLimbsMeshBuilder.CreateArmoredThigh(device);
            catalog.RegisterDisposable(thigh);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Armored Femoral Thigh Bone",
                ModelCategory.LimbsAndJoints,
                "Heavy octagonal femoral load spar with hip ball gimbal and dual-shear knee hinge.",
                thigh, chassisTex, new Vector3(0.72f, 0.75f, 0.78f),
                new BoundingBox(new Vector3(-0.18f, -0.65f, -0.18f), new Vector3(0.18f, 0, 0.18f))));

            // Calf Column
            var calf = MechLimbsMeshBuilder.CreateLowerLegCalf(device);
            catalog.RegisterDisposable(calf);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Calf Stanchion Column",
                ModelCategory.LimbsAndJoints,
                "Tapered lower leg stanchion column with structural reinforcement ribs.",
                calf, chassisTex, new Vector3(0.78f, 0.80f, 0.84f),
                new BoundingBox(new Vector3(-0.15f, -0.90f, -0.15f), new Vector3(0.15f, 0, 0.15f))));

            // Articulated Foot Pad
            var foot = MechLimbsMeshBuilder.CreateFootPad(device);
            catalog.RegisterDisposable(foot);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Articulated Foot Pad",
                ModelCategory.LimbsAndJoints,
                "Heavy-duty ground foot pad with deep traction lugs and spherical ankle socket.",
                foot, chassisTex, new Vector3(0.68f, 0.70f, 0.72f),
                new BoundingBox(new Vector3(-0.20f, -0.06f, -0.28f), new Vector3(0.20f, 0.06f, 0.28f))));
        }

        private static void BuildArmorAndFastenerModels(ModelCatalog catalog, GraphicsDevice device, Texture2D armorTex, Texture2D boltTex)
        {
            // Torso Plate
            var torsoPlate = ArmorPlateMeshBuilder.CreateTorsoPlate(device, new Vector3(0.85f, 0.80f, 0.04f), 0.28f, 0.28f);
            catalog.RegisterDisposable(torsoPlate);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Torso Glacis Armor Plate",
                ModelCategory.ArmorAndFasteners,
                "4-facet ballistic prow plate with 4 counterbored mounting apertures.",
                torsoPlate, armorTex, Vector3.One,
                new BoundingBox(new Vector3(-0.45f, -0.40f, -0.03f), new Vector3(0.45f, 0.40f, 0.03f))));

            // Shoulder Pauldron
            var shoulderPlate = ArmorPlateMeshBuilder.CreateShoulderPlate(device, new Vector3(0.55f, 0.65f, 0.035f), 0.20f, 0.20f);
            catalog.RegisterDisposable(shoulderPlate);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Shoulder Pauldron Plate",
                ModelCategory.ArmorAndFasteners,
                "Curved clamshell shoulder pauldron with reinforced deflector ridge.",
                shoulderPlate, armorTex, Vector3.One,
                new BoundingBox(new Vector3(-0.30f, -0.35f, -0.05f), new Vector3(0.30f, 0.38f, 0.05f))));

            // Leg Shin Guard
            var legPlate = ArmorPlateMeshBuilder.CreateLegPlate(device, new Vector3(0.38f, 0.70f, 0.03f), 0.14f, 0.22f);
            catalog.RegisterDisposable(legPlate);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Leg Shin Guard Plate",
                ModelCategory.ArmorAndFasteners,
                "Tapered lower leg armor plate engineered for oblique deflections.",
                legPlate, armorTex, Vector3.One,
                new BoundingBox(new Vector3(-0.20f, -0.35f, -0.03f), new Vector3(0.20f, 0.35f, 0.03f))));

            // Hex Bolt
            var bolt = BoltMeshBuilder.CreateDetailedBolt(device);
            catalog.RegisterDisposable(bolt);
            catalog.RegisterModel(new InspectableModel(
                "ISO Metric Flanged Bolt (Grade 10.9)",
                ModelCategory.ArmorAndFasteners,
                "High-tensile flanged hex bolt with continuous helical 60° V-threads.",
                new BoundingBox(new Vector3(-0.06f, -0.09f, -0.06f), new Vector3(0.06f, 0.03f, 0.06f)),
                bolt.HeadMesh.VertexCount + bolt.ShankMesh.VertexCount,
                bolt.HeadMesh.PrimitiveCount + bolt.ShankMesh.PrimitiveCount,
                (dev, eff, mode) =>
                {
                    eff.Texture = boltTex;
                    eff.DiffuseColor = Vector3.One;
                    eff.World = Matrix.Identity;
                    bolt.HeadMesh.Draw(dev, eff);
                    bolt.ShankMesh.Draw(dev, eff);
                }));
        }

        private static void BuildGantryRigModels(ModelCatalog catalog, GraphicsDevice device, Texture2D chassisTex)
        {
            // Spreader Beam
            var beam = MechGantryRigMeshBuilder.CreateSpreaderBeam(device);
            catalog.RegisterDisposable(beam);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Gantry Spreader Beam",
                ModelCategory.GantryRig,
                "Industrial yellow lifting I-beam with dual center crane eyelets and rigging shackles.",
                beam, chassisTex, new Vector3(0.95f, 0.85f, 0.15f),
                new BoundingBox(new Vector3(-1.8f, -0.15f, -0.2f), new Vector3(1.8f, 0.15f, 0.2f))));

            // Suspension Cable
            var cable = MechGantryRigMeshBuilder.CreateSuspensionCable(device);
            catalog.RegisterDisposable(cable);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Wire Rope Rigging Sling",
                ModelCategory.GantryRig,
                "Braided steel wire rope sling with swaged eye terminals suspending heavy loads.",
                cable, chassisTex, new Vector3(0.70f, 0.72f, 0.75f),
                new BoundingBox(new Vector3(-0.05f, -2.4f, -0.05f), new Vector3(0.05f, 0, 0.05f))));

            // Umbilical Boom
            var umbilical = MechGantryRigMeshBuilder.CreateUmbilicalBoom(device);
            catalog.RegisterDisposable(umbilical);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Articulated Umbilical Boom",
                ModelCategory.GantryRig,
                "Hydraulic & cryogenic coolant supply boom docked into the reactor core.",
                umbilical, chassisTex, new Vector3(0.85f, 0.50f, 0.20f),
                new BoundingBox(new Vector3(-0.05f, -0.05f, -0.05f), new Vector3(1.85f, 0.05f, 0.05f))));
        }
    }
}
