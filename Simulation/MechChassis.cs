using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using BuildMechanic.Simulation.Graph;

namespace BuildMechanic.Simulation
{
    public class MechChassis
    {
        public List<ArmorPlate> Plates { get; set; } = new List<ArmorPlate>();
        public List<HydraulicCylinder> Hydraulics { get; set; } = new List<HydraulicCylinder>();
        public List<ElectricalRelay> Relays { get; set; } = new List<ElectricalRelay>();
        public List<WiringHarness> Harnesses { get; set; } = new List<WiringHarness>();
        public List<RetainingBracket> Brackets { get; set; } = new List<RetainingBracket>();
        public List<MechComponent> Components { get; set; } = new List<MechComponent>();

        // 6 Modular Armor Plates across 3 Sectors
        public ArmorPlate LeftTorsoPlate { get; private set; }
        public ArmorPlate RightTorsoPlate { get; private set; }
        public ArmorPlate LeftShoulderPlate { get; private set; }
        public ArmorPlate RightShoulderPlate { get; private set; }
        public ArmorPlate LeftLegPlate { get; private set; }
        public ArmorPlate RightLegPlate { get; private set; }

        public void Initialize()
        {
            Plates.Clear();

            Vector3 leftWorkbench = new Vector3(-1.80f, 2.10f, 1.20f);
            Vector3 rightWorkbench = new Vector3(1.80f, 2.10f, 1.20f);

            // 1. Torso Sector (Left & Right Breastplates)
            LeftTorsoPlate = CreatePlate(
                "plate_torso_left",
                "Titan Vanguard - Left Breastplate",
                new Vector3(-0.52f, 2.70f, 0.50f),
                leftWorkbench,
                new Vector3(0.85f, 1.05f, 0.08f),
                bx: 0.30f, by: 0.38f,
                prefix: "bolt_tl"
            );

            RightTorsoPlate = CreatePlate(
                "plate_torso_right",
                "Titan Vanguard - Right Breastplate",
                new Vector3(0.52f, 2.70f, 0.50f),
                rightWorkbench,
                new Vector3(0.85f, 1.05f, 0.08f),
                bx: 0.30f, by: 0.38f,
                prefix: "bolt_tr"
            );

            // 2. Shoulder Sector (Left & Right Pauldrons)
            LeftShoulderPlate = CreatePlate(
                "plate_shoulder_left",
                "Titan Vanguard - Left Pauldron",
                new Vector3(-1.45f, 3.35f, 0.50f),
                leftWorkbench,
                new Vector3(0.75f, 0.52f, 0.08f),
                bx: 0.26f, by: 0.17f,
                prefix: "bolt_sl"
            );

            RightShoulderPlate = CreatePlate(
                "plate_shoulder_right",
                "Titan Vanguard - Right Pauldron",
                new Vector3(1.45f, 3.35f, 0.50f),
                rightWorkbench,
                new Vector3(0.75f, 0.52f, 0.08f),
                bx: 0.26f, by: 0.17f,
                prefix: "bolt_sr"
            );

            // 3. Leg Sector (Left & Right Greaves)
            LeftLegPlate = CreatePlate(
                "plate_leg_left",
                "Titan Vanguard - Left Greave",
                new Vector3(-0.55f, 0.85f, 0.50f),
                leftWorkbench,
                new Vector3(0.50f, 0.82f, 0.08f),
                bx: 0.16f, by: 0.28f,
                prefix: "bolt_ll"
            );

            RightLegPlate = CreatePlate(
                "plate_leg_right",
                "Titan Vanguard - Right Greave",
                new Vector3(0.55f, 0.85f, 0.50f),
                rightWorkbench,
                new Vector3(0.50f, 0.82f, 0.08f),
                bx: 0.16f, by: 0.28f,
                prefix: "bolt_lr"
            );

            Plates.Add(LeftTorsoPlate);
            Plates.Add(RightTorsoPlate);
            Plates.Add(LeftShoulderPlate);
            Plates.Add(RightShoulderPlate);
            Plates.Add(LeftLegPlate);
            Plates.Add(RightLegPlate);

            // Setup workbench occupancy gating to prevent 2 plates from sharing the same bench simultaneously
            foreach (var plate in Plates)
            {
                plate.DetachGuard = p => IsWorkbenchAvailable(p);
            }

            // Setup internal sub-armor hydraulics, relays, wiring harnesses, retaining brackets, and deep component graph
            MechChassisSubBayBuilder.PopulateSubBays(Hydraulics, Relays, Harnesses, Brackets);
            TorsoDeepBayBuilder.PopulateLeftTorsoDeepStack(Components);
        }

        private ArmorPlate CreatePlate(string id, string name, Vector3 mountPos, Vector3 detachPos, Vector3 dims, float bx, float by, string prefix)
        {
            var plate = new ArmorPlate
            {
                Id = id,
                DisplayName = name,
                MountedPosition = mountPos,
                DetachedPosition = detachPos,
                Dimensions = dims
            };

            (float x, float y)[] corners = { (-bx, by), (bx, by), (-bx, -by), (bx, -by) };
            for (int i = 0; i < corners.Length; i++)
            {
                float surfZ = GetPlateSurfaceZ(id, dims, corners[i].x, corners[i].y, out Vector3 surfNormal);
                plate.Bolts.Add(new Bolt
                {
                    Id = $"{prefix}_{i + 1}",
                    LocalPosition = new Vector3(corners[i].x, corners[i].y, surfZ + 0.001f),
                    Normal = surfNormal
                });
            }

            return plate;
        }

        public static float GetPlateSurfaceZ(string plateId, Vector3 dims, float x, float y, out Vector3 normal)
        {
            float hw = dims.X * 0.5f;
            float hh = dims.Y * 0.5f;
            float hz = dims.Z * 0.5f;

            if (plateId.Contains("shoulder"))
            {
                float ty = Math.Clamp((y + hh) / (2f * hh), 0f, 1f);
                normal = Vector3.Normalize(new Vector3(0f, -0.025f / dims.Y, 1f));
                return MathHelper.Lerp(hz - 0.01f, hz + 0.015f, ty);
            }

            if (plateId.Contains("leg"))
            {
                float ty = Math.Clamp((y + hh) / (2f * hh), 0f, 1f);
                float localHw = MathHelper.Lerp(hw * 0.85f, hw, ty);
                float tx = Math.Clamp(Math.Abs(x) / localHw, 0f, 1f);
                float slopeX = 0.016f / localHw;
                normal = x >= 0f
                    ? Vector3.Normalize(new Vector3(slopeX, 0f, 1f))
                    : Vector3.Normalize(new Vector3(-slopeX, 0f, 1f));
                return hz + 0.016f * (1f - tx);
            }

            // Torso faceted prow
            float ihw = Math.Max(0.01f, hw - 0.04f);
            float ihh = Math.Max(0.01f, hh - 0.04f);
            float ptx = Math.Clamp(Math.Abs(x) / ihw, 0f, 1f);
            float pty = Math.Clamp(Math.Abs(y) / ihh, 0f, 1f);
            normal = Vector3.Backward;
            return hz + 0.012f * (1f - Math.Max(ptx, pty));
        }

        public bool IsWorkbenchAvailable(ArmorPlate requestingPlate)
        {
            bool isLeft = requestingPlate.MountedPosition.X < 0;
            // Check if any other plate targeting the same workbench is currently in transit or detached
            return !Plates.Any(p => p != requestingPlate &&
                                   (p.MountedPosition.X < 0) == isLeft &&
                                   p.State != PlateMountState.Mounted);
        }

        /// <summary>
        /// Clears cracks on all plates and adds damage to specific targeted plates.
        /// </summary>
        public void ConfigureDamage(IEnumerable<string> damagedPlateIds)
        {
            var targetSet = new HashSet<string>(damagedPlateIds);

            foreach (var plate in Plates)
            {
                plate.Cracks.Clear();

                if (targetSet.Contains(plate.Id))
                {
                    float hw = plate.Dimensions.X * 0.35f;
                    float hh = plate.Dimensions.Y * 0.35f;

                    (float x, float y)[] pts =
                    {
                        (-hw, -hh * 0.8f),
                        (-hw * 0.4f, -hh * 0.2f),
                        (hw * 0.1f, hh * 0.2f),
                        (hw * 0.5f, hh * 0.5f),
                        (hw * 0.85f, hh * 0.9f)
                    };

                    var localPoints = new List<Vector3>(pts.Length);
                    foreach (var pt in pts)
                    {
                        float surfZ = GetPlateSurfaceZ(plate.Id, plate.Dimensions, pt.x, pt.y, out _);
                        localPoints.Add(new Vector3(pt.x, pt.y, surfZ + 0.002f));
                    }

                    var crack = new CrackSegment
                    {
                        Id = $"crack_{plate.Id}_main",
                        LocalPoints = localPoints
                    };
                    plate.Cracks.Add(crack);
                }
            }

            // Configure internal faults for damaged sectors
            foreach (var hydro in Hydraulics)
            {
                if (targetSet.Contains(hydro.SectorId))
                {
                    hydro.ConfigureFault(pressure: 42.0f, contamination: 68.0f, airPocket: true);
                }
                else
                {
                    hydro.ConfigureFault(pressure: 100.0f, contamination: 0.0f, airPocket: false);
                }
            }

            foreach (var relay in Relays)
            {
                if (targetSet.Contains(relay.SectorId) && relay.SlotIndex == 1)
                {
                    relay.ConfigureFault(blown: true, resistance: 999.0f);
                }
                else
                {
                    relay.ConfigureFault(blown: false, resistance: 0.45f);
                }
            }

            foreach (var harness in Harnesses)
            {
                bool isFirstInSector = harness.Id.EndsWith("_pwr") || harness.Id.EndsWith("_sen");
                harness.ConfigureFault(charred: targetSet.Contains(harness.SectorId) && isFirstInSector);
            }

            foreach (var bracket in Brackets)
            {
                bracket.ConfigureState(BracketState.Secured, boltsTorqued: true);
            }

            foreach (var comp in Components)
            {
                bool faulty = comp.Id == "comp_tl_impeller" && targetSet.Contains(comp.SectorId);
                comp.Configure(ComponentMountState.Mounted, faulty, fastenersTorqued: true);
            }
        }

        public bool IsSectorSubBayNominal(string sectorId)
        {
            return Hydraulics.Where(h => h.SectorId == sectorId).All(h => h.IsNominal)
                && Relays.Where(r => r.SectorId == sectorId).All(r => r.IsNominal)
                && Harnesses.Where(w => w.SectorId == sectorId).All(w => w.IsNominal)
                && Brackets.Where(b => b.SectorId == sectorId).All(b => b.IsFullySecured)
                && Components.Where(c => c.SectorId == sectorId).All(c => c.IsNominal);
        }

        public void Update(float dt)
        {
            foreach (var plate in Plates)
            {
                plate.Update(dt);
            }
        }
    }
}
