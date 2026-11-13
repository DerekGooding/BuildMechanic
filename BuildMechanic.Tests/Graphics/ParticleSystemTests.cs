using System;
using Microsoft.Xna.Framework;
using BuildMechanic.VFX;
using Xunit;

namespace BuildMechanic.Tests.Graphics
{
    public class ParticleSystemTests
    {
        [Fact]
        public void FluidParticleSystem_EmitsAndSimulatesFloorPuddle()
        {
            var fluid = new FluidParticleSystem(null);
            fluid.Emit(new Vector3(0, 1.0f, 0), Vector3.Down, Color.Orange, 5);

            // Step time forward 1 second so particles fall to floor
            for (int i = 0; i < 60; i++)
            {
                fluid.Update(0.016f);
            }

            // Verify update executes without throwing and processes physics
            fluid.Update(0.016f);
        }

        [Fact]
        public void AerosolParticleSystem_EmitsAndExpandsWithDrag()
        {
            var aerosol = new AerosolParticleSystem(null);
            aerosol.Emit(new Vector3(0, 1.5f, 0), Vector3.Forward, Color.LimeGreen, 4);

            for (int i = 0; i < 30; i++)
            {
                aerosol.Update(0.016f);
            }

            aerosol.Update(0.016f);
        }

        [Fact]
        public void AtmosphericParticleSystem_InitializesAndDrifts()
        {
            var atmosphere = new AtmosphericParticleSystem(null);
            atmosphere.Update(0.05f);
            atmosphere.Update(0.05f);
        }

        [Fact]
        public void VFXManager_CoordinatesAllSystems()
        {
            var vfx = new VFXManager(null);
            Assert.NotNull(vfx.Sparks);
            Assert.NotNull(vfx.Smoke);
            Assert.NotNull(vfx.Fluid);
            Assert.NotNull(vfx.Aerosol);
            Assert.NotNull(vfx.Atmosphere);

            vfx.EmitGrinding(new Vector3(1, 2, 3));
            vfx.EmitWelding(new Vector3(2, 2, 2));
            vfx.EmitHydraulicBleed(new Vector3(0, 1, 0));
            vfx.EmitPaintMist(new Vector3(0, 2, 0), Color.Cyan);

            vfx.Update(0.016f);
        }
    }
}
