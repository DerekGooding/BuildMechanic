using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics
{
    public static class TextureGenerator
    {
        public static Texture2D CreateWhitePixel(GraphicsDevice device)
        {
            var tex = new Texture2D(device, 1, 1);
            tex.SetData(new[] { Color.White });
            return tex;
        }

        public static Texture2D CreateRadialGlow(GraphicsDevice device, int size = 64)
        {
            var tex = new Texture2D(device, size, size);
            var data = new Color[size * size];
            float center = size / 2f;
            float radius = center;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) - center;
                    float dy = (y + 0.5f) - center;
                    float dist = MathF.Sqrt(dx * dx + dy * dy);
                    float normDist = Math.Clamp(dist / radius, 0f, 1f);
                    float alpha = MathF.Pow(1.0f - normDist, 2.5f);

                    // Bright core (white-yellow) fading to orange glow
                    float warmth = Math.Clamp(normDist * 1.5f, 0f, 1f);
                    byte r = 255;
                    byte g = (byte)(255 * (1.0f - warmth * 0.4f));
                    byte b = (byte)(255 * MathF.Pow(1.0f - normDist, 4f));
                    byte a = (byte)(alpha * 255);

                    data[y * size + x] = new Color(r, g, b, a);
                }
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D CreateSmokeTexture(GraphicsDevice device, int size = 64)
        {
            var tex = new Texture2D(device, size, size);
            var data = new Color[size * size];
            float center = size / 2f;
            var rng = new Random(1337);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) - center;
                    float dy = (y + 0.5f) - center;
                    float dist = MathF.Sqrt(dx * dx + dy * dy) / center;
                    if (dist >= 1f)
                    {
                        data[y * size + x] = Color.Transparent;
                        continue;
                    }

                    float falloff = MathF.Pow(1.0f - dist, 1.8f);
                    float noise = (float)rng.NextDouble() * 0.25f + 0.75f;
                    byte gray = (byte)(180 * noise);
                    byte alpha = (byte)(falloff * 160 * noise);

                    data[y * size + x] = new Color(gray, gray, gray, alpha);
                }
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D CreateArmorPlateTexture(GraphicsDevice device, int width = 512, int height = 512, bool withHazardStripes = true)
        {
            var tex = new Texture2D(device, width, height);
            var data = new Color[width * height];
            var rng = new Random(42);

            Color baseDrab = new Color(74, 86, 70); // Military olive green
            Color edgeSteel = new Color(175, 182, 185); // Scuffed metal
            Color hazardYellow = new Color(225, 180, 25);
            Color hazardBlack = new Color(35, 35, 38);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Distances from border
                    int distBorder = Math.Min(Math.Min(x, width - 1 - x), Math.Min(y, height - 1 - y));

                    // Micro-grain noise
                    float grain = ((float)rng.NextDouble() - 0.5f) * 0.08f;

                    Color c = baseDrab;

                    // Scratched bare steel bevel on edges
                    if (distBorder < 18)
                    {
                        float edgeFactor = 1.0f - (distBorder / 18.0f);
                        c = Color.Lerp(baseDrab, edgeSteel, edgeFactor * 0.9f);
                    }

                    // Optional hazard stripes along top 48 pixels
                    if (withHazardStripes && y < 52 && y > 16 && distBorder > 16)
                    {
                        int stripePos = (x + y * 2) % 64;
                        c = stripePos < 32 ? hazardYellow : hazardBlack;
                    }

                    // Fine surface scratches
                    if (rng.NextDouble() < 0.002)
                    {
                        c = edgeSteel;
                    }

                    // Apply grain
                    int r = Math.Clamp((int)(c.R * (1.0f + grain)), 0, 255);
                    int g = Math.Clamp((int)(c.G * (1.0f + grain)), 0, 255);
                    int b = Math.Clamp((int)(c.B * (1.0f + grain)), 0, 255);

                    data[y * width + x] = new Color(r, g, b);
                }
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D CreateChassisTexture(GraphicsDevice device, int size = 256)
        {
            var tex = new Texture2D(device, size, size);
            var data = new Color[size * size];

            Color darkSteel = new Color(42, 45, 50);
            Color ribSteel = new Color(28, 30, 34);
            Color highlight = new Color(75, 80, 90);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Ribbed horizontal venting
                    bool isRib = (y % 16) < 4;
                    bool isBorder = (x < 6 || x > size - 7 || y < 6 || y > size - 7);

                    Color c = isBorder ? highlight : (isRib ? ribSteel : darkSteel);
                    data[y * size + x] = c;
                }
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D CreateHangarFloorTexture(GraphicsDevice device, int size = 256)
        {
            var tex = new Texture2D(device, size, size);
            var data = new Color[size * size];

            Color floorBase = new Color(50, 52, 58);
            Color floorGrid = new Color(25, 26, 30);
            Color cautionStripe = new Color(210, 160, 20);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isGridLine = (x % 64 == 0) || (y % 64 == 0);
                    bool isCaution = (x < 12 || x > size - 13) && ((y / 16) % 2 == 0);

                    Color c = isCaution ? cautionStripe : (isGridLine ? floorGrid : floorBase);
                    data[y * size + x] = c;
                }
            }

            tex.SetData(data);
            return tex;
        }
    }
}
