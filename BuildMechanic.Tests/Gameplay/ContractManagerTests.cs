using System.Linq;
using BuildMechanic.Gameplay;
using BuildMechanic.Simulation;
using Xunit;

namespace BuildMechanic.Tests.Gameplay
{
    public class ContractManagerTests
    {
        [Fact]
        public void Contract_FullOverhaulWorkflow_CompletesSuccessfullyWithLatching()
        {
            var chassis = new MechChassis();
            chassis.Initialize();

            var upgradeMgr = new UpgradeManager();
            var manager = new ContractManager();
            manager.LoadContract(1, chassis);

            var contract = manager.ActiveContract;
            Assert.NotNull(contract);
            Assert.Equal(4, contract.Objectives.Count);
            Assert.False(manager.IsFinished);

            // Step 1: Unscrew & Detach Left Breastplate
            foreach (var b in chassis.LeftTorsoPlate.Bolts)
            {
                b.Drive(-2.0f);
            }
            chassis.LeftTorsoPlate.ToggleMount();
            chassis.LeftTorsoPlate.Update(5.0f);
            Assert.True(contract.Objectives[0].Evaluate(chassis));
            Assert.True(contract.Objectives[0].IsLatched);

            // Step 2: Prep groove & Weld seam
            var crack = chassis.LeftTorsoPlate.Cracks[0];
            crack.ApplyGrinding(1.0f);
            crack.ApplyWelding(1.0f);
            Assert.True(contract.Objectives[1].Evaluate(chassis));
            Assert.True(contract.Objectives[1].IsLatched);

            // Step 3: Grind flush
            crack.ApplyGrinding(1.0f);
            Assert.True(contract.Objectives[2].Evaluate(chassis));
            Assert.True(contract.Objectives[2].IsLatched);

            // Step 4: Reinstall plate and torque all 24 fasteners across chassis
            chassis.LeftTorsoPlate.ToggleMount();
            chassis.LeftTorsoPlate.Update(5.0f);
            Assert.Equal(PlateMountState.Mounted, chassis.LeftTorsoPlate.State);

            foreach (var b in chassis.LeftTorsoPlate.Bolts)
            {
                b.Drive(2.0f);
            }
            Assert.True(chassis.LeftTorsoPlate.AllBoltsTorqued);

            // Objective 4 should now be met
            Assert.True(contract.Objectives[3].Evaluate(chassis));

            // CRITICAL TEST: Objectives 0, 1, 2 must remain true because they latched!
            Assert.True(contract.Objectives[0].Evaluate(chassis), "Objective 0 un-latched when re-bolted!");
            Assert.True(contract.Objectives[1].Evaluate(chassis), "Objective 1 un-latched!");
            Assert.True(contract.Objectives[2].Evaluate(chassis), "Objective 2 un-latched!");

            // Full contract completion & payout
            Assert.True(contract.AllCompleted(chassis));
            manager.Update(chassis);
            Assert.True(manager.IsFinished);
            Assert.True(manager.RewardClaimed);
            Assert.True(upgradeMgr.Credits >= 1500 + 4500);
        }

        [Fact]
        public void Contract_MultiContractProgression_AndPayouts()
        {
            var chassis = new MechChassis();
            var upgradeMgr = new UpgradeManager { Credits = 0 };
            var manager = new ContractManager();

            // Contract 1
            manager.LoadContract(1, chassis);
            Assert.Equal(1, manager.CurrentContractNumber);
            Assert.Equal(4500, manager.ActiveContract.Reward);
            Assert.Single(manager.ActiveContract.DamagedPlateIds);
            Assert.Contains("plate_torso_left", manager.ActiveContract.DamagedPlateIds);

            // Advance to Contract 2
            manager.AdvanceToNextContract(chassis);
            Assert.Equal(2, manager.CurrentContractNumber);
            Assert.Equal(7500, manager.ActiveContract.Reward);
            Assert.Equal(2, manager.ActiveContract.DamagedPlateIds.Count);
            Assert.Contains("plate_torso_left", manager.ActiveContract.DamagedPlateIds);
            Assert.Contains("plate_torso_right", manager.ActiveContract.DamagedPlateIds);

            // Advance to Contract 3
            manager.AdvanceToNextContract(chassis);
            Assert.Equal(3, manager.CurrentContractNumber);
            Assert.Equal(10500, manager.ActiveContract.Reward);
            Assert.Equal(3, manager.ActiveContract.DamagedPlateIds.Count);

            // Advance to Procedural Contract 4
            manager.AdvanceToNextContract(chassis);
            Assert.Equal(4, manager.CurrentContractNumber);
            Assert.True(manager.ActiveContract.Reward >= 8000);
            Assert.Contains("Mk.4", manager.ActiveContract.Title);
        }

        [Fact]
        public void Contract_Reset_ClearsLatchedObjectives()
        {
            var chassis = new MechChassis();
            chassis.Initialize();

            var manager = new ContractManager();
            manager.LoadContract(1, chassis);

            // Unbolt and detach to latch objective 0
            foreach (var b in chassis.LeftTorsoPlate.Bolts) b.Drive(-2.0f);
            chassis.LeftTorsoPlate.ToggleMount();
            chassis.LeftTorsoPlate.Update(5.0f);
            Assert.True(manager.ActiveContract.Objectives[0].Evaluate(chassis));
            Assert.True(manager.ActiveContract.Objectives[0].IsLatched);

            // Reset contract
            manager.ResetContract(chassis);

            Assert.False(manager.ActiveContract.Objectives[0].IsLatched);
            Assert.False(manager.ActiveContract.Objectives[0].Evaluate(chassis));
        }
    }
}
