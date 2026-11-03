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

            var manager = new ContractManager();
            manager.LoadContractOne();

            var contract = manager.ActiveContract;
            Assert.NotNull(contract);
            Assert.Equal(6, contract.Objectives.Count);
            Assert.False(manager.IsFinished);

            // Step 1: Unscrew Left Torso Plate
            foreach (var b in chassis.LeftTorsoPlate.Bolts)
            {
                b.Drive(-2.0f);
            }
            Assert.True(contract.Objectives[0].Evaluate(chassis));
            Assert.True(contract.Objectives[0].IsLatched);

            // Step 2: Detach Left Torso Plate
            chassis.LeftTorsoPlate.ToggleMount();
            chassis.LeftTorsoPlate.Update(5.0f);
            Assert.True(contract.Objectives[1].Evaluate(chassis));
            Assert.True(contract.Objectives[1].IsLatched);

            // Step 3: Grind crack into V-groove
            var crack = chassis.LeftTorsoPlate.Cracks[0];
            crack.ApplyGrinding(1.0f);
            Assert.True(contract.Objectives[2].Evaluate(chassis));
            Assert.True(contract.Objectives[2].IsLatched);

            // Step 4: Weld seam
            crack.ApplyWelding(1.0f);
            Assert.True(contract.Objectives[3].Evaluate(chassis));
            Assert.True(contract.Objectives[3].IsLatched);

            // Step 5: Grind weld flush
            crack.ApplyGrinding(1.0f);
            Assert.True(contract.Objectives[4].Evaluate(chassis));
            Assert.True(contract.Objectives[4].IsLatched);

            // Step 6: Remount Breastplate and torque all bolts
            chassis.LeftTorsoPlate.ToggleMount();
            chassis.LeftTorsoPlate.Update(5.0f);
            Assert.Equal(PlateMountState.Mounted, chassis.LeftTorsoPlate.State);

            foreach (var b in chassis.LeftTorsoPlate.Bolts)
            {
                b.Drive(2.0f);
            }
            Assert.True(chassis.LeftTorsoPlate.AllBoltsTorqued);

            // Objective 6 should now be met
            Assert.True(contract.Objectives[5].Evaluate(chassis));

            // CRITICAL TEST: Objectives 1 and 2 must remain true because they latched!
            Assert.True(contract.Objectives[0].Evaluate(chassis), "Objective 1 un-latched when re-bolted!");
            Assert.True(contract.Objectives[1].Evaluate(chassis), "Objective 2 un-latched when re-mounted!");

            // Full contract completion
            Assert.True(contract.AllCompleted(chassis));
            manager.Update(chassis);
            Assert.True(manager.IsFinished);
        }

        [Fact]
        public void Contract_Reset_ClearsLatchedObjectives()
        {
            var chassis = new MechChassis();
            chassis.Initialize();

            var manager = new ContractManager();
            manager.LoadContractOne();

            // Unbolt to latch objective 1
            foreach (var b in chassis.LeftTorsoPlate.Bolts) b.Drive(-2.0f);
            Assert.True(manager.ActiveContract.Objectives[0].Evaluate(chassis));
            Assert.True(manager.ActiveContract.Objectives[0].IsLatched);

            // Re-bolt and reset
            foreach (var b in chassis.LeftTorsoPlate.Bolts) b.Drive(2.0f);
            manager.ResetContract();

            Assert.False(manager.ActiveContract.Objectives[0].IsLatched);
            Assert.False(manager.ActiveContract.Objectives[0].Evaluate(chassis));
        }
    }
}
