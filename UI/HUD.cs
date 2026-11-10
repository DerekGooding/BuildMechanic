using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BuildMechanic.Camera;
using BuildMechanic.Gameplay;
using BuildMechanic.Graphics;
using BuildMechanic.Simulation;
using BuildMechanic.Tools;

namespace BuildMechanic.UI
{
    public class HUD
    {
        private readonly ProceduralFont _font;
        private readonly Texture2D _pixel;
        private readonly HUDShopView _shopView = new HUDShopView();
        private readonly HUDProvingView _provingView = new HUDProvingView();

        public HUD(GraphicsDevice device, Texture2D pixel)
        {
            _pixel = pixel;
            _font = new ProceduralFont(device);
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

            DrawCrosshair(spriteBatch, mousePos.X, mousePos.Y, toolManager);

            if (toolManager.ActiveTool == ToolType.Scanner)
                DrawARScannerTags(spriteBatch, viewport, chassis, camera);

            DrawTopStatusBar(spriteBatch, sw, contractManager);
            DrawContractTracker(spriteBatch, contractManager, chassis);
            DrawTargetCard(spriteBatch, sw - 390, 48, toolManager);
            DrawToolHotbar(spriteBatch, sw, sh, toolManager);
            DrawControlsLegend(spriteBatch, sw - 290, sh - 130);

            if (proving != null && proving.CurrentStage != ProvingStage.Idle)
                _provingView.Draw(spriteBatch, _pixel, _font, sw, sh, proving);

            if (toolManager.IsShopOpen)
                _shopView.Draw(spriteBatch, _pixel, _font, sw, sh);

            if (contractManager.IsFinished && (proving == null || proving.CurrentStage == ProvingStage.Idle))
                DrawCompletionModal(spriteBatch, sw, sh, contractManager);

            spriteBatch.End();
        }

        private void DrawCrosshair(SpriteBatch sb, int cx, int cy, ToolManager tools)
        {
            Color reticleColor = tools.HoveredBolt != null ? Color.Gold :
                                (tools.HoveredCrack != null ? Color.OrangeRed :
                                (tools.HoveredHydraulic != null ? Color.DeepSkyBlue :
                                (tools.HoveredRelay != null ? Color.Magenta :
                                (tools.HoveredPlate != null ? Color.Cyan : Color.White * 0.75f))));

            int len = 8, gap = 4, thick = 2;
            sb.Draw(_pixel, new Rectangle(cx - gap - len, cy - thick / 2, len, thick), reticleColor);
            sb.Draw(_pixel, new Rectangle(cx + gap, cy - thick / 2, len, thick), reticleColor);
            sb.Draw(_pixel, new Rectangle(cx - thick / 2, cy - gap - len, thick, len), reticleColor);
            sb.Draw(_pixel, new Rectangle(cx - thick / 2, cy + gap, thick, len), reticleColor);
            sb.Draw(_pixel, new Rectangle(cx - 1, cy - 1, 2, 2), reticleColor);
        }

        private void DrawContractTracker(SpriteBatch sb, ContractManager contractManager, MechChassis chassis)
        {
            if (contractManager.ActiveContract == null) return;
            var c = contractManager.ActiveContract;
            int x = 20, y = 44, w = 410, h = 48 + c.Objectives.Count * 22;

            sb.Draw(_pixel, new Rectangle(x, y, w, h), new Color(15, 20, 25, 230));
            sb.Draw(_pixel, new Rectangle(x, y, 4, h), new Color(40, 180, 220));

            _font.DrawString(sb, c.Title.ToUpper(), new Vector2(x + 14, y + 10), Color.Cyan, 1.7f);
            _font.DrawString(sb, $"PAYOUT: ${c.Reward:N0}", new Vector2(x + 14, y + 27), Color.Gold, 1.4f);

            int objY = y + 48;
            foreach (var obj in c.Objectives)
            {
                bool done = obj.Evaluate(chassis);
                Color col = done ? Color.LimeGreen : Color.Silver;
                string check = done ? "[X]" : "[ ]";
                _font.DrawString(sb, $"{check} {obj.Description}", new Vector2(x + 14, objY), col, 1.35f);
                objY += 22;
            }
        }

        private void DrawTopStatusBar(SpriteBatch sb, int sw, ContractManager contractManager)
        {
            int barH = 32;
            sb.Draw(_pixel, new Rectangle(0, 0, sw, barH), new Color(12, 15, 20, 240));
            sb.Draw(_pixel, new Rectangle(0, barH - 2, sw, 2), new Color(40, 60, 80));

            string contractText = contractManager.ActiveContract != null
                ? $"CONTRACT #{contractManager.CurrentContractNumber}: {contractManager.ActiveContract.Client.ToUpper()}"
                : "NO ACTIVE CONTRACT";
            _font.DrawString(sb, contractText, new Vector2(20, 8), Color.Cyan, 1.4f);

            int credits = UpgradeManager.Instance?.Credits ?? 0;
            string creditText = $"WORKSHOP CREDITS: ${credits:N0}";
            float credW = ProceduralFont.CalculateStringWidth(creditText, 1.5f);
            _font.DrawString(sb, creditText, new Vector2((sw - credW) * 0.5f, 7), Color.Gold, 1.5f);

            string shopHint = "[U] SHOP | [T] TEST RUN";
            float shopW = ProceduralFont.CalculateStringWidth(shopHint, 1.4f);
            _font.DrawString(sb, shopHint, new Vector2(sw - shopW - 20, 8), Color.Yellow, 1.4f);
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
                if (sx < 60 || sx > viewport.Width - 180 || sy < 50 || sy > viewport.Height - 80) continue;

                int tagW = 165, tagH = 44, tx = sx - tagW / 2, ty = sy - tagH - 12;
                bool damaged = plate.HasUnrepairedCracks;
                Color tagColor = damaged ? Color.OrangeRed : Color.LimeGreen;

                sb.Draw(_pixel, new Rectangle(tx, ty, tagW, tagH), new Color(10, 20, 30, 215));
                sb.Draw(_pixel, new Rectangle(tx, ty, 3, tagH), tagColor);
                sb.Draw(_pixel, new Rectangle(tx, ty, tagW, 1), tagColor * 0.7f);
                sb.Draw(_pixel, new Rectangle(sx - 1, ty + tagH, 2, 8), tagColor);

                _font.DrawString(sb, plate.DisplayName.ToUpper(), new Vector2(tx + 8, ty + 6), Color.White, 1.15f);
                string integ = damaged ? $"INTEG: {(int)plate.Integrity}% [DMG]" : "100% OK";
                int torqued = plate.Bolts.Count(b => b.IsTorqued);
                _font.DrawString(sb, $"{integ} | {torqued}/{plate.Bolts.Count} BOLTS", new Vector2(tx + 8, ty + 24), tagColor, 1.05f);
            }
        }

        private void DrawTargetCard(SpriteBatch sb, int x, int y, ToolManager tools)
        {
            int w = 370;
            if (tools.HoveredPlate == null && tools.HoveredBolt == null && tools.HoveredCrack == null &&
                tools.HoveredHydraulic == null && tools.HoveredRelay == null) return;

            int h = 145;
            sb.Draw(_pixel, new Rectangle(x, y, w, h), new Color(20, 22, 28, 235));
            sb.Draw(_pixel, new Rectangle(x, y, 4, h), Color.Yellow);

            if (tools.HoveredHydraulic != null)
            {
                var hyl = tools.HoveredHydraulic;
                _font.DrawString(sb, "HYDRAULIC ACTUATOR", new Vector2(x + 14, y + 12), Color.DeepSkyBlue, 1.6f);
                _font.DrawString(sb, $"PRESSURE: {(int)hyl.CurrentPressure} / {(int)hyl.TargetPressure} BAR", new Vector2(x + 14, y + 34), hyl.IsNominal ? Color.LimeGreen : Color.OrangeRed, 1.3f);
                string status = hyl.HasAirPocket ? "AIR POCKET DETECTED [HOLD 7: BLEED]" : (hyl.Contamination > 15f ? "CONTAMINATED OIL [HOLD 7: FLUSH]" : "NOMINAL");
                _font.DrawString(sb, $"STATUS: {status}", new Vector2(x + 14, y + 54), Color.Silver, 1.2f);
                DrawProgressBar(sb, x + 14, y + 74, w - 28, 6, hyl.CurrentPressure / hyl.TargetPressure, Color.DeepSkyBlue);
            }
            else if (tools.HoveredRelay != null)
            {
                var r = tools.HoveredRelay;
                _font.DrawString(sb, $"AVIONICS RELAY FUSE #{r.SlotIndex}", new Vector2(x + 14, y + 12), Color.Magenta, 1.6f);
                string status = r.IsBlown ? "BLOWN [CLICK/RMB: REPLACE FUSE]" : (r.TestedWithProbe ? "CONTINUITY OK (0.5 OHM)" : "UNTESTED [CLICK: PROBE]");
                _font.DrawString(sb, $"STATUS: {status}", new Vector2(x + 14, y + 36), r.IsBlown ? Color.Tomato : Color.LimeGreen, 1.25f);
                _font.DrawString(sb, $"SLOT ID: {r.Id.ToUpper()}", new Vector2(x + 14, y + 58), Color.Silver, 1.2f);
            }
            else if (tools.HoveredBolt != null)
            {
                _font.DrawString(sb, "HEX FASTENER", new Vector2(x + 14, y + 12), Color.Gold, 1.7f);
                string status = tools.HoveredBolt.IsTorqued ? "TORQUED (100%)" : (tools.HoveredBolt.IsRemoved ? "LOOSE / UNTHREADED (0%)" : $"THREADED ({(int)(tools.HoveredBolt.ThreadProgress * 100)}%)");
                Color statusColor = tools.HoveredBolt.IsTorqued ? Color.LimeGreen : (tools.HoveredBolt.IsRemoved ? Color.OrangeRed : Color.Gold);
                _font.DrawString(sb, $"STATUS: {status}", new Vector2(x + 14, y + 34), statusColor, 1.35f);
                DrawProgressBar(sb, x + 14, y + 58, w - 28, 6, tools.HoveredBolt.ThreadProgress, statusColor);
            }
            else if (tools.HoveredCrack != null)
            {
                _font.DrawString(sb, "STRUCTURAL DEFECT", new Vector2(x + 14, y + 12), Color.OrangeRed, 1.7f);
                _font.DrawString(sb, $"STAGE: {tools.HoveredCrack.Stage.ToString().ToUpper()}", new Vector2(x + 14, y + 34), Color.Yellow, 1.35f);
                DrawProgressBar(sb, x + 14, y + 58, w - 28, 6, tools.HoveredCrack.StageProgress, Color.Orange);
            }
            else if (tools.HoveredPlate != null)
            {
                var p = tools.HoveredPlate;
                _font.DrawString(sb, p.DisplayName.ToUpper(), new Vector2(x + 14, y + 12), Color.Cyan, 1.45f);
                _font.DrawString(sb, $"INTEGRITY: {(int)p.Integrity}% | FASTENERS: {p.Bolts.Count(b => b.IsTorqued)}/{p.Bolts.Count}", new Vector2(x + 14, y + 34), Color.Silver, 1.25f);
                DrawProgressBar(sb, x + 14, y + 58, w - 28, 6, p.Integrity / 100f, p.Integrity > 90 ? Color.LimeGreen : Color.OrangeRed);
            }
        }

        private void DrawProgressBar(SpriteBatch sb, int x, int y, int width, int height, float progress, Color fill)
        {
            progress = Math.Clamp(progress, 0f, 1f);
            sb.Draw(_pixel, new Rectangle(x, y, width, height), new Color(40, 45, 52));
            sb.Draw(_pixel, new Rectangle(x, y, (int)(width * progress), height), fill);
        }

        private void DrawToolHotbar(SpriteBatch sb, int sw, int sh, ToolManager tools)
        {
            string colorName = ArmorDecal.PaletteNames[tools.ActiveColorIndex];
            string[] toolNames = new string[]
            {
                "[1] SCAN",
                $"[2] DRV [{(tools.DriverDir == DriverDirection.Forward ? "FWD" : "REV")}]",
                "[3] PRY",
                "[4] GRIND",
                "[5] WELD",
                "[6] METER",
                "[7] BLEED",
                $"[8] PAINT [{colorName.Substring(0, Math.Min(5, colorName.Length))}]"
            };

            int barW = 1040, barH = 46, startX = (sw - barW) / 2, y = sh - barH - 15;
            sb.Draw(_pixel, new Rectangle(startX, y, barW, barH), new Color(15, 18, 24, 235));

            int slotW = barW / 8;
            for (int i = 0; i < 8; i++)
            {
                ToolType type = (ToolType)(i + 1);
                bool isActive = tools.ActiveTool == type;
                int slotX = startX + i * slotW;
                Rectangle slotRect = new Rectangle(slotX, y, slotW, barH);

                if (isActive)
                {
                    sb.Draw(_pixel, slotRect, new Color(40, 80, 100, 200));
                    sb.Draw(_pixel, new Rectangle(slotX, y, slotW, 3), Color.Cyan);
                }
                _font.DrawString(sb, toolNames[i], new Vector2(slotX + 8, y + 15), isActive ? Color.Cyan : Color.Gray, 1.15f);
                if (i > 0) sb.Draw(_pixel, new Rectangle(slotX, y, 1, barH), new Color(40, 45, 55));
            }
        }

        private void DrawControlsLegend(SpriteBatch sb, int x, int y)
        {
            sb.Draw(_pixel, new Rectangle(x, y, 280, 115), new Color(15, 18, 22, 190));
            sb.Draw(_pixel, new Rectangle(x, y, 2, 115), Color.DarkSlateGray);

            _font.DrawString(sb, "HANGAR CONTROLS", new Vector2(x + 10, y + 8), Color.Yellow, 1.3f);
            _font.DrawString(sb, "RMB + Drag: Orbit | MMB: Pan", new Vector2(x + 10, y + 24), Color.Silver, 1.2f);
            _font.DrawString(sb, "C: Cycle Sector | F: Focus", new Vector2(x + 10, y + 39), Color.Silver, 1.2f);
            _font.DrawString(sb, "T: Proving Grounds Live Test", new Vector2(x + 10, y + 54), Color.Gold, 1.2f);
            _font.DrawString(sb, "P: Cycle Paint Color", new Vector2(x + 10, y + 69), Color.Silver, 1.2f);
            _font.DrawString(sb, "Space: Overview | U: Shop", new Vector2(x + 10, y + 84), Color.Silver, 1.2f);
        }

        private void DrawCompletionModal(SpriteBatch sb, int sw, int sh, ContractManager contractManager)
        {
            int modalW = 600, modalH = 155, mx = (sw - modalW) / 2, my = 120;
            sb.Draw(_pixel, new Rectangle(mx, my, modalW, modalH), new Color(10, 35, 25, 245));
            sb.Draw(_pixel, new Rectangle(mx, my, modalW, 4), Color.LimeGreen);
            sb.Draw(_pixel, new Rectangle(mx, my + modalH - 4, modalW, 4), Color.LimeGreen);

            _font.DrawString(sb, "CONTRACT OVERHAUL COMPLETE!", new Vector2(mx + 30, my + 20), Color.LimeGreen, 2.1f);
            _font.DrawString(sb, $"+${contractManager.ActiveContract.Reward:N0} CREDITS TRANSFERRED TO WORKSHOP", new Vector2(mx + 30, my + 52), Color.Gold, 1.6f);
            _font.DrawString(sb, "Mech armor refit certified to 100% combat specification.", new Vector2(mx + 30, my + 82), Color.White, 1.35f);
            _font.DrawString(sb, "[PRESS ENTER TO COMMENCE NEXT OVERHAUL CONTRACT]", new Vector2(mx + 30, my + 112), Color.Cyan, 1.35f);
        }
    }
}
