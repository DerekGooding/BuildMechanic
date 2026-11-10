using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BuildMechanic.Audio;
using BuildMechanic.Camera;
using BuildMechanic.Gameplay;
using BuildMechanic.Graphics;
using BuildMechanic.Simulation;
using BuildMechanic.Tools;
using BuildMechanic.UI;
using BuildMechanic.VFX;

namespace BuildMechanic
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private AudioManager _audioManager;
        private OrbitCamera _camera;
        private MechChassis _chassis;
        private ContractManager _contractManager;
        private ToolManager _toolManager;
        private UpgradeManager _upgradeManager;
        private ProvingGroundsManager _proving;
        private SceneRenderer _sceneRenderer;
        private SparkParticleSystem _sparks;
        private SmokeParticleSystem _smoke;
        private BasicEffect _particleEffect;
        private HUD _hud;

        private KeyboardState _prevKeyboard;
        private int _sectorIndex = 0;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720,
                PreferMultiSampling = true,
                SynchronizeWithVerticalRetrace = true
            };

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.Title = "BuildMechanic — 3D Mech Mechanic Simulator";
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            _audioManager = new AudioManager();
            _audioManager.Initialize();

            _camera = new OrbitCamera(GraphicsDevice);
            _camera.FocusSector(new Vector3(0, 2.6f, 0.50f), 3.2f, 0.0f, 0.12f);

            _upgradeManager = new UpgradeManager();

            _chassis = new MechChassis();
            _contractManager = new ContractManager();
            _contractManager.LoadContract(1, _chassis);

            _toolManager = new ToolManager();
            _proving = new ProvingGroundsManager();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _sceneRenderer = new SceneRenderer(GraphicsDevice);
            _sparks = new SparkParticleSystem(_sceneRenderer.GlowTexture);
            _smoke = new SmokeParticleSystem(_sceneRenderer.GlowTexture);

            _particleEffect = new BasicEffect(GraphicsDevice)
            {
                VertexColorEnabled = true,
                TextureEnabled = true
            };

            _hud = new HUD(GraphicsDevice, _sceneRenderer.PixelTexture);
        }

        protected override void Update(GameTime gameTime)
        {
            var kbd = Keyboard.GetState();
            var mouse = Mouse.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (kbd.IsKeyDown(Keys.Escape) && !_toolManager.IsShopOpen)
                Exit();

            // Cycle camera sectors (C key)
            if (kbd.IsKeyDown(Keys.C) && _prevKeyboard.IsKeyUp(Keys.C))
            {
                _sectorIndex = (_sectorIndex + 1) % 5;
                switch (_sectorIndex)
                {
                    case 0: // Torso
                        _camera.FocusSector(new Vector3(0, 2.70f, 0.50f), 2.6f, 0.0f, 0.10f);
                        break;
                    case 1: // Shoulders
                        _camera.FocusSector(new Vector3(0, 3.35f, 0.50f), 2.8f, 0.0f, 0.10f);
                        break;
                    case 2: // Legs
                        _camera.FocusSector(new Vector3(0, 0.85f, 0.50f), 2.5f, 0.0f, 0.10f);
                        break;
                    case 3: // Left Workbench
                        _camera.FocusSector(new Vector3(-1.80f, 2.10f, 1.20f), 2.1f, -0.22f, 0.12f);
                        break;
                    case 4: // Right Workbench
                        _camera.FocusSector(new Vector3(1.80f, 2.10f, 1.20f), 2.1f, 0.22f, 0.12f);
                        break;
                }
            }

            // Smart camera focus hotkey (F key)
            if (kbd.IsKeyDown(Keys.F) && _prevKeyboard.IsKeyUp(Keys.F))
            {
                if (_toolManager.HoveredPlate != null)
                {
                    var p = _toolManager.HoveredPlate;
                    Vector3 focusPoint = p.State == PlateMountState.Mounted ? p.MountedPosition : p.DetachedPosition;
                    float yaw = focusPoint.X < -0.5f ? -0.22f : (focusPoint.X > 0.5f ? 0.22f : 0f);
                    _camera.FocusSector(focusPoint, 2.1f, yaw, 0.10f);
                }
                else
                {
                    // Check if any plate is detached and focus on it
                    ArmorPlate detached = null;
                    foreach (var pl in _chassis.Plates)
                    {
                        if (pl.State == PlateMountState.Detached) { detached = pl; break; }
                    }

                    if (detached != null)
                    {
                        float yaw = detached.DetachedPosition.X < 0 ? -0.22f : 0.22f;
                        _camera.FocusSector(detached.DetachedPosition, 2.1f, yaw, 0.12f);
                    }
                    else
                    {
                        // Focus on Torso Breastplate Workstation
                        _camera.FocusSector(new Vector3(0, 2.70f, 0.50f), 2.5f, 0.0f, 0.10f);
                    }
                }
            }

            if (kbd.IsKeyDown(Keys.Space) && _prevKeyboard.IsKeyUp(Keys.Space))
            {
                // Reset to full hangar gantry overview
                _camera.FocusSector(new Vector3(0, 2.6f, 0.50f), 4.5f, 0.0f, 0.15f);
            }

            // Trigger Proving Grounds live testing cycle (T key)
            if (kbd.IsKeyDown(Keys.T) && _prevKeyboard.IsKeyUp(Keys.T))
            {
                _proving.TriggerTest(_chassis);
            }

            // Advance to next contract if complete and Enter pressed
            if (_contractManager.IsFinished && kbd.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
            {
                _proving.Reset();
                _contractManager.AdvanceToNextContract(_chassis);
            }

            // Update camera
            _camera.Update(gameTime, GraphicsDevice.Viewport);

            // Get 3D ray under mouse cursor
            Ray mouseRay = _camera.GetMouseRay(mouse.Position, GraphicsDevice.Viewport);

            // Update tool system & interactions
            _toolManager.Update(gameTime, mouseRay, _chassis, _sparks, _smoke);

            // Update physics & chassis parts
            _chassis.Update(dt);

            // Update Proving Grounds engine
            _proving.Update(dt, _chassis);

            // Update VFX
            _sparks.Update(dt);
            _smoke.Update(dt);

            // Update contracts & objectives
            _contractManager.Update(_chassis);

            _prevKeyboard = kbd;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clear to industrial hangar atmosphere
            GraphicsDevice.Clear(new Color(22, 25, 30));

            // 1. Draw 3D Hangar, Mech Chassis, Armor Plates, Hex Bolts, Cracks, and Internals
            _sceneRenderer.DrawScene(
                _camera.ViewMatrix,
                _camera.ProjectionMatrix,
                _chassis,
                _toolManager,
                _proving.ServoSineOffset
            );

            // 2. Draw 3D Sparks & Welding VFX
            _sparks.Draw(
                GraphicsDevice,
                _particleEffect,
                _camera.ViewMatrix,
                _camera.ProjectionMatrix
            );

            // 3. Draw 3D Smoke Particles
            _smoke.Draw(
                GraphicsDevice,
                _particleEffect,
                _camera.ViewMatrix,
                _camera.ProjectionMatrix
            );

            // 4. Draw 2D Sci-Fi HUD Overlay
            _hud.Draw(
                _spriteBatch,
                GraphicsDevice.Viewport,
                _toolManager,
                _contractManager,
                _chassis,
                _camera,
                _proving
            );

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            _audioManager?.Dispose();
            _sceneRenderer?.Dispose();
            _particleEffect?.Dispose();
            base.UnloadContent();
        }
    }
}
