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

            // 2. Top-Left: Contract & Objectives
            DrawContractTracker(spriteBatch, contractManager, chassis);

            // 3. Top-Right: Target Inspection Card
            DrawTargetCard(spriteBatch, sw - 390, 20, toolManager);

            // 4. Bottom-Center: Tool Hotbar
            DrawToolHotbar(spriteBatch, sw, sh, toolManager);

            // 5. Bottom-Right: Controls Legend
            DrawControlsLegend(spriteBatch, sw - 280, sh - 110);

            // 6. Contract Completion Banner
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
            int y = 20;
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
            sb.Draw(_pixel, new Rectangle(x, y, 260, 95), new Color(15, 18, 22, 190));
            sb.Draw(_pixel, new Rectangle(x, y, 2, 95), Color.DarkSlateGray);

            _font.DrawString(sb, "HANGAR CONTROLS", new Vector2(x + 10, y + 8), Color.Yellow, 1.3f);
            _font.DrawString(sb, "RMB + Drag: Orbit Camera", new Vector2(x + 10, y + 25), Color.Silver, 1.2f);
            _font.DrawString(sb, "MMB / Shift+RMB: Pan", new Vector2(x + 10, y + 40), Color.Silver, 1.2f);
            _font.DrawString(sb, "Scroll Wheel: Zoom", new Vector2(x + 10, y + 55), Color.Silver, 1.2f);
            _font.DrawString(sb, "F: Focus Torso | Space: Reset", new Vector2(x + 10, y + 70), Color.Silver, 1.2f);
        }

        private void DrawCompletionModal(SpriteBatch sb, int sw, int sh, ContractManager contractManager)
        {
            int modalW = 560;
            int modalH = 150;
            int mx = (sw - modalW) / 2;
            int my = 120;

            sb.Draw(_pixel, new Rectangle(mx, my, modalW, modalH), new Color(10, 35, 25, 245));
            sb.Draw(_pixel, new Rectangle(mx, my, modalW, 4), Color.LimeGreen);
            sb.Draw(_pixel, new Rectangle(mx, my + modalH - 4, modalW, 4), Color.LimeGreen);

            _font.DrawString(sb, "CONTRACT OVERHAUL COMPLETE!", new Vector2(mx + 30, my + 20), Color.LimeGreen, 2.1f);
            _font.DrawString(sb, $"+${contractManager.ActiveContract.Reward:N0} CREDITS TRANSFERRED", new Vector2(mx + 30, my + 52), Color.Gold, 1.7f);
            _font.DrawString(sb, "Vanguard Titan combat readiness restored to 100%", new Vector2(mx + 30, my + 82), Color.White, 1.35f);
            _font.DrawString(sb, "[PRESS ENTER TO REPLAY / START NEXT OVERHAUL]", new Vector2(mx + 30, my + 110), Color.Cyan, 1.3f);
        }
    }
}
