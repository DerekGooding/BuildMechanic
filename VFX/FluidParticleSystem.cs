using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.VFX
{
    public struct FluidParticle
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public Color Tint;
        public float Life;
        public float MaxLife;
        public float Size;
    }

    public class FluidParticleSystem
    {
        private const int MaxParticles = 250;
        private readonly List<FluidParticle> _particles = new List<FluidParticle>(MaxParticles);
        private readonly VertexPositionColorTexture[] _vertexBuffer = new VertexPositionColorTexture[MaxParticles * 6];
        private readonly Random _rng = new Random(456);
        private readonly Texture2D _particleTex;

        public FluidParticleSystem(Texture2D particleTex)
        {
            _particleTex = particleTex;
        }

        public void Emit(Vector3 origin, Vector3 direction, Color tint, int count = 3)
        {
            for (int i = 0; i < count; i++)
            {
                if (_particles.Count >= MaxParticles) break;

                Vector3 spread = new Vector3(
                    ((float)_rng.NextDouble() - 0.5f) * 0.4f,
                    -((float)_rng.NextDouble() * 0.5f + 0.5f), // Drips downward
                    ((float)_rng.NextDouble() - 0.5f) * 0.4f
                );

                Vector3 vel = (direction * 0.5f + spread) * ((float)_rng.NextDouble() * 1.5f + 0.8f);
                float maxLife = (float)_rng.NextDouble() * 0.6f + 0.4f;

                _particles.Add(new FluidParticle
                {
                    Position = origin,
                    Velocity = vel,
                    Tint = tint,
                    Life = maxLife,
                    MaxLife = maxLife,
                    Size = (float)_rng.NextDouble() * 0.025f + 0.015f
                });
            }
        }

        public void Update(float dt)
        {
            Vector3 gravity = new Vector3(0, -9.8f, 0);

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Life -= dt;
                if (p.Life <= 0)
                {
                    _particles.RemoveAt(i);
                    continue;
                }

                p.Velocity += gravity * dt;
                p.Position += p.Velocity * dt;

                // Splat & puddle spread on floor
                if (p.Position.Y < 0.05f)
                {
                    p.Position = new Vector3(p.Position.X, 0.05f, p.Position.Z);
                    p.Velocity = Vector3.Zero;
                    p.Size += dt * 0.03f; // Expands outward
                }

                _particles[i] = p;
            }
        }

        public void Draw(GraphicsDevice device, BasicEffect effect, Matrix view, Matrix proj)
        {
            if (_particles.Count == 0 || _particleTex == null) return;

            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.DepthRead;
            device.RasterizerState = RasterizerState.CullNone;

            effect.World = Matrix.Identity;
            effect.View = view;
            effect.Projection = proj;
            effect.Texture = _particleTex;
            effect.TextureEnabled = true;
            effect.LightingEnabled = false;

            Matrix invView = Matrix.Invert(view);
            Vector3 right = invView.Right;
            Vector3 up = invView.Up;

            int vertIdx = 0;
            int count = Math.Min(_particles.Count, MaxParticles);

            for (int i = 0; i < count; i++)
            {
                var p = _particles[i];
                float t = p.Life / p.MaxLife;
                Color col = p.Tint * Math.Clamp(t * 1.5f, 0f, 1f);

                float s = p.Size;
                Vector3 p0 = p.Position - right * s - up * s;
                Vector3 p1 = p.Position + right * s - up * s;
                Vector3 p2 = p.Position + right * s + up * s;
                Vector3 p3 = p.Position - right * s + up * s;

                _vertexBuffer[vertIdx++] = new VertexPositionColorTexture(p0, col, new Vector2(0, 1));
                _vertexBuffer[vertIdx++] = new VertexPositionColorTexture(p1, col, new Vector2(1, 1));
                _vertexBuffer[vertIdx++] = new VertexPositionColorTexture(p2, col, new Vector2(1, 0));

                _vertexBuffer[vertIdx++] = new VertexPositionColorTexture(p0, col, new Vector2(0, 1));
                _vertexBuffer[vertIdx++] = new VertexPositionColorTexture(p2, col, new Vector2(1, 0));
                _vertexBuffer[vertIdx++] = new VertexPositionColorTexture(p3, col, new Vector2(0, 0));
            }

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertexBuffer, 0, vertIdx / 3);
            }

            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
        }
    }
}
