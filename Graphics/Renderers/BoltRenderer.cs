using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Graphics.MeshBuilders;
using BuildMechanic.Graphics.Textures;
using BuildMechanic.Simulation;
using BuildMechanic.Tools;

namespace BuildMechanic.Graphics.Renderers
{
    public class BoltRenderer : IDisposable
    {
        private readonly DetailedBoltMesh _boltMesh;
        private readonly Texture2D _headTex;
        private readonly Texture2D _threadTex;

        public BoltRenderer(GraphicsDevice device)
        {
            _headTex = ThreadTextureGenerator.CreateBoltHeadTexture(device, 512, 256);
            _threadTex = ThreadTextureGenerator.CreateScrewThreadTexture(device, 512, 256);
            _boltMesh = BoltMeshBuilder.CreateDetailedBolt(device, 0.045f, 0.022f, 0.054f, 0.024f, 0.08f);
        }

        public void Draw(GraphicsDevice device, BasicEffect effect, MechChassis chassis, ToolManager tools)
        {
            foreach (var plate in chassis.Plates)
            {
                Matrix plateWorld = plate.GetWorldMatrix();
                Matrix plateRot = plate.GetRotationMatrix();

                foreach (var bolt in plate.Bolts)
                {
                    Vector3 boltPos = bolt.GetWorldPosition(plateWorld);
                    Vector3 tint;

                    if (tools.HoveredBolt == bolt)
                    {
                        tint = new Vector3(1.8f, 1.6f, 0.4f);
                    }
                    else if (bolt.IsTorqued)
                    {
                        tint = new Vector3(0.95f, 0.98f, 1.0f);
                    }
                    else
                    {
                        tint = new Vector3(1.2f, 0.85f, 0.45f);
                    }

                    effect.DiffuseColor = tint;
                    Matrix boltRot = Matrix.CreateRotationY(bolt.SpinAngle) * Matrix.CreateRotationX(MathHelper.PiOver2) * plateRot;
                    effect.World = boltRot * Matrix.CreateTranslation(boltPos);

                    // 1. Draw Flanged Hex Head Crown & Socket
                    effect.Texture = _headTex;
                    _boltMesh.HeadMesh.Draw(device, effect);

                    // 2. Draw Realistic Helical Threaded Shank
                    effect.Texture = _threadTex;
                    _boltMesh.ShankMesh.Draw(device, effect);
                }
            }
        }

        public void Dispose()
        {
            _boltMesh?.Dispose();
            _headTex?.Dispose();
            _threadTex?.Dispose();
        }
    }
}
