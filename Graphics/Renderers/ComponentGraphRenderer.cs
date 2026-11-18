using System;
using System.Collections.Generic;
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
        private readonly Dictionary<ComponentCategory, SimpleMesh> _meshes = new Dictionary<ComponentCategory, SimpleMesh>();

        public void Draw(GraphicsDevice device, BasicEffect effect, Texture2D chassisTex, MechChassis chassis, ToolManager tools)
        {
            if (_meshes.Count == 0)
            {
                _meshes[ComponentCategory.Cowling] = DeepBayMeshBuilder.CreateThermalCowling(device);
                _meshes[ComponentCategory.StructuralRib] = DeepBayMeshBuilder.CreateStructuralRib(device);
                _meshes[ComponentCategory.Carrier] = DeepBayMeshBuilder.CreatePumpCarrier(device);
                _meshes[ComponentCategory.ImpellerPump] = DeepBayMeshBuilder.CreateImpellerPump(device);
                _meshes[ComponentCategory.GyroCage] = CoreInternalsMeshBuilder.CreateGyroCage(device);
                _meshes[ComponentCategory.PowerPCB] = CoreInternalsMeshBuilder.CreatePowerPCB(device);
                _meshes[ComponentCategory.GearRing] = CoreInternalsMeshBuilder.CreateGearRing(device);
                _meshes[ComponentCategory.Bearing] = CoreInternalsMeshBuilder.CreateBearingAssembly(device);
                _meshes[ComponentCategory.ShockStrut] = CoreInternalsMeshBuilder.CreateShockStrut(device);
                _meshes[ComponentCategory.Accumulator] = CoreInternalsMeshBuilder.CreateAccumulator(device);
            }

            effect.Texture = chassisTex;

            foreach (var comp in chassis.Components)
            {
                var parentPlate = chassis.Plates.FirstOrDefault(p => p.Id == comp.SectorId);
                if (parentPlate != null && parentPlate.State == PlateMountState.Mounted) continue;

                bool isHovered = tools.HoveredComponent == comp;
                effect.DiffuseColor = GetMaterialColor(comp, isHovered);
                effect.World = Matrix.CreateScale(comp.Dimensions) * comp.GetWorldMatrix();

                if (_meshes.TryGetValue(comp.Category, out var mesh))
                {
                    mesh.Draw(device, effect);
                }
            }
        }

        private static Vector3 GetMaterialColor(MechComponent comp, bool isHovered)
        {
            if (isHovered) return new Vector3(1.5f, 1.35f, 0.45f);
            if (comp.IsFaulty || comp.WearState == ComponentWearState.Seized) return new Vector3(0.95f, 0.22f, 0.14f);

            Vector3 baseCol = comp.Category switch
            {
                ComponentCategory.Cowling => new Vector3(0.35f, 0.40f, 0.48f),
                ComponentCategory.StructuralRib => new Vector3(0.64f, 0.68f, 0.74f),
                ComponentCategory.Carrier => new Vector3(0.42f, 0.52f, 0.58f),
                ComponentCategory.ImpellerPump => new Vector3(0.20f, 0.78f, 0.68f),
                ComponentCategory.GyroCage => new Vector3(0.78f, 0.68f, 0.35f),
                ComponentCategory.PowerPCB => new Vector3(0.18f, 0.58f, 0.32f),
                ComponentCategory.GearRing => new Vector3(0.72f, 0.74f, 0.78f),
                ComponentCategory.Bearing => new Vector3(0.84f, 0.86f, 0.90f),
                ComponentCategory.ShockStrut => new Vector3(0.82f, 0.48f, 0.18f),
                _ => new Vector3(0.28f, 0.62f, 0.82f)
            };

            if (comp.WearState == ComponentWearState.Rusty && comp.RustLevel > 0f)
            {
                Vector3 rustTint = new Vector3(0.48f, 0.24f, 0.12f);
                return Vector3.Lerp(baseCol, rustTint, comp.RustLevel * 0.85f);
            }

            return baseCol;
        }

        public void Dispose()
        {
            foreach (var m in _meshes.Values) m?.Dispose();
            _meshes.Clear();
        }
    }
}
