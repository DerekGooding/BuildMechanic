using Microsoft.Xna.Framework;
using BuildMechanic.Simulation;
using Xunit;

namespace BuildMechanic.Tests.Simulation
{
    public class BoltPhysicsTests
    {
        [Fact]
        public void Bolt_StartsFullyTorqued()
        {
            var bolt = new Bolt();
            Assert.True(bolt.IsTorqued);
            Assert.False(bolt.IsRemoved);
            Assert.Equal(1.0f, bolt.ThreadProgress);
        }

        [Fact]
        public void Bolt_Loosening_ReachesRemovedState_AndSignalsUnthreaded()
        {
            var bolt = new Bolt();

            // Partial loosen
            int signal1 = bolt.Drive(-0.5f);
            Assert.Equal(0, signal1);
            Assert.False(bolt.IsTorqued);
            Assert.False(bolt.IsRemoved);
            Assert.Equal(0.5f, bolt.ThreadProgress);

            // Complete loosen
            int signal2 = bolt.Drive(-0.6f);
            Assert.Equal(-1, signal2); // Signals unthreaded
            Assert.True(bolt.IsRemoved);
            Assert.False(bolt.IsTorqued);
            Assert.Equal(0.0f, bolt.ThreadProgress);

            // Driving further stays clamped
            int signal3 = bolt.Drive(-0.5f);
            Assert.Equal(0, signal3);
            Assert.Equal(0.0f, bolt.ThreadProgress);
        }

        [Fact]
        public void Bolt_Tightening_ReachesTorquedState_AndSignalsFullTorque()
        {
            var bolt = new Bolt();
            bolt.Drive(-1.5f); // Fully loosen
            Assert.True(bolt.IsRemoved);

            // Drive forward
            int signal1 = bolt.Drive(0.5f);
            Assert.Equal(0, signal1);
            Assert.Equal(0.5f, bolt.ThreadProgress);

            // Full torque
            int signal2 = bolt.Drive(0.6f);
            Assert.Equal(1, signal2); // Signals torqued snap
            Assert.True(bolt.IsTorqued);
            Assert.Equal(1.0f, bolt.ThreadProgress);
        }

        [Fact]
        public void Bolt_WorldPosition_ExtendsAlongNormalWhenUnthreaded()
        {
            var bolt = new Bolt
            {
                LocalPosition = new Vector3(0.30f, 0.38f, 0.041f),
                Normal = Vector3.Backward // (0, 0, 1)
            };

            Matrix world = Matrix.Identity;

            // When torqued, extension is zero
            Vector3 torquedPos = bolt.GetWorldPosition(world);
            Assert.Equal(new Vector3(0.30f, 0.38f, 0.041f), torquedPos);

            // When fully loosened, extends outward by Bolt.MaxExtension (0.12m)
            bolt.Drive(-2.0f);
            Vector3 unthreadedPos = bolt.GetWorldPosition(world);
            Assert.Equal(new Vector3(0.30f, 0.38f, 0.041f + Bolt.MaxExtension), unthreadedPos);
        }
    }
}
