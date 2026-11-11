using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Inspector
{
    public class InspectorGridRenderer : IDisposable
    {
        private readonly BasicEffect _lineEffect;

        public InspectorGridRenderer(GraphicsDevice device)
        {
            _lineEffect = new BasicEffect(device)
            {
                VertexColorEnabled = true,
                LightingEnabled = false
            };
        }

        public void DrawGridAndBounds(
            GraphicsDevice device,
            Matrix view,
            Matrix proj,
            BoundingBox? bounds,
            bool drawGrid = true,
            bool drawBounds = true)
        {
            _lineEffect.View = view;
            _lineEffect.Projection = proj;
            _lineEffect.World = Matrix.Identity;

            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = DepthStencilState.Default;

            var lines = new List<VertexPositionColor>();

            if (drawGrid)
            {
                BuildGridLines(lines, 6.0f, 0.5f, 0.1f);
                BuildAxesLines(lines, 1.0f);
            }

            if (drawBounds && bounds.HasValue)
            {
                BuildBoundingBoxLines(lines, bounds.Value, new Color(255, 215, 0, 180));
            }

            if (lines.Count >= 2)
            {
                var verts = lines.ToArray();
                foreach (var pass in _lineEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserPrimitives(PrimitiveType.LineList, verts, 0, verts.Length / 2);
                }
            }
        }

        private static void BuildGridLines(List<VertexPositionColor> lines, float halfSize, float majorStep, float minorStep)
        {
            Color minorColor = new Color(45, 55, 65, 80);
            Color majorColor = new Color(80, 100, 120, 140);

            for (float x = -halfSize; x <= halfSize + 0.001f; x += minorStep)
            {
                bool isMajor = MathF.Abs(x % majorStep) < 0.01f || MathF.Abs(MathF.Abs(x % majorStep) - majorStep) < 0.01f;
                Color c = isMajor ? majorColor : minorColor;
                lines.Add(new VertexPositionColor(new Vector3(x, 0, -halfSize), c));
                lines.Add(new VertexPositionColor(new Vector3(x, 0, halfSize), c));
            }

            for (float z = -halfSize; z <= halfSize + 0.001f; z += minorStep)
            {
                bool isMajor = MathF.Abs(z % majorStep) < 0.01f || MathF.Abs(MathF.Abs(z % majorStep) - majorStep) < 0.01f;
                Color c = isMajor ? majorColor : minorColor;
                lines.Add(new VertexPositionColor(new Vector3(-halfSize, 0, z), c));
                lines.Add(new VertexPositionColor(new Vector3(halfSize, 0, z), c));
            }
        }

        private static void BuildAxesLines(List<VertexPositionColor> lines, float length)
        {
            // X-Axis = Red
            lines.Add(new VertexPositionColor(Vector3.Zero, Color.Red));
            lines.Add(new VertexPositionColor(new Vector3(length, 0, 0), Color.Red));

            // Y-Axis = Green
            lines.Add(new VertexPositionColor(Vector3.Zero, Color.LimeGreen));
            lines.Add(new VertexPositionColor(new Vector3(0, length, 0), Color.LimeGreen));

            // Z-Axis = Blue
            lines.Add(new VertexPositionColor(Vector3.Zero, Color.DeepSkyBlue));
            lines.Add(new VertexPositionColor(new Vector3(0, 0, length), Color.DeepSkyBlue));
        }

        private static void BuildBoundingBoxLines(List<VertexPositionColor> lines, BoundingBox b, Color color)
        {
            Vector3[] c = b.GetCorners();
            // Edges: 0-1, 1-2, 2-3, 3-0, 4-5, 5-6, 6-7, 7-4, 0-4, 1-5, 2-6, 3-7
            int[] pairs = { 0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 2, 6, 3, 7 };
            for (int i = 0; i < pairs.Length; i += 2)
            {
                lines.Add(new VertexPositionColor(c[pairs[i]], color));
                lines.Add(new VertexPositionColor(c[pairs[i + 1]], color));
            }
        }

        public void Dispose()
        {
            _lineEffect?.Dispose();
        }
    }
}
