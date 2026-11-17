using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BuildMechanic.Audio;
using BuildMechanic.Gameplay;

namespace BuildMechanic.UI
{
    public class HUDShopView
    {
        public static (Rectangle driverBtn, Rectangle grinderBtn, Rectangle welderBtn, Rectangle closeBtn) GetButtonRects(int sw, int sh)
        {
            int w = 620, h = 410;
            int x = (sw - w) / 2;
            int y = (sh - h) / 2;
            int cardW = w - 48;
            int cardH = 82;
            int btnW = 160, btnH = 38;
            int bx = x + 24 + cardW - btnW - 14;

            Rectangle MakeBtn(int cardIndex)
            {
                int cy = y + 88 + cardIndex * (cardH + 12);
                int by = cy + (cardH - btnH) / 2;
                return new Rectangle(bx, by, btnW, btnH);
            }

            Rectangle closeBtn = new Rectangle(x + w - 110, y + 18, 86, 28);
            return (MakeBtn(0), MakeBtn(1), MakeBtn(2), closeBtn);
        }

        public static bool TryHandleClick(Point mousePos, int sw, int sh, out bool closeRequested)
        {
            closeRequested = false;
            var rects = GetButtonRects(sw, sh);
            if (rects.closeBtn.Contains(mousePos))
            {
                closeRequested = true;
                return true;
            }
            if (UpgradeManager.Instance == null) return false;

            if (rects.driverBtn.Contains(mousePos))
            {
                if (UpgradeManager.Instance.TryUpgradeDriver()) AudioManager.Instance?.PlayScannerPing();
                else AudioManager.Instance?.PlayTorqueClick();
                return true;
            }
            if (rects.grinderBtn.Contains(mousePos))
            {
                if (UpgradeManager.Instance.TryUpgradeGrinder()) AudioManager.Instance?.PlayScannerPing();
                else AudioManager.Instance?.PlayTorqueClick();
                return true;
            }
            if (rects.welderBtn.Contains(mousePos))
            {
                if (UpgradeManager.Instance.TryUpgradeWelder()) AudioManager.Instance?.PlayScannerPing();
                else AudioManager.Instance?.PlayTorqueClick();
                return true;
            }
            return false;
        }

        public static void HandleInput(
            KeyboardState kbd, KeyboardState prevKbd,
            MouseState mouse, MouseState prevMouse,
            int sw, int sh, out bool closeRequested)
        {
            closeRequested = false;
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released &&
                TryHandleClick(mouse.Position, sw, sh, out closeRequested) && closeRequested)
                return;

            if (UpgradeManager.Instance == null) return;
            if (kbd.IsKeyDown(Keys.F1) && prevKbd.IsKeyUp(Keys.F1) && UpgradeManager.Instance.TryUpgradeDriver())
                AudioManager.Instance?.PlayScannerPing();
            if (kbd.IsKeyDown(Keys.F2) && prevKbd.IsKeyUp(Keys.F2) && UpgradeManager.Instance.TryUpgradeGrinder())
                AudioManager.Instance?.PlayScannerPing();
            if (kbd.IsKeyDown(Keys.F3) && prevKbd.IsKeyUp(Keys.F3) && UpgradeManager.Instance.TryUpgradeWelder())
                AudioManager.Instance?.PlayScannerPing();
        }

        public void Draw(SpriteBatch sb, Texture2D pixel, ProceduralFont font, int sw, int sh)
        {
            int w = 620;
            int h = 410;
            int x = (sw - w) / 2;
            int y = (sh - h) / 2;
            Point mouse = Mouse.GetState().Position;
            var rects = GetButtonRects(sw, sh);

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

            // Close button
            bool closeHover = rects.closeBtn.Contains(mouse);
            sb.Draw(pixel, rects.closeBtn, closeHover ? new Color(120, 40, 40) : new Color(60, 30, 35));
            sb.Draw(pixel, new Rectangle(rects.closeBtn.X, rects.closeBtn.Y, rects.closeBtn.Width, 2), Color.Tomato);
            font.DrawString(sb, "[X] CLOSE", new Vector2(rects.closeBtn.X + 10, rects.closeBtn.Y + 8), Color.White, 1.2f);

            sb.Draw(pixel, new Rectangle(x + 24, y + 72, w - 48, 1), new Color(50, 70, 90));

            int cardY = y + 88;
            int cardH = 82;
            int cardW = w - 48;

            int dLvl = UpgradeManager.Instance?.DriverLevel ?? 1;
            int dCost = UpgradeManager.Instance?.GetUpgradeCost(dLvl) ?? 0;
            float dMult = UpgradeManager.Instance?.GetDriverSpeed() ?? 1.0f;
            string dStat = dLvl < 3 ? $"TORQUE SPEED: {dMult:0.00}x -> {dMult + 0.65f:0.00}x" : $"TORQUE SPEED: {dMult:0.00}x [MAX]";
            DrawUpgradeCard(sb, pixel, font, x + 24, cardY, cardW, cardH,
                "PNEUMATIC IMPACT DRIVER", dLvl, dStat, "[F1]", dCost, credits >= dCost && dLvl < 3, rects.driverBtn.Contains(mouse));

            cardY += cardH + 12;
            int gLvl = UpgradeManager.Instance?.GrinderLevel ?? 1;
            int gCost = UpgradeManager.Instance?.GetUpgradeCost(gLvl) ?? 0;
            float gMult = UpgradeManager.Instance?.GetGrinderSpeed() ?? 1.0f;
            string gStat = gLvl < 3 ? $"ABRASIVE RPM: {gMult:0.00}x -> {gMult + 0.60f:0.00}x" : $"ABRASIVE RPM: {gMult:0.00}x [MAX]";
            DrawUpgradeCard(sb, pixel, font, x + 24, cardY, cardW, cardH,
                "HIGH-RPM ANGLE GRINDER", gLvl, gStat, "[F2]", gCost, credits >= gCost && gLvl < 3, rects.grinderBtn.Contains(mouse));

            cardY += cardH + 12;
            int wLvl = UpgradeManager.Instance?.WelderLevel ?? 1;
            int wCost = UpgradeManager.Instance?.GetUpgradeCost(wLvl) ?? 0;
            float wMult = UpgradeManager.Instance?.GetWelderSpeed() ?? 1.0f;
            string wStat = wLvl < 3 ? $"ARC DEPOSITION: {wMult:0.00}x -> {wMult + 0.60f:0.00}x" : $"ARC DEPOSITION: {wMult:0.00}x [MAX]";
            DrawUpgradeCard(sb, pixel, font, x + 24, cardY, cardW, cardH,
                "MIG FLUX WELDING TORCH", wLvl, wStat, "[F3]", wCost, credits >= wCost && wLvl < 3, rects.welderBtn.Contains(mouse));

            font.DrawString(sb, "CLICK BUTTON OR PRESS [F1]-[F3] | [U] / [ESC] RETURN", new Vector2(x + 24, y + h - 28), Color.Silver, 1.3f);
        }

        private void DrawUpgradeCard(SpriteBatch sb, Texture2D pixel, ProceduralFont font, int x, int y, int w, int h, string title, int level, string stat, string key, int cost, bool canAfford, bool hovered)
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
                Color btnBg = canAfford ? (hovered ? new Color(45, 145, 80) : new Color(30, 110, 60)) : new Color(60, 30, 30);
                Color textCol = canAfford ? Color.LimeGreen : Color.Tomato;
                sb.Draw(pixel, new Rectangle(bx, by, btnW, btnH), btnBg);
                sb.Draw(pixel, new Rectangle(bx, by, btnW, 2), textCol);

                font.DrawString(sb, $"{key} UPGRADE", new Vector2(bx + 16, by + 6), Color.White, 1.3f);
                font.DrawString(sb, $"${cost:N0}", new Vector2(bx + 16, by + 22), textCol, 1.3f);
            }
        }
    }
}
