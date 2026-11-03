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
        public Func<MechChassis, bool> Condition { get; set; }
        public bool LatchOnceDone { get; set; }
        public bool IsLatched { get; set; }

        public bool Evaluate(MechChassis chassis)
        {
            if (LatchOnceDone && IsLatched) return true;
            bool met = Condition != null && Condition(chassis);
            if (LatchOnceDone && met)
            {
                IsLatched = true;
            }
            return met;
        }

        public void Reset()
        {
            IsLatched = false;
        }
    }

    public class Contract
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Client { get; set; }
        public int Reward { get; set; }
        public List<string> DamagedPlateIds { get; set; } = new List<string>();
        public List<ContractObjective> Objectives { get; set; } = new List<ContractObjective>();

        public bool AllCompleted(MechChassis chassis) => Objectives.All(obj => obj.Evaluate(chassis));

        public void Reset()
        {
            foreach (var obj in Objectives)
            {
                obj.Reset();
            }
        }
    }

    public class ContractManager
    {
        public Contract ActiveContract { get; private set; }
        public int CurrentContractNumber { get; private set; } = 1;
        public bool IsFinished { get; private set; }
        public bool RewardClaimed { get; set; }

        public void LoadContract(int contractNumber, MechChassis chassis)
        {
            IsFinished = false;
            RewardClaimed = false;
            CurrentContractNumber = contractNumber;

            ActiveContract = BuildContract(contractNumber);

            // Re-initialize plates and apply contract's battle damage
            chassis.Initialize();
            chassis.ConfigureDamage(ActiveContract.DamagedPlateIds);
        }

        public void LoadContractOne()
        {
            // Fallback default without chassis reference
            CurrentContractNumber = 1;
            ActiveContract = BuildContract(1);
        }

        public void AdvanceToNextContract(MechChassis chassis)
        {
            if (!RewardClaimed && ActiveContract != null)
            {
                UpgradeManager.Instance?.AddReward(ActiveContract.Reward);
                RewardClaimed = true;
            }

            LoadContract(CurrentContractNumber + 1, chassis);
        }

        public void ResetContract(MechChassis chassis)
        {
            LoadContract(CurrentContractNumber, chassis);
        }

        public void Update(MechChassis chassis)
        {
            if (ActiveContract != null && !IsFinished)
            {
                if (ActiveContract.AllCompleted(chassis))
                {
                    IsFinished = true;
                    if (!RewardClaimed)
                    {
                        UpgradeManager.Instance?.AddReward(ActiveContract.Reward);
                        RewardClaimed = true;
                    }
                    AudioManager.Instance?.PlayContractSuccess();
                }
            }
        }

        private Contract BuildContract(int number)
        {
            var objectives = new List<ContractObjective>();
            string title;
            string client;
            int reward;
            var damagedIds = new List<string>();

            switch (number)
            {
                case 1:
                    title = "Vanguard Titan - Left Torso Overhaul";
                    client = "Ironclad Mercenary Corps";
                    reward = 4500;
                    damagedIds.Add("plate_torso_left");
                    break;

                case 2:
                    title = "Ironclad Bruiser - Dual Torso Refit";
                    client = "Solaris Heavy Industries";
                    reward = 7500;
                    damagedIds.Add("plate_torso_left");
                    damagedIds.Add("plate_torso_right");
                    break;

                case 3:
                    title = "Aegis Colossus - Pauldron & Greave Overhaul";
                    client = "Garrison Defense Fleet";
                    reward = 10500;
                    damagedIds.Add("plate_shoulder_left");
                    damagedIds.Add("plate_shoulder_right");
                    damagedIds.Add("plate_leg_left");
                    break;

                default:
                    // Procedural Contract #4+
                    title = $"Combat Veteran Mk.{number} - Field Rebuild";
                    client = "Frontline Vanguard Command";
                    reward = 8000 + (number - 3) * 2500;

                    var allPlates = new string[]
                    {
                        "plate_torso_left", "plate_torso_right",
                        "plate_shoulder_left", "plate_shoulder_right",
                        "plate_leg_left", "plate_leg_right"
                    };

                    var rng = new Random(number * 313);
                    int count = rng.Next(2, 5); // 2 to 4 damaged plates
                    var shuffled = allPlates.OrderBy(_ => rng.Next()).Take(count).ToList();
                    damagedIds.AddRange(shuffled);
                    break;
            }

            // Create sequential objectives for damaged plates
            foreach (var pid in damagedIds)
            {
                string shortName = GetPlateShortName(pid);

                objectives.Add(new ContractObjective
                {
                    Description = $"Unscrew & Detach {shortName}",
                    LatchOnceDone = true,
                    Condition = c =>
                    {
                        var p = c.Plates.FirstOrDefault(x => x.Id == pid);
                        return p != null && p.State == PlateMountState.Detached;
                    }
                });

                objectives.Add(new ContractObjective
                {
                    Description = $"Prep & Weld cracks on {shortName}",
                    LatchOnceDone = true,
                    Condition = c =>
                    {
                        var p = c.Plates.FirstOrDefault(x => x.Id == pid);
                        return p != null && p.Cracks.All(cr => cr.Stage >= CrackStage.WeldedBead);
                    }
                });

                objectives.Add(new ContractObjective
                {
                    Description = $"Grind {shortName} flush (100% Integrity)",
                    LatchOnceDone = true,
                    Condition = c =>
                    {
                        var p = c.Plates.FirstOrDefault(x => x.Id == pid);
                        return p != null && p.Cracks.All(cr => cr.Stage == CrackStage.FlushFinished);
                    }
                });
            }

            // Final objective: All plates mounted and all 24 bolts torqued
            objectives.Add(new ContractObjective
            {
                Description = "Reinstall all plates & torque all fasteners",
                LatchOnceDone = false,
                Condition = c => c.Plates.All(p => p.State == PlateMountState.Mounted && p.AllBoltsTorqued && p.IsCrackRepaired)
            });

            return new Contract
            {
                Id = number,
                Title = title,
                Client = client,
                Reward = reward,
                DamagedPlateIds = damagedIds,
                Objectives = objectives
            };
        }

        private static string GetPlateShortName(string id) => id switch
        {
            "plate_torso_left" => "Left Breastplate",
            "plate_torso_right" => "Right Breastplate",
            "plate_shoulder_left" => "Left Pauldron",
            "plate_shoulder_right" => "Right Pauldron",
            "plate_leg_left" => "Left Greave",
            "plate_leg_right" => "Right Greave",
            _ => "Armor Plate"
        };
    }
}
