using System;
using Microsoft.Xna.Framework;
using BuildMechanic.Inspector;
using Xunit;

namespace BuildMechanic.Tests.Inspector
{
    public class ModelCatalogTests
    {
        [Fact]
        public void ModelCatalog_EnumCategories_HasAll7Categories()
        {
            var catalog = new ModelCatalog();
            Assert.Equal(7, catalog.Categories.Count);
            Assert.Contains(ModelCategory.ChassisAndCore, catalog.Categories);
            Assert.Contains(ModelCategory.LimbsAndJoints, catalog.Categories);
            Assert.Contains(ModelCategory.ArmorAndFasteners, catalog.Categories);
            Assert.Contains(ModelCategory.GantryRig, catalog.Categories);
            Assert.Contains(ModelCategory.ShopTools, catalog.Categories);
            Assert.Contains(ModelCategory.SubsystemsAndWelds, catalog.Categories);
            Assert.Contains(ModelCategory.HangarEnvironment, catalog.Categories);
        }

        [Fact]
        public void ModelCatalog_RegistrationAndNavigation_WorksCorrectly()
        {
            var catalog = new ModelCatalog();

            var model1 = new InspectableModel(
                "Test Model 1",
                ModelCategory.ChassisAndCore,
                "Description 1",
                new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1)),
                100, 50,
                (dev, eff, mode) => { });

            var model2 = new InspectableModel(
                "Test Model 2",
                ModelCategory.ChassisAndCore,
                "Description 2",
                new BoundingBox(new Vector3(-2, -2, -2), new Vector3(2, 2, 2)),
                200, 100,
                (dev, eff, mode) => { });

            var model3 = new InspectableModel(
                "Test Tool",
                ModelCategory.ShopTools,
                "Tool Description",
                new BoundingBox(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f)),
                80, 40,
                (dev, eff, mode) => { });

            catalog.RegisterModel(model1);
            catalog.RegisterModel(model2);
            catalog.RegisterModel(model3);

            Assert.Equal(3, catalog.GetTotalModelCount());
            Assert.Equal(2, catalog.ActiveCategoryModels.Count);
            Assert.Equal("Test Model 1", catalog.ActiveModel?.Name);

            // Navigate models
            catalog.NextModel();
            Assert.Equal("Test Model 2", catalog.ActiveModel?.Name);
            catalog.NextModel();
            Assert.Equal("Test Model 1", catalog.ActiveModel?.Name); // Wraps around

            catalog.PrevModel();
            Assert.Equal("Test Model 2", catalog.ActiveModel?.Name); // Wraps backwards

            // Switch category
            catalog.SelectCategory(ModelCategory.ShopTools);
            Assert.Single(catalog.ActiveCategoryModels);
            Assert.Equal("Test Tool", catalog.ActiveModel?.Name);

            // Category navigation
            catalog.NextCategory();
            Assert.Equal(ModelCategory.SubsystemsAndWelds, catalog.ActiveCategory);

            catalog.Dispose();
            Assert.Equal(0, catalog.GetTotalModelCount());
        }

        [Fact]
        public void InspectableModel_CalculatesValidBoundsAndCounts()
        {
            var bounds = new BoundingBox(new Vector3(-0.5f, -1.0f, -0.2f), new Vector3(0.5f, 1.0f, 0.2f));
            var model = new InspectableModel(
                "Cockpit",
                ModelCategory.ChassisAndCore,
                "Sloped armored canopy",
                bounds,
                350, 180,
                (dev, eff, mode) => { });

            Assert.Equal("Cockpit", model.Name);
            Assert.Equal(ModelCategory.ChassisAndCore, model.Category);
            Assert.Equal(350, model.VertexCount);
            Assert.Equal(180, model.TriangleCount);

            Vector3 dims = model.Bounds.Max - model.Bounds.Min;
            Assert.True(dims.X > 0f);
            Assert.True(dims.Y > 0f);
            Assert.True(dims.Z > 0f);
            Assert.Equal(1.0f, dims.X, 0.001f);
            Assert.Equal(2.0f, dims.Y, 0.001f);
            Assert.Equal(0.4f, dims.Z, 0.001f);
        }

        [Fact]
        public void ModelCatalog_CategoryWrappingAndClamping_OperatesSafely()
        {
            var catalog = new ModelCatalog();

            // PrevCategory from first should wrap to last category
            catalog.SelectCategory(ModelCategory.ChassisAndCore);
            catalog.PrevCategory();
            Assert.Equal(ModelCategory.HangarEnvironment, catalog.ActiveCategory);

            // NextCategory from last should wrap to first
            catalog.NextCategory();
            Assert.Equal(ModelCategory.ChassisAndCore, catalog.ActiveCategory);

            // Empty category ActiveModel should be null
            Assert.Null(catalog.ActiveModel);
            Assert.Empty(catalog.ActiveCategoryModels);

            // SelectModel on empty category does not crash
            catalog.SelectModel(99);
            Assert.Equal(0, catalog.ActiveModelIndex);
        }
    }
}
