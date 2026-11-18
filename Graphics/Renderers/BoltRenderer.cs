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
                    Matrix normalTilt = Matrix.Identity;
                    Vector3 n = Vector3.Normalize(bolt.Normal);
                    float dot = Math.Clamp(Vector3.Dot(Vector3.Backward, n), -1f, 1f);
                    if (dot < 0.9999f && dot > -0.9999f)
                    {
                        Vector3 axis = Vector3.Normalize(Vector3.Cross(Vector3.Backward, n));
                        normalTilt = Matrix.CreateFromAxisAngle(axis, MathF.Acos(dot));
                    }
                    Matrix boltRot = Matrix.CreateRotationY(bolt.SpinAngle) * Matrix.CreateRotationX(MathHelper.PiOver2) * normalTilt * plateRot;
                    effect.World = boltRot * Matrix.CreateTranslation(boltPos);

                    // 1. Draw Flanged Hex Head Crown & Socket
                    effect.Texture = _headTex;
                    _boltMesh.HeadMesh.Draw(device, effect);

                    // 2. Draw Realistic Helical Threaded Shank
                    effect.Texture = _threadTex;
                    _boltMesh.ShankMesh.Draw(device, effect);
                }
            }

            // Draw RetainingBracket captive bolts when their outer armor plate is unmounted
            foreach (var bracket in chassis.Brackets)
            {
                var parentPlate = chassis.Plates.Find(p => p.Id == bracket.SectorId);
                if (parentPlate != null && parentPlate.State == PlateMountState.Mounted) continue;

                Matrix bracketWorld = bracket.GetWorldMatrix();
                foreach (var bolt in bracket.Bolts)
                {
                    Vector3 boltPos = bolt.GetWorldPosition(bracketWorld);
                    effect.DiffuseColor = tools.HoveredBolt == bolt
                        ? new Vector3(1.8f, 1.6f, 0.4f)
                        : (bolt.IsTorqued ? new Vector3(0.85f, 0.92f, 1.0f) : new Vector3(1.2f, 0.80f, 0.35f));

                    Matrix boltRot = Matrix.CreateScale(0.62f) * Matrix.CreateRotationY(bolt.SpinAngle) * Matrix.CreateRotationX(MathHelper.PiOver2);
                    effect.World = boltRot * Matrix.CreateTranslation(boltPos);
                    effect.Texture = _headTex;
                    _boltMesh.HeadMesh.Draw(device, effect);
                    effect.Texture = _threadTex;
                    _boltMesh.ShankMesh.Draw(device, effect);
                }
            }

            // Draw MechComponent fasteners when their outer armor plate is unmounted
            foreach (var comp in chassis.Components)
            {
                var parentPlate = chassis.Plates.Find(p => p.Id == comp.SectorId);
                if (parentPlate != null && parentPlate.State == PlateMountState.Mounted) continue;

                Matrix compWorld = comp.GetWorldMatrix();
                foreach (var bolt in comp.Fasteners)
                {
                    Vector3 boltPos = bolt.GetWorldPosition(compWorld);
                    effect.DiffuseColor = tools.HoveredBolt == bolt
                        ? new Vector3(1.8f, 1.6f, 0.4f)
                        : (bolt.IsTorqued ? new Vector3(0.75f, 0.92f, 1.0f) : new Vector3(1.2f, 0.75f, 0.30f));

                    Matrix boltRot = Matrix.CreateScale(0.55f) * Matrix.CreateRotationY(bolt.SpinAngle) * Matrix.CreateRotationX(MathHelper.PiOver2);
                    effect.World = boltRot * Matrix.CreateTranslation(boltPos);
                    effect.Texture = _headTex;
                    _boltMesh.HeadMesh.Draw(device, effect);
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
