using System;
using System.Collections.Generic;
using System.Linq;
using BuildMechanic.Audio;
using BuildMechanic.Simulation;

namespace BuildMechanic.Gameplay
{
    public enum ProvingStage
    {
        Idle,
        ReactorSpool,
        HydraulicPressurize,
        ActuatorRangeCheck,
        TelemetryVerdict
    }

    public class ProvingGroundsManager
    {
        public ProvingStage CurrentStage { get; private set; } = ProvingStage.Idle;
        public float StageTimer { get; private set; } = 0.0f;
        public float ReactorRpmPercent { get; private set; } = 0.0f;
        public float ServoSineOffset { get; private set; } = 0.0f;

        public bool HasRunTest { get; private set; } = false;
        public bool IsFlawless { get; private set; } = false;
        public int PassedChecks { get; private set; } = 0;
        public int TotalChecks { get; private set; } = 0;
        public List<string> FaultMessages { get; } = new List<string>();

        public int CertificationBonus { get; private set; } = 2500;
        public bool BonusAwarded { get; private set; } = false;

        public void TriggerTest(MechChassis chassis)
        {
            if (CurrentStage != ProvingStage.Idle) return;

            CurrentStage = ProvingStage.ReactorSpool;
            StageTimer = 0.0f;
            ReactorRpmPercent = 0.0f;
            ServoSineOffset = 0.0f;
            HasRunTest = false;
            IsFlawless = false;
            BonusAwarded = false;
            FaultMessages.Clear();

            AudioManager.Instance?.PlayReactorSpool();
        }

        public void Update(float dt, MechChassis chassis)
        {
            if (CurrentStage == ProvingStage.Idle) return;

            StageTimer += dt;

            switch (CurrentStage)
            {
                case ProvingStage.ReactorSpool:
                    ReactorRpmPercent = Math.Min(100.0f, (StageTimer / 1.5f) * 100.0f);
                    if (StageTimer >= 1.5f)
                    {
                        CurrentStage = ProvingStage.HydraulicPressurize;
                        StageTimer = 0.0f;
                        AudioManager.Instance?.PlayHydraulicHiss();
                    }
                    break;

                case ProvingStage.HydraulicPressurize:
                    if (StageTimer >= 1.5f)
                    {
                        CurrentStage = ProvingStage.ActuatorRangeCheck;
                        StageTimer = 0.0f;
                    }
                    break;

                case ProvingStage.ActuatorRangeCheck:
                    ServoSineOffset = (float)Math.Sin(StageTimer * 4.0f) * 0.05f;
                    if (StageTimer >= 1.8f)
                    {
                        CurrentStage = ProvingStage.TelemetryVerdict;
                        StageTimer = 0.0f;
                        EvaluateTelemetry(chassis);
                    }
                    break;

                case ProvingStage.TelemetryVerdict:
                    // Stays on verdict until user clears or triggers next test
                    break;
            }
        }

        private void EvaluateTelemetry(MechChassis chassis)
        {
            HasRunTest = true;
            PassedChecks = 0;
            TotalChecks = 5;
            FaultMessages.Clear();

            // 1. Armor Mounts Check
            bool allMounted = chassis.Plates.All(p => p.State == PlateMountState.Mounted);
            if (allMounted) PassedChecks++;
            else FaultMessages.Add("UNMOUNTED ARMOR PLATING DETECTED");

            // 2. Fastener Torque Check
            bool allTorqued = chassis.Plates.All(p => p.AllBoltsTorqued);
            if (allTorqued) PassedChecks++;
            else FaultMessages.Add("FASTENER TORQUE LOSS / LOOSE BOLT DETECTED");

            // 3. Structural Integrity Check
            bool allRepaired = chassis.Plates.All(p => p.IsCrackRepaired);
            if (allRepaired) PassedChecks++;
            else FaultMessages.Add("STRUCTURAL FRACTURE COMPROMISING CHASSIS");

            // 4. Hydraulics Check
            bool hydroOk = chassis.Hydraulics.All(h => h.IsNominal);
            if (hydroOk) PassedChecks++;
            else FaultMessages.Add("HYDRAULIC PRESSURE LOSS / AIR POCKET DETECTED");

            // 5. Electrical Relays Check
            bool relayOk = chassis.Relays.All(r => r.IsNominal);
            if (relayOk) PassedChecks++;
            else FaultMessages.Add("BLOWN FUSE / OPEN CIRCUIT IN AVIONICS RELAY");

            IsFlawless = (PassedChecks == TotalChecks);

            if (IsFlawless)
            {
                if (!BonusAwarded)
                {
                    UpgradeManager.Instance?.AddReward(CertificationBonus);
                    BonusAwarded = true;
                }
                AudioManager.Instance?.PlayContractSuccess();
            }
            else
            {
                AudioManager.Instance?.PlayKlaxon();
            }
        }

        public void Reset()
        {
            CurrentStage = ProvingStage.Idle;
            StageTimer = 0.0f;
            ReactorRpmPercent = 0.0f;
            ServoSineOffset = 0.0f;
            HasRunTest = false;
            IsFlawless = false;
            BonusAwarded = false;
            FaultMessages.Clear();
        }
    }
}
