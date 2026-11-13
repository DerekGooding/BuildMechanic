using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics;
using BuildMechanic.Graphics.MeshBuilders;

namespace BuildMechanic.Inspector
{
    public static class ModelCatalogToolAssemblies
    {
        public static void RegisterToolAssemblies(
            ModelCatalog catalog, GraphicsDevice device, SimpleMesh box, SimpleMesh cyl,
            Texture2D metalTex, Texture2D gripTex, Texture2D gaugeTex)
        {
            var socket = ToolMeshBuilder.CreateImpactSocket(device);
            var disc = ToolMeshBuilder.CreateGrinderDisc(device);
            var shroud = ToolMeshBuilder.CreateWelderShroud(device);
            var cup = ToolMeshBuilder.CreateSprayCup(device);
            var pryBar = ToolMeshBuilder.CreatePryBarShaft(device);
            catalog.RegisterDisposable(socket);
            catalog.RegisterDisposable(disc);
            catalog.RegisterDisposable(shroud);
            catalog.RegisterDisposable(cup);
            catalog.RegisterDisposable(pryBar);

            BuildImpactDriver(catalog, box, cyl, socket, metalTex, gripTex);
            BuildAngleGrinder(catalog, box, cyl, disc, metalTex, gripTex);
            BuildTigWelderTorch(catalog, box, cyl, shroud, metalTex, gripTex);
            BuildHvlpSprayGun(catalog, box, cyl, cup, metalTex, gripTex);
            BuildDigitalMultimeter(catalog, box, cyl, metalTex, gripTex);
            BuildHydraulicBleeder(catalog, box, cyl, metalTex, gripTex, gaugeTex);
            BuildHeavyPryBar(catalog, box, cyl, pryBar, metalTex, gripTex);
        }

        private static void BuildImpactDriver(ModelCatalog catalog, SimpleMesh box, SimpleMesh cyl, SimpleMesh socket, Texture2D metalTex, Texture2D gripTex)
        {
            catalog.RegisterModel(new InspectableModel(
                "Impact Driver (Assembly)",
                ModelCategory.ShopTools,
                "Brushless 18V impact driver with rubberized handle, anvil collar, battery pack, and 19mm hex socket.",
                new BoundingBox(new Vector3(-0.08f, -0.22f, -0.14f), new Vector3(0.08f, 0.12f, 0.14f)),
                socket.VertexCount + 220, socket.PrimitiveCount + 120,
                (dev, eff, mode) =>
                {
                    eff.Texture = metalTex;
                    eff.DiffuseColor = new Vector3(0.85f, 0.88f, 0.95f);
                    eff.World = Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(0, 0, 0.08f);
                    socket.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.72f, 0.75f, 0.80f);
                    eff.World = Matrix.CreateScale(0.076f, 0.05f, 0.076f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(0, 0, 0.04f);
                    cyl.Draw(dev, eff);

                    eff.Texture = gripTex;
                    eff.DiffuseColor = new Vector3(0.95f, 0.45f, 0.12f);
                    eff.World = Matrix.CreateScale(0.11f, 0.12f, 0.15f);
                    box.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.20f, 0.22f, 0.25f);
                    eff.World = Matrix.CreateScale(0.06f, 0.15f, 0.07f) * Matrix.CreateTranslation(0, -0.12f, -0.04f);
                    box.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.15f, 0.16f, 0.18f);
                    eff.World = Matrix.CreateScale(0.10f, 0.05f, 0.14f) * Matrix.CreateTranslation(0, -0.20f, -0.03f);
                    box.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.2f, 1.6f, 0.3f);
                    eff.World = Matrix.CreateScale(0.02f, 0.01f, 0.01f) * Matrix.CreateTranslation(0, -0.19f, 0.045f);
                    box.Draw(dev, eff);
                }));
        }

        private static void BuildAngleGrinder(ModelCatalog catalog, SimpleMesh box, SimpleMesh cyl, SimpleMesh disc, Texture2D metalTex, Texture2D gripTex)
        {
            catalog.RegisterModel(new InspectableModel(
                "Angle Grinder (Assembly)",
                ModelCategory.ShopTools,
                "Heavy-duty angle grinder with cast gear housing, steel disc guard, cut-off disc, and side handle.",
                new BoundingBox(new Vector3(-0.14f, -0.06f, -0.16f), new Vector3(0.14f, 0.06f, 0.16f)),
                disc.VertexCount + 240, disc.PrimitiveCount + 130,
                (dev, eff, mode) =>
                {
                    eff.Texture = metalTex;
                    eff.DiffuseColor = new Vector3(0.35f, 0.32f, 0.30f);
                    eff.World = Matrix.CreateTranslation(0, -0.02f, 0.08f);
                    disc.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.18f, 0.20f, 0.22f);
                    eff.World = Matrix.CreateScale(0.11f, 0.016f, 0.07f) * Matrix.CreateTranslation(0, 0.005f, 0.06f);
                    box.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.72f, 0.75f, 0.80f);
                    eff.World = Matrix.CreateScale(0.065f, 0.055f, 0.065f) * Matrix.CreateTranslation(0, 0.02f, 0.08f);
                    box.Draw(dev, eff);

                    eff.Texture = gripTex;
                    eff.DiffuseColor = new Vector3(0.12f, 0.65f, 0.58f);
                    eff.World = Matrix.CreateScale(0.045f, 0.22f, 0.045f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(0, 0.02f, -0.08f);
                    cyl.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.25f, 0.28f, 0.30f);
                    eff.World = Matrix.CreateScale(0.038f, 0.12f, 0.038f) * Matrix.CreateRotationZ(1.57f) * Matrix.CreateTranslation(0.09f, 0.02f, 0.08f);
                    cyl.Draw(dev, eff);
                }));
        }

        private static void BuildTigWelderTorch(ModelCatalog catalog, SimpleMesh box, SimpleMesh cyl, SimpleMesh shroud, Texture2D metalTex, Texture2D gripTex)
        {
            catalog.RegisterModel(new InspectableModel(
                "TIG Welder Torch (Assembly)",
                ModelCategory.ShopTools,
                "Precision TIG welding torch: pink ceramic gas cup, exposed tungsten electrode, ribbed handle, and cable whip.",
                new BoundingBox(new Vector3(-0.06f, -0.22f, -0.16f), new Vector3(0.06f, 0.08f, 0.16f)),
                shroud.VertexCount + 200, shroud.PrimitiveCount + 110,
                (dev, eff, mode) =>
                {
                    eff.Texture = metalTex;
                    eff.DiffuseColor = new Vector3(0.95f, 0.65f, 0.70f);
                    eff.World = Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(0, 0, 0.06f);
                    shroud.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(1.4f, 1.4f, 1.5f);
                    eff.World = Matrix.CreateScale(0.006f, 0.06f, 0.006f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(0, 0, 0.11f);
                    cyl.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.85f, 0.68f, 0.28f);
                    eff.World = Matrix.CreateScale(0.032f, 0.04f, 0.032f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(0, 0, 0.02f);
                    cyl.Draw(dev, eff);

                    eff.Texture = gripTex;
                    eff.DiffuseColor = new Vector3(0.18f, 0.20f, 0.22f);
                    eff.World = Matrix.CreateScale(0.036f, 0.18f, 0.036f) * Matrix.CreateTranslation(0, -0.08f, -0.02f);
                    cyl.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.10f, 0.10f, 0.12f);
                    eff.World = Matrix.CreateScale(0.016f, 0.14f, 0.016f) * Matrix.CreateRotationX(-1.57f) * Matrix.CreateTranslation(0, 0, -0.06f);
                    cyl.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.12f, 0.12f, 0.14f);
                    eff.World = Matrix.CreateScale(0.022f, 0.12f, 0.022f) * Matrix.CreateTranslation(0, -0.19f, -0.04f);
                    cyl.Draw(dev, eff);
                }));
        }

        private static void BuildHvlpSprayGun(ModelCatalog catalog, SimpleMesh box, SimpleMesh cyl, SimpleMesh cup, Texture2D metalTex, Texture2D gripTex)
        {
            catalog.RegisterModel(new InspectableModel(
                "HVLP Paint Spray Gun (Assembly)",
                ModelCategory.ShopTools,
                "Precision HVLP spray gun: gravity feed reservoir cup, atomizing air cap, chrome trigger, and adjustment dials.",
                new BoundingBox(new Vector3(-0.08f, -0.20f, -0.14f), new Vector3(0.08f, 0.20f, 0.16f)),
                cup.VertexCount + 220, cup.PrimitiveCount + 120,
                (dev, eff, mode) =>
                {
                    eff.Texture = gripTex;
                    eff.DiffuseColor = new Vector3(0.90f, 0.88f, 0.80f);
                    eff.World = Matrix.CreateTranslation(0, 0.06f, 0.03f);
                    cup.Draw(dev, eff);

                    eff.Texture = metalTex;
                    eff.DiffuseColor = new Vector3(0.85f, 0.88f, 0.95f);
                    eff.World = Matrix.CreateScale(0.045f, 0.05f, 0.16f) * Matrix.CreateTranslation(0, 0.04f, 0.02f);
                    box.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.88f, 0.72f, 0.25f);
                    eff.World = Matrix.CreateScale(0.04f, 0.035f, 0.04f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(0, 0.04f, 0.12f);
                    cyl.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.40f, 0.44f, 0.48f);
                    eff.World = Matrix.CreateScale(0.038f, 0.14f, 0.05f) * Matrix.CreateRotationX(0.2f) * Matrix.CreateTranslation(0, -0.06f, -0.02f);
                    box.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.95f, 0.96f, 1.0f);
                    eff.World = Matrix.CreateScale(0.015f, 0.08f, 0.025f) * Matrix.CreateTranslation(0, -0.01f, 0.04f);
                    box.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.88f, 0.72f, 0.25f);
                    eff.World = Matrix.CreateScale(0.02f, 0.025f, 0.02f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(0, 0.04f, -0.07f);
                    cyl.Draw(dev, eff);
                }));
        }

        private static void BuildDigitalMultimeter(ModelCatalog catalog, SimpleMesh box, SimpleMesh cyl, Texture2D metalTex, Texture2D gripTex)
        {
            catalog.RegisterModel(new InspectableModel(
                "Ruggedized Digital Multimeter (Assembly)",
                ModelCategory.ShopTools,
                "High-voltage digital multimeter with rubber boot, LCD readout face, rotary selector switch, and probe leads.",
                new BoundingBox(new Vector3(-0.08f, -0.14f, -0.04f), new Vector3(0.08f, 0.14f, 0.04f)),
                240, 140,
                (dev, eff, mode) =>
                {
                    eff.Texture = gripTex;
                    eff.DiffuseColor = new Vector3(0.98f, 0.75f, 0.12f);
                    eff.World = Matrix.CreateScale(0.12f, 0.22f, 0.045f);
                    box.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.18f, 0.20f, 0.22f);
                    eff.World = Matrix.CreateScale(0.10f, 0.20f, 0.048f);
                    box.Draw(dev, eff);

                    eff.Texture = metalTex;
                    eff.DiffuseColor = new Vector3(0.25f, 0.65f, 0.55f);
                    eff.World = Matrix.CreateScale(0.08f, 0.05f, 0.005f) * Matrix.CreateTranslation(0, 0.055f, 0.026f);
                    box.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.28f, 0.30f, 0.32f);
                    eff.World = Matrix.CreateScale(0.05f, 0.012f, 0.05f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(0, -0.015f, 0.026f);
                    cyl.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.85f, 0.18f, 0.18f);
                    eff.World = Matrix.CreateScale(0.012f, 0.08f, 0.012f) * Matrix.CreateTranslation(0.06f, -0.08f, 0.02f);
                    cyl.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.15f, 0.15f, 0.18f);
                    eff.World = Matrix.CreateScale(0.012f, 0.08f, 0.012f) * Matrix.CreateTranslation(-0.06f, -0.08f, 0.02f);
                    cyl.Draw(dev, eff);
                }));
        }

        private static void BuildHydraulicBleeder(ModelCatalog catalog, SimpleMesh box, SimpleMesh cyl, Texture2D metalTex, Texture2D gripTex, Texture2D gaugeTex)
        {
            catalog.RegisterModel(new InspectableModel(
                "Hydraulic System Bleeder (Assembly)",
                ModelCategory.ShopTools,
                "Pressure bleeder unit with reservoir canister, 0-60 PSI analog dial gauge, quick coupler, and vinyl hose whip.",
                new BoundingBox(new Vector3(-0.10f, -0.22f, -0.10f), new Vector3(0.10f, 0.18f, 0.10f)),
                240, 130,
                (dev, eff, mode) =>
                {
                    eff.Texture = gripTex;
                    eff.DiffuseColor = new Vector3(0.85f, 0.88f, 0.90f);
                    eff.World = Matrix.CreateScale(0.10f, 0.18f, 0.10f) * Matrix.CreateTranslation(0, -0.08f, 0);
                    cyl.Draw(dev, eff);

                    eff.Texture = metalTex;
                    eff.DiffuseColor = new Vector3(0.20f, 0.45f, 0.85f);
                    eff.World = Matrix.CreateScale(0.108f, 0.035f, 0.108f) * Matrix.CreateTranslation(0, 0.02f, 0);
                    cyl.Draw(dev, eff);

                    eff.Texture = gaugeTex;
                    eff.DiffuseColor = Vector3.One;
                    eff.World = Matrix.CreateScale(0.06f, 0.02f, 0.06f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(0, 0.10f, 0.03f);
                    cyl.Draw(dev, eff);

                    eff.Texture = metalTex;
                    eff.DiffuseColor = new Vector3(0.88f, 0.72f, 0.25f);
                    eff.World = Matrix.CreateScale(0.022f, 0.05f, 0.022f) * Matrix.CreateRotationX(1.57f) * Matrix.CreateTranslation(0, 0.03f, 0.06f);
                    cyl.Draw(dev, eff);

                    eff.Texture = gripTex;
                    eff.DiffuseColor = new Vector3(0.92f, 0.95f, 0.98f);
                    eff.World = Matrix.CreateScale(0.014f, 0.16f, 0.014f) * Matrix.CreateRotationZ(0.2f) * Matrix.CreateTranslation(-0.04f, -0.06f, 0.06f);
                    cyl.Draw(dev, eff);
                }));
        }

        private static void BuildHeavyPryBar(ModelCatalog catalog, SimpleMesh box, SimpleMesh cyl, SimpleMesh pryBar, Texture2D metalTex, Texture2D gripTex)
        {
            catalog.RegisterModel(new InspectableModel(
                "Heavy Utility Pry Bar (Assembly)",
                ModelCategory.ShopTools,
                "Forged hex alloy alignment pry bar: cobalt blue shaft, tapered ground chisel wedge, and curved rocker heel.",
                new BoundingBox(new Vector3(-0.05f, -0.04f, -0.08f), new Vector3(0.05f, 0.42f, 0.08f)),
                pryBar.VertexCount + 100, pryBar.PrimitiveCount + 60,
                (dev, eff, mode) =>
                {
                    eff.Texture = metalTex;
                    eff.DiffuseColor = new Vector3(0.22f, 0.42f, 0.85f);
                    eff.World = Matrix.Identity;
                    pryBar.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.85f, 0.88f, 0.92f);
                    eff.World = Matrix.CreateScale(0.035f, 0.04f, 0.008f) * Matrix.CreateRotationX(-0.35f) * Matrix.CreateTranslation(0, 0.39f, 0.012f);
                    box.Draw(dev, eff);

                    eff.DiffuseColor = new Vector3(0.30f, 0.32f, 0.36f);
                    eff.World = Matrix.CreateScale(0.04f, 0.03f, 0.055f) * Matrix.CreateRotationX(0.5f) * Matrix.CreateTranslation(0, -0.015f, -0.02f);
                    box.Draw(dev, eff);
                }));
        }
    }
}
