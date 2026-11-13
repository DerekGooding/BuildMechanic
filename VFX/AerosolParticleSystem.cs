using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.VFX
{
    public struct AerosolParticle
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public Color Tint;
        public float Life;
        public float MaxLife;
        public float Size;
    }

    public class AerosolParticleSystem
    {
        private const int MaxParticles = 300;
        private readonly List<AerosolParticle> _particles = new List<AerosolParticle>(MaxParticles);
        private readonly VertexPositionColorTexture[] _vertexBuffer = new VertexPositionColorTexture[MaxParticles * 6];
        private readonly Random _rng = new Random(789);
        private readonly Texture2D _mistTex;

        public AerosolParticleSystem(Texture2D mistTex)
        {
            _mistTex = mistTex;
        }

        public void Emit(Vector3 origin, Vector3 direction, Color color, int count = 4)
        {
            for (int i = 0; i < count; i++)
            {
                if (_particles.Count >= MaxParticles) break;

                Vector3 spread = new Vector3(
                    ((float)_rng.NextDouble() - 0.5f) * 0.35f,
                    ((float)_rng.NextDouble() - 0.5f) * 0.35f,
                    ((float)_rng.NextDouble() - 0.5f) * 0.35f
                );

                Vector3 vel = (direction + spread) * ((float)_rng.NextDouble() * 1.8f + 1.2f);
                float maxLife = (float)_rng.NextDouble() * 0.45f + 0.25f;

                _particles.Add(new AerosolParticle
                {
                    Position = origin,
                    Velocity = vel,
                    Tint = color,
                    Life = maxLife,
                    MaxLife = maxLife,
                    Size = (float)_rng.NextDouble() * 0.045f + 0.035f
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

                p.Position += p.Velocity * dt;
                p.Velocity *= 0.94f; // Rapid aerosol drag
                p.Size += dt * 0.07f; // Expands as it atomizes

                _particles[i] = p;
            }
        }

        public void Draw(GraphicsDevice device, BasicEffect effect, Matrix view, Matrix proj)
        {
            if (_particles.Count == 0 || _mistTex == null) return;

            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.DepthRead;
            device.RasterizerState = RasterizerState.CullNone;

            effect.World = Matrix.Identity;
            effect.View = view;
            effect.Projection = proj;
            effect.Texture = _mistTex;
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
                Color col = p.Tint * (t * 0.75f);

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
