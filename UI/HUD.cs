using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BuildMechanic.Camera;
using BuildMechanic.Gameplay;
using BuildMechanic.Simulation;
using BuildMechanic.Tools;

namespace BuildMechanic.UI
{
    public class HUD
    {
        private readonly ProceduralFont _font;
        private readonly Texture2D _pixel;
        private readonly HUDShopView _shopView = new HUDShopView();
        private readonly HUDCatalogView _catalogView = new HUDCatalogView();
        private readonly HUDProvingView _provingView = new HUDProvingView();

        public bool ShowHelpDrawer { get; set; } = false;
        public int ActiveSectorIndex { get; set; } = 0;

        public HUD(GraphicsDevice device, Texture2D pixel)
        {
            _pixel = pixel;
            _font = new ProceduralFont(device);
        }

        public bool IsPointerOverHUD(Point mousePos, int sw, int sh)
        {
            if (mousePos.Y <= 34) return true;
            if (HUDSectorBar.IsMouseOver(mousePos)) return true;
            if (HUDHotbar.IsMouseOver(mousePos, sw, sh)) return true;
            return false;
        }

        public bool HandleClick(
            Point mousePos, int sw, int sh,
            ToolManager tools, OrbitCamera camera, ProvingGroundsManager proving, MechChassis chassis)
        {
            if (mousePos.Y <= 32)
            {
                if (GetHelpButtonRect(sw).Contains(mousePos)) { ShowHelpDrawer = !ShowHelpDrawer; return true; }
                if (GetTestButtonRect(sw).Contains(mousePos))
                {
                    if (proving != null) proving.TriggerTest(chassis);
                    return true;
                }
                if (GetShopButtonRect(sw).Contains(mousePos))
                {
                    tools.IsShopOpen = !tools.IsShopOpen;
                    if (tools.IsShopOpen) { tools.IsCatalogOpen = false; tools.StopOperations(); }
                    return true;
                }
                if (GetCatalogButtonRect(sw).Contains(mousePos))
                {
                    tools.IsCatalogOpen = !tools.IsCatalogOpen;
                    if (tools.IsCatalogOpen) { tools.IsShopOpen = false; tools.StopOperations(); }
                    return true;
                }
                return true;
            }

            int sectorIdx = ActiveSectorIndex;
            if (HUDSectorBar.HandleClick(mousePos, camera, ref sectorIdx))
            {
                ActiveSectorIndex = sectorIdx;
                return true;
            }

            if (HUDHotbar.HandleClick(mousePos, sw, sh, tools)) return true;
            return false;
        }

        public void Draw(
            SpriteBatch spriteBatch,
            Viewport viewport,
            ToolManager toolManager,
            ContractManager contractManager,
            MechChassis chassis,
            OrbitCamera camera,
            ProvingGroundsManager proving = null)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            int sw = viewport.Width;
            int sh = viewport.Height;
            Point mousePos = Mouse.GetState().Position;

            bool modalOpen = toolManager.IsShopOpen || toolManager.IsCatalogOpen;
            if (!modalOpen)
            {
                HUDTargetTelemetry.DrawHolographicReticle(spriteBatch, _pixel, _font, mousePos.X, mousePos.Y, toolManager);
                if (toolManager.ActiveTool == ToolType.Scanner)
                    DrawARScannerTags(spriteBatch, viewport, chassis, camera);
            }

            DrawTopStatusBar(spriteBatch, sw, contractManager, mousePos);
            HUDSectorBar.Draw(spriteBatch, _pixel, _font, ActiveSectorIndex, mousePos);
            DrawContractTracker(spriteBatch, sw, contractManager, chassis);

            if (!modalOpen)
            {
                int cardW = Math.Clamp(sw / 3, 320, 440);
                HUDTargetTelemetry.DrawTargetCard(spriteBatch, _pixel, _font, sw - cardW - 20, 42, toolManager, cardW);
            }

            HUDHotbar.Draw(spriteBatch, _pixel, _font, sw, sh, toolManager, mousePos);

            if (ShowHelpDrawer)
                HUDSectorBar.DrawHelpDrawer(spriteBatch, _pixel, _font, sw, sh);

            if (proving != null && proving.CurrentStage != ProvingStage.Idle)
                _provingView.Draw(spriteBatch, _pixel, _font, sw, sh, proving);

            if (toolManager.IsShopOpen)
                _shopView.Draw(spriteBatch, _pixel, _font, sw, sh);

            if (toolManager.IsCatalogOpen)
                _catalogView.Draw(spriteBatch, _pixel, _font, sw, sh);

            if (contractManager.IsFinished && (proving == null || proving.CurrentStage == ProvingStage.Idle))
                DrawCompletionModal(spriteBatch, sw, sh, contractManager);

            spriteBatch.End();
        }

        private static Rectangle GetHelpButtonRect(int sw) => new Rectangle(sw - 94, 4, 78, 24);
        private static Rectangle GetTestButtonRect(int sw) => new Rectangle(sw - 186, 4, 86, 24);
        private static Rectangle GetShopButtonRect(int sw) => new Rectangle(sw - 278, 4, 86, 24);
        private static Rectangle GetCatalogButtonRect(int sw) => new Rectangle(sw - 376, 4, 92, 24);

        private void DrawTopStatusBar(SpriteBatch sb, int sw, ContractManager contractManager, Point mousePos)
        {
            int barH = 32;
            sb.Draw(_pixel, new Rectangle(0, 0, sw, barH), new Color(12, 15, 20, 242));
            sb.Draw(_pixel, new Rectangle(0, barH - 2, sw, 2), new Color(40, 60, 80));

            string contractText = contractManager.ActiveContract != null
                ? $"CONTRACT #{contractManager.CurrentContractNumber}: {contractManager.ActiveContract.Client.ToUpper()}"
                : "NO ACTIVE CONTRACT";
            _font.DrawString(sb, contractText, new Vector2(20, 8), Color.Cyan, 1.35f);

            int credits = UpgradeManager.Instance?.Credits ?? 0;
            string creditText = $"CREDITS: ${credits:N0}";
            float credW = ProceduralFont.CalculateStringWidth(creditText, 1.45f);
            _font.DrawString(sb, creditText, new Vector2((sw - credW) * 0.5f, 7), Color.Gold, 1.45f);

            DrawTopButton(sb, GetCatalogButtonRect(sw), "[K] PARTS", Color.Orange, mousePos);
            DrawTopButton(sb, GetShopButtonRect(sw), "[U] SHOP", Color.Yellow, mousePos);
            DrawTopButton(sb, GetTestButtonRect(sw), "[T] TEST", Color.LimeGreen, mousePos);
            DrawTopButton(sb, GetHelpButtonRect(sw), "[H] HELP", ShowHelpDrawer ? Color.Cyan : Color.Silver, mousePos);
        }

        private void DrawTopButton(SpriteBatch sb, Rectangle r, string label, Color col, Point mousePos)
        {
            bool hov = r.Contains(mousePos);
            sb.Draw(_pixel, r, hov ? new Color(36, 52, 70, 240) : new Color(22, 28, 36, 220));
            float tw = ProceduralFont.CalculateStringWidth(label, 1.15f);
            _font.DrawString(sb, label, new Vector2(r.X + (r.Width - tw) * 0.5f, r.Y + 6), col, 1.15f);
        }

        private void DrawContractTracker(SpriteBatch sb, int sw, ContractManager contractManager, MechChassis chassis)
        {
            if (contractManager.ActiveContract == null) return;
            var c = contractManager.ActiveContract;
            int x = 20, y = 72;
            int w = Math.Clamp(sw / 3, 350, 440);
            int h = 48 + c.Objectives.Count * 22;

            sb.Draw(_pixel, new Rectangle(x, y, w, h), new Color(15, 20, 25, 225));
            sb.Draw(_pixel, new Rectangle(x, y, 4, h), new Color(40, 180, 220));

            _font.DrawString(sb, c.Title.ToUpper(), new Vector2(x + 14, y + 10), Color.Cyan, 1.45f);
            _font.DrawString(sb, $"PAYOUT: ${c.Reward:N0}", new Vector2(x + 14, y + 27), Color.Gold, 1.3f);

            int objY = y + 48;
            foreach (var obj in c.Objectives)
            {
                bool done = obj.Evaluate(chassis);
                Color col = done ? Color.LimeGreen : Color.Silver;
                string check = done ? "[X]" : "[ ]";
                _font.DrawString(sb, $"{check} {obj.Description}", new Vector2(x + 14, objY), col, 1.18f);
                objY += 22;
            }
        }

        private void DrawARScannerTags(SpriteBatch sb, Viewport viewport, MechChassis chassis, OrbitCamera camera)
        {
            foreach (var plate in chassis.Plates)
            {
                Vector3 plateWorld = Vector3.Transform(Vector3.Zero, plate.GetWorldMatrix());
                Vector3 tagWorld = plateWorld + new Vector3(0, plate.Dimensions.Y * 0.25f, 0.05f);
                Vector3 screenPos = viewport.Project(tagWorld, camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);

                if (screenPos.Z <= 0 || screenPos.Z >= 1) continue;
                int sx = (int)screenPos.X, sy = (int)screenPos.Y;
                if (sx < 60 || sx > viewport.Width - 180 || sy < 70 || sy > viewport.Height - 90) continue;

                int tagW = 165, tagH = 44, tx = sx - tagW / 2, ty = sy - tagH - 12;
                bool damaged = plate.HasUnrepairedCracks;
                Color tagColor = damaged ? Color.OrangeRed : Color.LimeGreen;

                sb.Draw(_pixel, new Rectangle(tx, ty, tagW, tagH), new Color(10, 20, 30, 215));
                sb.Draw(_pixel, new Rectangle(tx, ty, 3, tagH), tagColor);
                sb.Draw(_pixel, new Rectangle(tx, ty, tagW, 1), tagColor * 0.7f);
                sb.Draw(_pixel, new Rectangle(sx - 1, ty + tagH, 2, 8), tagColor);

                _font.DrawString(sb, plate.DisplayName.ToUpper(), new Vector2(tx + 8, ty + 6), Color.White, 1.1f);
                string integ = damaged ? $"INTEG: {(int)plate.Integrity}% [DMG]" : "100% OK";
                int torqued = plate.Bolts.Count(b => b.IsTorqued);
                string stateBadge = plate.State == PlateMountState.Detached ? "[BENCH]" :
                    (plate.CanDetach ? "[PRY]" : $"{torqued}/{plate.Bolts.Count}B");
                _font.DrawString(sb, $"{integ} | {stateBadge}", new Vector2(tx + 8, ty + 24), tagColor, 1.0f);
            }
        }

        private void DrawCompletionModal(SpriteBatch sb, int sw, int sh, ContractManager contractManager)
        {
            int modalW = Math.Min(600, sw - 40), modalH = 155, mx = (sw - modalW) / 2, my = 120;
            sb.Draw(_pixel, new Rectangle(mx, my, modalW, modalH), new Color(10, 35, 25, 245));
            sb.Draw(_pixel, new Rectangle(mx, my, modalW, 4), Color.LimeGreen);
            sb.Draw(_pixel, new Rectangle(mx, my + modalH - 4, modalW, 4), Color.LimeGreen);

            _font.DrawString(sb, "CONTRACT OVERHAUL COMPLETE!", new Vector2(mx + 30, my + 20), Color.LimeGreen, 2.0f);
            _font.DrawString(sb, $"+${contractManager.ActiveContract.Reward:N0} CREDITS TRANSFERRED TO WORKSHOP", new Vector2(mx + 30, my + 52), Color.Gold, 1.5f);
            _font.DrawString(sb, "Mech armor refit certified to 100% combat specification.", new Vector2(mx + 30, my + 82), Color.White, 1.3f);
            _font.DrawString(sb, "[PRESS ENTER TO COMMENCE NEXT OVERHAUL CONTRACT]", new Vector2(mx + 30, my + 112), Color.Cyan, 1.3f);
        }
    }
}
