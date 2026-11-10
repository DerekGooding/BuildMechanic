using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BuildMechanic.Audio;
using BuildMechanic.Gameplay;
using BuildMechanic.Graphics;
using BuildMechanic.Simulation;
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
        public DriverDirection DriverDir { get; private set; } = DriverDirection.Reverse;
        public bool IsShopOpen { get; set; } = false;
        public int ActiveColorIndex { get; private set; } = 1; // Default: Military Olive

        // Hovered Targets
        public ArmorPlate HoveredPlate { get; private set; }
        public Bolt HoveredBolt { get; private set; }
        public CrackSegment HoveredCrack { get; private set; }
        public HydraulicCylinder HoveredHydraulic { get; private set; }
        public ElectricalRelay HoveredRelay { get; private set; }
        public Vector3 HitPoint { get; private set; }

        public bool IsOperating { get; private set; }
        public float ArcFlickerIntensity { get; private set; }

        private readonly Random _rng = new Random();
        private KeyboardState _prevKbd;
        private MouseState _prevMouse;

        public void SelectTool(ToolType tool)
        {
            if (ActiveTool != tool)
            {
                StopOperations();
                ActiveTool = tool;
                if (tool == ToolType.Scanner)
                {
                    AudioManager.Instance?.PlayScannerPing();
                }
            }
        }

        public void ToggleDriverDirection()
        {
            DriverDir = (DriverDir == DriverDirection.Forward) ? DriverDirection.Reverse : DriverDirection.Forward;
        }

        public void CyclePaintColor()
        {
            ActiveColorIndex = (ActiveColorIndex + 1) % ArmorDecal.PaintPalette.Length;
        }

        public void Update(GameTime gameTime, Ray ray, MechChassis chassis, SparkParticleSystem sparks, SmokeParticleSystem smoke = null)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState kbd = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            // Shop hotkey toggle
            if (kbd.IsKeyDown(Keys.U) && _prevKbd.IsKeyUp(Keys.U))
            {
                IsShopOpen = !IsShopOpen;
                if (IsShopOpen) StopOperations();
            }

            if (IsShopOpen)
            {
                HandleShopInput(kbd);
                _prevKbd = kbd;
                _prevMouse = mouse;
                return;
            }

            // Hotkeys 1-8
            if (kbd.IsKeyDown(Keys.D1)) SelectTool(ToolType.Scanner);
            if (kbd.IsKeyDown(Keys.D2)) SelectTool(ToolType.ImpactDriver);
            if (kbd.IsKeyDown(Keys.D3)) SelectTool(ToolType.PryBar);
            if (kbd.IsKeyDown(Keys.D4)) SelectTool(ToolType.AngleGrinder);
            if (kbd.IsKeyDown(Keys.D5)) SelectTool(ToolType.Welder);
            if (kbd.IsKeyDown(Keys.D6)) SelectTool(ToolType.Multimeter);
            if (kbd.IsKeyDown(Keys.D7)) SelectTool(ToolType.HydraulicBleeder);
            if (kbd.IsKeyDown(Keys.D8)) SelectTool(ToolType.PaintGun);

            if (kbd.IsKeyDown(Keys.Tab) && _prevKbd.IsKeyUp(Keys.Tab))
                SelectTool(ActiveTool == ToolType.Scanner ? ToolType.ImpactDriver : ToolType.Scanner);

            if (kbd.IsKeyDown(Keys.R) && _prevKbd.IsKeyUp(Keys.R))
                ToggleDriverDirection();

            if (kbd.IsKeyDown(Keys.P) && _prevKbd.IsKeyUp(Keys.P))
                CyclePaintColor();

            // Raycast detection
            var hit = ToolRaycaster.PerformRaycast(ray, chassis, ActiveTool);
            HoveredPlate = hit.HoveredPlate;
            HoveredBolt = hit.HoveredBolt;
            HoveredCrack = hit.HoveredCrack;
            HoveredHydraulic = hit.HoveredHydraulic;
            HoveredRelay = hit.HoveredRelay;
            HitPoint = hit.HitPoint;

            bool isLeftMouseDown = mouse.LeftButton == ButtonState.Pressed;
            bool wasLeftMouseDown = _prevMouse.LeftButton == ButtonState.Pressed;
            bool justClicked = isLeftMouseDown && !wasLeftMouseDown;
            bool isRightClicked = mouse.RightButton == ButtonState.Pressed && _prevMouse.RightButton == ButtonState.Released;

            IsOperating = false;
            ArcFlickerIntensity = 0f;

            ExecuteActiveTool(dt, isLeftMouseDown, justClicked, isRightClicked, sparks, smoke);

            _prevKbd = kbd;
            _prevMouse = mouse;
        }

        private void ExecuteActiveTool(float dt, bool isLmb, bool justClicked, bool isRmb, SparkParticleSystem sparks, SmokeParticleSystem smoke)
        {
            switch (ActiveTool)
            {
                case ToolType.Scanner:
                    if (justClicked && HoveredPlate != null) AudioManager.Instance?.PlayScannerPing();
                    break;

                case ToolType.ImpactDriver:
                    if (isLmb && HoveredBolt != null && (HoveredPlate == null || HoveredPlate.State == PlateMountState.Mounted))
                    {
                        IsOperating = true;
                        AudioManager.Instance?.PlayDriverLoop(true);
                        float speedMult = UpgradeManager.Instance?.GetDriverSpeed() ?? 1.0f;
                        float speed = ((DriverDir == DriverDirection.Forward) ? 1.4f : -1.4f) * speedMult;
                        if (HoveredBolt.Drive(speed * dt) == 1) AudioManager.Instance?.PlayTorqueClick();
                    }
                    else AudioManager.Instance?.PlayDriverLoop(false);
                    break;

                case ToolType.PryBar:
                    if (justClicked && HoveredPlate != null)
                    {
                        if (HoveredPlate.State == PlateMountState.Mounted && HoveredPlate.CanDetach)
                        {
                            HoveredPlate.ToggleMount();
                            AudioManager.Instance?.PlayPlateDetach();
                        }
                        else if (HoveredPlate.State == PlateMountState.Detached)
                        {
                            HoveredPlate.ToggleMount();
                            AudioManager.Instance?.PlayPlateMount();
                        }
                    }
                    break;

                case ToolType.AngleGrinder:
                    if (isLmb && HoveredCrack != null)
                    {
                        IsOperating = true;
                        AudioManager.Instance?.PlayGrinderLoop(true);
                        sparks?.Emit(HitPoint, Vector3.Right, 6);
                        float grindMult = UpgradeManager.Instance?.GetGrinderSpeed() ?? 1.0f;
                        if (HoveredCrack.ApplyGrinding(dt * 0.45f * grindMult) && HoveredCrack.IsRepaired)
                            AudioManager.Instance?.PlayScannerPing();
                    }
                    else AudioManager.Instance?.PlayGrinderLoop(false);
                    break;

                case ToolType.Welder:
                    if (isLmb && HoveredCrack != null && HoveredCrack.Stage == CrackStage.PreppedGroove)
                    {
                        IsOperating = true;
                        AudioManager.Instance?.PlayWelderLoop(true);
                        ArcFlickerIntensity = (float)_rng.NextDouble() * 0.4f + 0.6f;
                        sparks?.Emit(HitPoint, Vector3.Up, 1);
                        smoke?.Emit(HitPoint, 1);
                        float weldMult = UpgradeManager.Instance?.GetWelderSpeed() ?? 1.0f;
                        if (HoveredCrack.ApplyWelding(dt * 0.35f * weldMult)) AudioManager.Instance?.PlayScannerPing();
                    }
                    else AudioManager.Instance?.PlayWelderLoop(false);
                    break;

                case ToolType.Multimeter:
                    if ((justClicked || isRmb) && HoveredRelay != null)
                    {
                        if (HoveredRelay.IsBlown)
                        {
                            HoveredRelay.ReplaceFuse();
                            AudioManager.Instance?.PlayTorqueClick();
                        }
                        else
                        {
                            HoveredRelay.TestContinuity();
                            AudioManager.Instance?.PlayMultimeterBeep();
                        }
                    }
                    break;

                case ToolType.HydraulicBleeder:
                    if (isLmb && HoveredHydraulic != null)
                    {
                        IsOperating = true;
                        HoveredHydraulic.BleedAir(dt);
                        HoveredHydraulic.FlushFluid(dt);
                        if (_rng.NextDouble() < 0.25) AudioManager.Instance?.PlayHydraulicHiss();
                    }
                    break;

                case ToolType.PaintGun:
                    if ((isLmb || justClicked) && HoveredPlate != null)
                    {
                        IsOperating = true;
                        HoveredPlate.BaseCoatingColor = ArmorDecal.PaintPalette[ActiveColorIndex];
                        if (justClicked) AudioManager.Instance?.PlayPaintSpray();
                    }
                    break;
            }
        }

        private void HandleShopInput(KeyboardState kbd)
        {
            if (kbd.IsKeyDown(Keys.Escape) && _prevKbd.IsKeyUp(Keys.Escape)) IsShopOpen = false;
            if (kbd.IsKeyDown(Keys.F1) && _prevKbd.IsKeyUp(Keys.F1) && UpgradeManager.Instance != null && UpgradeManager.Instance.TryUpgradeDriver())
                AudioManager.Instance?.PlayScannerPing();
            if (kbd.IsKeyDown(Keys.F2) && _prevKbd.IsKeyUp(Keys.F2) && UpgradeManager.Instance != null && UpgradeManager.Instance.TryUpgradeGrinder())
                AudioManager.Instance?.PlayScannerPing();
            if (kbd.IsKeyDown(Keys.F3) && _prevKbd.IsKeyUp(Keys.F3) && UpgradeManager.Instance != null && UpgradeManager.Instance.TryUpgradeWelder())
                AudioManager.Instance?.PlayScannerPing();
        }

        public void StopOperations()
        {
            IsOperating = false;
            ArcFlickerIntensity = 0f;
            AudioManager.Instance?.StopAllLoops();
        }
    }
}
