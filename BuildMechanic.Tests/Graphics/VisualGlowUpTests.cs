using System;
using System.IO;
using BuildMechanic.Graphics;
using BuildMechanic.Simulation;
using Microsoft.Xna.Framework;
using Xunit;

namespace BuildMechanic.Tests.Graphics
{
    public class VisualGlowUpTests
    {
        [Fact]
        public void Palette_ContainsExpectedFinishes()
        {
            Assert.Equal(5, ArmorDecal.PaintPalette.Length);
            Assert.Equal(5, ArmorDecal.PaletteNames.Length);
            Assert.Equal("FACTORY STEEL", ArmorDecal.PaletteNames[0]);
            Assert.Equal("MILITARY OLIVE", ArmorDecal.PaletteNames[1]);
            Assert.Equal("HAZARD YELLOW", ArmorDecal.PaletteNames[2]);
            Assert.Equal("STEALTH CARBON", ArmorDecal.PaletteNames[3]);
            Assert.Equal("DESERT TAN", ArmorDecal.PaletteNames[4]);
        }

        [Fact]
        public void Bolt_ThreadRotation_MaintainsPitchProportionality()
        {
            var bolt = new Bolt();
            Assert.Equal(0f, bolt.SpinAngle);

            // Driving by -0.1 delta should rotate by 4.5 radians (-0.1 * 45)
            bolt.Drive(-0.1f);
            Assert.Equal(-4.5f, bolt.SpinAngle, 3);

            // Thread progress decreases to 0.9
            Assert.Equal(0.9f, bolt.ThreadProgress, 3);
        }

        [Fact]
        public void AllModularRendererAndBuilderSourceFiles_ExistInRepository()
        {
            string? dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "BuildMechanic.sln")) && !File.Exists(Path.Combine(dir, "BuildMechanic.csproj")))
            {
                dir = Directory.GetParent(dir)?.FullName;
            }
            Assert.NotNull(dir);
            string coreDir = File.Exists(Path.Combine(dir, "BuildMechanic.csproj")) ? dir : Path.Combine(dir, "BuildMechanic");

            string[] expectedFiles = new[]
            {
                "Graphics/Textures/ThreadTextureGenerator.cs",
                "Graphics/Textures/ArmorTextureGenerator.cs",
                "Graphics/Textures/HangarTextureGenerator.cs",
                "Graphics/Textures/ToolTextureGenerator.cs",
                "Graphics/MeshBuilders/BoltMeshBuilder.cs",
                "Graphics/MeshBuilders/MechChassisMeshBuilder.cs",
                "Graphics/MeshBuilders/ArmorPlateMeshBuilder.cs",
                "Graphics/MeshBuilders/ToolMeshBuilder.cs",
                "Graphics/MeshBuilders/SubsystemMeshBuilder.cs",
                "Graphics/MeshBuilders/HangarMeshBuilder.cs",
                "Graphics/MeshBuilders/CrackMeshBuilder.cs",
                "Graphics/Renderers/BoltRenderer.cs",
                "Graphics/Renderers/ChassisRenderer.cs",
                "Graphics/Renderers/ArmorPlateRenderer.cs",
                "Graphics/Renderers/HangarRenderer.cs",
                "Graphics/Renderers/CrackRenderer.cs",
                "Graphics/Renderers/LightingManager.cs"
            };

            foreach (var relPath in expectedFiles)
            {
                string fullPath = Path.Combine(coreDir, relPath.Replace('/', Path.DirectorySeparatorChar));
                Assert.True(File.Exists(fullPath), $"Expected visual component file '{relPath}' not found at {fullPath}");
            }
        }
    }
}
