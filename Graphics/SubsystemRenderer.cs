using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics.MeshBuilders;
using BuildMechanic.Simulation;
using BuildMechanic.Tools;

namespace BuildMechanic.Graphics
{
    public class SubsystemRenderer : IDisposable
    {
        private SimpleMesh _banjoMesh;

        public void DrawSubsystems(GraphicsDevice device, BasicEffect effect, SimpleMesh boxMesh, SimpleMesh cylinderMesh, Texture2D chassisTex, MechChassis chassis, ToolManager tools)
        {
            if (_banjoMesh == null) _banjoMesh = SubsystemMeshBuilder.CreateBanjoFitting(device);
            effect.Texture = chassisTex;

            // 1. Hydraulic Actuator Cylinders
            foreach (var hydro in chassis.Hydraulics)
            {
                var parentPlate = chassis.Plates.FirstOrDefault(p => p.Id == hydro.SectorId);
                if (parentPlate != null && parentPlate.State == PlateMountState.Mounted) continue;

                Vector3 pos = hydro.Position;
                bool isHovered = tools.HoveredHydraulic == hydro;

                // Outer actuator barrel with heavy cast texture
                effect.DiffuseColor = isHovered ? new Color(255, 220, 80).ToVector3() :
                    (hydro.IsNominal ? new Vector3(0.38f, 0.44f, 0.52f) : new Vector3(0.80f, 0.32f, 0.22f));

                effect.World = Matrix.CreateScale(hydro.Radius * 2f, hydro.Length, hydro.Radius * 2f) *
                               Matrix.CreateTranslation(pos);
                cylinderMesh.Draw(device, effect);

                // Chrome mirror-finish piston rod extending upward
                effect.DiffuseColor = new Vector3(0.92f, 0.95f, 1.0f);
                Vector3 rodPos = pos + new Vector3(0, hydro.Length * 0.45f, 0);
                effect.World = Matrix.CreateScale(hydro.Radius * 1.15f, hydro.Length * 0.5f, hydro.Radius * 1.15f) *
                               Matrix.CreateTranslation(rodPos);
                cylinderMesh.Draw(device, effect);

                // Banjo hydraulic fluid port fitting
                effect.DiffuseColor = new Vector3(0.75f, 0.78f, 0.85f);
                Vector3 banjoPos = pos + new Vector3(0, hydro.Length * 0.28f, hydro.Radius * 0.8f);
                effect.World = Matrix.CreateTranslation(banjoPos);
                _banjoMesh.Draw(device, effect);

                // Knurled brass bleed nipple
                effect.DiffuseColor = hydro.HasAirPocket ? Color.OrangeRed.ToVector3() : Color.LimeGreen.ToVector3();
                Vector3 nipplePos = pos + new Vector3(0, -hydro.Length * 0.25f, hydro.Radius);
                effect.World = Matrix.CreateScale(0.022f, 0.035f, 0.022f) * Matrix.CreateTranslation(nipplePos);
                cylinderMesh.Draw(device, effect);
            }

            // 2. Electrical Relay Avionics Fuse Boxes
            foreach (var relay in chassis.Relays)
            {
                var parentPlate = chassis.Plates.FirstOrDefault(p => p.Id == relay.SectorId);
                if (parentPlate != null && parentPlate.State == PlateMountState.Mounted) continue;

                Vector3 pos = relay.Position;
                bool isHovered = tools.HoveredRelay == relay;

                // Metallic DIN-rail mount backing
                effect.DiffuseColor = new Vector3(0.65f, 0.68f, 0.72f);
                effect.World = Matrix.CreateScale(new Vector3(0.09f, 0.10f, 0.015f)) * Matrix.CreateTranslation(pos - new Vector3(0, 0, 0.015f));
                boxMesh.Draw(device, effect);

                // Relay socket block (black heat-resistant phenolic resin)
                effect.DiffuseColor = isHovered ? new Color(245, 245, 110).ToVector3() : new Vector3(0.18f, 0.19f, 0.22f);
                effect.World = Matrix.CreateScale(new Vector3(0.075f, 0.085f, 0.04f)) * Matrix.CreateTranslation(pos);
                boxMesh.Draw(device, effect);

                // Copper induction coil core
                effect.DiffuseColor = new Vector3(0.85f, 0.55f, 0.25f);
                effect.World = Matrix.CreateScale(new Vector3(0.03f, 0.04f, 0.018f)) * Matrix.CreateTranslation(pos + new Vector3(0, 0.015f, 0.015f));
                boxMesh.Draw(device, effect);

                // Blade fuse cartridge / diagnostic LED
                Vector3 ledPos = pos + new Vector3(0, -0.015f, 0.025f);
                effect.DiffuseColor = relay.IsBlown ? new Vector3(1.6f, 0.15f, 0.1f) : new Vector3(0.1f, 1.4f, 0.35f);
                effect.World = Matrix.CreateScale(new Vector3(0.038f, 0.025f, 0.015f)) * Matrix.CreateTranslation(ledPos);
                boxMesh.Draw(device, effect);
            }
        }

        public void Dispose()
        {
            _banjoMesh?.Dispose();
        }
    }
}
