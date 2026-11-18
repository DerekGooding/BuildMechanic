using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BuildMechanic.Audio;
using BuildMechanic.Graphics;
using BuildMechanic.Simulation;
using BuildMechanic.UI;
using BuildMechanic.VFX;

namespace BuildMechanic.Tools
{
    public enum ToolType
    {
        Scanner = 1,
        ImpactDriver = 2,
        PryBar = 3,
        AngleGrinder = 4,
        Welder = 5,
        Multimeter = 6,
        HydraulicBleeder = 7,
        PaintGun = 8
    }

    public enum DriverDirection
    {
        Forward,  // Tightening
        Reverse   // Loosening
    }

    public class ToolManager
    {
        public ToolType ActiveTool { get; private set; } = ToolType.Scanner;
        public ToolType PreviousWorkTool { get; private set; } = ToolType.ImpactDriver;
        public DriverDirection DriverDir { get; private set; } = DriverDirection.Reverse;
        public bool IsShopOpen { get; set; } = false;
        public bool IsMouseOverUI { get; set; } = false;
        public int ActiveColorIndex { get; private set; } = 1; // Default: Military Olive

        // Hovered Targets
        public ArmorPlate HoveredPlate { get; private set; }
        public Bolt HoveredBolt { get; private set; }
        public CrackSegment HoveredCrack { get; private set; }
        public HydraulicCylinder HoveredHydraulic { get; private set; }
        public ElectricalRelay HoveredRelay { get; private set; }
        public WiringHarness HoveredHarness { get; private set; }
        public RetainingBracket HoveredBracket { get; private set; }
        public Vector3 HitPoint { get; private set; }

        public bool IsOperating { get; private set; }
        public float ArcFlickerIntensity { get; private set; }
        public float PryImpulse { get; private set; }

        private readonly Random _rng = new Random();
        private KeyboardState _prevKbd;
        private MouseState _prevMouse;
        private int _prevScroll;

        public void SelectTool(ToolType tool)
        {
            if (ActiveTool != tool)
            {
                if (ActiveTool != ToolType.Scanner)
                {
                    PreviousWorkTool = ActiveTool;
                }
                StopOperations();
                ActiveTool = tool;
                if (tool == ToolType.Scanner)
                {
                    AudioManager.Instance?.PlayScannerPing();
                }
            }
        }

        public void CycleTool(int delta)
        {
            int count = 8;
            int current = (int)ActiveTool - 1;
            int next = ((current + delta) % count + count) % count;
            SelectTool((ToolType)(next + 1));
        }

        public void ToggleDriverDirection()
        {
            DriverDir = (DriverDir == DriverDirection.Forward) ? DriverDirection.Reverse : DriverDirection.Forward;
        }

        public void CyclePaintColor()
        {
            ActiveColorIndex = (ActiveColorIndex + 1) % ArmorDecal.PaintPalette.Length;
        }

        public bool CanRepairCrack(CrackSegment crack, ArmorPlate plate, ToolType tool)
        {
            if (crack == null || crack.IsRepaired) return false;
            if (plate != null && plate.State != PlateMountState.Detached) return false;
            if (tool == ToolType.AngleGrinder) return crack.Stage == CrackStage.Fractured || crack.Stage == CrackStage.WeldedBead;
            if (tool == ToolType.Welder) return crack.Stage == CrackStage.PreppedGroove;
            return false;
        }

        public void Update(GameTime gameTime, Ray ray, MechChassis chassis, VFXManager vfx)
            => Update(gameTime, ray, chassis, vfx, 1280, 720);

        public void Update(GameTime gameTime, Ray ray, MechChassis chassis, VFXManager vfx, int sw, int sh)
        {
            Update(gameTime, ray, chassis, vfx?.Sparks, vfx?.Smoke, vfx?.Fluid, vfx?.Aerosol, sw, sh);
        }

        public void Update(
            GameTime gameTime, Ray ray, MechChassis chassis,
            SparkParticleSystem sparks, SmokeParticleSystem smoke = null,
            FluidParticleSystem fluid = null, AerosolParticleSystem aerosol = null,
            int sw = 1280, int sh = 720)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState kbd = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            if (kbd.IsKeyDown(Keys.U) && _prevKbd.IsKeyUp(Keys.U))
            {
                IsShopOpen = !IsShopOpen;
                if (IsShopOpen) StopOperations();
            }

            if (IsShopOpen)
            {
                HUDShopView.HandleInput(kbd, _prevKbd, mouse, _prevMouse, sw, sh, out bool closeRequested);
                if (closeRequested) IsShopOpen = false;
                _prevKbd = kbd;
                _prevMouse = mouse;
                _prevScroll = mouse.ScrollWheelValue;
                return;
            }

            // Direct tool hotkeys 1-8
            if (kbd.IsKeyDown(Keys.D1)) SelectTool(ToolType.Scanner);
            if (kbd.IsKeyDown(Keys.D2)) SelectTool(ToolType.ImpactDriver);
            if (kbd.IsKeyDown(Keys.D3)) SelectTool(ToolType.PryBar);
            if (kbd.IsKeyDown(Keys.D4)) SelectTool(ToolType.AngleGrinder);
            if (kbd.IsKeyDown(Keys.D5)) SelectTool(ToolType.Welder);
            if (kbd.IsKeyDown(Keys.D6)) SelectTool(ToolType.Multimeter);
            if (kbd.IsKeyDown(Keys.D7)) SelectTool(ToolType.HydraulicBleeder);
            if (kbd.IsKeyDown(Keys.D8)) SelectTool(ToolType.PaintGun);

            // Q / E and Ctrl + Scroll tool cycling
            if (kbd.IsKeyDown(Keys.Q) && _prevKbd.IsKeyUp(Keys.Q)) CycleTool(-1);
            if (kbd.IsKeyDown(Keys.E) && _prevKbd.IsKeyUp(Keys.E)) CycleTool(1);
            int scrollDelta = mouse.ScrollWheelValue - _prevScroll;
            bool ctrlHeld = kbd.IsKeyDown(Keys.LeftControl) || kbd.IsKeyDown(Keys.RightControl);
            if (ctrlHeld && scrollDelta != 0)
            {
                CycleTool(scrollDelta > 0 ? -1 : 1);
            }
            _prevScroll = mouse.ScrollWheelValue;

            if (kbd.IsKeyDown(Keys.Tab) && _prevKbd.IsKeyUp(Keys.Tab))
                SelectTool(ActiveTool == ToolType.Scanner ? PreviousWorkTool : ToolType.Scanner);

            if (kbd.IsKeyDown(Keys.R) && _prevKbd.IsKeyUp(Keys.R))
                ToggleDriverDirection();

            if (kbd.IsKeyDown(Keys.P) && _prevKbd.IsKeyUp(Keys.P))
                CyclePaintColor();

            // Raycast detection (suppressed when pointer is over UI)
            if (IsMouseOverUI)
            {
                ClearHover();
                StopOperations();
                _prevKbd = kbd;
                _prevMouse = mouse;
                return;
            }

            var hit = ToolRaycaster.PerformRaycast(ray, chassis, ActiveTool);
            HoveredPlate = hit.HoveredPlate;
            HoveredBolt = hit.HoveredBolt;
            HoveredCrack = hit.HoveredCrack;
            HoveredHydraulic = hit.HoveredHydraulic;
            HoveredRelay = hit.HoveredRelay;
            HoveredHarness = hit.HoveredHarness;
            HoveredBracket = hit.HoveredBracket;
            HitPoint = hit.HitPoint;

            bool isLmb = mouse.LeftButton == ButtonState.Pressed;
            bool justClicked = isLmb && _prevMouse.LeftButton == ButtonState.Released;
            bool isRmb = mouse.RightButton == ButtonState.Pressed && _prevMouse.RightButton == ButtonState.Released;

            float pry = Math.Max(0f, PryImpulse - dt * 4.5f);
            ToolExecutionHelper.Execute(
                this, dt, isLmb, justClicked, isRmb, _rng,
                sparks, smoke, fluid, aerosol,
                out bool operating, out float arc, ref pry);

            IsOperating = operating;
            ArcFlickerIntensity = arc;
            PryImpulse = pry;

            _prevKbd = kbd;
            _prevMouse = mouse;
        }

        private void ClearHover()
        {
            HoveredPlate = null;
            HoveredBolt = null;
            HoveredCrack = null;
            HoveredHydraulic = null;
            HoveredRelay = null;
            HoveredHarness = null;
            HoveredBracket = null;
        }

        public void StopOperations()
        {
            IsOperating = false;
            ArcFlickerIntensity = 0f;
            PryImpulse = 0f;
            AudioManager.Instance?.StopToolLoops();
        }
    }
}
