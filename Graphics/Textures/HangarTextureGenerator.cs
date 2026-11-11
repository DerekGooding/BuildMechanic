using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Graphics.Textures
{
    public static class HangarTextureGenerator
    {
        public static Texture2D CreateDiamondPlateFloor(GraphicsDevice device, int size = 512)
        {
            var tex = new Texture2D(device, size, size);
            var data = new Color[size * size];

            Color floorBase = new Color(42, 45, 52);
            Color floorGrid = new Color(24, 25, 29);
            Color treadHighlight = new Color(92, 98, 110);
            Color treadShadow = new Color(28, 30, 36);
            Color cautionStripe = new Color(215, 165, 22);
            Color cautionBlack = new Color(28, 28, 32);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Perimeter safety hazard border (outer 32 pixels)
                    bool isPerimeter = (x < 32 || x > size - 33 || y < 32 || y > size - 33);
                    if (isPerimeter)
                    {
                        int stripe = (x + y) / 16;
                        data[y * size + x] = (stripe % 2 == 0) ? cautionStripe : cautionBlack;
                        continue;
                    }

                    // Panel division joints (every 128 pixels)
                    bool isPanelJoint = (x % 128 < 3) || (y % 128 < 3);
                    if (isPanelJoint)
                    {
                        data[y * size + x] = floorGrid;
                        continue;
                    }

                    // Diamond plate tread pattern (crisscross raised lozenges)
                    int cellX = x % 32;
                    int cellY = y % 32;
                    bool diamond1 = Math.Abs(cellX - 16) + Math.Abs(cellY - 16) < 7;
                    bool diamond2 = Math.Abs((cellX + 16) % 32 - 16) + Math.Abs((cellY + 16) % 32 - 16) < 5;

                    Color c = floorBase;
                    if (diamond1 || diamond2)
                    {
                        // Beveled raised diamond: upper-left highlight, lower-right shadow
                        bool isHighlight = (cellX + cellY) < 32;
                        c = isHighlight ? treadHighlight : treadShadow;
                    }

                    data[y * size + x] = c;
                }
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D CreateWorkstationMat(GraphicsDevice device, int size = 256)
        {
            var tex = new Texture2D(device, size, size);
            var data = new Color[size * size];

            Color matDark = new Color(26, 28, 32);
            Color matRib = new Color(38, 42, 48);
            Color borderAccent = new Color(55, 120, 200); // Blue shop accent border

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder = (x < 6 || x > size - 7 || y < 6 || y > size - 7);
                    if (isBorder)
                    {
                        data[y * size + x] = borderAccent;
                        continue;
                    }

                    // Horizontal anti-fatigue ribbing
                    bool isRib = (y % 8) < 3;
                    data[y * size + x] = isRib ? matRib : matDark;
                }
            }

            tex.SetData(data);
            return tex;
        }

        public static Texture2D CreateDiagnosticScreen(GraphicsDevice device, int size = 256)
        {
            var tex = new Texture2D(device, size, size);
            var data = new Color[size * size];

            Color screenBg = new Color(12, 18, 24);
            Color gridLine = new Color(20, 36, 48);
            Color traceCyan = new Color(40, 220, 240);
            Color alertAmber = new Color(255, 170, 30);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Oscilloscope grid lines
                    bool isGrid = (x % 32 == 0) || (y % 32 == 0);
                    Color c = isGrid ? gridLine : screenBg;

                    // Simulated ECG / sensor waveforms
                    float wave1 = MathF.Sin(x * 0.08f) * 28f + 100f;
                    float wave2 = MathF.Cos(x * 0.04f) * 18f + 180f;

                    if (MathF.Abs(y - wave1) < 2.0f) c = traceCyan;
                    if (MathF.Abs(y - wave2) < 2.0f && x > 64) c = alertAmber;

                    // Top status bar
                    if (y < 20 && x > 16 && x < size - 16) c = traceCyan;

                    data[y * size + x] = c;
                }
            }

            tex.SetData(data);
            return tex;
        }
    }
}
