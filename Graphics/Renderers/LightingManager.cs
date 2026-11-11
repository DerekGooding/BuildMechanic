using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Tools;

namespace BuildMechanic.Graphics.Renderers
{
    public class LightingManager
    {
        public void ApplyLighting(BasicEffect effect, ToolManager tools)
        {
            effect.LightingEnabled = true;
            effect.PreferPerPixelLighting = true;

            // 1. Key Light: High-angle cool industrial floodlight
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-0.55f, -1.0f, -0.75f));
            effect.DirectionalLight0.DiffuseColor = new Vector3(0.95f, 0.96f, 1.00f);
            effect.DirectionalLight0.SpecularColor = new Vector3(0.75f, 0.80f, 0.90f);

            // 2. Fill Light: Warm floor bounce light
            effect.DirectionalLight1.Enabled = true;
            effect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(0.75f, -0.35f, 0.55f));
            effect.DirectionalLight1.DiffuseColor = new Vector3(0.38f, 0.40f, 0.46f);
            effect.DirectionalLight1.SpecularColor = new Vector3(0.20f, 0.22f, 0.25f);

            // 3. Material Specular Power for high-gloss steel
            effect.SpecularPower = 32;

            // 4. Dynamic Interactive Tool Arc & Spark Light
            if (tools.IsOperating && tools.ActiveTool == ToolType.Welder)
            {
                float flicker = tools.ArcFlickerIntensity;
                // Blinding electric blue-white arc flash
                effect.AmbientLightColor = new Vector3(
                    0.28f + flicker * 0.40f,
                    0.32f + flicker * 0.48f,
                    0.42f + flicker * 0.85f
                );
            }
            else if (tools.IsOperating && tools.ActiveTool == ToolType.AngleGrinder)
            {
                float sparkPulse = (float)Math.Sin(DateTime.Now.Millisecond * 0.08) * 0.12f;
                // Warm amber grinding spark glow
                effect.AmbientLightColor = new Vector3(
                    0.36f + sparkPulse,
                    0.30f + sparkPulse * 0.6f,
                    0.26f
                );
            }
            else
            {
                effect.AmbientLightColor = new Vector3(0.26f, 0.28f, 0.32f);
            }
        }
    }
}
