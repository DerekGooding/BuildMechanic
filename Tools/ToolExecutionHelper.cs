using System;
using Microsoft.Xna.Framework;
using BuildMechanic.Audio;
using BuildMechanic.Gameplay;
using BuildMechanic.Graphics;
using BuildMechanic.Simulation;
using BuildMechanic.Simulation.Graph;
using BuildMechanic.VFX;

namespace BuildMechanic.Tools
{
    public static class ToolExecutionHelper
    {
        public static void Execute(
            ToolManager tools,
            MechChassis chassis,
            float dt,
            bool isLmb,
            bool justClicked,
            bool isRmb,
            Random rng,
            SparkParticleSystem sparks,
            SmokeParticleSystem smoke,
            FluidParticleSystem fluid,
            AerosolParticleSystem aerosol,
            out bool isOperating,
            out float arcFlicker,
            ref float pryImpulse)
        {
            isOperating = false;
            arcFlicker = 0f;

            switch (tools.ActiveTool)
            {
                case ToolType.Scanner:
                    if (justClicked && (tools.HoveredPlate != null || tools.HoveredHarness != null || tools.HoveredBracket != null))
                        AudioManager.Instance?.PlayScannerPing();
                    break;

                case ToolType.ImpactDriver:
                    bool canDrivePlateBolt = tools.HoveredBolt != null && tools.HoveredPlate != null && tools.HoveredPlate.State == PlateMountState.Mounted;
                    bool canDriveBracketBolt = tools.HoveredBolt != null && tools.HoveredBracket != null && tools.HoveredBracket.State == BracketState.Secured;
                    bool canDriveCompBolt = tools.HoveredBolt != null && tools.HoveredComponent != null && tools.HoveredComponent.State == ComponentMountState.Mounted;
                    if (isLmb && (canDrivePlateBolt || canDriveBracketBolt || canDriveCompBolt))
                    {
                        isOperating = true;
                        AudioManager.Instance?.PlayDriverLoop(true);
                        float speedMult = UpgradeManager.Instance?.GetDriverSpeed() ?? 1.0f;
                        float speed = ((tools.DriverDir == DriverDirection.Forward) ? 1.4f : -1.4f) * speedMult;
                        if (tools.HoveredBolt.Drive(speed * dt) != 0)
                            AudioManager.Instance?.PlayTorqueClick();
                    }
                    else
                    {
                        AudioManager.Instance?.PlayDriverLoop(false);
                    }
                    break;

                case ToolType.PryBar:
                    HandlePryBar(tools, chassis, justClicked, ref pryImpulse);
                    break;

                case ToolType.AngleGrinder:
                    if (isLmb && tools.HoveredComponent != null && tools.HoveredComponent.WearState == ComponentWearState.Rusty)
                    {
                        isOperating = true;
                        AudioManager.Instance?.PlayGrinderLoop(true);
                        sparks?.Emit(tools.HitPoint, Vector3.Right, 4);
                        float grindMult = UpgradeManager.Instance?.GetGrinderSpeed() ?? 1.0f;
                        if (tools.HoveredComponent.CleanRust(dt * grindMult))
                            AudioManager.Instance?.PlayScannerPing();
                    }
                    else if (isLmb && tools.CanRepairCrack(tools.HoveredCrack, tools.HoveredPlate, ToolType.AngleGrinder))
                    {
                        isOperating = true;
                        AudioManager.Instance?.PlayGrinderLoop(true);
                        sparks?.Emit(tools.HitPoint, Vector3.Right, 6);
                        float grindMult = UpgradeManager.Instance?.GetGrinderSpeed() ?? 1.0f;
                        if (tools.HoveredCrack.ApplyGrinding(dt * 0.45f * grindMult) && tools.HoveredCrack.IsRepaired)
                            AudioManager.Instance?.PlayScannerPing();
                    }
                    else
                    {
                        AudioManager.Instance?.PlayGrinderLoop(false);
                    }
                    break;

                case ToolType.Welder:
                    if (isLmb && tools.CanRepairCrack(tools.HoveredCrack, tools.HoveredPlate, ToolType.Welder))
                    {
                        isOperating = true;
                        AudioManager.Instance?.PlayWelderLoop(true);
                        arcFlicker = (float)rng.NextDouble() * 0.4f + 0.6f;
                        sparks?.Emit(tools.HitPoint, Vector3.Up, 1);
                        smoke?.Emit(tools.HitPoint, 1);
                        float weldMult = UpgradeManager.Instance?.GetWelderSpeed() ?? 1.0f;
                        if (tools.HoveredCrack.ApplyWelding(dt * 0.35f * weldMult))
                        {
                            AudioManager.Instance?.PlayScannerPing();
                            AudioManager.Instance?.PlayPurgeHiss();
                        }
                    }
                    else
                    {
                        AudioManager.Instance?.PlayWelderLoop(false);
                    }
                    break;

                case ToolType.Multimeter:
                    HandleMultimeter(tools, justClicked, isRmb);
                    break;

                case ToolType.HydraulicBleeder:
                    if (isLmb && tools.HoveredHydraulic != null)
                    {
                        isOperating = true;
                        tools.HoveredHydraulic.BleedAir(dt);
                        tools.HoveredHydraulic.FlushFluid(dt);
                        fluid?.Emit(tools.HitPoint, Vector3.Down, new Color(0.95f, 0.65f, 0.15f, 0.85f), 2);
                        if (rng.NextDouble() < 0.25) AudioManager.Instance?.PlayHydraulicHiss();
                    }
                    break;

                case ToolType.PaintGun:
                    if ((isLmb || justClicked) && tools.HoveredPlate != null)
                    {
                        isOperating = true;
                        Color pCol = ArmorDecal.PaintPalette[tools.ActiveColorIndex];
                        tools.HoveredPlate.BaseCoatingColor = pCol;
                        aerosol?.Emit(tools.HitPoint, Vector3.Forward, pCol, 3);
                        if (justClicked) AudioManager.Instance?.PlayPaintSpray();
                    }
                    break;
            }
        }

        private static void HandlePryBar(ToolManager tools, MechChassis chassis, bool justClicked, ref float pryImpulse)
        {
            if (!justClicked) return;

            if (tools.HoveredComponent != null)
            {
                pryImpulse = 0.9f;
                var comp = tools.HoveredComponent;
                if ((comp.IsFaulty || comp.WearState == ComponentWearState.Seized)
                    && ComponentDependencySolver.GetBlockingNames(comp, chassis).Count == 0)
                {
                    bool consumed = PartsCatalogManager.Instance?.TryConsumeForReplacement(comp.Category, comp.ReplacementCost) ?? true;
                    if (consumed && comp.ServiceOrReplace())
                    {
                        PartsCatalogManager.Instance?.ScrapPart(comp.ScrapValue);
                        AudioManager.Instance?.PlayScannerPing();
                    }
                    else
                    {
                        AudioManager.Instance?.PlayTorqueClick();
                    }
                    return;
                }

                if (comp.State == ComponentMountState.Mounted)
                {
                    if (ComponentDependencySolver.CanUnmount(comp, chassis, out _))
                    {
                        comp.SetMountedState(ComponentMountState.Removed);
                        AudioManager.Instance?.PlayPlateDetach();
                    }
                    else
                    {
                        AudioManager.Instance?.PlayTorqueClick();
                    }
                }
                else
                {
                    if (ComponentDependencySolver.CanMount(comp, chassis, out _))
                    {
                        comp.SetMountedState(ComponentMountState.Mounted);
                        AudioManager.Instance?.PlayPlateMount();
                    }
                    else
                    {
                        AudioManager.Instance?.PlayTorqueClick();
                    }
                }
                return;
            }

            if (tools.HoveredBracket != null)
            {
                pryImpulse = 1.0f;
                if (tools.HoveredBracket.TryToggle())
                {
                    if (tools.HoveredBracket.State == BracketState.Removed)
                        AudioManager.Instance?.PlayPlateDetach();
                    else
                        AudioManager.Instance?.PlayPlateMount();
                }
                else
                {
                    AudioManager.Instance?.PlayTorqueClick();
                }
                return;
            }

            if (tools.HoveredHarness != null)
            {
                pryImpulse = 0.8f;
                if (tools.HoveredHarness.State == HarnessState.Charred && tools.HoveredHarness.UnclipCharred())
                    AudioManager.Instance?.PlayTorqueClick();
                else if (tools.HoveredHarness.State == HarnessState.Unclipped && tools.HoveredHarness.SnapReplacement())
                    AudioManager.Instance?.PlayScannerPing();
                return;
            }

            if (tools.HoveredPlate != null)
            {
                pryImpulse = 1.0f;
                if (tools.HoveredPlate.State == PlateMountState.Mounted && tools.HoveredPlate.CanDetach)
                {
                    tools.HoveredPlate.ToggleMount();
                    AudioManager.Instance?.PlayPlateDetach();
                    AudioManager.Instance?.PlayCraneEcho();
                }
                else if (tools.HoveredPlate.State == PlateMountState.Mounted && !tools.HoveredPlate.CanDetach)
                {
                    AudioManager.Instance?.PlayTorqueClick();
                }
                else if (tools.HoveredPlate.State == PlateMountState.Detached)
                {
                    tools.HoveredPlate.ToggleMount();
                    AudioManager.Instance?.PlayPlateMount();
                    AudioManager.Instance?.PlayCraneEcho();
                }
            }
        }

        private static void HandleMultimeter(ToolManager tools, bool justClicked, bool isRmb)
        {
            if (!justClicked && !isRmb) return;

            if (tools.HoveredRelay != null)
            {
                if (tools.HoveredRelay.IsBlown)
                {
                    tools.HoveredRelay.ReplaceFuse();
                    AudioManager.Instance?.PlayTorqueClick();
                }
                else
                {
                    tools.HoveredRelay.TestContinuity();
                    AudioManager.Instance?.PlayMultimeterBeep();
                }
                return;
            }

            if (tools.HoveredHarness != null)
            {
                tools.HoveredHarness.ProbeContinuity();
                AudioManager.Instance?.PlayMultimeterBeep();
            }
        }
    }
}
