using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Gameplay;

namespace BuildMechanic.UI
{
    public class HUDShopView
    {
        public void Draw(SpriteBatch sb, Texture2D pixel, ProceduralFont font, int sw, int sh)
        {
            int w = 620;
            int h = 410;
            int x = (sw - w) / 2;
            int y = (sh - h) / 2;

            // Full screen dimmer
            sb.Draw(pixel, new Rectangle(0, 0, sw, sh), new Color(0, 0, 0, 180));

            // Modal background & borders
            sb.Draw(pixel, new Rectangle(x, y, w, h), new Color(16, 20, 28, 250));
            sb.Draw(pixel, new Rectangle(x, y, w, 4), Color.Cyan);
            sb.Draw(pixel, new Rectangle(x, y + h - 4, w, 4), Color.Cyan);
            sb.Draw(pixel, new Rectangle(x, y, 4, h), Color.Cyan);
            sb.Draw(pixel, new Rectangle(x + w - 4, y, 4, h), Color.Cyan);

            font.DrawString(sb, "WORKSHOP TOOL UPGRADE TERMINAL", new Vector2(x + 24, y + 20), Color.Cyan, 2.0f);
            int credits = UpgradeManager.Instance?.Credits ?? 0;
            font.DrawString(sb, $"AVAILABLE FUNDS: ${credits:N0}", new Vector2(x + 24, y + 46), Color.Gold, 1.6f);

            sb.Draw(pixel, new Rectangle(x + 24, y + 72, w - 48, 1), new Color(50, 70, 90));

            int cardY = y + 88;
            int cardH = 82;
            int cardW = w - 48;

            int dLvl = UpgradeManager.Instance?.DriverLevel ?? 1;
            int dCost = UpgradeManager.Instance?.GetUpgradeCost(dLvl) ?? 0;
            DrawUpgradeCard(sb, pixel, font, x + 24, cardY, cardW, cardH,
                "PNEUMATIC IMPACT DRIVER", dLvl, $"+{(int)((dLvl - 1) * 65)}% RPM Torque Speed", "[F1]", dCost, credits >= dCost && dLvl < 3);

            cardY += cardH + 12;
            int gLvl = UpgradeManager.Instance?.GrinderLevel ?? 1;
            int gCost = UpgradeManager.Instance?.GetUpgradeCost(gLvl) ?? 0;
            DrawUpgradeCard(sb, pixel, font, x + 24, cardY, cardW, cardH,
                "HIGH-RPM ANGLE GRINDER", gLvl, $"+{(int)((gLvl - 1) * 60)}% RPM Abrasive Prep Speed", "[F2]", gCost, credits >= gCost && gLvl < 3);

            cardY += cardH + 12;
            int wLvl = UpgradeManager.Instance?.WelderLevel ?? 1;
            int wCost = UpgradeManager.Instance?.GetUpgradeCost(wLvl) ?? 0;
            DrawUpgradeCard(sb, pixel, font, x + 24, cardY, cardW, cardH,
                "MIG FLUX WELDING TORCH", wLvl, $"+{(int)((wLvl - 1) * 60)}% Arc Thermal Puddle Deposition", "[F3]", wCost, credits >= wCost && wLvl < 3);

            font.DrawString(sb, "[U] / [ESC] RETURN TO HANGAR WORKSPACE", new Vector2(x + 24, y + h - 28), Color.Silver, 1.3f);
        }

        private void DrawUpgradeCard(SpriteBatch sb, Texture2D pixel, ProceduralFont font, int x, int y, int w, int h, string title, int level, string stat, string key, int cost, bool canAfford)
        {
            sb.Draw(pixel, new Rectangle(x, y, w, h), new Color(24, 30, 42, 220));
            sb.Draw(pixel, new Rectangle(x, y, 3, h), Color.Yellow);

            font.DrawString(sb, title, new Vector2(x + 14, y + 12), Color.White, 1.5f);
            font.DrawString(sb, $"TIER: Mk.{level} / Mk.3", new Vector2(x + 14, y + 34), Color.Cyan, 1.3f);
            font.DrawString(sb, stat, new Vector2(x + 14, y + 54), Color.Silver, 1.25f);

            int btnW = 160;
            int btnH = 38;
            int bx = x + w - btnW - 14;
            int by = y + (h - btnH) / 2;

            if (level >= 3)
            {
                sb.Draw(pixel, new Rectangle(bx, by, btnW, btnH), new Color(40, 50, 60));
                font.DrawString(sb, "MAX LEVEL", new Vector2(bx + 26, by + 12), Color.Silver, 1.35f);
            }
            else
            {
                Color btnBg = canAfford ? new Color(30, 110, 60) : new Color(60, 30, 30);
                Color textCol = canAfford ? Color.LimeGreen : Color.Tomato;
                sb.Draw(pixel, new Rectangle(bx, by, btnW, btnH), btnBg);
                sb.Draw(pixel, new Rectangle(bx, by, btnW, 2), textCol);

                font.DrawString(sb, $"{key} UPGRADE", new Vector2(bx + 16, by + 6), Color.White, 1.3f);
                font.DrawString(sb, $"${cost:N0}", new Vector2(bx + 16, by + 22), textCol, 1.3f);
            }
        }
    }
}
