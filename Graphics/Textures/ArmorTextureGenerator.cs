using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.Textures
{
    public static class ArmorTextureGenerator
    {
        public static Texture2D CreateDetailedArmorTexture(GraphicsDevice device, int width = 512, int height = 512, bool withChevrons = true)
        {
            var tex = new Texture2D(device, width, height);
            var data = new Color[width * height];
            var rng = new Random(84);

            Color baseDrab = new Color(68, 78, 64);      // Ballistic olive drab
            Color primerRed = new Color(130, 48, 38);     // Zinc chromate / red oxide primer
            Color bareSteel = new Color(185, 192, 198);   // Bare RHA steel
            Color hazardYellow = new Color(230, 185, 25);
            Color hazardBlack = new Color(30, 32, 36);
            Color sootStain = new Color(25, 26, 28);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int distBorder = Math.Min(Math.Min(x, width - 1 - x), Math.Min(y, height - 1 - y));
                    float grain = ((float)rng.NextDouble() - 0.5f) * 0.07f;
                    Color c = baseDrab;

                    // 1. Outer edge bevel chamfer: layered paint chipping
                    if (distBorder < 22)
                    {
                        float edgeFactor = 1.0f - (distBorder / 22.0f);
                        // Primer layer peeks through before bare steel
                        c = edgeFactor > 0.45f ?
                            Color.Lerp(primerRed, bareSteel, (edgeFactor - 0.45f) / 0.55f) :
                            Color.Lerp(baseDrab, primerRed, edgeFactor / 0.45f);
                    }

                    // 2. Hazard chevron stripes along upper band
                    if (withChevrons && y < 60 && y > 18 && distBorder > 18)
                    {
                        int stripePos = (x + y * 2) % 56;
                        c = stripePos < 28 ? hazardYellow : hazardBlack;
                    }

                    // 3. Ballistic armor panel division grooves
                    if (distBorder > 32 && (Math.Abs(y - height / 2) <= 1 || Math.Abs(x - width / 2) <= 1))
                    {
                        c = Color.Lerp(c, sootStain, 0.35f);
                    }

                    // 4. Ballistic pitting and fine metal scratches
                    if (rng.NextDouble() < 0.003) c = bareSteel;
                    else if (rng.NextDouble() < 0.001) c = primerRed;

                    int r = Math.Clamp((int)(c.R * (1.0f + grain)), 0, 255);
                    int g = Math.Clamp((int)(c.G * (1.0f + grain)), 0, 255);
                    int b = Math.Clamp((int)(c.B * (1.0f + grain)), 0, 255);
                    data[y * width + x] = new Color(r, g, b, 255);
                }
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D CreatePlateRearTexture(GraphicsDevice device, int size = 256)
        {
            var tex = new Texture2D(device, size, size);
            var data = new Color[size * size];

            Color darkCarbon = new Color(34, 36, 40);
            Color ribSteel = new Color(65, 70, 78);
            Color insulationPad = new Color(48, 52, 58);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Waffle-grid composite stiffener ribs
                    bool isRibX = (x % 32) < 4;
                    bool isRibY = (y % 32) < 4;
                    bool isBorder = (x < 8 || x > size - 9 || y < 8 || y > size - 9);

                    Color c = isBorder || isRibX || isRibY ? ribSteel : (
                        ((x / 32 + y / 32) % 2 == 0) ? insulationPad : darkCarbon
                    );

                    data[y * size + x] = c;
                }
            }

            tex.SetData(data);
            return tex;
        }
    }
}
