using System;
using System.Collections.Generic;
using System.Linq;
using BuildMechanic.Audio;
using BuildMechanic.Simulation;

namespace BuildMechanic.Gameplay
{
    public class ContractObjective
    {
        public string Description { get; set; }
        public Func<MechChassis, bool> IsCompleted { get; set; }
    }

    public class Contract
    {
        public string Title { get; set; }
        public string Client { get; set; }
        public int Reward { get; set; }
        public List<ContractObjective> Objectives { get; set; } = new List<ContractObjective>();

        public bool AllCompleted(MechChassis chassis) => Objectives.All(obj => obj.IsCompleted(chassis));
    }

    public class ContractManager
    {
        public Contract ActiveContract { get; private set; }
        public bool IsFinished { get; private set; }
        public bool RewardClaimed { get; set; }

        public void LoadContractOne()
        {
            IsFinished = false;
            RewardClaimed = false;

            ActiveContract = new Contract
            {
                Title = "Vanguard Titan — Torso Armor Overhaul",
                Client = "Ironclad Mercenary Corps",
                Reward = 4500,
                Objectives = new List<ContractObjective>
                {
                    new ContractObjective
                    {
                        Description = "Unscrew Left Breastplate (4/4 bolts)",
                        IsCompleted = c => c.LeftTorsoPlate != null && c.LeftTorsoPlate.AllBoltsRemoved
                    },
                    new ContractObjective
                    {
                        Description = "Detach damaged Breastplate for servicing",
                        IsCompleted = c => c.LeftTorsoPlate != null && c.LeftTorsoPlate.State == PlateMountState.Detached
                    },
                    new ContractObjective
                    {
                        Description = "Grind out fracture into prepped V-groove",
                        IsCompleted = c => c.LeftTorsoPlate != null && c.LeftTorsoPlate.Cracks.All(cr => cr.Stage >= CrackStage.PreppedGroove)
                    },
                    new ContractObjective
                    {
                        Description = "Weld the joint seam with MIG torch",
                        IsCompleted = c => c.LeftTorsoPlate != null && c.LeftTorsoPlate.Cracks.All(cr => cr.Stage >= CrackStage.WeldedBead)
                    },
                    new ContractObjective
                    {
                        Description = "Grind weld flush (100% Structural Integrity)",
                        IsCompleted = c => c.LeftTorsoPlate != null && c.LeftTorsoPlate.Cracks.All(cr => cr.Stage == CrackStage.FlushFinished)
                    },
                    new ContractObjective
                    {
                        Description = "Reinstall Breastplate & Torque all 4 bolts",
                        IsCompleted = c => c.LeftTorsoPlate != null && c.LeftTorsoPlate.State == PlateMountState.Mounted && c.LeftTorsoPlate.AllBoltsTorqued && c.LeftTorsoPlate.IsCrackRepaired
                    }
                }
            };
        }

        public void Update(MechChassis chassis)
        {
            if (ActiveContract != null && !IsFinished)
            {
                if (ActiveContract.AllCompleted(chassis))
                {
                    IsFinished = true;
                    AudioManager.Instance?.PlayContractSuccess();
                }
            }
        }
    }
}
