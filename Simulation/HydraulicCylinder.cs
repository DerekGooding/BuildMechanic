using System;
using Microsoft.Xna.Framework;

namespace BuildMechanic.Simulation
{
    public class HydraulicCylinder
    {
        public string Id { get; set; }
        public string SectorId { get; set; }
        public Vector3 Position { get; set; }
        public float Length { get; set; } = 0.45f;
        public float Radius { get; set; } = 0.04f;

        public float TargetPressure { get; set; } = 100.0f; // Nominal operating pressure in bar
        public float CurrentPressure { get; set; } = 100.0f;
        public float Contamination { get; set; } = 0.0f; // 0 to 100%
        public bool HasAirPocket { get; set; } = false;

        public bool IsNominal => Math.Abs(CurrentPressure - TargetPressure) <= 5.0f && !HasAirPocket && Contamination < 15.0f;

        public void ConfigureFault(float pressure, float contamination, bool airPocket)
        {
            CurrentPressure = pressure;
            Contamination = contamination;
            HasAirPocket = airPocket;
        }

        public bool BleedAir(float dt)
        {
            if (!HasAirPocket) return false;
            // Bleeding air drops residual pockets
            HasAirPocket = false;
            CurrentPressure = Math.Max(CurrentPressure, TargetPressure * 0.75f);
            return true;
        }

        public void FlushFluid(float dt)
        {
            // Flush old contaminated fluid with fresh synthetic hydraulic oil
            if (Contamination > 0.0f)
            {
                Contamination = Math.Max(0.0f, Contamination - dt * 35.0f);
            }

            // Pump up to target pressure
            if (CurrentPressure < TargetPressure)
            {
                CurrentPressure = Math.Min(TargetPressure, CurrentPressure + dt * 25.0f);
            }
            else if (CurrentPressure > TargetPressure)
            {
                CurrentPressure = Math.Max(TargetPressure, CurrentPressure - dt * 20.0f);
            }
        }
    }
}
