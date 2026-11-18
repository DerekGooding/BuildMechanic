using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BuildMechanic.Simulation
{
    public static class MechChassisSubBayBuilder
    {
        public static void PopulateSubBays(
            List<HydraulicCylinder> hydraulics,
            List<ElectricalRelay> relays,
            List<WiringHarness> harnesses,
            List<RetainingBracket> brackets)
        {
            hydraulics.Clear();
            hydraulics.Add(new HydraulicCylinder { Id = "hydro_torso_left", SectorId = "plate_torso_left", Position = new Vector3(-0.55f, 2.70f, 0.34f) });
            hydraulics.Add(new HydraulicCylinder { Id = "hydro_torso_right", SectorId = "plate_torso_right", Position = new Vector3(0.55f, 2.70f, 0.34f) });
            hydraulics.Add(new HydraulicCylinder { Id = "hydro_leg_left", SectorId = "plate_leg_left", Position = new Vector3(-0.55f, 0.85f, 0.34f) });
            hydraulics.Add(new HydraulicCylinder { Id = "hydro_leg_right", SectorId = "plate_leg_right", Position = new Vector3(0.55f, 0.85f, 0.34f) });

            relays.Clear();
            relays.Add(new ElectricalRelay { Id = "relay_tl_1", SectorId = "plate_torso_left", SlotIndex = 1, Position = new Vector3(-0.26f, 2.86f, 0.35f) });
            relays.Add(new ElectricalRelay { Id = "relay_tl_2", SectorId = "plate_torso_left", SlotIndex = 2, Position = new Vector3(-0.26f, 2.68f, 0.35f) });
            relays.Add(new ElectricalRelay { Id = "relay_tr_1", SectorId = "plate_torso_right", SlotIndex = 1, Position = new Vector3(0.26f, 2.86f, 0.35f) });
            relays.Add(new ElectricalRelay { Id = "relay_tr_2", SectorId = "plate_torso_right", SlotIndex = 2, Position = new Vector3(0.26f, 2.68f, 0.35f) });
            relays.Add(new ElectricalRelay { Id = "relay_sl_1", SectorId = "plate_shoulder_left", SlotIndex = 1, Position = new Vector3(-1.35f, 3.35f, 0.35f) });
            relays.Add(new ElectricalRelay { Id = "relay_sr_1", SectorId = "plate_shoulder_right", SlotIndex = 1, Position = new Vector3(1.35f, 3.35f, 0.35f) });

            harnesses.Clear();
            AddSectorHarness(harnesses, "harness_tl_pwr", "plate_torso_left", "24V SERVO BUS", Color.Orange,
                new Vector3(-0.68f, 2.90f, 0.35f), new Vector3(-0.34f, 2.86f, 0.36f));
            AddSectorHarness(harnesses, "harness_tl_can", "plate_torso_left", "CAN TELEMETRY", Color.DeepSkyBlue,
                new Vector3(-0.68f, 2.52f, 0.35f), new Vector3(-0.34f, 2.66f, 0.36f));

            AddSectorHarness(harnesses, "harness_tr_pwr", "plate_torso_right", "24V SERVO BUS", Color.Orange,
                new Vector3(0.68f, 2.90f, 0.35f), new Vector3(0.34f, 2.86f, 0.36f));
            AddSectorHarness(harnesses, "harness_tr_can", "plate_torso_right", "CAN TELEMETRY", Color.DeepSkyBlue,
                new Vector3(0.68f, 2.52f, 0.35f), new Vector3(0.34f, 2.66f, 0.36f));

            AddSectorHarness(harnesses, "harness_sl_pwr", "plate_shoulder_left", "PAULDRON ACTUATOR", Color.Gold,
                new Vector3(-1.62f, 3.35f, 0.35f), new Vector3(-1.42f, 3.35f, 0.36f));
            AddSectorHarness(harnesses, "harness_sr_pwr", "plate_shoulder_right", "PAULDRON ACTUATOR", Color.Gold,
                new Vector3(1.62f, 3.35f, 0.35f), new Vector3(1.42f, 3.35f, 0.36f));

            AddSectorHarness(harnesses, "harness_ll_sen", "plate_leg_left", "GYRO LOAD SENSOR", Color.LimeGreen,
                new Vector3(-0.66f, 0.98f, 0.35f), new Vector3(-0.44f, 0.72f, 0.35f));
            AddSectorHarness(harnesses, "harness_lr_sen", "plate_leg_right", "GYRO LOAD SENSOR", Color.LimeGreen,
                new Vector3(0.66f, 0.98f, 0.35f), new Vector3(0.44f, 0.72f, 0.35f));

            brackets.Clear();
            brackets.Add(CreateBracket("bracket_tl", "plate_torso_left", new Vector3(-0.48f, 2.70f, 0.405f), new Vector3(-2.38f, 1.60f, 1.15f), new Vector3(0.48f, 0.34f, 0.025f)));
            brackets.Add(CreateBracket("bracket_tr", "plate_torso_right", new Vector3(0.48f, 2.70f, 0.405f), new Vector3(2.38f, 1.60f, 1.15f), new Vector3(0.48f, 0.34f, 0.025f)));
            brackets.Add(CreateBracket("bracket_sl", "plate_shoulder_left", new Vector3(-1.45f, 3.35f, 0.405f), new Vector3(-2.38f, 1.60f, 1.15f), new Vector3(0.42f, 0.24f, 0.025f)));
            brackets.Add(CreateBracket("bracket_sr", "plate_shoulder_right", new Vector3(1.45f, 3.35f, 0.405f), new Vector3(2.38f, 1.60f, 1.15f), new Vector3(0.42f, 0.24f, 0.025f)));
            brackets.Add(CreateBracket("bracket_ll", "plate_leg_left", new Vector3(-0.55f, 0.85f, 0.405f), new Vector3(-2.38f, 1.60f, 1.15f), new Vector3(0.32f, 0.38f, 0.025f)));
            brackets.Add(CreateBracket("bracket_lr", "plate_leg_right", new Vector3(0.55f, 0.85f, 0.405f), new Vector3(2.38f, 1.60f, 1.15f), new Vector3(0.32f, 0.38f, 0.025f)));
        }

        private static void AddSectorHarness(
            List<WiringHarness> list, string id, string sectorId, string label, Color color, Vector3 start, Vector3 end)
        {
            list.Add(new WiringHarness
            {
                Id = id,
                SectorId = sectorId,
                BusLabel = label,
                WireColor = color,
                StartPos = start,
                EndPos = end
            });
        }

        private static RetainingBracket CreateBracket(string id, string sectorId, Vector3 mountPos, Vector3 removedPos, Vector3 dims)
        {
            var bracket = new RetainingBracket
            {
                Id = id,
                SectorId = sectorId,
                MountedPosition = mountPos,
                RemovedPosition = removedPos,
                Dimensions = dims
            };

            float bx = dims.X * 0.38f;
            float bz = dims.Z * 0.5f + 0.002f;
            bracket.Bolts.Add(new Bolt
            {
                Id = $"{id}_bolt_1",
                LocalPosition = new Vector3(-bx, 0f, bz),
                Normal = Vector3.Backward
            });
            bracket.Bolts.Add(new Bolt
            {
                Id = $"{id}_bolt_2",
                LocalPosition = new Vector3(bx, 0f, bz),
                Normal = Vector3.Backward
            });

            return bracket;
        }
    }
}
