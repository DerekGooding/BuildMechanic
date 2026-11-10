using BuildMechanic.Gameplay;
using BuildMechanic.Simulation;
using Xunit;

namespace BuildMechanic.Tests.Gameplay
{
    public class ProvingGroundsTests
    {
        [Fact]
        public void ProvingGrounds_TriggerTest_TransitionsThroughAllStages()
        {
            var chassis = new MechChassis();
            chassis.Initialize();

            var proving = new ProvingGroundsManager();
            Assert.Equal(ProvingStage.Idle, proving.CurrentStage);

            proving.TriggerTest(chassis);
            Assert.Equal(ProvingStage.ReactorSpool, proving.CurrentStage);

            // Fast forward through reactor spool (1.5s)
            proving.Update(1.6f, chassis);
            Assert.Equal(ProvingStage.HydraulicPressurize, proving.CurrentStage);

            // Fast forward through hydraulic pressurize (1.5s)
            proving.Update(1.6f, chassis);
            Assert.Equal(ProvingStage.ActuatorRangeCheck, proving.CurrentStage);

            // Fast forward through actuator range check (1.8s)
            proving.Update(1.9f, chassis);
            Assert.Equal(ProvingStage.TelemetryVerdict, proving.CurrentStage);
            Assert.True(proving.HasRunTest);
        }

        [Fact]
        public void ProvingGrounds_PristineChassis_PassesFlawlessWithBonus()
        {
            var chassis = new MechChassis();
            chassis.Initialize();
            var upgradeMgr = new UpgradeManager { Credits = 1000 };

            var proving = new ProvingGroundsManager();
            proving.TriggerTest(chassis);
            proving.Update(1.6f, chassis);
            proving.Update(1.6f, chassis);
            proving.Update(1.9f, chassis);

            Assert.Equal(ProvingStage.TelemetryVerdict, proving.CurrentStage);
            Assert.True(proving.IsFlawless);
            Assert.Equal(5, proving.PassedChecks);
            Assert.Equal(5, proving.TotalChecks);
            Assert.Empty(proving.FaultMessages);
            Assert.True(proving.BonusAwarded);
            Assert.Equal(1000 + proving.CertificationBonus, upgradeMgr.Credits);
        }

        [Fact]
        public void ProvingGrounds_LooseFastenerOrBlownRelay_FailsTelemetryCheck()
        {
            var chassis = new MechChassis();
            chassis.Initialize();
            // Loosen one bolt
            chassis.LeftTorsoPlate.Bolts[0].Drive(-0.5f);
            // Blow one relay
            chassis.Relays[0].ConfigureFault(blown: true, resistance: 999f);

            var proving = new ProvingGroundsManager();
            proving.TriggerTest(chassis);
            proving.Update(1.6f, chassis);
            proving.Update(1.6f, chassis);
            proving.Update(1.9f, chassis);

            Assert.Equal(ProvingStage.TelemetryVerdict, proving.CurrentStage);
            Assert.False(proving.IsFlawless);
            Assert.True(proving.PassedChecks < 5);
            Assert.NotEmpty(proving.FaultMessages);
            Assert.False(proving.BonusAwarded);
        }
    }
}
