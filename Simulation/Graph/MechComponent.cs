using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace BuildMechanic.Simulation.Graph
{
    public enum ComponentMountState
    {
        Mounted,
        Removed
    }

    public enum ComponentCategory
    {
        Cowling,
        StructuralRib,
        Carrier,
        ImpellerPump
    }

    public class MechComponent
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string SectorId { get; set; }
        public int LayerDepth { get; set; } = 2;
        public ComponentCategory Category { get; set; }

        public Vector3 MountedPosition { get; set; }
        public Vector3 RemovedPosition { get; set; }
        public Vector3 Dimensions { get; set; } = new Vector3(0.32f, 0.16f, 0.025f);

        public ComponentMountState State { get; private set; } = ComponentMountState.Mounted;
        public bool IsFaulty { get; private set; } = false;

        public List<Bolt> Fasteners { get; set; } = new List<Bolt>();
        public List<string> BlockedBy { get; set; } = new List<string>();
        public string RequiredBase { get; set; }

        public Vector3 CurrentPosition => State == ComponentMountState.Mounted ? MountedPosition : RemovedPosition;
        public bool AllFastenersRemoved => Fasteners.Count == 0 || Fasteners.All(b => b.IsRemoved);
        public bool AllFastenersTorqued => Fasteners.Count == 0 || Fasteners.All(b => b.IsTorqued);
        public bool IsNominal => !IsFaulty && State == ComponentMountState.Mounted && AllFastenersTorqued;

        public Matrix GetWorldMatrix() => Matrix.CreateTranslation(CurrentPosition);

        public void Configure(ComponentMountState state, bool faulty, bool fastenersTorqued)
        {
            State = state;
            IsFaulty = faulty;
            foreach (var b in Fasteners)
            {
                b.ThreadProgress = fastenersTorqued ? 1.0f : 0.0f;
            }
        }

        public bool ServiceOrReplace()
        {
            if (!IsFaulty) return false;
            IsFaulty = false;
            return true;
        }

        public void SetMountedState(ComponentMountState state) => State = state;

        public bool RayIntersects(Ray ray, out float distance)
        {
            distance = 0f;
            Vector3 half = Dimensions * 0.5f + new Vector3(0.015f, 0.015f, 0.02f);
            var box = new BoundingBox(CurrentPosition - half, CurrentPosition + half);
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
