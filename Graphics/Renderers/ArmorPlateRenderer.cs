using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics.MeshBuilders;
using BuildMechanic.Graphics.Textures;
using BuildMechanic.Simulation;
using BuildMechanic.Tools;

namespace BuildMechanic.Graphics.Renderers
{
    public class ArmorPlateRenderer : IDisposable
    {
        private readonly SimpleMesh _torsoPlateMesh;
        private readonly SimpleMesh _shoulderPlateMesh;
        private readonly SimpleMesh _legPlateMesh;
        private readonly Texture2D _armorTex;

        public Texture2D ArmorTexture => _armorTex;

        public ArmorPlateRenderer(GraphicsDevice device)
        {
            _armorTex = ArmorTextureGenerator.CreateDetailedArmorTexture(device, 512, 512, withChevrons: true);

            _torsoPlateMesh = ArmorPlateMeshBuilder.CreateTorsoPlate(device, new Vector3(0.85f, 1.05f, 0.08f), 0.30f, 0.38f);
            _shoulderPlateMesh = ArmorPlateMeshBuilder.CreateShoulderPlate(device, new Vector3(0.75f, 0.52f, 0.08f), 0.26f, 0.17f);
            _legPlateMesh = ArmorPlateMeshBuilder.CreateLegPlate(device, new Vector3(0.50f, 0.82f, 0.08f), 0.16f, 0.28f);
        }

        public void Draw(GraphicsDevice device, BasicEffect effect, MechChassis chassis, ToolManager tools)
        {
            effect.Texture = _armorTex;

            foreach (var plate in chassis.Plates)
            {
                Matrix plateWorld = plate.GetWorldMatrix();
                Vector3 tint = plate.BaseCoatingColor.ToVector3();

                if (tools.ActiveTool == ToolType.Scanner)
                {
                    effect.DiffuseColor = plate.HasUnrepairedCracks ?
                        new Vector3(1.4f, 0.4f, 0.3f) : new Vector3(0.4f, 1.3f, 0.5f);
                }
                else if (tools.HoveredPlate == plate && tools.HoveredBolt == null && tools.HoveredCrack == null)
                {
                    effect.DiffuseColor = tint * 1.35f;
                }
                else
                {
                    effect.DiffuseColor = tint;
                }

                effect.World = plateWorld;
                SimpleMesh mesh = SelectPlateMesh(plate);
                mesh.Draw(device, effect);
            }
        }

        private SimpleMesh SelectPlateMesh(ArmorPlate plate)
        {
            if (plate.Id.Contains("torso")) return _torsoPlateMesh;
            if (plate.Id.Contains("shoulder")) return _shoulderPlateMesh;
            return _legPlateMesh;
        }

        public void Dispose()
        {
            _torsoPlateMesh?.Dispose();
            _shoulderPlateMesh?.Dispose();
            _legPlateMesh?.Dispose();
            _armorTex?.Dispose();
        }
    }
}
