using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BuildMechanic.Simulation.Graph
{
    public static class TorsoDeepBayBuilder
    {
        public static void PopulateLeftTorsoDeepStack(List<MechComponent> components)
        {
            components.Clear();

            // L2: Thermal Louver Cowling (blocks L3 Titanium Spine Rib)
            components.Add(CreateWithBolts(
                id: "comp_tl_cowling",
                name: "Thermal Louver Cowling",
                sectorId: "plate_torso_left",
                layer: 2,
                category: ComponentCategory.Cowling,
                mountPos: new Vector3(-0.48f, 3.04f, 0.415f),
                removedPos: new Vector3(-1.80f, 2.95f, 1.15f),
                dims: new Vector3(0.38f, 0.14f, 0.022f),
                blockedBy: new List<string>(),
                requiredBase: "comp_tl_rib",
                boltCount: 2));

            // L3: Structural Titanium Rib (blocks L4 Pump Carrier)
            components.Add(CreateWithBolts(
                id: "comp_tl_rib",
                name: "Titanium Spine Rib",
                sectorId: "plate_torso_left",
                layer: 3,
                category: ComponentCategory.StructuralRib,
                mountPos: new Vector3(-0.48f, 3.04f, 0.390f),
                removedPos: new Vector3(-1.80f, 2.72f, 1.15f),
                dims: new Vector3(0.36f, 0.13f, 0.022f),
                blockedBy: new List<string> { "comp_tl_cowling" },
                requiredBase: "comp_tl_carrier",
                boltCount: 2));

            // L4: Coolant Pump Carrier (blocks L5 Coolant Impeller)
            components.Add(CreateWithBolts(
                id: "comp_tl_carrier",
                name: "Coolant Pump Carrier",
                sectorId: "plate_torso_left",
                layer: 4,
                category: ComponentCategory.Carrier,
                mountPos: new Vector3(-0.48f, 3.04f, 0.365f),
                removedPos: new Vector3(-1.80f, 2.49f, 1.15f),
                dims: new Vector3(0.30f, 0.12f, 0.022f),
                blockedBy: new List<string> { "comp_tl_rib" },
                requiredBase: "comp_tl_impeller",
                boltCount: 2));

            // L5: Buried Coolant Impeller & Viton Seal (0 bolts, snap-in core unit)
            components.Add(CreateWithBolts(
                id: "comp_tl_impeller",
                name: "Coolant Impeller & Seal",
                sectorId: "plate_torso_left",
                layer: 5,
                category: ComponentCategory.ImpellerPump,
                mountPos: new Vector3(-0.48f, 3.04f, 0.340f),
                removedPos: new Vector3(-1.80f, 2.26f, 1.15f),
                dims: new Vector3(0.18f, 0.11f, 0.028f),
                blockedBy: new List<string> { "comp_tl_carrier" },
                requiredBase: null,
                boltCount: 0));
        }

        private static MechComponent CreateWithBolts(
            string id, string name, string sectorId, int layer, ComponentCategory category,
            Vector3 mountPos, Vector3 removedPos, Vector3 dims,
            List<string> blockedBy, string requiredBase, int boltCount)
        {
            var comp = new MechComponent
            {
                Id = id,
                DisplayName = name,
                SectorId = sectorId,
                LayerDepth = layer,
                Category = category,
                MountedPosition = mountPos,
                RemovedPosition = removedPos,
                Dimensions = dims,
                BlockedBy = blockedBy,
                RequiredBase = requiredBase
            };

            if (boltCount == 2)
            {
                float bx = dims.X * 0.38f;
                float bz = dims.Z * 0.5f + 0.002f;
                comp.Fasteners.Add(new Bolt { Id = $"{id}_b1", LocalPosition = new Vector3(-bx, 0f, bz), Normal = Vector3.Backward });
                comp.Fasteners.Add(new Bolt { Id = $"{id}_b2", LocalPosition = new Vector3(bx, 0f, bz), Normal = Vector3.Backward });
            }

            return comp;
        }
    }
}
