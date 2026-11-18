using System.Collections.Generic;
using System.Linq;

namespace BuildMechanic.Simulation.Graph
{
    public static class ComponentDependencySolver
    {
        public static List<string> GetBlockingNames(MechComponent target, MechChassis chassis)
        {
            var blockers = new List<string>();
            if (target == null || chassis == null) return blockers;

            var parentPlate = chassis.Plates.FirstOrDefault(p => p.Id == target.SectorId);
            if (parentPlate != null && parentPlate.State == PlateMountState.Mounted)
            {
                blockers.Add(parentPlate.DisplayName);
            }

            foreach (var blockerId in target.BlockedBy)
            {
                var comp = chassis.Components.FirstOrDefault(c => c.Id == blockerId);
                if (comp != null && comp.State == ComponentMountState.Mounted)
                {
                    blockers.Add(comp.DisplayName);
                }
            }

            return blockers;
        }

        public static bool CanUnmount(MechComponent target, MechChassis chassis, out string reason)
        {
            reason = null;
            if (target == null || target.State != ComponentMountState.Mounted)
            {
                reason = "Already removed";
                return false;
            }

            var blockers = GetBlockingNames(target, chassis);
            if (blockers.Count > 0)
            {
                reason = $"BLOCKED BY: {blockers[0].ToUpper()}";
                return false;
            }

            if (!target.AllFastenersRemoved)
            {
                int rem = target.Fasteners.Count(b => !b.IsRemoved);
                reason = $"UNBOLT {rem} FASTENER(S) FIRST";
                return false;
            }

            return true;
        }

        public static bool CanMount(MechComponent target, MechChassis chassis, out string reason)
        {
            reason = null;
            if (target == null || target.State != ComponentMountState.Removed)
            {
                reason = "Already mounted";
                return false;
            }

            if (!string.IsNullOrEmpty(target.RequiredBase))
            {
                var baseComp = chassis.Components.FirstOrDefault(c => c.Id == target.RequiredBase);
                if (baseComp != null && baseComp.State != ComponentMountState.Mounted)
                {
                    reason = $"INSTALL {baseComp.DisplayName.ToUpper()} FIRST";
                    return false;
                }
            }

            return true;
        }
    }
}
