using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BuildMechanic.Audio;
using BuildMechanic.Gameplay;

namespace BuildMechanic.UI
{
    public class HUDCatalogView
    {
        public static void HandleInput(KeyboardState kbd, KeyboardState prevKbd, MouseState mouse, MouseState prevMouse, int sw, int sh, out bool closeRequested)
        {
            closeRequested = false;
            if (kbd.IsKeyDown(Keys.Escape) && prevKbd.IsKeyUp(Keys.Escape))
            {
                closeRequested = true;
                return;
            }

            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
            {
                int w = Math.Min(640, sw - 40);
                int h = 360;
                int x = (sw - w) / 2;
                int y = (sh - h) / 2;

                var items = PartsCatalogManager.CatalogItems;
                for (int i = 0; i < items.Length; i++)
                {
                    Rectangle rowBtn = new Rectangle(x + w - 150, y + 84 + i * 58, 126, 36);
                    if (rowBtn.Contains(mouse.Position))
                    {
                        if (PartsCatalogManager.Instance != null && PartsCatalogManager.Instance.OrderPart(items[i].Category, items[i].Cost))
                            AudioManager.Instance?.PlayTorqueClick();
                        return;
                    }
                }

                Rectangle closeBtn = new Rectangle(x + w - 110, y + h - 46, 90, 30);
                if (closeBtn.Contains(mouse.Position))
                    closeRequested = true;
            }
        }

        public void Draw(SpriteBatch sb, Texture2D pixel, ProceduralFont font, int sw, int sh)
        {
            sb.Draw(pixel, new Rectangle(0, 0, sw, sh), new Color(6, 10, 14, 185));

            int w = Math.Min(640, sw - 40);
            int h = 360;
            int x = (sw - w) / 2;
            int y = (sh - h) / 2;

            sb.Draw(pixel, new Rectangle(x, y, w, h), new Color(16, 22, 30, 245));
            sb.Draw(pixel, new Rectangle(x, y, w, 3), Color.Orange);

            font.DrawString(sb, "MECH PARTS CATALOG & INVENTORY [K]", new Vector2(x + 20, y + 18), Color.Orange, 1.6f);
            int credits = UpgradeManager.Instance?.Credits ?? 0;
            font.DrawString(sb, $"CREDITS: ${credits:N0} | SCRAPPED: {PartsCatalogManager.Instance?.TotalScrapped ?? 0}", new Vector2(x + 20, y + 46), Color.Gold, 1.25f);

            var items = PartsCatalogManager.CatalogItems;
            Point mp = Mouse.GetState().Position;
            for (int i = 0; i < items.Length; i++)
            {
                int ry = y + 80 + i * 58;
                sb.Draw(pixel, new Rectangle(x + 18, ry, w - 36, 46), new Color(24, 32, 42, 230));
                int stock = PartsCatalogManager.Instance?.GetStock(items[i].Category) ?? 0;

                font.DrawString(sb, items[i].Name.ToUpper(), new Vector2(x + 30, ry + 8), Color.White, 1.2f);
                font.DrawString(sb, $"IN RACK: {stock}", new Vector2(x + 30, ry + 26), stock > 0 ? Color.LimeGreen : Color.Tomato, 1.1f);

                Rectangle btn = new Rectangle(x + w - 150, ry + 4, 126, 36);
                bool canAfford = credits >= items[i].Cost;
                bool hov = btn.Contains(mp);
                Color btnCol = canAfford ? (hov ? new Color(40, 95, 65) : new Color(28, 70, 48)) : new Color(50, 32, 32);
                sb.Draw(pixel, btn, btnCol);
                font.DrawString(sb, $"ORDER ${items[i].Cost}", new Vector2(btn.X + 14, btn.Y + 11), canAfford ? Color.White : Color.Gray, 1.1f);
            }

            Rectangle closeBtn = new Rectangle(x + w - 110, y + h - 46, 90, 30);
            sb.Draw(pixel, closeBtn, closeBtn.Contains(mp) ? new Color(90, 40, 40) : new Color(60, 30, 30));
            font.DrawString(sb, "CLOSE", new Vector2(closeBtn.X + 20, closeBtn.Y + 8), Color.White, 1.15f);
        }
    }
}
