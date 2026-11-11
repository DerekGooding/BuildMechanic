using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.Textures
{
    public static class ToolTextureGenerator
    {
        public static Texture2D CreateBrushedMetalTexture(GraphicsDevice device, int size = 256)
        {
            var tex = new Texture2D(device, size, size);
            var data = new Color[size * size];
            var rng = new Random(55);

            for (int y = 0; y < size; y++)
            {
                // Fine horizontal brush streaks
                float streak = ((float)rng.NextDouble() - 0.5f) * 0.12f;
                for (int x = 0; x < size; x++)
                {
                    float noise = ((float)rng.NextDouble() - 0.5f) * 0.04f;
                    byte val = (byte)Math.Clamp((int)(175 * (1.0f + streak + noise)), 0, 255);
                    data[y * size + x] = new Color(val, val, (byte)Math.Clamp(val + 8, 0, 255));
                }
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D CreateRubberGripTexture(GraphicsDevice device, int size = 128)
        {
            var tex = new Texture2D(device, size, size);
            var data = new Color[size * size];

            Color gripDark = new Color(22, 23, 26);
            Color knurlPeak = new Color(52, 55, 62);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int kx = x % 16 - 8;
                    int ky = y % 16 - 8;
                    bool isPeak = (kx * kx + ky * ky) < 18;
                    data[y * size + x] = isPeak ? knurlPeak : gripDark;
                }
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D CreatePressureGaugeTexture(GraphicsDevice device, int size = 128)
        {
            var tex = new Texture2D(device, size, size);
            var data = new Color[size * size];
            float center = size / 2f;

            Color rimSteel = new Color(200, 205, 215);
            Color dialWhite = new Color(245, 246, 248);
            Color safeGreen = new Color(45, 175, 65);
            Color dangerRed = new Color(210, 45, 35);
            Color needleBlack = new Color(20, 20, 24);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = MathF.Sqrt(dx * dx + dy * dy);
                    float normDist = dist / center;

                    Color c;
                    if (normDist > 0.95f) c = Color.Transparent;
                    else if (normDist > 0.82f) c = rimSteel;
                    else if (normDist > 0.68f)
                    {
                        // Outer measurement scale arc
                        float angle = MathF.Atan2(dy, dx);
                        c = (angle > -0.5f && angle < 1.2f) ? dangerRed : safeGreen;
                    }
                    else if (normDist < 0.15f) c = needleBlack; // Center pivot
                    else c = dialWhite;

                    data[y * size + x] = c;
                }
            }

            tex.SetData(data);
            return tex;
        }
    }
}
