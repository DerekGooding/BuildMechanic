using System;
using System.IO;
using System.Linq;
using Xunit;

namespace BuildMechanic.Tests.Graphics
{
    public class ModularityLineCountTests
    {
        [Fact]
        public void AllCoreCodebaseFiles_MustNotExceed_300Lines()
        {
            // Find repo root directory
            string? dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "BuildMechanic.sln")) && !File.Exists(Path.Combine(dir, "BuildMechanic.csproj")))
            {
                dir = Directory.GetParent(dir)?.FullName;
            }

            Assert.NotNull(dir);

            // Target the core game project directory
            string coreDir = File.Exists(Path.Combine(dir, "BuildMechanic.csproj")) ? dir : Path.Combine(dir, "BuildMechanic");

            var csFiles = Directory.GetFiles(coreDir, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar) &&
                            !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar) &&
                            !f.Contains(Path.DirectorySeparatorChar + "BuildMechanic.Tests" + Path.DirectorySeparatorChar))
                .ToList();

            Assert.NotEmpty(csFiles);

            foreach (var file in csFiles)
            {
                int lineCount = File.ReadAllLines(file).Length;
                string relativePath = Path.GetRelativePath(coreDir, file);

                Assert.True(lineCount <= 300,
                    $"File '{relativePath}' exceeds 300 lines (current count: {lineCount}). Refactor modularly into dedicated components.");
            }
        }
    }
}
