using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BuildMechanic.Simulation
{
    public enum CrackStage
    {
        Fractured,     // Raw battle damage, oxidized, cracked
        PreppedGroove, // Ground out into clean V-channel ready for filler
        WeldedBead,    // Filled with weld bead (proud above surface)
        FlushFinished  // Ground flat and polished flush with plate surface
    }

    public class CrackSegment
    {
        public string Id { get; set; }
        public List<Vector3> LocalPoints { get; set; } = new List<Vector3>();

        public CrackStage Stage { get; private set; } = CrackStage.Fractured;
        public float StageProgress { get; private set; } = 0.0f; // 0.0f to 1.0f

        public bool IsRepaired => Stage == CrackStage.FlushFinished;

        public bool RayIntersects(Ray ray, Matrix parentWorld, out float distance, out Vector3 hitPoint)
        {
            distance = float.MaxValue;
            hitPoint = Vector3.Zero;
            bool anyHit = false;

            if (LocalPoints.Count < 2) return false;

            // Test tight bounding boxes per individual segment
            for (int i = 0; i < LocalPoints.Count - 1; i++)
            {
                Vector3 p0 = Vector3.Transform(LocalPoints[i], parentWorld);
                Vector3 p1 = Vector3.Transform(LocalPoints[i + 1], parentWorld);

                Vector3 min = Vector3.Min(p0, p1) - new Vector3(0.045f, 0.045f, 0.045f);
                Vector3 max = Vector3.Max(p0, p1) + new Vector3(0.045f, 0.045f, 0.045f);
                var segBox = new BoundingBox(min, max);

                float? hit = ray.Intersects(segBox);
                if (hit.HasValue && hit.Value < distance)
                {
                    distance = hit.Value;
                    hitPoint = ray.Position + ray.Direction * distance;
                    anyHit = true;
                }
            }

            return anyHit;
        }

        /// <summary>
        /// Applied when angle grinder is held on the crack.
        /// If Fractured -> advances PrepProgress towards PreppedGroove.
        /// If WeldedBead -> advances FlushProgress towards FlushFinished.
        /// </summary>
        public bool ApplyGrinding(float deltaProgress)
        {
            if (Stage == CrackStage.Fractured)
            {
                StageProgress += deltaProgress;
                if (StageProgress >= 1.0f)
                {
                    Stage = CrackStage.PreppedGroove;
                    StageProgress = 0.0f;
                    return true;
                }
            }
            else if (Stage == CrackStage.WeldedBead)
            {
                StageProgress += deltaProgress;
                if (StageProgress >= 1.0f)
                {
                    Stage = CrackStage.FlushFinished;
                    StageProgress = 1.0f;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Applied when welding torch is held on the prepped groove.
        /// Advances WeldProgress towards WeldedBead.
        /// </summary>
        public bool ApplyWelding(float deltaProgress)
        {
            if (Stage == CrackStage.PreppedGroove)
            {
                StageProgress += deltaProgress;
                if (StageProgress >= 1.0f)
                {
                    Stage = CrackStage.WeldedBead;
                    StageProgress = 0.0f;
                    return true;
                }
            }
            return false;
        }
    }
}
