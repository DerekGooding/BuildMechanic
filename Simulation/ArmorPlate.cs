using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace BuildMechanic.Simulation
{
    public enum PlateMountState
    {
        Mounted,
        MovingToDetached,
        Detached,
        MovingToMounted
    }

    public class ArmorPlate
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }

        public Vector3 MountedPosition { get; set; }
        public Vector3 DetachedPosition { get; set; }
        public Vector3 Dimensions { get; set; } = new Vector3(0.9f, 1.1f, 0.08f);

        public List<Bolt> Bolts { get; set; } = new List<Bolt>();
        public List<CrackSegment> Cracks { get; set; } = new List<CrackSegment>();

        public Color BaseCoatingColor { get; set; } = Color.White;
        public string StencilId { get; set; } = null;
        public float CoatingWear { get; set; } = 0.0f;

        public PlateMountState State { get; private set; } = PlateMountState.Mounted;
        public float MountTransition { get; private set; } = 1.0f; // 1.0 = Mounted, 0.0 = Detached

        public bool AllBoltsRemoved => Bolts.All(b => b.IsRemoved);
        public bool AllBoltsTorqued => Bolts.All(b => b.IsTorqued);
        public bool HasUnrepairedCracks => Cracks.Any(c => !c.IsRepaired);
        public bool IsCrackRepaired => Cracks.All(c => c.IsRepaired);

        public float Integrity
        {
            get
            {
                if (Cracks.Count == 0) return 100f;
                float total = 0f;
                foreach (var c in Cracks)
                {
                    total += c.Stage switch
                    {
                        CrackStage.Fractured => 40f,
                        CrackStage.PreppedGroove => 55f + c.StageProgress * 15f,
                        CrackStage.WeldedBead => 75f + c.StageProgress * 15f,
                        CrackStage.FlushFinished => 100f,
                        _ => 50f
                    };
                }
                return total / Cracks.Count;
            }
        }

        public Vector3 CurrentPosition => Vector3.Lerp(DetachedPosition, MountedPosition, MathHelper.SmoothStep(0f, 1f, MountTransition));

        public Matrix GetRotationMatrix()
        {
            float tilt = (1.0f - MountTransition) * 0.25f;
            return Matrix.CreateRotationX(tilt);
        }

        public Matrix GetWorldMatrix()
        {
            // Slight tilt when detached for better inspection
            return GetRotationMatrix() * Matrix.CreateTranslation(CurrentPosition);
        }

        public BoundingBox GetBoundingBox()
        {
            Matrix world = GetWorldMatrix();
            Vector3 half = Dimensions * 0.5f;
            Vector3[] corners = new Vector3[]
            {
                new Vector3(-half.X, -half.Y, -half.Z),
                new Vector3(half.X, -half.Y, -half.Z),
                new Vector3(-half.X, half.Y, -half.Z),
                new Vector3(half.X, half.Y, -half.Z),
                new Vector3(-half.X, -half.Y, half.Z),
                new Vector3(half.X, -half.Y, half.Z),
                new Vector3(-half.X, half.Y, half.Z),
                new Vector3(half.X, half.Y, half.Z)
            };
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);
            for (int i = 0; i < 8; i++)
            {
                Vector3 worldPt = Vector3.Transform(corners[i], world);
                min = Vector3.Min(min, worldPt);
                max = Vector3.Max(max, worldPt);
            }
            return new BoundingBox(min, max);
        }

        public bool RayIntersects(Ray ray, out float distance)
        {
            distance = float.MaxValue;
            Matrix world = GetWorldMatrix();
            Matrix invWorld = Matrix.Invert(world);

            Vector3 localOrigin = Vector3.Transform(ray.Position, invWorld);
            Vector3 localDir = Vector3.Normalize(Vector3.TransformNormal(ray.Direction, invWorld));
            Ray localRay = new Ray(localOrigin, localDir);

            Vector3 half = Dimensions * 0.5f;
            BoundingBox localBox = new BoundingBox(-half, half);
            float? hit = localRay.Intersects(localBox);
            if (hit.HasValue)
            {
                Vector3 hitPointLocal = localOrigin + localDir * hit.Value;
                Vector3 hitPointWorld = Vector3.Transform(hitPointLocal, world);
                distance = Vector3.Distance(ray.Position, hitPointWorld);
                return true;
            }
            return false;
        }

        public Func<ArmorPlate, bool> DetachGuard { get; set; }

        public bool CanDetach => State == PlateMountState.Mounted && AllBoltsRemoved && (DetachGuard == null || DetachGuard(this));
        public bool CanMount => State == PlateMountState.Detached;

        public bool ToggleMount()
        {
            if (State == PlateMountState.Mounted)
            {
                if (!AllBoltsRemoved || (DetachGuard != null && !DetachGuard(this))) return false;
                State = PlateMountState.MovingToDetached;
                return true;
            }
            else if (State == PlateMountState.Detached)
            {
                State = PlateMountState.MovingToMounted;
                return true;
            }
            return false;
        }

        public void Update(float dt)
        {
            if (State == PlateMountState.MovingToDetached)
            {
                MountTransition = Math.Max(0.0f, MountTransition - dt * 2.0f);
                if (MountTransition <= 0.0f)
                {
                    State = PlateMountState.Detached;
                }
            }
            else if (State == PlateMountState.MovingToMounted)
            {
                MountTransition = Math.Min(1.0f, MountTransition + dt * 2.0f);
                if (MountTransition >= 1.0f)
                {
                    State = PlateMountState.Mounted;
                }
            }
        }
    }
}
