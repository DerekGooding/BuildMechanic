using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics;
using BuildMechanic.Graphics.MeshBuilders;

namespace BuildMechanic.Inspector
{
    public static class ModelCatalogToolBuilder
    {
        public static void BuildToolsAndEnvironment(
            ModelCatalog catalog,
            GraphicsDevice device,
            Texture2D chassisTex,
            Texture2D metalTex,
            Texture2D gripTex,
            Texture2D gaugeTex,
            Texture2D pixelTex)
        {
            BuildShopToolModels(catalog, device, metalTex, gripTex, gaugeTex);
            BuildSubsystemModels(catalog, device, chassisTex, pixelTex);
            BuildHangarEnvironmentModels(catalog, device, chassisTex);
        }

        private static void BuildShopToolModels(
            ModelCatalog catalog,
            GraphicsDevice device,
            Texture2D metalTex,
            Texture2D gripTex,
            Texture2D gaugeTex)
        {
            // 1. Impact Socket
            var socket = ToolMeshBuilder.CreateImpactSocket(device);
            catalog.RegisterDisposable(socket);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Impact Socket (19mm Hex)",
                ModelCategory.ShopTools,
                "Chromium-vanadium 6-point hex socket engineered for heavy impact torque.",
                socket, metalTex, new Vector3(0.35f, 0.38f, 0.42f),
                new BoundingBox(new Vector3(-0.03f, 0, -0.03f), new Vector3(0.03f, 0.09f, 0.03f))));

            // 2. Grinder Guard & Disc
            var disc = ToolMeshBuilder.CreateGrinderDisc(device);
            catalog.RegisterDisposable(disc);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Abrasive Cut-Off Disc",
                ModelCategory.ShopTools,
                "Reinforced aluminum oxide cutting disc designed for weld grinding.",
                disc, metalTex, new Vector3(0.40f, 0.42f, 0.45f),
                new BoundingBox(new Vector3(-0.09f, 0, -0.09f), new Vector3(0.09f, 0.01f, 0.09f))));

            // 3. Hex Pry Bar
            var pryBar = ToolMeshBuilder.CreatePryBarShaft(device);
            catalog.RegisterDisposable(pryBar);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Hex Alloy Pry Bar Shaft",
                ModelCategory.ShopTools,
                "Blue powder-coated hex alloy lever shaft with ground fulcrum tip.",
                pryBar, metalTex, new Vector3(0.25f, 0.45f, 0.85f),
                new BoundingBox(new Vector3(-0.02f, 0, -0.02f), new Vector3(0.02f, 0.38f, 0.02f))));

            // 4. Welder Ceramic Shroud
            var shroud = ToolMeshBuilder.CreateWelderShroud(device);
            catalog.RegisterDisposable(shroud);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Ceramic TIG Gas Shroud Nozzle",
                ModelCategory.ShopTools,
                "High-temperature pink alumina ceramic gas diffuser nozzle cup.",
                shroud, metalTex, new Vector3(0.92f, 0.65f, 0.70f),
                new BoundingBox(new Vector3(-0.03f, 0, -0.03f), new Vector3(0.03f, 0.07f, 0.03f))));

            // 5. Spray Gun Paint Cup
            var cup = ToolMeshBuilder.CreateSprayCup(device);
            catalog.RegisterDisposable(cup);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Spray Gun Gravity Feed Cup",
                ModelCategory.ShopTools,
                "Translucent nylon paint reservoir cup with screw-locking lid.",
                cup, gripTex, new Vector3(0.92f, 0.82f, 0.25f),
                new BoundingBox(new Vector3(-0.05f, 0, -0.05f), new Vector3(0.05f, 0.12f, 0.05f))));
        }

        private static void BuildSubsystemModels(
            ModelCatalog catalog,
            GraphicsDevice device,
            Texture2D chassisTex,
            Texture2D pixelTex)
        {
            // Banjo Fitting
            var banjo = SubsystemMeshBuilder.CreateBanjoFitting(device);
            catalog.RegisterDisposable(banjo);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Banjo Hydraulic Coupling",
                ModelCategory.SubsystemsAndWelds,
                "High-pressure hydraulic banjo coupling fitting with integral fluid porting.",
                banjo, chassisTex, new Vector3(0.85f, 0.65f, 0.25f),
                new BoundingBox(new Vector3(-0.03f, 0, -0.03f), new Vector3(0.03f, 0.02f, 0.03f))));

            // DIN Rail
            var dinRail = SubsystemMeshBuilder.CreateDinRail(device);
            catalog.RegisterDisposable(dinRail);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Avionics DIN Rail Bracket",
                ModelCategory.SubsystemsAndWelds,
                "Standard 35mm slotted top-hat DIN rail for relay and avionics mounting.",
                dinRail, chassisTex, new Vector3(0.75f, 0.78f, 0.80f),
                new BoundingBox(new Vector3(-0.02f, -0.10f, -0.005f), new Vector3(0.02f, 0.10f, 0.005f))));

            // Weld Bead
            var weld = CrackMeshBuilder.CreateWeldBead(device);
            catalog.RegisterDisposable(weld);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Scalloped Weld Bead Seam",
                ModelCategory.SubsystemsAndWelds,
                "Multi-pass scalloped TIG weld repair seam with HAZ thermal temper rings.",
                weld, pixelTex, new Vector3(0.95f, 0.65f, 0.35f),
                new BoundingBox(new Vector3(-0.02f, 0, -0.50f), new Vector3(0.02f, 0.03f, 0.50f))));
        }

        private static void BuildHangarEnvironmentModels(
            ModelCatalog catalog,
            GraphicsDevice device,
            Texture2D chassisTex)
        {
            // Workbench
            var bench = MeshPrimitives.CreateBox(device, new Vector3(1.6f, 0.85f, 0.8f));
            catalog.RegisterDisposable(bench);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Heavy Industrial Workbench",
                ModelCategory.HangarEnvironment,
                "Welded steel workstation table with thick beechwood top and tool drawers.",
                bench, chassisTex, new Vector3(0.65f, 0.68f, 0.72f),
                new BoundingBox(new Vector3(-0.80f, -0.425f, -0.40f), new Vector3(0.80f, 0.425f, 0.40f))));

            // Bench Vise
            var vise = HangarMeshBuilder.CreateBenchVise(device);
            catalog.RegisterDisposable(vise);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Serrated Bench Vise",
                ModelCategory.HangarEnvironment,
                "Heavy bench-mounted vise with hardened serrated clamping jaws and lead screw.",
                vise, chassisTex, new Vector3(0.35f, 0.55f, 0.75f),
                new BoundingBox(new Vector3(-0.10f, -0.08f, -0.12f), new Vector3(0.10f, 0.08f, 0.12f))));

            // Crane Hook
            var hook = HangarMeshBuilder.CreateCraneHook(device);
            catalog.RegisterDisposable(hook);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Forged Crane Swivel Hook",
                ModelCategory.HangarEnvironment,
                "Heavy forged alloy steel swivel crane hook for gantry rigging.",
                hook, chassisTex, new Vector3(0.95f, 0.75f, 0.15f),
                new BoundingBox(new Vector3(-0.15f, -0.15f, -0.04f), new Vector3(0.15f, 0.05f, 0.04f))));

            // Lamp Shade
            var lamp = HangarMeshBuilder.CreateLampShade(device);
            catalog.RegisterDisposable(lamp);
            catalog.RegisterModel(InspectableModel.FromSimpleMesh(
                "Halogen Task Lamp Reflector",
                ModelCategory.HangarEnvironment,
                "High-intensity conical halogen reflector hood providing workbench illumination.",
                lamp, chassisTex, new Vector3(0.95f, 0.85f, 0.25f),
                new BoundingBox(new Vector3(-0.11f, -0.06f, -0.11f), new Vector3(0.11f, 0.06f, 0.11f))));
        }
    }
}
