using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics;
using BuildMechanic.Simulation;
using BuildMechanic.Tools;

namespace BuildMechanic.UI
{
    public static class HUDTargetTelemetry
    {
        public static void DrawHolographicReticle(
            SpriteBatch sb, Texture2D pixel, ProceduralFont font,
            int cx, int cy, ToolManager tools)
        {
            Color reticleColor = tools.HoveredBolt != null ? Color.Gold :
                                (tools.HoveredCrack != null ? Color.OrangeRed :
                                (tools.HoveredHydraulic != null ? Color.DeepSkyBlue :
                                (tools.HoveredRelay != null ? Color.Magenta :
                                (tools.HoveredPlate != null ? Color.Cyan : Color.White * 0.75f))));

            bool hasTarget = tools.HoveredBolt != null || tools.HoveredCrack != null ||
                             tools.HoveredHydraulic != null || tools.HoveredRelay != null ||
                             tools.HoveredPlate != null;

            int len = 8, gap = 4, thick = 2;
            sb.Draw(pixel, new Rectangle(cx - gap - len, cy - thick / 2, len, thick), reticleColor);
            sb.Draw(pixel, new Rectangle(cx + gap, cy - thick / 2, len, thick), reticleColor);
            sb.Draw(pixel, new Rectangle(cx - thick / 2, cy - gap - len, thick, len), reticleColor);
            sb.Draw(pixel, new Rectangle(cx - thick / 2, cy + gap, thick, len), reticleColor);
            sb.Draw(pixel, new Rectangle(cx - 1, cy - 1, 2, 2), reticleColor);

            if (hasTarget)
            {
                // Animated holographic lock-on corner brackets
                int bSize = 16;
                int bOffset = 18;
                Color bCol = reticleColor * 0.85f;

                // Top-Left
                sb.Draw(pixel, new Rectangle(cx - bOffset, cy - bOffset, bSize, 2), bCol);
                sb.Draw(pixel, new Rectangle(cx - bOffset, cy - bOffset, 2, bSize), bCol);
                // Top-Right
                sb.Draw(pixel, new Rectangle(cx + bOffset - bSize, cy - bOffset, bSize, 2), bCol);
                sb.Draw(pixel, new Rectangle(cx + bOffset - 2, cy - bOffset, 2, bSize), bCol);
                // Bottom-Left
                sb.Draw(pixel, new Rectangle(cx - bOffset, cy + bOffset - 2, bSize, 2), bCol);
                sb.Draw(pixel, new Rectangle(cx - bOffset, cy + bOffset - bSize, 2, bSize), bCol);
                // Bottom-Right
                sb.Draw(pixel, new Rectangle(cx + bOffset - bSize, cy + bOffset - 2, bSize, 2), bCol);
                sb.Draw(pixel, new Rectangle(cx + bOffset - 2, cy + bOffset - bSize, 2, bSize), bCol);
            }
        }

        public static void DrawTargetCard(
            SpriteBatch sb, Texture2D pixel, ProceduralFont font,
            int x, int y, ToolManager tools)
        {
            int w = 380;
            if (tools.HoveredPlate == null && tools.HoveredBolt == null && tools.HoveredCrack == null &&
                tools.HoveredHydraulic == null && tools.HoveredRelay == null) return;

            int h = 155;
            sb.Draw(pixel, new Rectangle(x, y, w, h), new Color(16, 20, 26, 240));
            sb.Draw(pixel, new Rectangle(x, y, 4, h), Color.Yellow);
            sb.Draw(pixel, new Rectangle(x, y, w, 1), new Color(50, 75, 100));

            if (tools.HoveredHydraulic != null)
                DrawHydraulicTelemetry(sb, pixel, font, x, y, w, tools.HoveredHydraulic);
            else if (tools.HoveredRelay != null)
                DrawRelayTelemetry(sb, pixel, font, x, y, w, tools.HoveredRelay);
            else if (tools.HoveredBolt != null)
                DrawFastenerTelemetry(sb, pixel, font, x, y, w, tools.HoveredBolt);
            else if (tools.HoveredCrack != null)
                DrawCrackTelemetry(sb, pixel, font, x, y, w, tools.HoveredCrack);
            else if (tools.HoveredPlate != null)
                DrawPlateTelemetry(sb, pixel, font, x, y, w, tools.HoveredPlate);
        }

        private static void DrawHydraulicTelemetry(SpriteBatch sb, Texture2D pixel, ProceduralFont font, int x, int y, int w, HydraulicCylinder hyl)
        {
            font.DrawString(sb, "HYDRAULIC ACTUATOR TELEMETRY", new Vector2(x + 14, y + 12), Color.DeepSkyBlue, 1.55f);
            Color pressCol = hyl.IsNominal ? Color.LimeGreen : (hyl.CurrentPressure < 100 ? Color.OrangeRed : Color.Gold);
            font.DrawString(sb, $"PRESSURE: {(int)hyl.CurrentPressure} / {(int)hyl.TargetPressure} BAR", new Vector2(x + 14, y + 36), pressCol, 1.35f);

            string status = hyl.HasAirPocket ? "AIR POCKET DETECTED [HOLD 7: BLEED]" : (hyl.Contamination > 15f ? "CONTAMINATED OIL [HOLD 7: FLUSH]" : "NOMINAL PRESSURE & VISCOSITY");
            Color statCol = hyl.HasAirPocket ? Color.Tomato : (hyl.Contamination > 15f ? Color.Orange : Color.Silver);
            font.DrawString(sb, $"STATUS: {status}", new Vector2(x + 14, y + 58), statCol, 1.15f);

            DrawGaugeBar(sb, pixel, x + 14, y + 80, w - 28, 8, hyl.CurrentPressure / hyl.TargetPressure, pressCol);

            font.DrawString(sb, $"OIL PURITY: {(int)(100f - hyl.Contamination)}% | CAVITATION: {(hyl.HasAirPocket ? "WARN" : "OK")}", new Vector2(x + 14, y + 96), Color.DarkGray, 1.15f);
        }

        private static void DrawRelayTelemetry(SpriteBatch sb, Texture2D pixel, ProceduralFont font, int x, int y, int w, ElectricalRelay r)
        {
            font.DrawString(sb, $"AVIONICS RELAY FUSE #{r.SlotIndex}", new Vector2(x + 14, y + 12), Color.Magenta, 1.55f);
            string status = r.IsBlown ? "BLOWN FUSE [CLICK/RMB: REPLACE]" : (r.TestedWithProbe ? "CONTINUITY OK (0.5 OHM)" : "UNTESTED [CLICK: PROBE]");
            font.DrawString(sb, $"DIAGNOSTIC: {status}", new Vector2(x + 14, y + 36), r.IsBlown ? Color.Tomato : Color.LimeGreen, 1.25f);
            font.DrawString(sb, $"CIRCUIT BUS ID: {r.Id.ToUpper()} | RATING: 30A DC", new Vector2(x + 14, y + 58), Color.Silver, 1.20f);

            // Simulated digital 7-segment OLED resistance meter readout
            string ohmReadout = r.IsBlown ? "O.L. (OPEN LOOP)" : (r.TestedWithProbe ? "000.48 OHM" : "---.- OHM");
            Color ohmColor = r.IsBlown ? Color.Tomato : (r.TestedWithProbe ? Color.Cyan : Color.DarkGray);
            font.DrawString(sb, $"RESISTANCE: {ohmReadout}", new Vector2(x + 14, y + 80), ohmColor, 1.4f);
        }

        private static void DrawFastenerTelemetry(SpriteBatch sb, Texture2D pixel, ProceduralFont font, int x, int y, int w, Bolt bolt)
        {
            font.DrawString(sb, "ISO METRIC FASTENER TELEMETRY", new Vector2(x + 14, y + 12), Color.Gold, 1.55f);
            string status = bolt.IsTorqued ? "TORQUED TO SPEC (100%)" : (bolt.IsRemoved ? "LOOSE / UNTHREADED (0%)" : $"THREADED ({(int)(bolt.ThreadProgress * 100)}%)");
            Color statusColor = bolt.IsTorqued ? Color.LimeGreen : (bolt.IsRemoved ? Color.OrangeRed : Color.Gold);
            font.DrawString(sb, $"TORQUE STATUS: {status}", new Vector2(x + 14, y + 36), statusColor, 1.35f);

            DrawGaugeBar(sb, pixel, x + 14, y + 62, w - 28, 8, bolt.ThreadProgress, statusColor);

            int torqueNm = (int)(bolt.ThreadProgress * 145f);
            font.DrawString(sb, $"CLAMP LOAD: {torqueNm} N*m | SPEC: 145 N*m | GRADE: 10.9", new Vector2(x + 14, y + 78), Color.Silver, 1.15f);
        }

        private static void DrawCrackTelemetry(SpriteBatch sb, Texture2D pixel, ProceduralFont font, int x, int y, int w, CrackSegment crack)
        {
            font.DrawString(sb, "STRUCTURAL METALLURGICAL DEFECT", new Vector2(x + 14, y + 12), Color.OrangeRed, 1.55f);
            font.DrawString(sb, $"REPAIR STAGE: {crack.Stage.ToString().ToUpper()}", new Vector2(x + 14, y + 36), Color.Yellow, 1.35f);

            DrawGaugeBar(sb, pixel, x + 14, y + 62, w - 28, 8, crack.StageProgress, Color.Orange);

            string action = crack.Stage == CrackStage.Fractured
                ? "ACTION REQUIRED: PREP WELD GROOVE [HOLD 4: GRINDER]"
                : (crack.Stage == CrackStage.PreppedGroove ? "ACTION REQUIRED: DEPOSIT WELD BEAD [HOLD 5: WELDER]" : "REPAIR COMPLETE - WELD BEAD FLUSH");
            font.DrawString(sb, action, new Vector2(x + 14, y + 78), Color.Silver, 1.15f);
        }

        private static void DrawPlateTelemetry(SpriteBatch sb, Texture2D pixel, ProceduralFont font, int x, int y, int w, ArmorPlate p)
        {
            font.DrawString(sb, p.DisplayName.ToUpper(), new Vector2(x + 14, y + 12), Color.Cyan, 1.50f);
            font.DrawString(sb, $"BALLISTIC INTEGRITY: {(int)p.Integrity}% | FASTENERS: {p.Bolts.Count(b => b.IsTorqued)}/{p.Bolts.Count}", new Vector2(x + 14, y + 36), Color.Silver, 1.25f);

            Color integCol = p.Integrity > 90 ? Color.LimeGreen : (p.Integrity > 50 ? Color.Gold : Color.OrangeRed);
            DrawGaugeBar(sb, pixel, x + 14, y + 62, w - 28, 8, p.Integrity / 100f, integCol);

            string mountStatus = p.State == PlateMountState.Mounted ? "MOUNTED FIRMLY" : "DETACHED ON RIG / CRANE SLING";
            font.DrawString(sb, $"MOUNT STATE: {mountStatus}", new Vector2(x + 14, y + 78), Color.DarkGray, 1.15f);
        }

        private static void DrawGaugeBar(SpriteBatch sb, Texture2D pixel, int x, int y, int width, int height, float progress, Color fill)
        {
            progress = Math.Clamp(progress, 0f, 1f);
            sb.Draw(pixel, new Rectangle(x, y, width, height), new Color(30, 36, 45));
            sb.Draw(pixel, new Rectangle(x, y, (int)(width * progress), height), fill);
            sb.Draw(pixel, new Rectangle(x, y, width, 1), new Color(60, 75, 90));
            sb.Draw(pixel, new Rectangle(x, y + height - 1, width, 1), new Color(60, 75, 90));
        }
    }
}
