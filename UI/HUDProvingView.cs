using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Gameplay;

namespace BuildMechanic.UI
{
    public class HUDProvingView
    {
        public void Draw(SpriteBatch sb, Texture2D pixel, ProceduralFont font, int sw, int sh, ProvingGroundsManager proving)
        {
            if (proving.CurrentStage == ProvingStage.Idle) return;

            int w = 580;
            int h = proving.CurrentStage == ProvingStage.TelemetryVerdict ? 230 : 130;
            int x = (sw - w) / 2;
            int y = 55;

            // Background panel
            sb.Draw(pixel, new Rectangle(x, y, w, h), new Color(12, 18, 25, 240));
            Color borderColor = proving.CurrentStage == ProvingStage.TelemetryVerdict
                ? (proving.IsFlawless ? Color.LimeGreen : Color.OrangeRed)
                : Color.Cyan;

            sb.Draw(pixel, new Rectangle(x, y, w, 3), borderColor);
            sb.Draw(pixel, new Rectangle(x, y + h - 3, w, 3), borderColor);
            sb.Draw(pixel, new Rectangle(x, y, 3, h), borderColor);
            sb.Draw(pixel, new Rectangle(x + w - 3, y, 3, h), borderColor);

            font.DrawString(sb, "HANGAR PROVING GROUNDS: TELEMETRY RUN", new Vector2(x + 20, y + 14), Color.Cyan, 1.6f);

            if (proving.CurrentStage != ProvingStage.TelemetryVerdict)
            {
                string stageText = proving.CurrentStage switch
                {
                    ProvingStage.ReactorSpool => $"SPOOLING CORE REACTOR TURBINE... [{(int)proving.ReactorRpmPercent}% RPM]",
                    ProvingStage.HydraulicPressurize => "PRESSURIZING ACTUATOR HYDRAULIC LINES... [100 BAR]",
                    ProvingStage.ActuatorRangeCheck => "CYCLING ARTICULATION SERVO MOTIONS...",
                    _ => ""
                };

                font.DrawString(sb, stageText, new Vector2(x + 20, y + 42), Color.Gold, 1.35f);

                // Smooth overall test progress across all 3 active phases (4.8s total)
                int barW = w - 40;
                sb.Draw(pixel, new Rectangle(x + 20, y + 74, barW, 14), new Color(30, 40, 50));
                float progress = proving.CurrentStage switch
                {
                    ProvingStage.ReactorSpool => (proving.StageTimer / 1.5f) * 0.33f,
                    ProvingStage.HydraulicPressurize => 0.33f + (proving.StageTimer / 1.5f) * 0.33f,
                    ProvingStage.ActuatorRangeCheck => 0.66f + (proving.StageTimer / 1.8f) * 0.34f,
                    _ => 1.0f
                };
                progress = Math.Clamp(progress, 0.04f, 1.0f);
                sb.Draw(pixel, new Rectangle(x + 20, y + 74, (int)(barW * progress), 14), Color.Cyan);
                return;
            }

            // Telemetry Verdict
            if (proving.IsFlawless)
            {
                font.DrawString(sb, "DIAGNOSTIC TEST: 100% COMBAT CERTIFIED", new Vector2(x + 20, y + 45), Color.LimeGreen, 1.8f);
                font.DrawString(sb, $"ALL {proving.TotalChecks} SYSTEMS NOMINAL (HYDRAULICS, RELAYS, PLATES, TORQUE)", new Vector2(x + 20, y + 78), Color.White, 1.25f);
                font.DrawString(sb, $"+${proving.CertificationBonus:N0} CLIENT PERFECTION BONUS CREDITED!", new Vector2(x + 20, y + 106), Color.Gold, 1.5f);
                font.DrawString(sb, "[T: RETEST | ESC: CLOSE]", new Vector2(x + 20, y + 142), Color.Cyan, 1.3f);
            }
            else
            {
                font.DrawString(sb, $"SYSTEM FAULTS DETECTED: {proving.PassedChecks}/{proving.TotalChecks} PASSED", new Vector2(x + 20, y + 45), Color.OrangeRed, 1.8f);
                int msgY = y + 78;
                foreach (var fault in proving.FaultMessages)
                {
                    font.DrawString(sb, $"[!] {fault}", new Vector2(x + 20, msgY), Color.Tomato, 1.2f);
                    msgY += 20;
                }
                font.DrawString(sb, "[RESOLVE FAULTS] [T: RETEST | ESC: CLOSE]", new Vector2(x + 20, y + h - 28), Color.Yellow, 1.25f);
            }
        }
    }
}
