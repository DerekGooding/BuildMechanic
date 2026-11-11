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

        public static Texture2D CreateBoltHeadTexture(GraphicsDevice device, int size = 256)
        {
            var tex = new Texture2D(device, size, size);
            var data = new Color[size * size];
            float center = size / 2f;
            var rng = new Random(77);

            Color steelBase = new Color(155, 160, 168);
            Color flangeRim = new Color(220, 226, 235);
            Color stampDark = new Color(48, 50, 56);
            Color socketRecess = new Color(32, 34, 38);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) - center;
                    float dy = (y + 0.5f) - center;
                    float dist = MathF.Sqrt(dx * dx + dy * dy);
                    float normDist = dist / center;

                    float grain = ((float)rng.NextDouble() - 0.5f) * 0.05f;
                    Color pixel;

                    if (normDist < 0.28f)
                    {
                        // Hex socket recess in center
                        float socketBorder = normDist / 0.28f;
                        pixel = socketBorder > 0.88f ? new Color(75, 78, 86) : socketRecess;
                    }
                    else if (normDist < 0.72f)
                    {
                        // Crown top surface with stamped grade markings "10.9"
                        pixel = steelBase;
                        // Concentric radial lathe tooling rings
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
                        // Integrated washer flange with beveled perimeter
                        float flangeFactor = (normDist - 0.72f) / 0.23f;
                        pixel = Color.Lerp(flangeRim, steelBase, flangeFactor);
                    }
                    else
                    {
                        // Outer beveled edge
                        pixel = new Color(60, 62, 68);
                    }

                    int r = Math.Clamp((int)(pixel.R * (1.0f + grain)), 0, 255);
                    int g = Math.Clamp((int)(pixel.G * (1.0f + grain)), 0, 255);
                    int b = Math.Clamp((int)(pixel.B * (1.0f + grain)), 0, 255);
                    data[y * size + x] = new Color(r, g, b, 255);
                }
            }

            tex.SetData(data);
            return tex;
        }
    }
}
