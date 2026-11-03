using System;
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
            OrbitCamera camera)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            int sw = viewport.Width;
            int sh = viewport.Height;
            Point mousePos = Mouse.GetState().Position;

            // 1. Crosshair dynamically tracks mouse cursor (matching 3D raycast aim)
            DrawCrosshair(spriteBatch, mousePos.X, mousePos.Y, toolManager);

            // 2. AR Scanner Holographic Tags (World-to-Screen projected)
            if (toolManager.ActiveTool == ToolType.Scanner)
            {
                DrawARScannerTags(spriteBatch, viewport, chassis, camera);
            }

            // 3. Top Status Bar: Credits, Current Contract, Shop Hint
            DrawTopStatusBar(spriteBatch, sw, contractManager);

            // 4. Top-Left: Contract & Objectives
            DrawContractTracker(spriteBatch, contractManager, chassis);

            // 5. Top-Right: Target Inspection Card
            DrawTargetCard(spriteBatch, sw - 390, 48, toolManager);

            // 6. Bottom-Center: Tool Hotbar
            DrawToolHotbar(spriteBatch, sw, sh, toolManager);

            // 7. Bottom-Right: Controls Legend
            DrawControlsLegend(spriteBatch, sw - 290, sh - 125);

            // 8. Tool Upgrade Shop Modal Overlay
            if (toolManager.IsShopOpen)
            {
                DrawShopOverlay(spriteBatch, sw, sh);
            }

            // 9. Contract Completion Banner
            if (contractManager.IsFinished)
            {
                DrawCompletionModal(spriteBatch, sw, sh, contractManager);
            }

            spriteBatch.End();
        }

        private void DrawCrosshair(SpriteBatch sb, int cx, int cy, ToolManager tools)
        {
            Color reticleColor = tools.HoveredBolt != null ? Color.Gold :
                                (tools.HoveredCrack != null ? Color.OrangeRed :
                                (tools.HoveredPlate != null ? Color.Cyan : Color.White * 0.75f));

            int len = 8;
            int gap = 4;
            int thick = 2;

            // Horizontal
            sb.Draw(_pixel, new Rectangle(cx - gap - len, cy - thick / 2, len, thick), reticleColor);
            sb.Draw(_pixel, new Rectangle(cx + gap, cy - thick / 2, len, thick), reticleColor);

            // Vertical
            sb.Draw(_pixel, new Rectangle(cx - thick / 2, cy - gap - len, thick, len), reticleColor);
            sb.Draw(_pixel, new Rectangle(cx - thick / 2, cy + gap, thick, len), reticleColor);

            // Center pip
            sb.Draw(_pixel, new Rectangle(cx - 1, cy - 1, 2, 2), reticleColor);
        }

        private void DrawContractTracker(SpriteBatch sb, ContractManager contractManager, MechChassis chassis)
        {
            if (contractManager.ActiveContract == null) return;
            var c = contractManager.ActiveContract;

            int x = 20;
            int y = 44;
            int w = 410;
            int h = 48 + c.Objectives.Count * 22;

            // Panel Background
            sb.Draw(_pixel, new Rectangle(x, y, w, h), new Color(15, 20, 25, 230));
            sb.Draw(_pixel, new Rectangle(x, y, 4, h), new Color(40, 180, 220)); // Left accent line

            // Header
            _font.DrawString(sb, c.Title.ToUpper(), new Vector2(x + 14, y + 10), Color.Cyan, 1.7f);
            _font.DrawString(sb, $"PAYOUT: ${c.Reward:N0}", new Vector2(x + 14, y + 27), Color.Gold, 1.4f);

            // Objectives
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

            // Contract info
            string contractText = contractManager.ActiveContract != null
                ? $"CONTRACT #{contractManager.CurrentContractNumber}: {contractManager.ActiveContract.Client.ToUpper()}"
                : "NO ACTIVE CONTRACT";
            _font.DrawString(sb, contractText, new Vector2(20, 8), Color.Cyan, 1.4f);

            // Workshop Credits
            int credits = UpgradeManager.Instance?.Credits ?? 0;
            string creditText = $"WORKSHOP CREDITS: ${credits:N0}";
            float credW = ProceduralFont.CalculateStringWidth(creditText, 1.5f);
            _font.DrawString(sb, creditText, new Vector2((sw - credW) * 0.5f, 7), Color.Gold, 1.5f);

            // Shop Hotkey Hint
            string shopHint = "[U] TOOL UPGRADE SHOP";
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
                int sx = (int)screenPos.X;
                int sy = (int)screenPos.Y;

                if (sx < 60 || sx > viewport.Width - 180 || sy < 50 || sy > viewport.Height - 80) continue;

                int tagW = 165;
                int tagH = 44;
                int tx = sx - tagW / 2;
                int ty = sy - tagH - 12;

                bool damaged = plate.HasUnrepairedCracks;
                Color tagColor = damaged ? Color.OrangeRed : Color.LimeGreen;

                // Background
                sb.Draw(_pixel, new Rectangle(tx, ty, tagW, tagH), new Color(10, 20, 30, 215));
                sb.Draw(_pixel, new Rectangle(tx, ty, 3, tagH), tagColor);
                sb.Draw(_pixel, new Rectangle(tx, ty, tagW, 1), tagColor * 0.7f);

                // Pointer tick
                sb.Draw(_pixel, new Rectangle(sx - 1, ty + tagH, 2, 8), tagColor);

                // Plate Title
                _font.DrawString(sb, plate.DisplayName.ToUpper(), new Vector2(tx + 8, ty + 6), Color.White, 1.15f);

                // Status
                string integ = damaged ? $"INTEG: {(int)plate.Integrity}% [DMG]" : "100% SECURE";
                int torqued = 0;
                foreach (var b in plate.Bolts) if (b.IsTorqued) torqued++;
                string boltInfo = $"{torqued}/{plate.Bolts.Count} BOLTS";

                _font.DrawString(sb, $"{integ} | {boltInfo}", new Vector2(tx + 8, ty + 24), tagColor, 1.05f);
            }
        }

        private void DrawTargetCard(SpriteBatch sb, int x, int y, ToolManager tools)
        {
            int w = 370;

            if (tools.HoveredPlate == null && tools.HoveredBolt == null && tools.HoveredCrack == null)
            {
                if (tools.ActiveTool == ToolType.Scanner)
                {
                    // Scanner idle card
                    sb.Draw(_pixel, new Rectangle(x, y, w, 65), new Color(15, 25, 35, 190));
                    sb.Draw(_pixel, new Rectangle(x, y, 4, 65), Color.Cyan);
                    _font.DrawString(sb, "AR SCANNER ACTIVE", new Vector2(x + 14, y + 12), Color.Cyan, 1.7f);
                    _font.DrawString(sb, "Aim at armor plates, bolts, or seams", new Vector2(x + 14, y + 34), Color.Silver, 1.3f);
                }
                return;
            }

            int h = 145;
            sb.Draw(_pixel, new Rectangle(x, y, w, h), new Color(20, 22, 28, 235));
            sb.Draw(_pixel, new Rectangle(x, y, 4, h), Color.Yellow);

            if (tools.HoveredBolt != null)
            {
                _font.DrawString(sb, "HEX FASTENER", new Vector2(x + 14, y + 12), Color.Gold, 1.7f);
                _font.DrawString(sb, $"BOLT ID: {tools.HoveredBolt.Id}", new Vector2(x + 14, y + 32), Color.Silver, 1.3f);

                string status = tools.HoveredBolt.IsTorqued ? "TORQUED (100%)" :
                               (tools.HoveredBolt.IsRemoved ? "LOOSE / UNTHREADED (0%)" :
                               $"THREADED ({(int)(tools.HoveredBolt.ThreadProgress * 100)}%)");

                Color statusColor = tools.HoveredBolt.IsTorqued ? Color.LimeGreen : (tools.HoveredBolt.IsRemoved ? Color.OrangeRed : Color.Gold);
                _font.DrawString(sb, $"STATUS: {status}", new Vector2(x + 14, y + 50), statusColor, 1.35f);

                // Progress Bar
                DrawProgressBar(sb, x + 14, y + 70, w - 28, 6, tools.HoveredBolt.ThreadProgress, statusColor);

                // Action hint
                string hint = tools.ActiveTool == ToolType.ImpactDriver
                    ? $"HOLD LMB: {(tools.DriverDir == DriverDirection.Forward ? "DRIVE / TORQUE" : "UNSCREW")} [R: TOGGLE]"
                    : "SWITCH TO [2] IMPACT DRIVER TO SERVICE";

                _font.DrawString(sb, hint, new Vector2(x + 14, y + 88), Color.Cyan, 1.25f);
            }
            else if (tools.HoveredCrack != null)
            {
                _font.DrawString(sb, "STRUCTURAL DEFECT", new Vector2(x + 14, y + 12), Color.OrangeRed, 1.7f);
                _font.DrawString(sb, $"STAGE: {tools.HoveredCrack.Stage.ToString().ToUpper()}", new Vector2(x + 14, y + 32), Color.Yellow, 1.35f);

                int pct = (int)(tools.HoveredCrack.StageProgress * 100);
                string hint = tools.HoveredCrack.Stage switch
                {
                    CrackStage.Fractured => "HOLD [4] GRINDER: PREP V-GROOVE",
                    CrackStage.PreppedGroove => "HOLD [5] WELDER: FILL SEAM BEAD",
                    CrackStage.WeldedBead => "HOLD [4] GRINDER: GRIND BEAD FLUSH",
                    CrackStage.FlushFinished => "REPAIR COMPLETE (100% INTEGRITY)",
                    _ => ""
                };

                _font.DrawString(sb, $"STAGE PROGRESS: {pct}%", new Vector2(x + 14, y + 50), Color.Silver, 1.3f);

                // Progress Bar
                Color barColor = tools.HoveredCrack.Stage == CrackStage.FlushFinished ? Color.LimeGreen : Color.Orange;
                DrawProgressBar(sb, x + 14, y + 70, w - 28, 6, tools.HoveredCrack.StageProgress, barColor);

                _font.DrawString(sb, hint, new Vector2(x + 14, y + 88), Color.Cyan, 1.25f);
            }
            else if (tools.HoveredPlate != null)
            {
                var plate = tools.HoveredPlate;
                _font.DrawString(sb, plate.DisplayName.ToUpper(), new Vector2(x + 14, y + 12), Color.Cyan, 1.55f);

                int intPct = (int)plate.Integrity;
                _font.DrawString(sb, $"INTEGRITY: {intPct}%", new Vector2(x + 14, y + 32), intPct > 90 ? Color.LimeGreen : Color.OrangeRed, 1.35f);

                int torquedCount = 0;
                int removedCount = 0;
                foreach (var b in plate.Bolts)
                {
                    if (b.IsTorqued) torquedCount++;
                    if (b.IsRemoved) removedCount++;
                }

                _font.DrawString(sb, $"BOLTS: {torquedCount}/{plate.Bolts.Count} SECURED ({removedCount} REMOVED)", new Vector2(x + 14, y + 50), Color.Silver, 1.3f);

                // Progress Bar
                DrawProgressBar(sb, x + 14, y + 70, w - 28, 6, plate.Integrity / 100f, intPct > 90 ? Color.LimeGreen : Color.OrangeRed);

                string mountHint = plate.State == PlateMountState.Mounted
                    ? (plate.CanDetach ? "CLICK WITH [3] PRY BAR TO DETACH" : "UNBOLT ALL 4 FASTENERS BEFORE DETACHING")
                    : "CLICK WITH [3] PRY BAR TO MOUNT ON CHASSIS";

                _font.DrawString(sb, mountHint, new Vector2(x + 14, y + 88), plate.CanDetach ? Color.LimeGreen : Color.Gold, 1.25f);
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
            string[] toolNames = new string[]
            {
                "[1] SCANNER",
                $"[2] DRIVER [{(tools.DriverDir == DriverDirection.Forward ? "FWD" : "REV")}]",
                "[3] PRY / MOUNT",
                "[4] GRINDER",
                "[5] WELDER"
            };

            int barW = 860;
            int barH = 46;
            int startX = (sw - barW) / 2;
            int y = sh - barH - 15;

            // Bar background
            sb.Draw(_pixel, new Rectangle(startX, y, barW, barH), new Color(15, 18, 24, 235));

            int slotW = barW / 5;
            for (int i = 0; i < 5; i++)
            {
                ToolType type = (ToolType)(i + 1);
                bool isActive = tools.ActiveTool == type;

                int slotX = startX + i * slotW;
                Rectangle slotRect = new Rectangle(slotX, y, slotW, barH);

                if (isActive)
                {
                    sb.Draw(_pixel, slotRect, new Color(40, 80, 100, 200));
                    sb.Draw(_pixel, new Rectangle(slotX, y, slotW, 3), Color.Cyan); // Top active stripe
                }

                Color textCol = isActive ? Color.Cyan : Color.Gray;
                _font.DrawString(sb, toolNames[i], new Vector2(slotX + 12, y + 15), textCol, 1.25f);

                // Slot separator line
                if (i > 0)
                {
                    sb.Draw(_pixel, new Rectangle(slotX, y, 1, barH), new Color(40, 45, 55));
                }
            }
        }

        private void DrawControlsLegend(SpriteBatch sb, int x, int y)
        {
            sb.Draw(_pixel, new Rectangle(x, y, 280, 110), new Color(15, 18, 22, 190));
            sb.Draw(_pixel, new Rectangle(x, y, 2, 110), Color.DarkSlateGray);

            _font.DrawString(sb, "HANGAR CONTROLS", new Vector2(x + 10, y + 8), Color.Yellow, 1.3f);
            _font.DrawString(sb, "RMB + Drag: Orbit Camera", new Vector2(x + 10, y + 24), Color.Silver, 1.2f);
            _font.DrawString(sb, "MMB / Shift+RMB: Pan Camera", new Vector2(x + 10, y + 39), Color.Silver, 1.2f);
            _font.DrawString(sb, "Scroll Wheel: Zoom", new Vector2(x + 10, y + 54), Color.Silver, 1.2f);
            _font.DrawString(sb, "C: Cycle Sector | F: Focus", new Vector2(x + 10, y + 69), Color.Silver, 1.2f);
            _font.DrawString(sb, "Space: Overview | U: Shop", new Vector2(x + 10, y + 84), Color.Silver, 1.2f);
        }

        private void DrawShopOverlay(SpriteBatch sb, int sw, int sh)
        {
            int w = 620;
            int h = 410;
            int x = (sw - w) / 2;
            int y = (sh - h) / 2;

            // Full screen dimmer
            sb.Draw(_pixel, new Rectangle(0, 0, sw, sh), new Color(0, 0, 0, 180));

            // Modal background & borders
            sb.Draw(_pixel, new Rectangle(x, y, w, h), new Color(16, 20, 28, 250));
            sb.Draw(_pixel, new Rectangle(x, y, w, 4), Color.Cyan);
            sb.Draw(_pixel, new Rectangle(x, y + h - 4, w, 4), Color.Cyan);
            sb.Draw(_pixel, new Rectangle(x, y, 4, h), Color.Cyan);
            sb.Draw(_pixel, new Rectangle(x + w - 4, y, 4, h), Color.Cyan);

            // Title
            _font.DrawString(sb, "WORKSHOP TOOL UPGRADE TERMINAL", new Vector2(x + 24, y + 20), Color.Cyan, 2.0f);
            int credits = UpgradeManager.Instance?.Credits ?? 0;
            _font.DrawString(sb, $"AVAILABLE FUNDS: ${credits:N0}", new Vector2(x + 24, y + 46), Color.Gold, 1.6f);

            // Separator
            sb.Draw(_pixel, new Rectangle(x + 24, y + 72, w - 48, 1), new Color(50, 70, 90));

            int cardY = y + 88;
            int cardH = 82;
            int cardW = w - 48;

            // 1. Impact Driver
            int dLvl = UpgradeManager.Instance?.DriverLevel ?? 1;
            int dCost = UpgradeManager.Instance?.GetUpgradeCost(dLvl) ?? 0;
            DrawUpgradeCard(sb, x + 24, cardY, cardW, cardH,
                "PNEUMATIC IMPACT DRIVER",
                dLvl,
                $"+{(int)((dLvl - 1) * 65)}% RPM Torque Speed",
                "[F1]",
                dCost,
                credits >= dCost && dLvl < 3);

            // 2. Angle Grinder
            cardY += cardH + 12;
            int gLvl = UpgradeManager.Instance?.GrinderLevel ?? 1;
            int gCost = UpgradeManager.Instance?.GetUpgradeCost(gLvl) ?? 0;
            DrawUpgradeCard(sb, x + 24, cardY, cardW, cardH,
                "HIGH-RPM ANGLE GRINDER",
                gLvl,
                $"+{(int)((gLvl - 1) * 60)}% RPM Abrasive Prep Speed",
                "[F2]",
                gCost,
                credits >= gCost && gLvl < 3);

            // 3. Welder
            cardY += cardH + 12;
            int wLvl = UpgradeManager.Instance?.WelderLevel ?? 1;
            int wCost = UpgradeManager.Instance?.GetUpgradeCost(wLvl) ?? 0;
            DrawUpgradeCard(sb, x + 24, cardY, cardW, cardH,
                "MIG FLUX WELDING TORCH",
                wLvl,
                $"+{(int)((wLvl - 1) * 60)}% Arc Thermal Puddle Deposition",
                "[F3]",
                wCost,
                credits >= wCost && wLvl < 3);

            // Footer Hint
            _font.DrawString(sb, "[U] / [ESC] RETURN TO HANGAR WORKSPACE", new Vector2(x + 24, y + h - 28), Color.Silver, 1.3f);
        }

        private void DrawUpgradeCard(SpriteBatch sb, int x, int y, int w, int h, string title, int level, string stat, string key, int cost, bool canAfford)
        {
            sb.Draw(_pixel, new Rectangle(x, y, w, h), new Color(24, 30, 42, 220));
            sb.Draw(_pixel, new Rectangle(x, y, 3, h), Color.Yellow);

            _font.DrawString(sb, title, new Vector2(x + 14, y + 12), Color.White, 1.5f);
            _font.DrawString(sb, $"TIER: Mk.{level} / Mk.3", new Vector2(x + 14, y + 34), Color.Cyan, 1.3f);
            _font.DrawString(sb, stat, new Vector2(x + 14, y + 54), Color.Silver, 1.25f);

            int btnW = 160;
            int btnH = 38;
            int bx = x + w - btnW - 14;
            int by = y + (h - btnH) / 2;

            if (level >= 3)
            {
                sb.Draw(_pixel, new Rectangle(bx, by, btnW, btnH), new Color(40, 50, 60));
                _font.DrawString(sb, "MAX LEVEL", new Vector2(bx + 26, by + 12), Color.Silver, 1.35f);
            }
            else
            {
                Color btnBg = canAfford ? new Color(30, 110, 60) : new Color(60, 30, 30);
                Color textCol = canAfford ? Color.LimeGreen : Color.Tomato;
                sb.Draw(_pixel, new Rectangle(bx, by, btnW, btnH), btnBg);
                sb.Draw(_pixel, new Rectangle(bx, by, btnW, 2), textCol);

                _font.DrawString(sb, $"{key} UPGRADE", new Vector2(bx + 16, by + 6), Color.White, 1.3f);
                _font.DrawString(sb, $"${cost:N0}", new Vector2(bx + 16, by + 22), textCol, 1.3f);
            }
        }

        private void DrawCompletionModal(SpriteBatch sb, int sw, int sh, ContractManager contractManager)
        {
            int modalW = 600;
            int modalH = 155;
            int mx = (sw - modalW) / 2;
            int my = 120;

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
