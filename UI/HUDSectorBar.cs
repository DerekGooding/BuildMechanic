using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Camera;

namespace BuildMechanic.UI
{
    public static class HUDSectorBar
    {
        private static readonly (string Label, Vector3 Target, float Dist, float Yaw, float Pitch)[] Sectors =
        {
            ("TORSO", new Vector3(0, 2.70f, 0.50f), 2.6f, 0.0f, 0.10f),
            ("SHOULDERS", new Vector3(0, 3.35f, 0.50f), 2.8f, 0.0f, 0.10f),
            ("LEGS", new Vector3(0, 0.85f, 0.50f), 2.5f, 0.0f, 0.10f),
            ("L-BENCH", new Vector3(-1.80f, 2.10f, 1.20f), 2.1f, -0.22f, 0.12f),
            ("R-BENCH", new Vector3(1.80f, 2.10f, 1.20f), 2.1f, 0.22f, 0.12f),
            ("OVERVIEW", new Vector3(0, 2.30f, 0.20f), 5.6f, 0.0f, 0.18f)
        };

        public static Rectangle GetBounds() => new Rectangle(20, 38, 456, 26);

        public static bool IsMouseOver(Point mousePos) => GetBounds().Contains(mousePos);

        public static bool HandleClick(Point mousePos, OrbitCamera camera, ref int activeSectorIdx)
        {
            Rectangle bar = GetBounds();
            if (!bar.Contains(mousePos)) return false;

            int btnW = bar.Width / Sectors.Length;
            int idx = Math.Clamp((mousePos.X - bar.X) / btnW, 0, Sectors.Length - 1);
            activeSectorIdx = idx;
            var s = Sectors[idx];
            camera.FocusSector(s.Target, s.Dist, s.Yaw, s.Pitch);
            return true;
        }

        public static void Draw(SpriteBatch sb, Texture2D pixel, ProceduralFont font, int activeSectorIdx, Point mousePos)
        {
            Rectangle bar = GetBounds();
            int btnW = bar.Width / Sectors.Length;

            for (int i = 0; i < Sectors.Length; i++)
            {
                Rectangle r = new Rectangle(bar.X + i * btnW, bar.Y, btnW - 2, bar.Height);
                bool active = i == activeSectorIdx;
                bool hovered = r.Contains(mousePos);

                Color bg = active ? new Color(30, 70, 95, 235) : (hovered ? new Color(28, 36, 46, 230) : new Color(15, 20, 26, 220));
                sb.Draw(pixel, r, bg);
                if (active) sb.Draw(pixel, new Rectangle(r.X, r.Bottom - 2, r.Width, 2), Color.Cyan);

                Color textCol = active ? Color.Cyan : (hovered ? Color.White : Color.Silver);
                string lbl = Sectors[i].Label;
                float tw = ProceduralFont.CalculateStringWidth(lbl, 1.0f);
                font.DrawString(sb, lbl, new Vector2(r.X + (r.Width - tw) * 0.5f, r.Y + 7), textCol, 1.0f);
            }
        }

        public static void DrawHelpDrawer(SpriteBatch sb, Texture2D pixel, ProceduralFont font, int sw, int sh)
        {
            int w = 310, h = 152;
            int x = sw - w - 20;
            int y = sh - 60 - h - 10;

            sb.Draw(pixel, new Rectangle(x, y, w, h), new Color(12, 16, 22, 235));
            sb.Draw(pixel, new Rectangle(x, y, 3, h), Color.Cyan);
            font.DrawString(sb, "WORKSHOP CONTROLS [H]", new Vector2(x + 12, y + 10), Color.Cyan, 1.25f);
            font.DrawString(sb, "LMB: USE TOOL | RMB DRAG: ORBIT", new Vector2(x + 12, y + 34), Color.Silver, 1.05f);
            font.DrawString(sb, "MMB / SHIFT+RMB: PAN | WHEEL: ZOOM", new Vector2(x + 12, y + 54), Color.Silver, 1.05f);
            font.DrawString(sb, "1-8 / Q,E / CTRL+WHEEL: TOOLS", new Vector2(x + 12, y + 74), Color.Silver, 1.05f);
            font.DrawString(sb, "TAB: SCANNER | R: FWD/REV | P: PAINT", new Vector2(x + 12, y + 94), Color.Silver, 1.05f);
            font.DrawString(sb, "C: SECTOR | F: FOCUS | SPACE: RESET", new Vector2(x + 12, y + 114), Color.Silver, 1.05f);
        }
    }
}
