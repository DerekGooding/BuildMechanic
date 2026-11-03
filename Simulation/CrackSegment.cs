using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        public BoundingBox GetBoundingBox(Matrix parentWorld)
        {
            if (LocalPoints.Count == 0) return new BoundingBox(Vector3.Zero, Vector3.Zero);

            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach (var pt in LocalPoints)
            {
                Vector3 w = Vector3.Transform(pt, parentWorld);
                min = Vector3.Min(min, w - new Vector3(0.08f));
                max = Vector3.Max(max, w + new Vector3(0.08f));
            }

            return new BoundingBox(min, max);
        }

        public bool RayIntersects(Ray ray, Matrix parentWorld, out float distance, out Vector3 hitPoint)
        {
            distance = float.MaxValue;
            hitPoint = Vector3.Zero;
            var box = GetBoundingBox(parentWorld);

            float? hit = ray.Intersects(box);
            if (hit.HasValue)
            {
                distance = hit.Value;
                hitPoint = ray.Position + ray.Direction * distance;
                return true;
            }
            return false;
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
                    return true; // Advanced stage
                }
            }
            else if (Stage == CrackStage.WeldedBead)
            {
                StageProgress += deltaProgress;
                if (StageProgress >= 1.0f)
                {
                    Stage = CrackStage.FlushFinished;
                    StageProgress = 1.0f;
                    return true; // Finished repair!
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
                    return true; // Advanced stage
                }
            }
            return false;
        }
    }
}
