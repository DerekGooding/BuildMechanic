using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace BuildMechanic.Simulation
{
    public enum BracketState
    {
        Secured,
        Removed
    }

    public class RetainingBracket
    {
        public string Id { get; set; }
        public string SectorId { get; set; }
        public Vector3 MountedPosition { get; set; }
        public Vector3 RemovedPosition { get; set; }
        public Vector3 Dimensions { get; set; } = new Vector3(0.42f, 0.24f, 0.03f);

        public BracketState State { get; private set; } = BracketState.Secured;
        public List<Bolt> Bolts { get; set; } = new List<Bolt>();

        public Vector3 CurrentPosition => State == BracketState.Secured ? MountedPosition : RemovedPosition;
        public bool CanRemove => Bolts.All(b => b.IsRemoved);
        public bool AllBoltsTorqued => Bolts.All(b => b.IsTorqued);
        public bool IsFullySecured => State == BracketState.Secured && AllBoltsTorqued;

        public Matrix GetWorldMatrix() => Matrix.CreateTranslation(CurrentPosition);

        public void ConfigureState(BracketState state, bool boltsTorqued)
        {
            State = state;
            foreach (var b in Bolts)
            {
                b.ThreadProgress = boltsTorqued ? 1.0f : 0.0f;
            }
        }

        public bool TryToggle()
        {
            if (State == BracketState.Secured)
            {
                if (!CanRemove) return false;
                State = BracketState.Removed;
                return true;
            }

            State = BracketState.Secured;
            return true;
        }

        public bool RayIntersects(Ray ray, out float distance)
        {
            distance = 0f;
            Vector3 half = Dimensions * 0.5f + new Vector3(0.02f, 0.02f, 0.03f);
            Vector3 center = CurrentPosition;
            var box = new BoundingBox(center - half, center + half);
            float? hit = ray.Intersects(box);
            if (hit.HasValue)
            {
                distance = hit.Value;
                return true;
            }
            return false;
        }
    }
}
