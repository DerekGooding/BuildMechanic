using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BuildMechanic.Simulation
{
    public class Bolt
    {
        public string Id { get; set; }
        public Vector3 LocalPosition { get; set; }
        public Vector3 Normal { get; set; } = Vector3.Backward; // Normal pointing out of plate

        /// <summary>
        /// 1.0 = fully tightened/torqued. 0.0 = fully unthreaded.
        /// </summary>
        public float ThreadProgress { get; set; } = 1.0f;
        public float SpinAngle { get; private set; } = 0.0f;

        public bool IsTorqued => ThreadProgress >= 0.999f;
        public bool IsRemoved => ThreadProgress <= 0.001f;

        public const float MaxExtension = 0.12f;

        public Vector3 GetWorldPosition(Matrix parentWorld)
        {
            Vector3 worldLocal = LocalPosition + Normal * ((1.0f - ThreadProgress) * MaxExtension);
            return Vector3.Transform(worldLocal, parentWorld);
        }

        public BoundingSphere GetBoundingSphere(Matrix parentWorld)
        {
            Vector3 pos = GetWorldPosition(parentWorld);
            return new BoundingSphere(pos, 0.09f);
        }

        /// <summary>
        /// Drives the bolt. Positive delta = tighten, Negative delta = loosen.
        /// Returns 1 if just reached full torque, -1 if just popped loose, 0 otherwise.
        /// </summary>
        public int Drive(float delta)
        {
            float prev = ThreadProgress;
            ThreadProgress = Math.Clamp(ThreadProgress + delta, 0.0f, 1.0f);

            // Spin bolt proportionally
            SpinAngle += delta * 45f;

            if (prev < 1.0f && ThreadProgress >= 1.0f)
            {
                return 1; // Snapped torqued!
            }
            if (prev > 0.0f && ThreadProgress <= 0.0f)
            {
                return -1; // Unthreaded!
            }
            return 0;
        }
    }
}
