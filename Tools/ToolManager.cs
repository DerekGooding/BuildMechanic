using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BuildMechanic.Audio;
using BuildMechanic.Gameplay;
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
        Welder = 5
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

        // Hovered Targets
        public ArmorPlate HoveredPlate { get; private set; }
        public Bolt HoveredBolt { get; private set; }
        public CrackSegment HoveredCrack { get; private set; }
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
                if (kbd.IsKeyDown(Keys.Escape) && _prevKbd.IsKeyUp(Keys.Escape))
                {
                    IsShopOpen = false;
                }

                if (kbd.IsKeyDown(Keys.F1) && _prevKbd.IsKeyUp(Keys.F1))
                {
                    if (UpgradeManager.Instance != null && UpgradeManager.Instance.TryUpgradeDriver())
                        AudioManager.Instance?.PlayScannerPing();
                }
                if (kbd.IsKeyDown(Keys.F2) && _prevKbd.IsKeyUp(Keys.F2))
                {
                    if (UpgradeManager.Instance != null && UpgradeManager.Instance.TryUpgradeGrinder())
                        AudioManager.Instance?.PlayScannerPing();
                }
                if (kbd.IsKeyDown(Keys.F3) && _prevKbd.IsKeyUp(Keys.F3))
                {
                    if (UpgradeManager.Instance != null && UpgradeManager.Instance.TryUpgradeWelder())
                        AudioManager.Instance?.PlayScannerPing();
                }

                _prevKbd = kbd;
                _prevMouse = mouse;
                return;
            }

            // Hotkeys 1-5
            if (kbd.IsKeyDown(Keys.D1)) SelectTool(ToolType.Scanner);
            if (kbd.IsKeyDown(Keys.D2)) SelectTool(ToolType.ImpactDriver);
            if (kbd.IsKeyDown(Keys.D3)) SelectTool(ToolType.PryBar);
            if (kbd.IsKeyDown(Keys.D4)) SelectTool(ToolType.AngleGrinder);
            if (kbd.IsKeyDown(Keys.D5)) SelectTool(ToolType.Welder);

            // Tab toggles Scanner
            if (kbd.IsKeyDown(Keys.Tab) && _prevKbd.IsKeyUp(Keys.Tab))
            {
                SelectTool(ActiveTool == ToolType.Scanner ? ToolType.ImpactDriver : ToolType.Scanner);
            }

            // 'R' toggles driver direction
            if (kbd.IsKeyDown(Keys.R) && _prevKbd.IsKeyUp(Keys.R))
            {
                ToggleDriverDirection();
            }

            // Raycast detection
            PerformRaycast(ray, chassis);

            bool isLeftMouseDown = mouse.LeftButton == ButtonState.Pressed;
            bool wasLeftMouseDown = _prevMouse.LeftButton == ButtonState.Pressed;
            bool justClicked = isLeftMouseDown && !wasLeftMouseDown;

            IsOperating = false;
            ArcFlickerIntensity = 0f;

            switch (ActiveTool)
            {
                case ToolType.Scanner:
                    if (justClicked && HoveredPlate != null)
                    {
                        AudioManager.Instance?.PlayScannerPing();
                    }
                    break;

                case ToolType.ImpactDriver:
                    if (isLeftMouseDown && HoveredBolt != null && (HoveredPlate == null || HoveredPlate.State == PlateMountState.Mounted))
                    {
                        IsOperating = true;
                        AudioManager.Instance?.PlayDriverLoop(true);

                        float speedMultiplier = UpgradeManager.Instance?.GetDriverSpeed() ?? 1.0f;
                        float speed = ((DriverDir == DriverDirection.Forward) ? 1.4f : -1.4f) * speedMultiplier;
                        int result = HoveredBolt.Drive(speed * dt);

                        if (result == 1) // Just torqued
                        {
                            AudioManager.Instance?.PlayTorqueClick();
                        }
                    }
                    else
                    {
                        AudioManager.Instance?.PlayDriverLoop(false);
                    }
                    break;

                case ToolType.PryBar:
                    if (justClicked && HoveredPlate != null)
                    {
                        if (HoveredPlate.State == PlateMountState.Mounted)
                        {
                            if (HoveredPlate.CanDetach)
                            {
                                HoveredPlate.ToggleMount();
                                AudioManager.Instance?.PlayPlateDetach();
                            }
                        }
                        else if (HoveredPlate.State == PlateMountState.Detached)
                        {
                            HoveredPlate.ToggleMount();
                            AudioManager.Instance?.PlayPlateMount();
                        }
                    }
                    break;

                case ToolType.AngleGrinder:
                    if (isLeftMouseDown && HoveredCrack != null)
                    {
                        IsOperating = true;
                        AudioManager.Instance?.PlayGrinderLoop(true);

                        // Emit sparks
                        sparks?.Emit(HitPoint, Vector3.Right, 6);

                        float grindMultiplier = UpgradeManager.Instance?.GetGrinderSpeed() ?? 1.0f;
                        // Advance grinding
                        bool stageChanged = HoveredCrack.ApplyGrinding(dt * 0.45f * grindMultiplier);
                        if (stageChanged && HoveredCrack.IsRepaired)
                        {
                            AudioManager.Instance?.PlayScannerPing();
                        }
                    }
                    else
                    {
                        AudioManager.Instance?.PlayGrinderLoop(false);
                    }
                    break;

                case ToolType.Welder:
                    if (isLeftMouseDown && HoveredCrack != null && HoveredCrack.Stage == CrackStage.PreppedGroove)
                    {
                        IsOperating = true;
                        AudioManager.Instance?.PlayWelderLoop(true);

                        // Dynamic arc light flicker using reusable RNG
                        ArcFlickerIntensity = (float)_rng.NextDouble() * 0.4f + 0.6f;

                        // Little arc sparks
                        sparks?.Emit(HitPoint, Vector3.Up, 1);

                        // Smoke emission
                        smoke?.Emit(HitPoint, 1);

                        float weldMultiplier = UpgradeManager.Instance?.GetWelderSpeed() ?? 1.0f;
                        // Advance welding
                        bool stageChanged = HoveredCrack.ApplyWelding(dt * 0.35f * weldMultiplier);
                        if (stageChanged)
                        {
                            AudioManager.Instance?.PlayScannerPing();
                        }
                    }
                    else
                    {
                        AudioManager.Instance?.PlayWelderLoop(false);
                    }
                    break;
            }

            _prevKbd = kbd;
            _prevMouse = mouse;
        }

        private void PerformRaycast(Ray ray, MechChassis chassis)
        {
            HoveredBolt = null;
            HoveredCrack = null;
            HoveredPlate = null;

            float closestDist = float.MaxValue;

            // When Pry Bar is active, prioritize plate detection directly
            if (ActiveTool == ToolType.PryBar)
            {
                foreach (var plate in chassis.Plates)
                {
                    if (plate.RayIntersects(ray, out float dist))
                    {
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            HoveredPlate = plate;
                            HitPoint = ray.Position + ray.Direction * dist;
                        }
                    }
                }
                return;
            }

            // 1. Check Bolts first (highest precision when servicing fasteners)
            foreach (var plate in chassis.Plates)
            {
                Matrix plateWorld = plate.GetWorldMatrix();
                foreach (var bolt in plate.Bolts)
                {
                    var sphere = bolt.GetBoundingSphere(plateWorld);
                    float? hit = ray.Intersects(sphere);
                    if (hit.HasValue && hit.Value < closestDist)
                    {
                        closestDist = hit.Value;
                        HoveredBolt = bolt;
                        HoveredPlate = plate;
                        HitPoint = ray.Position + ray.Direction * hit.Value;
                    }
                }
            }

            if (HoveredBolt != null) return;

            // 2. Check Cracks
            foreach (var plate in chassis.Plates)
            {
                Matrix plateWorld = plate.GetWorldMatrix();
                foreach (var crack in plate.Cracks)
                {
                    if (crack.RayIntersects(ray, plateWorld, out float dist, out Vector3 hp))
                    {
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            HoveredCrack = crack;
                            HoveredPlate = plate;
                            HitPoint = hp;
                        }
                    }
                }
            }

            if (HoveredCrack != null) return;

            // 3. Check Plates
            foreach (var plate in chassis.Plates)
            {
                if (plate.RayIntersects(ray, out float dist))
                {
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        HoveredPlate = plate;
                        HitPoint = ray.Position + ray.Direction * dist;
                    }
                }
            }
        }

        public void StopOperations()
        {
            IsOperating = false;
            ArcFlickerIntensity = 0f;
            AudioManager.Instance?.StopAllLoops();
        }
    }
}
