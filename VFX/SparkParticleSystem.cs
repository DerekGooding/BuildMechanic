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
        private readonly List<SparkParticle> _particles = new List<SparkParticle>(500);
        private readonly Random _rng = new Random(42);
        private Texture2D _glowTex;

        public SparkParticleSystem(Texture2D glowTex)
        {
            _glowTex = glowTex;
        }

        public void Emit(Vector3 origin, Vector3 direction, int count = 4)
        {
            for (int i = 0; i < count; i++)
            {
                if (_particles.Count >= 400) break;

                // Random cone spread
                Vector3 spread = new Vector3(
                    ((float)_rng.NextDouble() - 0.5f) * 0.8f,
                    ((float)_rng.NextDouble() - 0.5f) * 0.6f + 0.4f,
                    ((float)_rng.NextDouble() - 0.5f) * 0.8f
                );

                Vector3 vel = (direction + spread) * ((float)_rng.NextDouble() * 3.5f + 2.0f);
                float maxLife = (float)_rng.NextDouble() * 0.35f + 0.15f;

                _particles.Add(new SparkParticle
                {
                    Position = origin,
                    Velocity = vel,
                    Life = maxLife,
                    MaxLife = maxLife,
                    Size = (float)_rng.NextDouble() * 0.035f + 0.02f
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

            var vertices = new List<VertexPositionColorTexture>(_particles.Count * 6);

            foreach (var p in _particles)
            {
                float t = p.Life / p.MaxLife; // 1.0 down to 0.0
                Color col = Color.Lerp(Color.DarkOrange, Color.White, t * t);
                col.A = (byte)(t * 255);

                float s = p.Size * (0.4f + 0.6f * t);

                Vector3 p0 = p.Position - right * s - up * s;
                Vector3 p1 = p.Position + right * s - up * s;
                Vector3 p2 = p.Position + right * s + up * s;
                Vector3 p3 = p.Position - right * s + up * s;

                vertices.Add(new VertexPositionColorTexture(p0, col, new Vector2(0, 1)));
                vertices.Add(new VertexPositionColorTexture(p1, col, new Vector2(1, 1)));
                vertices.Add(new VertexPositionColorTexture(p2, col, new Vector2(1, 0)));

                vertices.Add(new VertexPositionColorTexture(p0, col, new Vector2(0, 1)));
                vertices.Add(new VertexPositionColorTexture(p2, col, new Vector2(1, 0)));
                vertices.Add(new VertexPositionColorTexture(p3, col, new Vector2(0, 0)));
            }

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3);
            }

            // Restore defaults
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
        }
    }
}
