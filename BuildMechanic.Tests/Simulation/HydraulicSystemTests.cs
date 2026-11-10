using BuildMechanic.Simulation;
using Xunit;

namespace BuildMechanic.Tests.Simulation
{
    public class HydraulicSystemTests
    {
        [Fact]
        public void HydraulicCylinder_InitialState_IsNominal()
        {
            var cylinder = new HydraulicCylinder();
            Assert.Equal(100.0f, cylinder.CurrentPressure);
            Assert.Equal(100.0f, cylinder.TargetPressure);
            Assert.Equal(0.0f, cylinder.Contamination);
            Assert.False(cylinder.HasAirPocket);
            Assert.True(cylinder.IsNominal);
        }

        [Fact]
        public void HydraulicCylinder_FaultConfiguration_AndBleedAir()
        {
            var cylinder = new HydraulicCylinder();
            cylinder.ConfigureFault(pressure: 35.0f, contamination: 70.0f, airPocket: true);

            Assert.False(cylinder.IsNominal);
            Assert.True(cylinder.HasAirPocket);
            Assert.Equal(35.0f, cylinder.CurrentPressure);

            // Bleed air pocket
            bool bled = cylinder.BleedAir(1.0f);
            Assert.True(bled);
            Assert.False(cylinder.HasAirPocket);
            Assert.True(cylinder.CurrentPressure >= 75.0f);

            // Bleeding again returns false (already purged)
            Assert.False(cylinder.BleedAir(1.0f));
        }

        [Fact]
        public void HydraulicCylinder_FlushFluid_RestoresPressureAndCleansContamination()
        {
            var cylinder = new HydraulicCylinder();
            cylinder.ConfigureFault(pressure: 50.0f, contamination: 80.0f, airPocket: false);

            Assert.False(cylinder.IsNominal);

            // Flush fluid over multiple time steps
            cylinder.FlushFluid(1.0f);
            Assert.True(cylinder.Contamination < 80.0f);
            Assert.True(cylinder.CurrentPressure > 50.0f);

            // Full flush until nominal
            cylinder.FlushFluid(3.0f);
            Assert.Equal(0.0f, cylinder.Contamination);
            Assert.Equal(100.0f, cylinder.CurrentPressure);
            Assert.True(cylinder.IsNominal);
        }
    }
}
