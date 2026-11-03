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
        private SceneRenderer _sceneRenderer;
        private SparkParticleSystem _sparks;
        private BasicEffect _particleEffect;
        private HUD _hud;

        private KeyboardState _prevKeyboard;

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

            _chassis = new MechChassis();
            _chassis.Initialize();

            _contractManager = new ContractManager();
            _contractManager.LoadContractOne();

            _toolManager = new ToolManager();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _sceneRenderer = new SceneRenderer(GraphicsDevice);
            _sparks = new SparkParticleSystem(_sceneRenderer.GlowTexture);

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

            if (kbd.IsKeyDown(Keys.Escape))
                Exit();

            // Camera shortcut hotkeys
            if (kbd.IsKeyDown(Keys.F) && _prevKeyboard.IsKeyUp(Keys.F))
            {
                if (_chassis.LeftTorsoPlate != null && _chassis.LeftTorsoPlate.State == PlateMountState.Detached)
                {
                    // Focus on Workbench Inspection Jig
                    _camera.FocusSector(new Vector3(-1.80f, 2.10f, 1.20f), 2.1f, -0.22f, 0.12f);
                }
                else
                {
                    // Focus on Torso Breastplate Workstation
                    _camera.FocusSector(new Vector3(-0.52f, 2.70f, 0.50f), 2.2f, 0.15f, 0.10f);
                }
            }
            if (kbd.IsKeyDown(Keys.Space) && _prevKeyboard.IsKeyUp(Keys.Space))
            {
                // Reset to full hangar gantry overview
                _camera.FocusSector(new Vector3(0, 2.6f, 0.50f), 4.5f, 0.0f, 0.15f);
            }

            // Restart contract if complete and Enter pressed
            if (_contractManager.IsFinished && kbd.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
            {
                _chassis.Initialize();
                _contractManager.LoadContractOne();
            }

            // Update camera
            _camera.Update(gameTime, GraphicsDevice.Viewport);

            // Get 3D ray under mouse cursor
            Ray mouseRay = _camera.GetMouseRay(mouse.Position, GraphicsDevice.Viewport);

            // Update tool system & interactions
            _toolManager.Update(gameTime, mouseRay, _chassis, _sparks);

            // Update physics & chassis parts
            _chassis.Update(dt);

            // Update VFX
            _sparks.Update(dt);

            // Update contracts & objectives
            _contractManager.Update(_chassis);

            _prevKeyboard = kbd;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clear to industrial hangar atmosphere
            GraphicsDevice.Clear(new Color(22, 25, 30));

            // 1. Draw 3D Hangar, Mech Chassis, Armor Plates, Hex Bolts, and Cracks
            _sceneRenderer.DrawScene(
                _camera.ViewMatrix,
                _camera.ProjectionMatrix,
                _chassis,
                _toolManager
            );

            // 2. Draw 3D Sparks & Welding VFX
            _sparks.Draw(
                GraphicsDevice,
                _particleEffect,
                _camera.ViewMatrix,
                _camera.ProjectionMatrix
            );

            // 3. Draw 2D Sci-Fi HUD Overlay
            _hud.Draw(
                _spriteBatch,
                GraphicsDevice.Viewport,
                _toolManager,
                _contractManager,
                _chassis,
                _camera
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
