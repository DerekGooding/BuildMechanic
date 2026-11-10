using System;
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
        public Vector3 HitPoint { get; set; }
    }

    public static class ToolRaycaster
    {
        public static RaycastHitResult PerformRaycast(Ray ray, MechChassis chassis, ToolType activeTool)
        {
            var result = new RaycastHitResult();
            float closestDist = float.MaxValue;

            // When Pry Bar or Paint Gun is active, prioritize plate detection directly
            if (activeTool == ToolType.PryBar || activeTool == ToolType.PaintGun)
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
                return result;
            }

            // 1. Check Bolts
            foreach (var plate in chassis.Plates)
            {
                Matrix plateWorld = plate.GetWorldMatrix();
                foreach (var bolt in plate.Bolts)
                {
                    var sphere = bolt.GetBoundingSphere(plateWorld);
                    float? hit = ray.Intersects(sphere);
                    if (hit.HasValue && hit.Value < closestDist)
                    {
                        closestDist = hit.Value;
                        result.HoveredBolt = bolt;
                        result.HoveredPlate = plate;
                        result.HitPoint = ray.Position + ray.Direction * hit.Value;
                    }
                }
            }
            if (result.HoveredBolt != null) return result;

            // 2. Check Cracks
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

            // 3. Check Sub-Armor Internals (only exposed when parent plate is detached!)
            foreach (var hydro in chassis.Hydraulics)
            {
                var parentPlate = chassis.Plates.FirstOrDefault(p => p.Id == hydro.SectorId);
                if (parentPlate != null && parentPlate.State == PlateMountState.Detached)
                {
                    var sphere = new BoundingSphere(hydro.Position, hydro.Radius * 2.2f);
                    float? hit = ray.Intersects(sphere);
                    if (hit.HasValue && hit.Value < closestDist)
                    {
                        closestDist = hit.Value;
                        result.HoveredHydraulic = hydro;
                        result.HitPoint = ray.Position + ray.Direction * hit.Value;
                    }
                }
            }
            if (result.HoveredHydraulic != null) return result;

            foreach (var relay in chassis.Relays)
            {
                var parentPlate = chassis.Plates.FirstOrDefault(p => p.Id == relay.SectorId);
                if (parentPlate != null && parentPlate.State == PlateMountState.Detached)
                {
                    var sphere = new BoundingSphere(relay.Position, 0.06f);
                    float? hit = ray.Intersects(sphere);
                    if (hit.HasValue && hit.Value < closestDist)
                    {
                        closestDist = hit.Value;
                        result.HoveredRelay = relay;
                        result.HitPoint = ray.Position + ray.Direction * hit.Value;
                    }
                }
            }
            if (result.HoveredRelay != null) return result;

            // 4. Check Plates
            foreach (var plate in chassis.Plates)
            {
                if (plate.RayIntersects(ray, out float dist) && dist < closestDist)
                {
                    closestDist = dist;
                    result.HoveredPlate = plate;
                    result.HitPoint = ray.Position + ray.Direction * dist;
                }
            }

            return result;
        }
    }
}
