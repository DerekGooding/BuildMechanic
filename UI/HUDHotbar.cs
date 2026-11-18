using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics;
using BuildMechanic.Tools;

namespace BuildMechanic.UI
{
    public static class HUDHotbar
    {
        public static Rectangle GetBounds(int sw, int sh)
        {
            int barW = Math.Min(sw - 24, 1080);
            int barH = 48;
            int startX = (sw - barW) / 2;
            int y = sh - barH - 12;
            return new Rectangle(startX, y, barW, barH);
        }

        public static bool IsMouseOver(Point mousePos, int sw, int sh)
        {
            return GetBounds(sw, sh).Contains(mousePos);
        }

        public static bool HandleClick(Point mousePos, int sw, int sh, ToolManager tools)
        {
            Rectangle bar = GetBounds(sw, sh);
            if (!bar.Contains(mousePos)) return false;

            int slotCount = 8;
            int slotW = bar.Width / slotCount;
            int idx = Math.Clamp((mousePos.X - bar.X) / slotW, 0, slotCount - 1);
            ToolType clickedTool = (ToolType)(idx + 1);

            if (clickedTool == ToolType.ImpactDriver && tools.ActiveTool == ToolType.ImpactDriver)
            {
                tools.ToggleDriverDirection();
            }
            else if (clickedTool == ToolType.PaintGun && tools.ActiveTool == ToolType.PaintGun)
            {
                tools.CyclePaintColor();
            }
            else
            {
                tools.SelectTool(clickedTool);
            }
            return true;
        }

        public static void Draw(SpriteBatch sb, Texture2D pixel, ProceduralFont font, int sw, int sh, ToolManager tools, Point mousePos)
        {
            Rectangle bar = GetBounds(sw, sh);
            sb.Draw(pixel, bar, new Color(14, 18, 24, 240));
            sb.Draw(pixel, new Rectangle(bar.X, bar.Y, bar.Width, 1), new Color(60, 85, 110));

            string colorName = ArmorDecal.PaletteNames[tools.ActiveColorIndex];
            string[] toolNames =
            {
                "[1] SCAN",
                $"[2] DRV [{(tools.DriverDir == DriverDirection.Forward ? "FWD" : "REV")}]",
                "[3] PRY",
                "[4] GRIND",
                "[5] WELD",
                "[6] METER",
                "[7] BLEED",
                $"[8] {colorName.Substring(0, Math.Min(5, colorName.Length)).ToUpper()}"
            };

            int slotW = bar.Width / toolNames.Length;
            float fontScale = bar.Width < 860 ? 1.0f : 1.18f;

            for (int i = 0; i < toolNames.Length; i++)
            {
                Rectangle slotRect = new Rectangle(bar.X + i * slotW, bar.Y, slotW, bar.Height);
                bool active = (int)tools.ActiveTool == (i + 1);
                bool hovered = slotRect.Contains(mousePos);

                if (active)
                {
                    sb.Draw(pixel, slotRect, new Color(32, 72, 102, 235));
                    sb.Draw(pixel, new Rectangle(slotRect.X, slotRect.Y, slotRect.Width, 3), Color.Cyan);
                }
                else if (hovered)
                {
                    sb.Draw(pixel, slotRect, new Color(28, 38, 50, 220));
                }

                if (i == 7)
                {
                    sb.Draw(pixel, new Rectangle(slotRect.X + slotRect.Width - 14, slotRect.Y + 15, 8, 16), ArmorDecal.PaintPalette[tools.ActiveColorIndex]);
                }

                Color textCol = active ? Color.Cyan : (hovered ? Color.White : Color.Silver);
                if (i == 1 && active)
                {
                    textCol = tools.DriverDir == DriverDirection.Forward ? Color.LimeGreen : Color.Orange;
                }

                float tw = ProceduralFont.CalculateStringWidth(toolNames[i], fontScale);
                font.DrawString(sb, toolNames[i], new Vector2(slotRect.X + (slotW - tw) * 0.5f, slotRect.Y + 16), textCol, fontScale);
            }
        }
    }
}
