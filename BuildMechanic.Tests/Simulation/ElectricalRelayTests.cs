using BuildMechanic.Simulation;
using Xunit;

namespace BuildMechanic.Tests.Simulation
{
    public class ElectricalRelayTests
    {
        [Fact]
        public void ElectricalRelay_InitialState_IsNominal()
        {
            var relay = new ElectricalRelay();
            Assert.False(relay.IsBlown);
            Assert.True(relay.ResistanceOhms <= 2.0f);
            Assert.True(relay.IsNominal);
            Assert.False(relay.TestedWithProbe);
        }

        [Fact]
        public void ElectricalRelay_TestContinuity_SetsTestedFlag()
        {
            var relay = new ElectricalRelay();
            bool continuity = relay.TestContinuity();

            Assert.True(continuity);
            Assert.True(relay.TestedWithProbe);
        }

        [Fact]
        public void ElectricalRelay_BlownFuse_FailsContinuity_AndReplacingRestoresState()
        {
            var relay = new ElectricalRelay();
            relay.ConfigureFault(blown: true, resistance: 999.0f);

            Assert.True(relay.IsBlown);
            Assert.False(relay.IsNominal);

            bool continuity = relay.TestContinuity();
            Assert.False(continuity);

            // Replace fuse
            bool replaced = relay.ReplaceFuse();
            Assert.True(replaced);
            Assert.False(relay.IsBlown);
            Assert.True(relay.ResistanceOhms <= 2.0f);
            Assert.True(relay.IsNominal);
            Assert.True(relay.TestedWithProbe);

            // Replacing a healthy fuse returns false
            Assert.False(relay.ReplaceFuse());
        }
    }
}
