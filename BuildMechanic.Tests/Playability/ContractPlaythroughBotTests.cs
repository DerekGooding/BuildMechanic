using System;
using System.Linq;
using BuildMechanic.Gameplay;
using BuildMechanic.Simulation;
using BuildMechanic.Simulation.Graph;
using BuildMechanic.Tools;
using Microsoft.Xna.Framework;
using Xunit;

namespace BuildMechanic.Tests.Playability
{
    public class ContractPlaythroughBotTests
    {
        private static Ray RayAt(Vector3 target) =>
            new Ray(target + new Vector3(0, 0, 1.5f), Vector3.Forward);

        private static void SetTargetFromRay(ToolManager tools, MechChassis chassis, Ray ray)
        {
            var hit = ToolRaycaster.PerformRaycast(ray, chassis, tools.ActiveTool);
            typeof(ToolManager).GetProperty(nameof(ToolManager.HoveredPlate))!.SetValue(tools, hit.HoveredPlate);
            typeof(ToolManager).GetProperty(nameof(ToolManager.HoveredBolt))!.SetValue(tools, hit.HoveredBolt);
            typeof(ToolManager).GetProperty(nameof(ToolManager.HoveredCrack))!.SetValue(tools, hit.HoveredCrack);
            typeof(ToolManager).GetProperty(nameof(ToolManager.HoveredHydraulic))!.SetValue(tools, hit.HoveredHydraulic);
            typeof(ToolManager).GetProperty(nameof(ToolManager.HoveredRelay))!.SetValue(tools, hit.HoveredRelay);
            typeof(ToolManager).GetProperty(nameof(ToolManager.HoveredHarness))!.SetValue(tools, hit.HoveredHarness);
            typeof(ToolManager).GetProperty(nameof(ToolManager.HoveredBracket))!.SetValue(tools, hit.HoveredBracket);
            typeof(ToolManager).GetProperty(nameof(ToolManager.HoveredComponent))!.SetValue(tools, hit.HoveredComponent);
            typeof(ToolManager).GetProperty(nameof(ToolManager.HitPoint))!.SetValue(tools, hit.HitPoint);
        }

        private static void RunTool(ToolManager tools, MechChassis chassis, Vector3 targetPos, bool holdLmb, int steps = 1, float dt = 0.25f)
        {
            var rng = new Random(42);
            for (int i = 0; i < steps; i++)
            {
                SetTargetFromRay(tools, chassis, RayAt(targetPos));
                float pry = 0f;
                ToolExecutionHelper.Execute(
                    tools, chassis, dt, isLmb: holdLmb, justClicked: !holdLmb, isRmb: false,
                    rng, null, null, null, null, out _, out _, ref pry);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void Bot_CompletesEntireContract_UsingOnlyRaycastsAndTools(int contractNumber)
        {
            var upgrades = new UpgradeManager { Credits = 1500 };
            var catalog = new PartsCatalogManager();
            var chassis = new MechChassis();
            var contracts = new ContractManager();
            contracts.LoadContract(contractNumber, chassis);

            var tools = new ToolManager();

            foreach (var plateId in contracts.ActiveContract.DamagedPlateIds)
            {
                var plate = chassis.Plates.First(p => p.Id == plateId);

                // 1. Unscrew 4 plate bolts (REV)
                tools.SelectTool(ToolType.ImpactDriver);
                if (tools.DriverDir != DriverDirection.Reverse) tools.ToggleDriverDirection();
                foreach (var bolt in plate.Bolts)
                    RunTool(tools, chassis, bolt.GetWorldPosition(plate.GetWorldMatrix()), holdLmb: true, steps: 6);

                // 2. Detach plate (PryBar)
                tools.SelectTool(ToolType.PryBar);
                RunTool(tools, chassis, plate.MountedPosition, holdLmb: false);
                plate.Update(5f);
                contracts.Update(chassis);
                Assert.Equal(PlateMountState.Detached, plate.State);

                // 3. Grind -> Weld -> Grind Flush cracks on bench
                foreach (var crack in plate.Cracks)
                {
                    Vector3 crackPos = Vector3.Transform(crack.LocalPoints[2], plate.GetWorldMatrix());
                    tools.SelectTool(ToolType.AngleGrinder);
                    RunTool(tools, chassis, crackPos, holdLmb: true, steps: 12);
                    tools.SelectTool(ToolType.Welder);
                    RunTool(tools, chassis, crackPos, holdLmb: true, steps: 15);
                    contracts.Update(chassis);
                    tools.SelectTool(ToolType.AngleGrinder);
                    RunTool(tools, chassis, crackPos, holdLmb: true, steps: 12);
                    contracts.Update(chassis);
                    Assert.True(crack.IsRepaired);
                }

                // 4. Disassemble L2..L5 MechComponents, clean rust, replace seized core unit, reassemble
                var comps = chassis.Components.Where(c => c.SectorId == plateId).OrderBy(c => c.LayerDepth).ToList();
                foreach (var comp in comps)
                {
                    if (comp.Fasteners.Count > 0)
                    {
                        tools.SelectTool(ToolType.ImpactDriver);
                        if (tools.DriverDir != DriverDirection.Reverse) tools.ToggleDriverDirection();
                        foreach (var cb in comp.Fasteners)
                            RunTool(tools, chassis, cb.GetWorldPosition(comp.GetWorldMatrix()), holdLmb: true, steps: 6);
                    }

                    if (comp.WearState == ComponentWearState.Rusty)
                    {
                        tools.SelectTool(ToolType.AngleGrinder);
                        RunTool(tools, chassis, comp.CurrentPosition, holdLmb: true, steps: 10);
                    }

                    tools.SelectTool(ToolType.PryBar);
                    RunTool(tools, chassis, comp.CurrentPosition, holdLmb: false);
                }

                foreach (var comp in comps.AsEnumerable().Reverse().Where(c => c.State == ComponentMountState.Removed))
                {
                    tools.SelectTool(ToolType.PryBar);
                    RunTool(tools, chassis, comp.CurrentPosition, holdLmb: false);

                    if (comp.Fasteners.Count > 0)
                    {
                        tools.SelectTool(ToolType.ImpactDriver);
                        if (tools.DriverDir != DriverDirection.Forward) tools.ToggleDriverDirection();
                        foreach (var cb in comp.Fasteners)
                            RunTool(tools, chassis, cb.GetWorldPosition(comp.GetWorldMatrix()), holdLmb: true, steps: 6);
                    }
                }

                // 5. Unbolt & remove RetainingBracket, service Hydraulics/Relays/Harnesses, re-secure Bracket
                var bracket = chassis.Brackets.First(b => b.SectorId == plateId);
                tools.SelectTool(ToolType.ImpactDriver);
                if (tools.DriverDir != DriverDirection.Reverse) tools.ToggleDriverDirection();
                foreach (var bb in bracket.Bolts)
                    RunTool(tools, chassis, bb.GetWorldPosition(bracket.GetWorldMatrix()), holdLmb: true, steps: 6);

                tools.SelectTool(ToolType.PryBar);
                RunTool(tools, chassis, bracket.MountedPosition, holdLmb: false);

                foreach (var hydro in chassis.Hydraulics.Where(h => h.SectorId == plateId))
                {
                    tools.SelectTool(ToolType.HydraulicBleeder);
                    RunTool(tools, chassis, hydro.Position, holdLmb: true, steps: 16);
                }
                foreach (var relay in chassis.Relays.Where(r => r.SectorId == plateId && r.IsBlown))
                {
                    tools.SelectTool(ToolType.Multimeter);
                    RunTool(tools, chassis, relay.Position, holdLmb: false);
                }
                foreach (var harness in chassis.Harnesses.Where(w => w.SectorId == plateId && !w.IsNominal))
                {
                    tools.SelectTool(ToolType.PryBar);
                    RunTool(tools, chassis, harness.MidPoint, holdLmb: false); // Unclip
                    RunTool(tools, chassis, harness.MidPoint, holdLmb: false); // Snap replacement
                }

                tools.SelectTool(ToolType.PryBar);
                RunTool(tools, chassis, bracket.RemovedPosition, holdLmb: false);
                tools.SelectTool(ToolType.ImpactDriver);
                if (tools.DriverDir != DriverDirection.Forward) tools.ToggleDriverDirection();
                foreach (var bb in bracket.Bolts)
                    RunTool(tools, chassis, bb.GetWorldPosition(bracket.GetWorldMatrix()), holdLmb: true, steps: 6);

                // 6. Re-mount plate & torque 4 bolts
                tools.SelectTool(ToolType.PryBar);
                RunTool(tools, chassis, plate.DetachedPosition, holdLmb: false);
                plate.Update(5f);
                tools.SelectTool(ToolType.ImpactDriver);
                if (tools.DriverDir != DriverDirection.Forward) tools.ToggleDriverDirection();
                foreach (var bolt in plate.Bolts)
                    RunTool(tools, chassis, bolt.GetWorldPosition(plate.GetWorldMatrix()), holdLmb: true, steps: 6);
            }

            contracts.Update(chassis);
            foreach (var obj in contracts.ActiveContract.Objectives)
            {
                Assert.True(obj.Evaluate(chassis), $"Contract #{contractNumber} unmet objective: {obj.Description}");
            }
            Assert.True(contracts.IsFinished, $"Bot failed to finish Contract #{contractNumber} through raycasts & tools.");
            Assert.True(contracts.RewardClaimed);
        }
    }
}
