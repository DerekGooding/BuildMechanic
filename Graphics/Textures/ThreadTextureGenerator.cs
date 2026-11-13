using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.Textures
{
    public static class ThreadTextureGenerator
    {
        public static Texture2D CreateScrewThreadTexture(GraphicsDevice device, int width = 512, int height = 256)
        {
            var tex = new Texture2D(device, width, height);
            var data = new Color[width * height];
            var rng = new Random(109);

            // 8 full thread pitches vertically along texture height
            float pitch = height / 8.0f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Normalized horizontal coordinate around cylinder (0 to 1)
                    float u = (float)x / width;

                    // Helical pitch angle shear: shifts along thread pitch per revolution
                    float helicalY = (y + u * pitch) % height;
                    if (helicalY < 0) helicalY += height;

                    // Position within current thread wave [0, 1)
                    float threadPhase = (helicalY % pitch) / pitch;

                    // Triangular / sinusoidal ISO 60-degree V-thread profile
                    // threadPhase: 0.0 = crest, 0.5 = root groove, 1.0 = crest
                    float profile = MathF.Cos(threadPhase * MathF.PI * 2f) * 0.5f + 0.5f;

                    // Base machined alloy steel
                    float latheGrain = ((float)rng.NextDouble() - 0.5f) * 0.06f;

                    Color c;
                    if (profile > 0.85f)
                    {
                        // Thread Crest: Polished high-stress contact peak with bright glint
                        float crestFactor = (profile - 0.85f) / 0.15f;
                        c = Color.Lerp(new Color(185, 192, 202), new Color(240, 245, 255), crestFactor);
                    }
                    else if (profile < 0.22f)
                    {
                        // Thread Root: Dark ambient occlusion groove with blue threadlocker
                        float rootFactor = 1.0f - (profile / 0.22f);
                        Color rootDark = new Color(38, 40, 46);
                        Color threadLockerBlue = new Color(28, 72, 160);
                        c = Color.Lerp(rootDark, threadLockerBlue, rootFactor * 0.65f);
                    }
                    else
                    {
                        // Thread Flank: Machined flank slope with directional metallic sheen
                        float flankShading = (profile - 0.22f) / 0.63f;
                        c = Color.Lerp(new Color(90, 95, 105), new Color(180, 186, 196), flankShading);
                    }

                    // Apply lathe machining micro-grooves
                    int r = Math.Clamp((int)(c.R * (1.0f + latheGrain)), 0, 255);
                    int g = Math.Clamp((int)(c.G * (1.0f + latheGrain)), 0, 255);
                    int b = Math.Clamp((int)(c.B * (1.0f + latheGrain)), 0, 255);

                    data[y * width + x] = new Color(r, g, b, 255);
                }
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D CreateBoltHeadTexture(GraphicsDevice device, int width = 512, int height = 256)
        {
            var tex = new Texture2D(device, width, height);
            var data = new Color[width * height];
            float headCenter = height / 2f;
            var rng = new Random(77);

            Color steelBase = new Color(155, 160, 168);
            Color flangeRim = new Color(220, 226, 235);
            Color stampDark = new Color(42, 45, 50);
            Color socketRecess = new Color(30, 32, 36);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel;
                    float grain = ((float)rng.NextDouble() - 0.5f) * 0.05f;

                    if (x < height)
                    {
                        // Left half: Circular top face [0..255]
                        float dx = (x + 0.5f) - headCenter;
                        float dy = (y + 0.5f) - headCenter;
                        float dist = MathF.Sqrt(dx * dx + dy * dy);
                        float normDist = dist / headCenter;

                        if (normDist < 0.28f)
                        {
                            // Hex socket recess in center with beveled lip
                            float socketBorder = normDist / 0.28f;
                            pixel = socketBorder > 0.88f ? new Color(75, 78, 86) : socketRecess;
                        }
                        else if (normDist < 0.72f)
                        {
                            // Crown top surface with stamped grade markings "10.9"
                            pixel = steelBase;
                            if ((int)(dist * 0.75f) % 7 == 0) pixel = new Color(135, 140, 148);

                            // Simulated stamped markings ring
                            if (normDist >= 0.44f && normDist <= 0.56f)
                            {
                                float angle = MathF.Atan2(dy, dx);
                                if (MathF.Cos(angle * 6f) > 0.65f) pixel = stampDark;
                            }
                        }
                        else if (normDist < 0.95f)
                        {
                            // Integrated washer flange deck with beveled perimeter
                            float flangeFactor = (normDist - 0.72f) / 0.23f;
                            pixel = Color.Lerp(flangeRim, steelBase, flangeFactor);
                        }
                        else if (normDist <= 1.0f)
                        {
                            // Outer beveled edge
                            pixel = new Color(70, 72, 80);
                        }
                        else
                        {
                            // Margin padding to prevent filtering boundary artifacts
                            pixel = steelBase;
                        }
                    }
                    else
                    {
                        // Right half: Machined Alloy Steel for hex flats & rim [256..511]
                        float rx = (float)(x - height) / height;
                        float ry = (float)y / height;

                        float latheLine = MathF.Sin(rx * 64f * MathF.PI) * 0.04f;
                        float vertMicro = ((float)rng.NextDouble() - 0.5f) * 0.07f;
                        float highlight = MathF.Cos((rx * 6f % 1f) * MathF.PI - (MathF.PI * 0.5f)) * 0.12f;

                        Color flatSteel = Color.Lerp(
                            new Color(140, 145, 154),
                            new Color(205, 212, 222),
                            Math.Clamp(0.5f + highlight + latheLine, 0f, 1f));

                        if (ry < 0.10f || ry > 0.90f)
                        {
                            flatSteel = Color.Lerp(flatSteel, new Color(85, 90, 98), 0.45f);
                        }

                        pixel = flatSteel;
                        grain = vertMicro;
                    }

                    int r = Math.Clamp((int)(pixel.R * (1.0f + grain)), 0, 255);
                    int g = Math.Clamp((int)(pixel.G * (1.0f + grain)), 0, 255);
                    int b = Math.Clamp((int)(pixel.B * (1.0f + grain)), 0, 255);
                    data[y * width + x] = new Color(r, g, b, 255);
                }
            }

            tex.SetData(data);
            return tex;
        }
    }
}
