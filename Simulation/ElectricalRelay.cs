using System;
using Microsoft.Xna.Framework;

namespace BuildMechanic.Simulation
{
    public class ElectricalRelay
    {
        public string Id { get; set; }
        public string SectorId { get; set; }
        public int SlotIndex { get; set; }
        public Vector3 Position { get; set; }

        public bool IsBlown { get; set; } = false;
        public float ResistanceOhms { get; set; } = 0.5f; // Nominal ~0.5 ohms
        public bool TestedWithProbe { get; set; } = false;

        public bool IsNominal => !IsBlown && ResistanceOhms <= 2.0f;

        public void ConfigureFault(bool blown, float resistance)
        {
            IsBlown = blown;
            ResistanceOhms = resistance;
            TestedWithProbe = false;
        }

        public bool TestContinuity()
        {
            TestedWithProbe = true;
            return !IsBlown && ResistanceOhms <= 2.0f;
        }

        public bool ReplaceFuse()
        {
            if (!IsBlown && ResistanceOhms <= 2.0f) return false;
            IsBlown = false;
            ResistanceOhms = 0.45f;
            TestedWithProbe = true;
            return true;
        }
    }
}
