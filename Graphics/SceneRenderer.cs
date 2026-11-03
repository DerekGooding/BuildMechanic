using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BuildMechanic.Simulation;
using BuildMechanic.Tools;

namespace BuildMechanic.Graphics
{
    public class SceneRenderer : IDisposable
    {
        private readonly GraphicsDevice _device;
        private readonly BasicEffect _effect;

        // Meshes
        private SimpleMesh _boxMesh;
        private SimpleMesh _cylinderMesh;
        private SimpleMesh _boltMesh;

        // Textures
        private Texture2D _armorTex;
        private Texture2D _chassisTex;
        private Texture2D _floorTex;
        private Texture2D _boltTex;
        private Texture2D _pixelTex;
        private Texture2D _glowTex;

        public Texture2D PixelTexture => _pixelTex;
        public Texture2D GlowTexture => _glowTex;

        public SceneRenderer(GraphicsDevice device)
        {
            _device = device;
            _effect = new BasicEffect(device)
            {
                TextureEnabled = true,
                LightingEnabled = true,
                PreferPerPixelLighting = true
            };

            // Set up lighting
            _effect.DirectionalLight0.Enabled = true;
            _effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-0.6f, -1.0f, -0.8f));
            _effect.DirectionalLight0.DiffuseColor = new Vector3(0.95f, 0.95f, 0.90f);
            _effect.DirectionalLight0.SpecularColor = new Vector3(0.4f, 0.4f, 0.4f);

            _effect.DirectionalLight1.Enabled = true;
            _effect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(0.8f, -0.4f, 0.6f));
            _effect.DirectionalLight1.DiffuseColor = new Vector3(0.35f, 0.40f, 0.50f); // Cool fill light

            _effect.AmbientLightColor = new Vector3(0.28f, 0.30f, 0.35f);

            // Create procedural textures
            _armorTex = TextureGenerator.CreateArmorPlateTexture(device, 512, 512, withHazardStripes: true);
            _chassisTex = TextureGenerator.CreateChassisTexture(device, 256);
            _floorTex = TextureGenerator.CreateHangarFloorTexture(device, 256);
            _boltTex = TextureGenerator.CreateBoltTexture(device, 64);
            _pixelTex = TextureGenerator.CreateWhitePixel(device);
            _glowTex = TextureGenerator.CreateRadialGlow(device, 64);

            // Create meshes
            _boxMesh = MeshPrimitives.CreateBox(device, Vector3.One);
            _cylinderMesh = MeshPrimitives.CreateCylinder(device, 0.5f, 1.0f, 16);
            _boltMesh = MeshPrimitives.CreateHexBolt(device, 0.045f, 0.022f, 0.024f, 0.08f);
        }

        public void DrawScene(
            Matrix view,
            Matrix proj,
            MechChassis chassis,
            ToolManager tools)
        {
            _device.DepthStencilState = DepthStencilState.Default;
            _device.RasterizerState = RasterizerState.CullCounterClockwise;
            _device.SamplerStates[0] = SamplerState.LinearWrap;

            _effect.View = view;
            _effect.Projection = proj;

            // Handle dynamic welding arc light
            if (tools.IsOperating && tools.ActiveTool == ToolType.Welder)
            {
                float flicker = tools.ArcFlickerIntensity;
                _effect.AmbientLightColor = new Vector3(0.28f + flicker * 0.4f, 0.30f + flicker * 0.45f, 0.35f + flicker * 0.7f);
            }
            else
            {
                _effect.AmbientLightColor = new Vector3(0.28f, 0.30f, 0.35f);
            }

            DrawHangar();
            DrawChassis(chassis);
            DrawPlates(chassis, tools);
            DrawBolts(chassis, tools);
            DrawCracks(chassis, tools);
        }

        private void DrawHangar()
        {
            _effect.Texture = _floorTex;
            _effect.DiffuseColor = Vector3.One;

            // Floor
            DrawBox(new Vector3(0, -0.05f, 0), new Vector3(16f, 0.1f, 16f));

            // Back bulkhead wall
            _effect.Texture = _chassisTex;
            DrawBox(new Vector3(0, 5.0f, -4.5f), new Vector3(16f, 10f, 0.5f));

            // Left & Right Gantry Support Trusses
            DrawBox(new Vector3(-3.2f, 4.0f, -0.5f), new Vector3(0.4f, 8.0f, 0.4f));
            DrawBox(new Vector3(3.2f, 4.0f, -0.5f), new Vector3(0.4f, 8.0f, 0.4f));

            // Overhead Crane Beam
            DrawBox(new Vector3(0, 7.8f, -0.5f), new Vector3(7.0f, 0.5f, 0.6f));

            // Workbench / Inspection Jig (left side where detached plate rests)
            DrawBox(new Vector3(-1.8f, 1.0f, 1.2f), new Vector3(1.2f, 2.0f, 0.8f));
        }

        private void DrawChassis(MechChassis chassis)
        {
            _effect.Texture = _chassisTex;
            _effect.DiffuseColor = new Vector3(0.85f, 0.85f, 0.90f);

            // Central Core / Pelvis
            DrawBox(new Vector3(0, 1.4f, 0), new Vector3(1.2f, 1.0f, 1.0f));

            // Torso Inner Frame (under plates)
            DrawBox(new Vector3(0, 2.7f, 0.15f), new Vector3(1.6f, 1.6f, 0.85f));

            // Cockpit Canopy / Armored Pod
            DrawBox(new Vector3(0, 3.7f, 0.3f), new Vector3(0.8f, 0.6f, 0.7f));

            // Left & Right Shoulder Mount Armatures
            DrawBox(new Vector3(-1.4f, 3.4f, 0.1f), new Vector3(1.0f, 0.5f, 0.6f));
            DrawBox(new Vector3(1.4f, 3.4f, 0.1f), new Vector3(1.0f, 0.5f, 0.6f));

            // Left & Right Arm Pylons
            DrawCylinder(new Vector3(-2.0f, 2.5f, 0.1f), 0.25f, 1.4f);
            DrawCylinder(new Vector3(2.0f, 2.5f, 0.1f), 0.25f, 1.4f);

            // Leg Actuator Struts
            DrawCylinder(new Vector3(-0.6f, 0.7f, 0), 0.22f, 1.4f);
            DrawCylinder(new Vector3(0.6f, 0.7f, 0), 0.22f, 1.4f);

            // Mounting Boss Blocks with Tapped Holes (where plates dock)
            DrawBox(new Vector3(-0.52f, 2.7f, 0.44f), new Vector3(0.7f, 0.9f, 0.06f));
            DrawBox(new Vector3(0.52f, 2.7f, 0.44f), new Vector3(0.7f, 0.9f, 0.06f));
        }

        private void DrawPlates(MechChassis chassis, ToolManager tools)
        {
            _effect.Texture = _armorTex;

            foreach (var plate in chassis.Plates)
            {
                Matrix plateWorld = plate.GetWorldMatrix();

                // Scanner AR Highlight or Hover Tint
                if (tools.ActiveTool == ToolType.Scanner)
                {
                    _effect.DiffuseColor = plate.HasUnrepairedCracks ? new Vector3(1.2f, 0.5f, 0.4f) : new Vector3(0.5f, 1.1f, 0.6f);
                }
                else if (tools.HoveredPlate == plate && tools.HoveredBolt == null && tools.HoveredCrack == null)
                {
                    _effect.DiffuseColor = new Vector3(1.2f, 1.2f, 0.8f);
                }
                else
                {
                    _effect.DiffuseColor = Vector3.One;
                }

                _effect.World = Matrix.CreateScale(plate.Dimensions) * plateWorld;
                _boxMesh.Draw(_device, _effect);
            }
        }

        private void DrawBolts(MechChassis chassis, ToolManager tools)
        {
            _effect.Texture = _boltTex;

            foreach (var plate in chassis.Plates)
            {
                Matrix plateWorld = plate.GetWorldMatrix();
                Matrix plateRot = plate.GetRotationMatrix();

                foreach (var bolt in plate.Bolts)
                {
                    // If plate is detached and bolt is completely removed, tuck it into the mounting socket
                    Vector3 boltPos = bolt.GetWorldPosition(plateWorld);

                    if (tools.HoveredBolt == bolt)
                    {
                        _effect.DiffuseColor = new Vector3(1.8f, 1.6f, 0.4f); // Highlight bright yellow
                    }
                    else if (bolt.IsTorqued)
                    {
                        _effect.DiffuseColor = new Vector3(0.9f, 0.95f, 1.0f);
                    }
                    else
                    {
                        _effect.DiffuseColor = new Vector3(1.2f, 0.8f, 0.4f); // Loose orange tint
                    }

                    // Rotate bolt around shank (Y axis of bolt mesh) and tilt with parent plate
                    Matrix boltRot = Matrix.CreateRotationY(bolt.SpinAngle) * Matrix.CreateRotationX(MathHelper.PiOver2) * plateRot;
                    _effect.World = boltRot * Matrix.CreateTranslation(boltPos);
                    _boltMesh.Draw(_device, _effect);
                }
            }
        }

        private void DrawCracks(MechChassis chassis, ToolManager tools)
        {
            _effect.Texture = _pixelTex;

            foreach (var plate in chassis.Plates)
            {
                Matrix plateWorld = plate.GetWorldMatrix();

                foreach (var crack in plate.Cracks)
                {
                    int segCount = crack.LocalPoints.Count - 1;
                    if (segCount <= 0) continue;

                    // Draw crack line segments with progressive bead/groove states
                    for (int i = 0; i < segCount; i++)
                    {
                        Vector3 p0 = Vector3.Transform(crack.LocalPoints[i], plateWorld);
                        Vector3 p1 = Vector3.Transform(crack.LocalPoints[i + 1], plateWorld);

                        Vector3 mid = (p0 + p1) * 0.5f;
                        Vector3 dir = p1 - p0;
                        float len = dir.Length();
                        if (len < 0.001f) continue;

                        dir /= len;

                        float segStart = (float)i / segCount;
                        float segEnd = (float)(i + 1) / segCount;
                        float progress = crack.StageProgress;

                        Color segColor;
                        float thickness = 0.022f;

                        switch (crack.Stage)
                        {
                            case CrackStage.Fractured:
                                if (progress >= segEnd)
                                {
                                    segColor = new Color(205, 215, 230); // Clean prepped groove
                                }
                                else if (progress > segStart && tools.IsOperating && tools.HoveredCrack == crack)
                                {
                                    segColor = new Color(255, 240, 150); // Active grind flash
                                }
                                else
                                {
                                    segColor = new Color(25, 25, 28); // Raw dark jagged fracture
                                }
                                break;

                            case CrackStage.PreppedGroove:
                                if (progress >= segEnd)
                                {
                                    segColor = new Color(255, 130, 20); // Solidified weld bead
                                    thickness = 0.036f;
                                }
                                else if (progress > segStart && tools.IsOperating && tools.HoveredCrack == crack)
                                {
                                    segColor = new Color(255, 255, 220); // Molten welding puddle
                                    thickness = 0.040f;
                                }
                                else
                                {
                                    segColor = new Color(205, 215, 230); // Clean prepped groove
                                    thickness = 0.025f;
                                }
                                break;

                            case CrackStage.WeldedBead:
                                if (progress >= segEnd)
                                {
                                    segColor = new Color(185, 195, 205); // Ground flush steel
                                    thickness = 0.022f;
                                }
                                else if (progress > segStart && tools.IsOperating && tools.HoveredCrack == crack)
                                {
                                    segColor = new Color(255, 220, 100); // Grinder smoothing bead
                                    thickness = 0.030f;
                                }
                                else
                                {
                                    segColor = new Color(245, 120, 20); // Raised proud weld bead
                                    thickness = 0.036f;
                                }
                                break;

                            case CrackStage.FlushFinished:
                            default:
                                segColor = new Color(185, 195, 205); // Polished flush steel
                                thickness = 0.022f;
                                break;
                        }

                        if (tools.HoveredCrack == crack && !tools.IsOperating)
                        {
                            segColor = Color.Yellow;
                        }

                        _effect.DiffuseColor = segColor.ToVector3();

                        Matrix orient = Matrix.CreateScale(thickness, thickness, len) *
                                        Matrix.CreateWorld(mid, dir, Vector3.Up);

                        _effect.World = orient;
                        _boxMesh.Draw(_device, _effect);
                    }
                }
            }
        }

        private void DrawBox(Vector3 position, Vector3 size)
        {
            _effect.World = Matrix.CreateScale(size) * Matrix.CreateTranslation(position);
            _boxMesh.Draw(_device, _effect);
        }

        private void DrawCylinder(Vector3 position, float radius, float height)
        {
            _effect.World = Matrix.CreateScale(radius * 2f, height, radius * 2f) * Matrix.CreateTranslation(position);
            _cylinderMesh.Draw(_device, _effect);
        }

        public void Dispose()
        {
            _effect?.Dispose();
            _boxMesh?.Dispose();
            _cylinderMesh?.Dispose();
            _boltMesh?.Dispose();
            _armorTex?.Dispose();
            _chassisTex?.Dispose();
            _floorTex?.Dispose();
            _boltTex?.Dispose();
            _pixelTex?.Dispose();
            _glowTex?.Dispose();
        }
    }
}
