using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics.MeshBuilders;
using BuildMechanic.Simulation;
using BuildMechanic.Simulation.Graph;
using BuildMechanic.Tools;

namespace BuildMechanic.Graphics.Renderers
{
    public class ComponentGraphRenderer : IDisposable
    {
        private SimpleMesh _cowlingMesh;
        private SimpleMesh _ribMesh;
        private SimpleMesh _carrierMesh;
        private SimpleMesh _impellerMesh;

        public void Draw(GraphicsDevice device, BasicEffect effect, Texture2D chassisTex, MechChassis chassis, ToolManager tools)
        {
            if (_cowlingMesh == null)
            {
                _cowlingMesh = DeepBayMeshBuilder.CreateThermalCowling(device);
                _ribMesh = DeepBayMeshBuilder.CreateStructuralRib(device);
                _carrierMesh = DeepBayMeshBuilder.CreatePumpCarrier(device);
                _impellerMesh = DeepBayMeshBuilder.CreateImpellerPump(device);
            }

            effect.Texture = chassisTex;

            foreach (var comp in chassis.Components)
            {
                var parentPlate = chassis.Plates.FirstOrDefault(p => p.Id == comp.SectorId);
                if (parentPlate != null && parentPlate.State == PlateMountState.Mounted) continue;

                bool isHovered = tools.HoveredComponent == comp;
                effect.DiffuseColor = GetColor(comp, isHovered);
                effect.World = Matrix.CreateScale(comp.Dimensions) * comp.GetWorldMatrix();

                SimpleMesh mesh = comp.Category switch
                {
                    ComponentCategory.Cowling => _cowlingMesh,
                    ComponentCategory.StructuralRib => _ribMesh,
                    ComponentCategory.Carrier => _carrierMesh,
                    _ => _impellerMesh
                };
                mesh.Draw(device, effect);
            }
        }

        private static Vector3 GetColor(MechComponent comp, bool isHovered)
        {
            if (isHovered) return new Vector3(1.5f, 1.35f, 0.45f);
            if (comp.IsFaulty) return new Vector3(0.95f, 0.25f, 0.15f);
            return comp.Category switch
            {
                ComponentCategory.Cowling => new Vector3(0.35f, 0.40f, 0.48f),
                ComponentCategory.StructuralRib => new Vector3(0.62f, 0.66f, 0.72f),
                ComponentCategory.Carrier => new Vector3(0.42f, 0.52f, 0.58f),
                _ => new Vector3(0.20f, 0.78f, 0.68f)
            };
        }

        public void Dispose()
        {
            _cowlingMesh?.Dispose();
            _ribMesh?.Dispose();
            _carrierMesh?.Dispose();
            _impellerMesh?.Dispose();
        }
    }
}
