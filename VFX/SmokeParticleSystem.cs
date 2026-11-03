using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.VFX
{
    public struct SmokeParticle
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Life;
        public float MaxLife;
        public float Size;
    }

    public class SmokeParticleSystem
    {
        private const int MaxParticles = 200;
        private readonly List<SmokeParticle> _particles = new List<SmokeParticle>(MaxParticles);
        private readonly VertexPositionColorTexture[] _vertexBuffer = new VertexPositionColorTexture[MaxParticles * 6];
        private readonly Random _rng = new Random(888);
        private readonly Texture2D _smokeTex;

        public SmokeParticleSystem(Texture2D smokeTex)
        {
            _smokeTex = smokeTex;
        }

        public void Emit(Vector3 origin, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                if (_particles.Count >= MaxParticles) break;

                Vector3 vel = new Vector3(
                    ((float)_rng.NextDouble() - 0.5f) * 0.25f,
                    (float)_rng.NextDouble() * 0.45f + 0.35f, // Drifts upward
                    ((float)_rng.NextDouble() - 0.5f) * 0.25f
                );

                float maxLife = (float)_rng.NextDouble() * 0.6f + 0.5f;

                _particles.Add(new SmokeParticle
                {
                    Position = origin,
                    Velocity = vel,
                    Life = maxLife,
                    MaxLife = maxLife,
                    Size = (float)_rng.NextDouble() * 0.06f + 0.05f
                });
            }
        }

        public void Update(float dt)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Life -= dt;
                if (p.Life <= 0)
                {
                    _particles.RemoveAt(i);
                    continue;
                }

                // Rising and slight deceleration
                p.Position += p.Velocity * dt;
                p.Velocity *= 0.98f;
                p.Size += dt * 0.08f; // Expands as it rises

                _particles[i] = p;
            }
        }

        public void Draw(GraphicsDevice device, BasicEffect effect, Matrix view, Matrix proj)
        {
            if (_particles.Count == 0 || _smokeTex == null) return;

            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.DepthRead;
            device.RasterizerState = RasterizerState.CullNone;

            effect.World = Matrix.Identity;
            effect.View = view;
            effect.Projection = proj;
            effect.Texture = _smokeTex;
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
                float t = p.Life / p.MaxLife; // 1.0 down to 0.0
                float alpha = MathF.Sin(t * MathF.PI) * 0.65f; // Fade in and out smoothly
                Color col = new Color(200, 205, 215) * alpha;

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
