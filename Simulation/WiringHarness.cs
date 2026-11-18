using System;
using Microsoft.Xna.Framework;

namespace BuildMechanic.Simulation
{
    public enum HarnessState
    {
        Nominal,
        Charred,
        Unclipped
    }

    public class WiringHarness
    {
        public string Id { get; set; }
        public string SectorId { get; set; }
        public string BusLabel { get; set; } = "24V SERVO BUS";
        public Color WireColor { get; set; } = Color.Orange;
        public Vector3 StartPos { get; set; }
        public Vector3 EndPos { get; set; }

        public HarnessState State { get; private set; } = HarnessState.Nominal;
        public bool IsProbed { get; private set; } = false;

        public bool IsNominal => State == HarnessState.Nominal;

        public Vector3 MidPoint => (StartPos + EndPos) * 0.5f;

        public void ConfigureFault(bool charred)
        {
            State = charred ? HarnessState.Charred : HarnessState.Nominal;
            IsProbed = false;
        }

        public bool ProbeContinuity()
        {
            IsProbed = true;
            return State == HarnessState.Nominal;
        }

        public bool UnclipCharred()
        {
            if (State != HarnessState.Charred) return false;
            State = HarnessState.Unclipped;
            return true;
        }

        public bool SnapReplacement()
        {
            if (State != HarnessState.Unclipped) return false;
            State = HarnessState.Nominal;
            IsProbed = true;
            return true;
        }

        public bool RayIntersects(Ray ray, out float distance)
        {
            distance = 0f;
            Vector3 center = MidPoint + new Vector3(0, 0, 0.01f);
            float radius = Math.Max(0.06f, Vector3.Distance(StartPos, EndPos) * 0.35f);
            float? hit = ray.Intersects(new BoundingSphere(center, radius));
            if (hit.HasValue)
            {
                distance = hit.Value;
                return true;
            }
            return false;
        }
    }
}
