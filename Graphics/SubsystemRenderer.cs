using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Simulation;
using BuildMechanic.Tools;

namespace BuildMechanic.Graphics
{
    public class SubsystemRenderer
    {
        public void DrawSubsystems(GraphicsDevice device, BasicEffect effect, SimpleMesh boxMesh, SimpleMesh cylinderMesh, Texture2D chassisTex, MechChassis chassis, ToolManager tools)
        {
            effect.Texture = chassisTex;

            // 1. Draw Hydraulic Cylinders
            foreach (var hydro in chassis.Hydraulics)
            {
                var parentPlate = chassis.Plates.FirstOrDefault(p => p.Id == hydro.SectorId);
                // Only render when parent plate is detached or moving so player can see internal components
                if (parentPlate != null && parentPlate.State == PlateMountState.Mounted) continue;

                Vector3 pos = hydro.Position;
                bool isHovered = tools.HoveredHydraulic == hydro;

                // Outer actuator barrel
                effect.DiffuseColor = isHovered ? new Color(255, 220, 80).ToVector3() :
                    (hydro.IsNominal ? new Vector3(0.35f, 0.40f, 0.48f) : new Vector3(0.75f, 0.35f, 0.25f));

                effect.World = Matrix.CreateScale(hydro.Radius * 2f, hydro.Length, hydro.Radius * 2f) *
                               Matrix.CreateTranslation(pos);
                cylinderMesh.Draw(device, effect);

                // Chrome piston rod extending upward
                effect.DiffuseColor = new Vector3(0.85f, 0.88f, 0.92f);
                Vector3 rodPos = pos + new Vector3(0, hydro.Length * 0.45f, 0);
                effect.World = Matrix.CreateScale(hydro.Radius * 1.1f, hydro.Length * 0.5f, hydro.Radius * 1.1f) *
                               Matrix.CreateTranslation(rodPos);
                cylinderMesh.Draw(device, effect);

                // Quick connect fluid bleed nipple
                effect.DiffuseColor = hydro.HasAirPocket ? Color.OrangeRed.ToVector3() : Color.LimeGreen.ToVector3();
                Vector3 nipplePos = pos + new Vector3(0, -hydro.Length * 0.25f, hydro.Radius);
                effect.World = Matrix.CreateScale(0.02f, 0.03f, 0.02f) * Matrix.CreateTranslation(nipplePos);
                cylinderMesh.Draw(device, effect);
            }

            // 2. Draw Electrical Relay Fuse Boxes
            foreach (var relay in chassis.Relays)
            {
                var parentPlate = chassis.Plates.FirstOrDefault(p => p.Id == relay.SectorId);
                if (parentPlate != null && parentPlate.State == PlateMountState.Mounted) continue;

                Vector3 pos = relay.Position;
                bool isHovered = tools.HoveredRelay == relay;

                // Relay socket housing (black industrial plastic)
                effect.DiffuseColor = isHovered ? new Color(240, 240, 100).ToVector3() : new Vector3(0.15f, 0.16f, 0.18f);
                effect.World = Matrix.CreateScale(new Vector3(0.07f, 0.08f, 0.04f)) * Matrix.CreateTranslation(pos);
                boxMesh.Draw(device, effect);

                // Fuse glass cartridge / status LED
                Vector3 ledPos = pos + new Vector3(0, 0, 0.025f);
                effect.DiffuseColor = relay.IsBlown ? new Vector3(1.4f, 0.2f, 0.1f) : new Vector3(0.1f, 1.2f, 0.3f);
                effect.World = Matrix.CreateScale(new Vector3(0.035f, 0.045f, 0.02f)) * Matrix.CreateTranslation(ledPos);
                boxMesh.Draw(device, effect);
            }
        }
    }
}
