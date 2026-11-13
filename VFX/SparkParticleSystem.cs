using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.VFX
{
    public struct SparkParticle
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Life;
        public float MaxLife;
        public float Size;
    }

    public class SparkParticleSystem
    {
        private const int MaxParticles = 400;
        private readonly List<SparkParticle> _particles = new List<SparkParticle>(MaxParticles);
        private readonly VertexPositionColorTexture[] _vertexBuffer = new VertexPositionColorTexture[MaxParticles * 6];
        private readonly Random _rng = new Random(42);
        private Texture2D _glowTex;

        public SparkParticleSystem(Texture2D glowTex)
        {
            _glowTex = glowTex;
        }

        public void Emit(Vector3 origin, Vector3 direction, int count = 4)
        {
            EmitGrindingStream(origin, direction, count);
        }

        public void EmitGrindingStream(Vector3 origin, Vector3 direction, int count = 6)
        {
            for (int i = 0; i < count; i++)
            {
                if (_particles.Count >= MaxParticles) break;

                // Tangential grinding abrasive stream spread
                Vector3 spread = new Vector3(
                    ((float)_rng.NextDouble() - 0.5f) * 0.7f,
                    ((float)_rng.NextDouble() - 0.5f) * 0.5f + 0.3f,
                    ((float)_rng.NextDouble() - 0.5f) * 0.7f
                );

                Vector3 vel = (direction + spread) * ((float)_rng.NextDouble() * 4.2f + 2.5f);
                float maxLife = (float)_rng.NextDouble() * 0.40f + 0.18f;

                _particles.Add(new SparkParticle
                {
                    Position = origin,
                    Velocity = vel,
                    Life = maxLife,
                    MaxLife = maxLife,
                    Size = (float)_rng.NextDouble() * 0.032f + 0.018f
                });
            }
        }

        public void EmitWeldingSpatter(Vector3 origin, int count = 2)
        {
            for (int i = 0; i < count; i++)
            {
                if (_particles.Count >= MaxParticles) break;

                // High-angle globular molten spatter
                Vector3 vel = new Vector3(
                    ((float)_rng.NextDouble() - 0.5f) * 1.8f,
                    (float)_rng.NextDouble() * 2.2f + 1.0f,
                    ((float)_rng.NextDouble() - 0.5f) * 1.8f
                );
                float maxLife = (float)_rng.NextDouble() * 0.55f + 0.25f;

                _particles.Add(new SparkParticle
                {
                    Position = origin,
                    Velocity = vel,
                    Life = maxLife,
                    MaxLife = maxLife,
                    Size = (float)_rng.NextDouble() * 0.045f + 0.025f
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

                // Simple ground bounce
                if (p.Position.Y < 0.05f)
                {
                    p.Position = new Vector3(p.Position.X, 0.05f, p.Position.Z);
                    p.Velocity = new Vector3(p.Velocity.X * 0.5f, -p.Velocity.Y * 0.3f, p.Velocity.Z * 0.5f);
                }

                _particles[i] = p;
            }
        }

        public void Draw(GraphicsDevice device, BasicEffect effect, Matrix view, Matrix proj)
        {
            if (_particles.Count == 0 || _glowTex == null) return;

            device.BlendState = BlendState.Additive;
            device.DepthStencilState = DepthStencilState.DepthRead;
            device.RasterizerState = RasterizerState.CullNone;

            effect.World = Matrix.Identity;
            effect.View = view;
            effect.Projection = proj;
            effect.Texture = _glowTex;
            effect.TextureEnabled = true;
            effect.LightingEnabled = false;

            // Invert view to get billboard Right and Up
            Matrix invView = Matrix.Invert(view);
            Vector3 right = invView.Right;
            Vector3 up = invView.Up;

            int vertIdx = 0;
            int count = Math.Min(_particles.Count, MaxParticles);

            for (int i = 0; i < count; i++)
            {
                var p = _particles[i];
                float t = p.Life / p.MaxLife; // 1.0 down to 0.0
                // Premultiply by t for smooth fading in Additive blend state
                Color col = Color.Lerp(Color.DarkOrange, Color.White, t * t) * (t * t);

                float s = p.Size * (0.4f + 0.6f * t);

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

            // Restore defaults
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
        }
    }
}
