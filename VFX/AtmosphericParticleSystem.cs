using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.VFX
{
    public struct DustMote
    {
        public Vector3 Position;
        public Vector3 BasePosition;
        public float Phase;
        public float Speed;
        public float Size;
        public float Brightness;
    }

    public class AtmosphericParticleSystem
    {
        private const int MoteCount = 80;
        private readonly DustMote[] _motes = new DustMote[MoteCount];
        private readonly VertexPositionColorTexture[] _vertexBuffer = new VertexPositionColorTexture[MoteCount * 6];
        private readonly Texture2D _particleTex;
        private float _time;

        public AtmosphericParticleSystem(Texture2D particleTex)
        {
            _particleTex = particleTex;
            var rng = new Random(321);

            for (int i = 0; i < MoteCount; i++)
            {
                Vector3 pos = new Vector3(
                    ((float)rng.NextDouble() - 0.5f) * 8.0f,
                    (float)rng.NextDouble() * 4.5f + 0.2f,
                    ((float)rng.NextDouble() - 0.5f) * 6.0f
                );

                _motes[i] = new DustMote
                {
                    Position = pos,
                    BasePosition = pos,
                    Phase = (float)rng.NextDouble() * MathF.PI * 2f,
                    Speed = (float)rng.NextDouble() * 0.4f + 0.2f,
                    Size = (float)rng.NextDouble() * 0.015f + 0.008f,
                    Brightness = (float)rng.NextDouble() * 0.4f + 0.25f
                };
            }
        }

        public void Update(float dt)
        {
            _time += dt;
            for (int i = 0; i < MoteCount; i++)
            {
                var m = _motes[i];
                float p = m.Phase + _time * m.Speed;
                m.Position = m.BasePosition + new Vector3(
                    MathF.Sin(p) * 0.25f,
                    MathF.Sin(p * 0.7f) * 0.15f,
                    MathF.Cos(p * 0.8f) * 0.25f
                );
                _motes[i] = m;
            }
        }

        public void Draw(GraphicsDevice device, BasicEffect effect, Matrix view, Matrix proj)
        {
            if (_particleTex == null) return;

            device.BlendState = BlendState.Additive;
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

            for (int i = 0; i < MoteCount; i++)
            {
                var m = _motes[i];
                float shimmer = (MathF.Sin(m.Phase + _time * 2f) * 0.2f + 0.8f) * m.Brightness;
                Color col = new Color(0.85f * shimmer, 0.90f * shimmer, 1.0f * shimmer, shimmer);

                float s = m.Size;
                Vector3 p0 = m.Position - right * s - up * s;
                Vector3 p1 = m.Position + right * s - up * s;
                Vector3 p2 = m.Position + right * s + up * s;
                Vector3 p3 = m.Position - right * s + up * s;

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
