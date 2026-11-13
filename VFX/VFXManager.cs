using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.VFX
{
    public class VFXManager
    {
        public SparkParticleSystem Sparks { get; }
        public SmokeParticleSystem Smoke { get; }
        public FluidParticleSystem Fluid { get; }
        public AerosolParticleSystem Aerosol { get; }
        public AtmosphericParticleSystem Atmosphere { get; }

        public VFXManager(Texture2D particleTex)
        {
            Sparks = new SparkParticleSystem(particleTex);
            Smoke = new SmokeParticleSystem(particleTex);
            Fluid = new FluidParticleSystem(particleTex);
            Aerosol = new AerosolParticleSystem(particleTex);
            Atmosphere = new AtmosphericParticleSystem(particleTex);
        }

        public void EmitGrinding(Vector3 hitPoint)
        {
            Sparks.EmitGrindingStream(hitPoint, Vector3.Right, 6);
        }

        public void EmitWelding(Vector3 hitPoint)
        {
            Sparks.EmitWeldingSpatter(hitPoint, 2);
            Smoke.Emit(hitPoint, 1);
        }

        public void EmitHydraulicBleed(Vector3 hitPoint)
        {
            Fluid.Emit(hitPoint, Vector3.Down, new Color(0.95f, 0.65f, 0.15f, 0.85f), 3);
        }

        public void EmitPaintMist(Vector3 hitPoint, Color paintColor)
        {
            Aerosol.Emit(hitPoint + new Vector3(0, 0, 0.08f), Vector3.Forward, paintColor, 4);
        }

        public void Update(float dt)
        {
            Sparks.Update(dt);
            Smoke.Update(dt);
            Fluid.Update(dt);
            Aerosol.Update(dt);
            Atmosphere.Update(dt);
        }

        public void Draw(GraphicsDevice device, BasicEffect effect, Matrix view, Matrix proj)
        {
            Atmosphere.Draw(device, effect, view, proj);
            Sparks.Draw(device, effect, view, proj);
            Smoke.Draw(device, effect, view, proj);
            Fluid.Draw(device, effect, view, proj);
            Aerosol.Draw(device, effect, view, proj);
        }
    }
}
