using System.Linq;
using Microsoft.Xna.Framework;
using BuildMechanic.Simulation;

namespace BuildMechanic.Tools
{
    public class RaycastHitResult
    {
        public ArmorPlate HoveredPlate { get; set; }
        public Bolt HoveredBolt { get; set; }
        public CrackSegment HoveredCrack { get; set; }
        public HydraulicCylinder HoveredHydraulic { get; set; }
        public ElectricalRelay HoveredRelay { get; set; }
        public WiringHarness HoveredHarness { get; set; }
        public RetainingBracket HoveredBracket { get; set; }
        public Vector3 HitPoint { get; set; }
    }

    public static class ToolRaycaster
    {
        public static RaycastHitResult PerformRaycast(Ray ray, MechChassis chassis, ToolType activeTool)
        {
            var result = new RaycastHitResult();
            float closestDist = float.MaxValue;

            if (activeTool == ToolType.PaintGun)
            {
                RaycastPlates(ray, chassis, result, ref closestDist);
                return result;
            }

            if (activeTool == ToolType.PryBar)
            {
                // In PryBar mode, prioritize exposed brackets & unclipped/charred harnesses in open bays, then plates
                RaycastBrackets(ray, chassis, result, ref closestDist, includeRemoved: true);
                if (result.HoveredBracket != null) return result;

                RaycastHarnesses(ray, chassis, result, ref closestDist);
                if (result.HoveredHarness != null) return result;

                RaycastPlates(ray, chassis, result, ref closestDist);
                return result;
            }

            // 1. Plate Bolts & Bracket Bolts
            RaycastPlateBolts(ray, chassis, result, ref closestDist);
            RaycastBracketBolts(ray, chassis, result, ref closestDist);
            if (result.HoveredBolt != null) return result;

            // 2. Cracks
            foreach (var plate in chassis.Plates)
            {
                Matrix plateWorld = plate.GetWorldMatrix();
                foreach (var crack in plate.Cracks)
                {
                    if (crack.RayIntersects(ray, plateWorld, out float dist, out Vector3 hp) && dist < closestDist)
                    {
                        closestDist = dist;
                        result.HoveredCrack = crack;
                        result.HoveredPlate = plate;
                        result.HitPoint = hp;
                    }
                }
            }
            if (result.HoveredCrack != null) return result;

            // 3. Secured Retaining Brackets (shield sub-bay internals until removed)
            RaycastBrackets(ray, chassis, result, ref closestDist, includeRemoved: false);
            if (result.HoveredBracket != null) return result;

            // 4. Sub-Armor Internals (only exposed when parent plate is Detached AND bracket is Removed)
            RaycastHydraulics(ray, chassis, result, ref closestDist);
            if (result.HoveredHydraulic != null) return result;

            RaycastRelays(ray, chassis, result, ref closestDist);
            if (result.HoveredRelay != null) return result;

            RaycastHarnesses(ray, chassis, result, ref closestDist);
            if (result.HoveredHarness != null) return result;

            // 5. Removed Brackets on bench + Plates
            RaycastBrackets(ray, chassis, result, ref closestDist, includeRemoved: true);
            if (result.HoveredBracket != null) return result;

            RaycastPlates(ray, chassis, result, ref closestDist);
            return result;
        }

        private static bool IsSubBayAccessible(MechChassis chassis, string sectorId)
        {
            var plate = chassis.Plates.FirstOrDefault(p => p.Id == sectorId);
            if (plate == null || plate.State != PlateMountState.Detached) return false;
            var bracket = chassis.Brackets.FirstOrDefault(b => b.SectorId == sectorId);
            return bracket == null || bracket.State == BracketState.Removed;
        }

        private static void RaycastPlateBolts(Ray ray, MechChassis chassis, RaycastHitResult result, ref float closestDist)
        {
            foreach (var plate in chassis.Plates)
            {
                Matrix plateWorld = plate.GetWorldMatrix();
                foreach (var bolt in plate.Bolts)
                {
                    float? hit = ray.Intersects(bolt.GetBoundingSphere(plateWorld));
                    if (hit.HasValue && hit.Value < closestDist)
                    {
                        closestDist = hit.Value;
                        result.HoveredBolt = bolt;
                        result.HoveredPlate = plate;
                        result.HitPoint = ray.Position + ray.Direction * hit.Value;
                    }
                }
            }
        }

        private static void RaycastBracketBolts(Ray ray, MechChassis chassis, RaycastHitResult result, ref float closestDist)
        {
            foreach (var bracket in chassis.Brackets)
            {
                var plate = chassis.Plates.FirstOrDefault(p => p.Id == bracket.SectorId);
                if (plate == null || plate.State != PlateMountState.Detached || bracket.State != BracketState.Secured) continue;

                Matrix bracketWorld = bracket.GetWorldMatrix();
                foreach (var bolt in bracket.Bolts)
                {
                    float? hit = ray.Intersects(bolt.GetBoundingSphere(bracketWorld));
                    if (hit.HasValue && hit.Value < closestDist)
                    {
                        closestDist = hit.Value;
                        result.HoveredBolt = bolt;
                        result.HoveredBracket = bracket;
                        result.HoveredPlate = null;
                        result.HitPoint = ray.Position + ray.Direction * hit.Value;
                    }
                }
            }
        }

        private static void RaycastBrackets(Ray ray, MechChassis chassis, RaycastHitResult result, ref float closestDist, bool includeRemoved)
        {
            foreach (var bracket in chassis.Brackets)
            {
                var plate = chassis.Plates.FirstOrDefault(p => p.Id == bracket.SectorId);
                if (plate == null || plate.State != PlateMountState.Detached) continue;
                if (!includeRemoved && bracket.State != BracketState.Secured) continue;

                if (bracket.RayIntersects(ray, out float dist) && dist < closestDist)
                {
                    closestDist = dist;
                    result.HoveredBracket = bracket;
                    result.HitPoint = ray.Position + ray.Direction * dist;
                }
            }
        }

        private static void RaycastHydraulics(Ray ray, MechChassis chassis, RaycastHitResult result, ref float closestDist)
        {
            foreach (var hydro in chassis.Hydraulics)
            {
                if (!IsSubBayAccessible(chassis, hydro.SectorId)) continue;
                float? hit = ray.Intersects(new BoundingSphere(hydro.Position, hydro.Radius * 2.2f));
                if (hit.HasValue && hit.Value < closestDist)
                {
                    closestDist = hit.Value;
                    result.HoveredHydraulic = hydro;
                    result.HitPoint = ray.Position + ray.Direction * hit.Value;
                }
            }
        }

        private static void RaycastRelays(Ray ray, MechChassis chassis, RaycastHitResult result, ref float closestDist)
        {
            foreach (var relay in chassis.Relays)
            {
                if (!IsSubBayAccessible(chassis, relay.SectorId)) continue;
                float? hit = ray.Intersects(new BoundingSphere(relay.Position, 0.06f));
                if (hit.HasValue && hit.Value < closestDist)
                {
                    closestDist = hit.Value;
                    result.HoveredRelay = relay;
                    result.HitPoint = ray.Position + ray.Direction * hit.Value;
                }
            }
        }

        private static void RaycastHarnesses(Ray ray, MechChassis chassis, RaycastHitResult result, ref float closestDist)
        {
            foreach (var harness in chassis.Harnesses)
            {
                if (!IsSubBayAccessible(chassis, harness.SectorId)) continue;
                if (harness.RayIntersects(ray, out float dist) && dist < closestDist)
                {
                    closestDist = dist;
                    result.HoveredHarness = harness;
                    result.HitPoint = ray.Position + ray.Direction * dist;
                }
            }
        }

        private static void RaycastPlates(Ray ray, MechChassis chassis, RaycastHitResult result, ref float closestDist)
        {
            foreach (var plate in chassis.Plates)
            {
                if (plate.RayIntersects(ray, out float dist) && dist < closestDist)
                {
                    closestDist = dist;
                    result.HoveredPlate = plate;
                    result.HitPoint = ray.Position + ray.Direction * dist;
                }
            }
        }
    }
}
