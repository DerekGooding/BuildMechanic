using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BuildMechanic.Graphics;

namespace BuildMechanic.Inspector
{
    public class ModelViewerGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private BasicEffect _effect;

        private InspectorCamera _camera;
        private ModelCatalog _catalog;
        private InspectorGridRenderer _gridRenderer;
        private InspectorUI _ui;

        private InspectorRenderMode _mode = InspectorRenderMode.ShadedLit;
        private bool _wireframe = false;
        private bool _drawGrid = true;
        private bool _doubleSided = false;
        private KeyboardState _prevKbd;

        private RasterizerState _wireframeState;

        public ModelViewerGame()
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
            Window.Title = "BuildMechanic — 3D Model Inspector & Diagnostics Library";
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            _camera = new InspectorCamera(GraphicsDevice);
            _wireframeState = new RasterizerState
            {
                FillMode = FillMode.WireFrame,
                CullMode = CullMode.None
            };
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _effect = new BasicEffect(GraphicsDevice)
            {
                TextureEnabled = true,
                LightingEnabled = true,
                PreferPerPixelLighting = true
            };

            _gridRenderer = new InspectorGridRenderer(GraphicsDevice);
            _catalog = ModelCatalogBuilder.BuildCatalog(GraphicsDevice);
            _ui = new InspectorUI(GraphicsDevice, TextureGenerator.CreateWhitePixel(GraphicsDevice));

            if (_catalog.ActiveModel != null)
            {
                _camera.FocusOnBounds(_catalog.ActiveModel.Bounds);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            var kbd = Keyboard.GetState();

            if (kbd.IsKeyDown(Keys.Escape))
                Exit();

            // Next / Prev Model
            if ((kbd.IsKeyDown(Keys.OemCloseBrackets) && _prevKbd.IsKeyUp(Keys.OemCloseBrackets)) ||
                (kbd.IsKeyDown(Keys.Right) && _prevKbd.IsKeyUp(Keys.Right)))
            {
                _catalog.NextModel();
                if (_catalog.ActiveModel != null) _camera.FocusOnBounds(_catalog.ActiveModel.Bounds);
            }
            if ((kbd.IsKeyDown(Keys.OemOpenBrackets) && _prevKbd.IsKeyUp(Keys.OemOpenBrackets)) ||
                (kbd.IsKeyDown(Keys.Left) && _prevKbd.IsKeyUp(Keys.Left)))
            {
                _catalog.PrevModel();
                if (_catalog.ActiveModel != null) _camera.FocusOnBounds(_catalog.ActiveModel.Bounds);
            }

            // Next / Prev Category
            if (kbd.IsKeyDown(Keys.Down) && _prevKbd.IsKeyUp(Keys.Down))
            {
                _catalog.NextCategory();
                if (_catalog.ActiveModel != null) _camera.FocusOnBounds(_catalog.ActiveModel.Bounds);
            }
            if (kbd.IsKeyDown(Keys.Up) && _prevKbd.IsKeyUp(Keys.Up))
            {
                _catalog.PrevCategory();
                if (_catalog.ActiveModel != null) _camera.FocusOnBounds(_catalog.ActiveModel.Bounds);
            }

            // Mode toggles
            if (kbd.IsKeyDown(Keys.L) && _prevKbd.IsKeyUp(Keys.L)) _mode = InspectorRenderMode.ShadedLit;
            if (kbd.IsKeyDown(Keys.U) && _prevKbd.IsKeyUp(Keys.U)) _mode = InspectorRenderMode.FlatUnlit;
            if (kbd.IsKeyDown(Keys.N) && _prevKbd.IsKeyUp(Keys.N)) _mode = InspectorRenderMode.Normals;
            if (kbd.IsKeyDown(Keys.W) && _prevKbd.IsKeyUp(Keys.W)) _wireframe = !_wireframe;
            if (kbd.IsKeyDown(Keys.G) && _prevKbd.IsKeyUp(Keys.G)) _drawGrid = !_drawGrid;
            if (kbd.IsKeyDown(Keys.C) && _prevKbd.IsKeyUp(Keys.C)) _doubleSided = !_doubleSided;
            if (kbd.IsKeyDown(Keys.Space) && _prevKbd.IsKeyUp(Keys.Space)) _camera.IsSpinning = !_camera.IsSpinning;

            // Camera angle presets
            if (kbd.IsKeyDown(Keys.D1) && _prevKbd.IsKeyUp(Keys.D1)) _camera.SetPreset(CameraPreset.Front);
            if (kbd.IsKeyDown(Keys.D2) && _prevKbd.IsKeyUp(Keys.D2)) _camera.SetPreset(CameraPreset.Side);
            if (kbd.IsKeyDown(Keys.D3) && _prevKbd.IsKeyUp(Keys.D3)) _camera.SetPreset(CameraPreset.Top);
            if (kbd.IsKeyDown(Keys.D4) && _prevKbd.IsKeyUp(Keys.D4)) _camera.SetPreset(CameraPreset.Isometric);

            // Refocus camera
            if (kbd.IsKeyDown(Keys.F) && _prevKbd.IsKeyUp(Keys.F) && _catalog.ActiveModel != null)
                _camera.FocusOnBounds(_catalog.ActiveModel.Bounds);

            _ui.HandleInput(_catalog, _camera, ref _mode, ref _wireframe, ref _drawGrid);
            _camera.Update(gameTime, GraphicsDevice.Viewport);

            _prevKbd = kbd;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(20, 24, 30));

            _effect.View = _camera.ViewMatrix;
            _effect.Projection = _camera.ProjectionMatrix;

            // Setup Studio Lighting
            if (_mode == InspectorRenderMode.ShadedLit)
            {
                _effect.LightingEnabled = true;
                _effect.AmbientLightColor = new Vector3(0.25f, 0.28f, 0.32f);
                _effect.DirectionalLight0.Enabled = true;
                _effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1f, -1.5f, -1f));
                _effect.DirectionalLight0.DiffuseColor = new Vector3(1.1f, 1.1f, 1.15f);
                _effect.DirectionalLight1.Enabled = true;
                _effect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(1f, -0.5f, 1f));
                _effect.DirectionalLight1.DiffuseColor = new Vector3(0.4f, 0.45f, 0.55f);
                _effect.DirectionalLight2.Enabled = true;
                _effect.DirectionalLight2.Direction = Vector3.Normalize(new Vector3(0, 1.5f, 0.5f));
                _effect.DirectionalLight2.DiffuseColor = new Vector3(0.3f, 0.35f, 0.4f);
            }

            var model = _catalog.ActiveModel;

            // 1. Draw 3D Grid and Model Bounding Box
            _gridRenderer.DrawGridAndBounds(
                GraphicsDevice,
                _camera.ViewMatrix,
                _camera.ProjectionMatrix,
                model?.Bounds,
                _drawGrid,
                _drawGrid);

            // 2. Draw Active Model
            if (model != null)
            {
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.RasterizerState = _wireframe ? _wireframeState :
                    (_doubleSided ? RasterizerState.CullNone : RasterizerState.CullCounterClockwise);
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

                model.Draw(GraphicsDevice, _effect, _mode);
            }

            // 3. Draw 2D UI Overlay
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _ui.Draw(
                _spriteBatch,
                GraphicsDevice.Viewport,
                _catalog,
                _camera,
                _mode,
                _wireframe,
                _drawGrid,
                _doubleSided);

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            _catalog?.Dispose();
            _gridRenderer?.Dispose();
            _effect?.Dispose();
            _wireframeState?.Dispose();
            _spriteBatch?.Dispose();
            base.UnloadContent();
        }
    }
}
