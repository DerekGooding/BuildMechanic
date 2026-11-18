using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BuildMechanic.Simulation.Graph
{
    public static class SectorDeepBayBuilder
    {
        public static void PopulateOtherSectors(List<MechComponent> components)
        {
            // Right Torso: L2 Cowling -> L3 Gyro Cage -> L4 Carrier -> L5 Power Distribution PCB
            AddStack(components, "rt", "plate_torso_right", new Vector3(0.48f, 3.04f, 0.415f), new Vector3(2.38f, 2.95f, 1.15f),
                ("cowling", "Thermal Louver Cowling", ComponentCategory.Cowling, new Vector3(0.38f, 0.14f, 0.022f), 2),
                ("gyro", "Gyro Stabilizer Cage", ComponentCategory.GyroCage, new Vector3(0.34f, 0.13f, 0.022f), 2),
                ("carrier", "Avionics Bus Carrier", ComponentCategory.Carrier, new Vector3(0.28f, 0.12f, 0.022f), 2),
                ("pcb", "Power Distribution PCB", ComponentCategory.PowerPCB, new Vector3(0.18f, 0.11f, 0.026f), 0));

            // Left Shoulder: L2 Gimbal Collar -> L3 Rotator Gear Ring -> L4 Torque Bearing
            AddShoulderStack(components, "sl", "plate_shoulder_left", new Vector3(-1.45f, 3.48f, 0.415f), new Vector3(-2.38f, 2.95f, 1.15f));
            // Right Shoulder
            AddShoulderStack(components, "sr", "plate_shoulder_right", new Vector3(1.45f, 3.48f, 0.415f), new Vector3(2.38f, 2.95f, 1.15f));

            // Left Leg: L2 Knee Guard Shroud -> L3 Shock-Absorber Strut -> L4 Pressure Accumulator
            AddLegStack(components, "ll", "plate_leg_left", new Vector3(-0.55f, 1.05f, 0.415f), new Vector3(-2.38f, 2.95f, 1.15f));
            // Right Leg
            AddLegStack(components, "lr", "plate_leg_right", new Vector3(0.55f, 1.05f, 0.415f), new Vector3(2.38f, 2.95f, 1.15f));
        }

        private static void AddShoulderStack(List<MechComponent> components, string tag, string sectorId, Vector3 basePos, Vector3 rackPos)
        {
            Add3LayerStack(components, tag, sectorId, basePos, rackPos,
                ("collar", "Gimbal Collar Shroud", ComponentCategory.Cowling, new Vector3(0.32f, 0.12f, 0.022f), 2),
                ("gear", "Rotator Gear Ring", ComponentCategory.GearRing, new Vector3(0.26f, 0.12f, 0.022f), 2),
                ("bearing", "Torque Bearing Assembly", ComponentCategory.Bearing, new Vector3(0.16f, 0.11f, 0.026f), 0));
        }

        private static void AddLegStack(List<MechComponent> components, string tag, string sectorId, Vector3 basePos, Vector3 rackPos)
        {
            Add3LayerStack(components, tag, sectorId, basePos, rackPos,
                ("shroud", "Knee Guard Shroud", ComponentCategory.Cowling, new Vector3(0.28f, 0.14f, 0.022f), 2),
                ("strut", "Shock-Absorber Strut", ComponentCategory.ShockStrut, new Vector3(0.22f, 0.14f, 0.022f), 2),
                ("accum", "Pressure Accumulator", ComponentCategory.Accumulator, new Vector3(0.14f, 0.12f, 0.026f), 0));
        }

        private static void AddStack(
            List<MechComponent> list, string tag, string sectorId, Vector3 basePos, Vector3 rackPos,
            params (string Suffix, string Name, ComponentCategory Cat, Vector3 Dims, int Bolts)[] layers)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                var l = layers[i];
                string id = $"comp_{tag}_{l.Suffix}";
                var blockedBy = i > 0 ? new List<string> { $"comp_{tag}_{layers[i - 1].Suffix}" } : new List<string>();
                string reqBase = i < layers.Length - 1 ? $"comp_{tag}_{layers[i + 1].Suffix}" : null;

                list.Add(CreateComp(id, l.Name, sectorId, i + 2, l.Cat,
                    basePos - new Vector3(0, 0, i * 0.025f),
                    rackPos - new Vector3(0, i * 0.23f, 0),
                    l.Dims, blockedBy, reqBase, l.Bolts));
            }
        }

        private static void Add3LayerStack(
            List<MechComponent> list, string tag, string sectorId, Vector3 basePos, Vector3 rackPos,
            (string Suffix, string Name, ComponentCategory Cat, Vector3 Dims, int Bolts) l2,
            (string Suffix, string Name, ComponentCategory Cat, Vector3 Dims, int Bolts) l3,
            (string Suffix, string Name, ComponentCategory Cat, Vector3 Dims, int Bolts) l4)
            => AddStack(list, tag, sectorId, basePos, rackPos, l2, l3, l4);

        private static MechComponent CreateComp(
            string id, string name, string sectorId, int layer, ComponentCategory cat,
            Vector3 mountPos, Vector3 removedPos, Vector3 dims,
            List<string> blockedBy, string reqBase, int bolts)
        {
            var c = new MechComponent
            {
                Id = id, DisplayName = name, SectorId = sectorId, LayerDepth = layer, Category = cat,
                MountedPosition = mountPos, RemovedPosition = removedPos, Dimensions = dims,
                BlockedBy = blockedBy, RequiredBase = reqBase
            };
            if (bolts == 2)
            {
                float bx = dims.X * 0.36f;
                float bz = dims.Z * 0.5f + 0.002f;
                c.Fasteners.Add(new Bolt { Id = $"{id}_b1", LocalPosition = new Vector3(-bx, 0, bz), Normal = Vector3.Backward });
                c.Fasteners.Add(new Bolt { Id = $"{id}_b2", LocalPosition = new Vector3(bx, 0, bz), Normal = Vector3.Backward });
            }
            return c;
        }
    }
}
