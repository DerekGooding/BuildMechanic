using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace BuildMechanic.Simulation
{
    public class MechChassis
    {
        public List<ArmorPlate> Plates { get; set; } = new List<ArmorPlate>();
        public List<HydraulicCylinder> Hydraulics { get; set; } = new List<HydraulicCylinder>();
        public List<ElectricalRelay> Relays { get; set; } = new List<ElectricalRelay>();

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

            // Setup internal sub-armor hydraulic cylinders
            Hydraulics.Clear();
            Hydraulics.Add(new HydraulicCylinder { Id = "hydro_torso_left", SectorId = "plate_torso_left", Position = new Vector3(-0.45f, 2.70f, 0.35f) });
            Hydraulics.Add(new HydraulicCylinder { Id = "hydro_torso_right", SectorId = "plate_torso_right", Position = new Vector3(0.45f, 2.70f, 0.35f) });
            Hydraulics.Add(new HydraulicCylinder { Id = "hydro_leg_left", SectorId = "plate_leg_left", Position = new Vector3(-0.55f, 0.85f, 0.35f) });
            Hydraulics.Add(new HydraulicCylinder { Id = "hydro_leg_right", SectorId = "plate_leg_right", Position = new Vector3(0.55f, 0.85f, 0.35f) });

            // Setup internal electrical relay fuse boxes
            Relays.Clear();
            Relays.Add(new ElectricalRelay { Id = "relay_tl_1", SectorId = "plate_torso_left", SlotIndex = 1, Position = new Vector3(-0.25f, 2.85f, 0.36f) });
            Relays.Add(new ElectricalRelay { Id = "relay_tl_2", SectorId = "plate_torso_left", SlotIndex = 2, Position = new Vector3(-0.25f, 2.70f, 0.36f) });
            Relays.Add(new ElectricalRelay { Id = "relay_tr_1", SectorId = "plate_torso_right", SlotIndex = 1, Position = new Vector3(0.25f, 2.85f, 0.36f) });
            Relays.Add(new ElectricalRelay { Id = "relay_tr_2", SectorId = "plate_torso_right", SlotIndex = 2, Position = new Vector3(0.25f, 2.70f, 0.36f) });
            Relays.Add(new ElectricalRelay { Id = "relay_sl_1", SectorId = "plate_shoulder_left", SlotIndex = 1, Position = new Vector3(-1.35f, 3.35f, 0.36f) });
            Relays.Add(new ElectricalRelay { Id = "relay_sr_1", SectorId = "plate_shoulder_right", SlotIndex = 1, Position = new Vector3(1.35f, 3.35f, 0.36f) });
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

            float bz = 0.041f;
            plate.Bolts.Add(new Bolt { Id = $"{prefix}_1", LocalPosition = new Vector3(-bx, by, bz) });
            plate.Bolts.Add(new Bolt { Id = $"{prefix}_2", LocalPosition = new Vector3(bx, by, bz) });
            plate.Bolts.Add(new Bolt { Id = $"{prefix}_3", LocalPosition = new Vector3(-bx, -by, bz) });
            plate.Bolts.Add(new Bolt { Id = $"{prefix}_4", LocalPosition = new Vector3(bx, -by, bz) });

            return plate;
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
                    float bz = 0.041f;
                    float hw = plate.Dimensions.X * 0.35f;
                    float hh = plate.Dimensions.Y * 0.35f;

                    var crack = new CrackSegment
                    {
                        Id = $"crack_{plate.Id}_main",
                        LocalPoints = new List<Vector3>
                        {
                            new Vector3(-hw, -hh * 0.8f, bz),
                            new Vector3(-hw * 0.4f, -hh * 0.2f, bz),
                            new Vector3(hw * 0.1f, hh * 0.2f, bz),
                            new Vector3(hw * 0.5f, hh * 0.5f, bz),
                            new Vector3(hw * 0.85f, hh * 0.9f, bz)
                        }
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
