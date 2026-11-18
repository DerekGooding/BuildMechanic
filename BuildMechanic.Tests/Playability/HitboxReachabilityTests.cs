using System.Linq;
using BuildMechanic.Simulation;
using BuildMechanic.Simulation.Graph;
using BuildMechanic.Tools;
using Microsoft.Xna.Framework;
using Xunit;

namespace BuildMechanic.Tests.Playability
{
    public class HitboxReachabilityTests
    {
        private static Ray FrontRayAt(Vector3 target) =>
            new Ray(target + new Vector3(0, 0, 1.5f), Vector3.Forward);

        [Fact]
        public void AllSixSectors_InteractiveTargetsAreRaycastReachable_AndRackClearsPlate()
        {
            var chassis = new MechChassis();
            chassis.Initialize();
            chassis.ConfigureDamage(chassis.Plates.Select(p => p.Id));

            foreach (var plate in chassis.Plates)
            {
                // 1. All 4 plate bolts reachable with ImpactDriver
                Matrix plateWorld = plate.GetWorldMatrix();
                foreach (var bolt in plate.Bolts)
                {
                    var hit = ToolRaycaster.PerformRaycast(FrontRayAt(bolt.GetWorldPosition(plateWorld)), chassis, ToolType.ImpactDriver);
                    Assert.Equal(bolt, hit.HoveredBolt);
                    bolt.ThreadProgress = 0f;
                }

                // 2. Plate reachable with PryBar & detaches
                var plateHit = ToolRaycaster.PerformRaycast(FrontRayAt(plate.MountedPosition), chassis, ToolType.PryBar);
                Assert.Equal(plate, plateHit.HoveredPlate);
                Assert.True(plate.ToggleMount());
                plate.Update(5f);

                // 3. Nested L2..L5 components & fasteners reachable in order
                var sectorComps = chassis.Components
                    .Where(c => c.SectorId == plate.Id)
                    .OrderBy(c => c.LayerDepth)
                    .ToList();

                foreach (var comp in sectorComps)
                {
                    Matrix compWorld = comp.GetWorldMatrix();
                    foreach (var cb in comp.Fasteners)
                    {
                        var cbHit = ToolRaycaster.PerformRaycast(FrontRayAt(cb.GetWorldPosition(compWorld)), chassis, ToolType.ImpactDriver);
                        Assert.Equal(cb, cbHit.HoveredBolt);
                        cb.ThreadProgress = 0f;
                    }

                    var compHit = ToolRaycaster.PerformRaycast(FrontRayAt(comp.MountedPosition), chassis, ToolType.PryBar);
                    Assert.Equal(comp, compHit.HoveredComponent);
                    if (!string.IsNullOrEmpty(comp.RequiredBase))
                    {
                        comp.SetMountedState(ComponentMountState.Removed);
                        // Verify removed rack position does not intersect detached plate bounding box
                        BoundingBox plateBox = plate.GetBoundingBox();
                        Vector3 half = comp.Dimensions * 0.5f;
                        BoundingBox compBox = new BoundingBox(comp.RemovedPosition - half, comp.RemovedPosition + half);
                        Assert.False(plateBox.Intersects(compBox), $"Removed component {comp.Id} intersects detached plate {plate.Id} on workbench.");
                    }
                }

                // 4. RetainingBracket bolts & body, then Hydraulic, Relay, Harness
                var bracket = chassis.Brackets.First(b => b.SectorId == plate.Id);
                Matrix brWorld = bracket.GetWorldMatrix();
                foreach (var bb in bracket.Bolts)
                {
                    var bbHit = ToolRaycaster.PerformRaycast(FrontRayAt(bb.GetWorldPosition(brWorld)), chassis, ToolType.ImpactDriver);
                    Assert.Equal(bb, bbHit.HoveredBolt);
                    bb.ThreadProgress = 0f;
                }
                var brHit = ToolRaycaster.PerformRaycast(FrontRayAt(bracket.MountedPosition), chassis, ToolType.PryBar);
                Assert.Equal(bracket, brHit.HoveredBracket);
                Assert.True(bracket.TryToggle());

                foreach (var hydro in chassis.Hydraulics.Where(h => h.SectorId == plate.Id))
                {
                    var hHit = ToolRaycaster.PerformRaycast(FrontRayAt(hydro.Position), chassis, ToolType.HydraulicBleeder);
                    Assert.Equal(hydro, hHit.HoveredHydraulic);
                }
                foreach (var relay in chassis.Relays.Where(r => r.SectorId == plate.Id))
                {
                    var rHit = ToolRaycaster.PerformRaycast(FrontRayAt(relay.Position), chassis, ToolType.Multimeter);
                    Assert.Equal(relay, rHit.HoveredRelay);
                }
                foreach (var harness in chassis.Harnesses.Where(w => w.SectorId == plate.Id))
                {
                    var wHit = ToolRaycaster.PerformRaycast(FrontRayAt(harness.MidPoint), chassis, ToolType.PryBar);
                    Assert.Equal(harness, wHit.HoveredHarness);
                }

                // Restore sector so next sector's workbench is free
                Assert.True(bracket.TryToggle());
                foreach (var comp in sectorComps.AsEnumerable().Reverse())
                    comp.SetMountedState(ComponentMountState.Mounted);
                Assert.True(plate.ToggleMount());
                plate.Update(5f);
                foreach (var b in plate.Bolts) b.ThreadProgress = 1f;
            }
        }
    }
}
