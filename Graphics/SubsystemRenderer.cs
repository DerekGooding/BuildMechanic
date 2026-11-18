using System;
using System.Collections.Generic;
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
        private SimpleMesh _bracketMesh;
        private SimpleMesh _manifoldMesh;
        private SimpleMesh _plugMesh;
        private readonly Dictionary<string, SimpleMesh> _harnessMeshes = new Dictionary<string, SimpleMesh>();

        public void DrawSubsystems(GraphicsDevice device, BasicEffect effect, SimpleMesh boxMesh, SimpleMesh cylinderMesh, Texture2D chassisTex, MechChassis chassis, ToolManager tools)
        {
            EnsureMeshes(device);
            effect.Texture = chassisTex;

            DrawHydraulics(device, effect, cylinderMesh, chassis, tools);
            DrawRelays(device, effect, boxMesh, chassis, tools);
            DrawHarnesses(device, effect, chassis, tools);
            DrawBrackets(device, effect, chassis, tools);
        }

        private void EnsureMeshes(GraphicsDevice device)
        {
            if (_banjoMesh == null) _banjoMesh = SubsystemMeshBuilder.CreateBanjoFitting(device);
            if (_bracketMesh == null) _bracketMesh = MechanicalBayMeshBuilder.CreateRetainingBracket(device);
            if (_manifoldMesh == null) _manifoldMesh = MechanicalBayMeshBuilder.CreateHydraulicManifold(device);
            if (_plugMesh == null) _plugMesh = WiringMeshBuilder.CreateConnectorPlug(device);
        }

        private void DrawHydraulics(GraphicsDevice device, BasicEffect effect, SimpleMesh cylinderMesh, MechChassis chassis, ToolManager tools)
        {
            foreach (var hydro in chassis.Hydraulics)
            {
                var parentPlate = chassis.Plates.FirstOrDefault(p => p.Id == hydro.SectorId);
                if (parentPlate != null && parentPlate.State == PlateMountState.Mounted) continue;

                Vector3 pos = hydro.Position;
                bool isHovered = tools.HoveredHydraulic == hydro;

                // Outer actuator barrel
                effect.DiffuseColor = isHovered ? new Color(255, 220, 80).ToVector3() :
                    (hydro.IsNominal ? new Vector3(0.38f, 0.44f, 0.52f) : new Vector3(0.80f, 0.32f, 0.22f));
                effect.World = Matrix.CreateScale(hydro.Radius * 2f, hydro.Length, hydro.Radius * 2f) * Matrix.CreateTranslation(pos);
                cylinderMesh.Draw(device, effect);

                // Chrome piston rod
                effect.DiffuseColor = new Vector3(0.92f, 0.95f, 1.0f);
                effect.World = Matrix.CreateScale(hydro.Radius * 1.15f, hydro.Length * 0.5f, hydro.Radius * 1.15f)
                    * Matrix.CreateTranslation(pos + new Vector3(0, hydro.Length * 0.45f, 0));
                cylinderMesh.Draw(device, effect);

                // Banjo hydraulic fitting
                effect.DiffuseColor = new Vector3(0.75f, 0.78f, 0.85f);
                effect.World = Matrix.CreateTranslation(pos + new Vector3(0, hydro.Length * 0.28f, hydro.Radius * 0.8f));
                _banjoMesh.Draw(device, effect);

                // Machined Hydraulic Manifold Block beside actuator
                effect.DiffuseColor = isHovered ? new Color(255, 220, 80).ToVector3() : new Vector3(0.46f, 0.50f, 0.56f);
                effect.World = Matrix.CreateTranslation(pos + new Vector3(hydro.Radius * 2.4f, -0.05f, 0.02f));
                _manifoldMesh.Draw(device, effect);

                // Bleed nipple status indicator
                effect.DiffuseColor = hydro.HasAirPocket ? Color.OrangeRed.ToVector3() : Color.LimeGreen.ToVector3();
                effect.World = Matrix.CreateScale(0.022f, 0.035f, 0.022f)
                    * Matrix.CreateTranslation(pos + new Vector3(0, -hydro.Length * 0.25f, hydro.Radius));
                cylinderMesh.Draw(device, effect);
            }
        }

        private void DrawRelays(GraphicsDevice device, BasicEffect effect, SimpleMesh boxMesh, MechChassis chassis, ToolManager tools)
        {
            foreach (var relay in chassis.Relays)
            {
                var parentPlate = chassis.Plates.FirstOrDefault(p => p.Id == relay.SectorId);
                if (parentPlate != null && parentPlate.State == PlateMountState.Mounted) continue;

                Vector3 pos = relay.Position;
                bool isHovered = tools.HoveredRelay == relay;

                effect.DiffuseColor = new Vector3(0.65f, 0.68f, 0.72f);
                effect.World = Matrix.CreateScale(new Vector3(0.09f, 0.10f, 0.015f)) * Matrix.CreateTranslation(pos - new Vector3(0, 0, 0.015f));
                boxMesh.Draw(device, effect);

                effect.DiffuseColor = isHovered ? new Color(245, 245, 110).ToVector3() : new Vector3(0.18f, 0.19f, 0.22f);
                effect.World = Matrix.CreateScale(new Vector3(0.075f, 0.085f, 0.04f)) * Matrix.CreateTranslation(pos);
                boxMesh.Draw(device, effect);

                effect.DiffuseColor = new Vector3(0.85f, 0.55f, 0.25f);
                effect.World = Matrix.CreateScale(new Vector3(0.03f, 0.04f, 0.018f)) * Matrix.CreateTranslation(pos + new Vector3(0, 0.015f, 0.015f));
                boxMesh.Draw(device, effect);

                effect.DiffuseColor = relay.IsBlown ? new Vector3(1.6f, 0.15f, 0.1f) : new Vector3(0.1f, 1.4f, 0.35f);
                effect.World = Matrix.CreateScale(new Vector3(0.038f, 0.025f, 0.015f)) * Matrix.CreateTranslation(pos + new Vector3(0, -0.015f, 0.025f));
                boxMesh.Draw(device, effect);
            }
        }

        private void DrawHarnesses(GraphicsDevice device, BasicEffect effect, MechChassis chassis, ToolManager tools)
        {
            foreach (var harness in chassis.Harnesses)
            {
                var parentPlate = chassis.Plates.FirstOrDefault(p => p.Id == harness.SectorId);
                if (parentPlate != null && parentPlate.State == PlateMountState.Mounted) continue;

                if (!_harnessMeshes.TryGetValue(harness.Id, out var cableMesh))
                {
                    cableMesh = WiringMeshBuilder.CreateCableHarness(device, harness.StartPos, harness.EndPos);
                    _harnessMeshes[harness.Id] = cableMesh;
                }

                bool isHovered = tools.HoveredHarness == harness;

                // Terminal connector plugs at both ends
                effect.DiffuseColor = isHovered ? new Vector3(1.4f, 1.3f, 0.4f) : new Vector3(0.28f, 0.30f, 0.35f);
                effect.World = Matrix.CreateTranslation(harness.StartPos);
                _plugMesh.Draw(device, effect);
                effect.World = Matrix.CreateTranslation(harness.EndPos);
                _plugMesh.Draw(device, effect);

                // Cable bundle (charred = blackened/amber fault; unclipped = faint ghost guide; nominal = bus color)
                if (harness.State == HarnessState.Unclipped)
                {
                    effect.DiffuseColor = isHovered ? new Vector3(0.4f, 1.5f, 1.8f) : new Vector3(0.15f, 0.45f, 0.55f);
                }
                else if (harness.State == HarnessState.Charred)
                {
                    effect.DiffuseColor = isHovered ? new Vector3(1.6f, 0.8f, 0.2f) : new Vector3(0.24f, 0.12f, 0.08f);
                }
                else
                {
                    effect.DiffuseColor = isHovered ? new Vector3(1.4f, 1.4f, 0.6f) : harness.WireColor.ToVector3();
                }

                effect.World = Matrix.Identity;
                cableMesh.Draw(device, effect);
            }
        }

        private void DrawBrackets(GraphicsDevice device, BasicEffect effect, MechChassis chassis, ToolManager tools)
        {
            foreach (var bracket in chassis.Brackets)
            {
                var parentPlate = chassis.Plates.FirstOrDefault(p => p.Id == bracket.SectorId);
                if (parentPlate != null && parentPlate.State == PlateMountState.Mounted) continue;

                bool isHovered = tools.HoveredBracket == bracket;
                effect.DiffuseColor = isHovered
                    ? new Vector3(1.4f, 1.3f, 0.5f)
                    : (bracket.IsFullySecured ? new Vector3(0.42f, 0.48f, 0.56f) : new Vector3(0.72f, 0.52f, 0.25f));

                effect.World = Matrix.CreateScale(bracket.Dimensions) * bracket.GetWorldMatrix();
                _bracketMesh.Draw(device, effect);
            }
        }

        public void Dispose()
        {
            _banjoMesh?.Dispose();
            _bracketMesh?.Dispose();
            _manifoldMesh?.Dispose();
            _plugMesh?.Dispose();
            foreach (var mesh in _harnessMeshes.Values)
            {
                mesh?.Dispose();
            }
            _harnessMeshes.Clear();
        }
    }
}
