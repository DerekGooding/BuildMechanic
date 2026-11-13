using System;

namespace BuildMechanic.Audio
{
    public static class WorkshopSounds
    {
        private const int SampleRate = 44100;

        public static byte[] GenerateHydraulicHiss(float duration = 1.0f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];
            var rng = new Random(55);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = MathF.Sin(Math.Clamp(t / duration, 0f, 1f) * MathF.PI);
                // Filtered pressurized hiss noise
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.45f;
                float fluidGurgle = MathF.Sin(2.0f * MathF.PI * 95.0f * t) * 0.2f;
                buffer[i] = (noise + fluidGurgle) * env * 0.7f;
            }

            return SoundGenerator.CreateWav(buffer);
        }

        public static byte[] GenerateMultimeterBeep(float duration = 0.22f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = Math.Min(1.0f, Math.Min(t * 100f, (duration - t) * 100f));
                // Pure 880 Hz standard electronics continuity test beep
                float tone = MathF.Sin(2.0f * MathF.PI * 880.0f * t);
                buffer[i] = tone * env * 0.65f;
            }

            return SoundGenerator.CreateWav(buffer);
        }

        public static byte[] GenerateReactorSpool(float duration = 1.5f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float normT = t / duration;
                // Pitch rises smoothly from 80 Hz up to 640 Hz
                float freq = 80.0f + normT * normT * 560.0f;
                float turbine = MathF.Sin(2.0f * MathF.PI * freq * t) * 0.5f;
                float subBass = MathF.Sin(2.0f * MathF.PI * 45.0f * t) * 0.4f;
                float env = Math.Min(1.0f, normT * 3.0f);
                buffer[i] = (turbine + subBass) * env * 0.8f;
            }

            return SoundGenerator.CreateWav(buffer);
        }

        public static byte[] GenerateKlaxon(float duration = 0.75f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                // Dissonant dual alarm tone (440 Hz + 466 Hz tritone beat)
                float tone = MathF.Sin(2.0f * MathF.PI * 440.0f * t) * 0.5f +
                             MathF.Sin(2.0f * MathF.PI * 466.0f * t) * 0.5f;
                float pulse = MathF.Sin(2.0f * MathF.PI * 6.0f * t) > 0f ? 1.0f : 0.2f;
                buffer[i] = tone * pulse * 0.75f;
            }

            return SoundGenerator.CreateWav(buffer);
        }

        public static byte[] GeneratePaintSpray(float duration = 0.8f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];
            var rng = new Random(77);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.5f;
                float aerosolPulse = MathF.Sin(2.0f * MathF.PI * 18.0f * t) * 0.15f;
                buffer[i] = (noise + aerosolPulse) * 0.6f;
            }

            return SoundGenerator.CreateWav(buffer);
        }

        public static byte[] GeneratePlateMount(float duration = 0.6f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];
            var rng = new Random(222);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = MathF.Exp(-t * 9.0f);

                float thud = MathF.Sin(2.0f * MathF.PI * 80.0f * t) * 0.6f +
                             MathF.Sin(2.0f * MathF.PI * 320.0f * t) * 0.35f * MathF.Exp(-t * 14.0f) +
                             MathF.Sin(2.0f * MathF.PI * 740.0f * t) * 0.2f * MathF.Exp(-t * 22.0f);

                float rattle = ((float)rng.NextDouble() * 2.0f - 1.0f) * MathF.Exp(-t * 30.0f) * 0.4f;
                buffer[i] = (thud + rattle) * env * 0.9f;
            }

            return SoundGenerator.CreateWav(buffer);
        }

        public static byte[] GeneratePlateDetach(float duration = 0.5f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = MathF.Exp(-t * 8.0f);

                float tone = MathF.Sin(2.0f * MathF.PI * 440.0f * t) * 0.5f +
                             MathF.Sin(2.0f * MathF.PI * 880.0f * t) * 0.3f +
                             MathF.Sin(2.0f * MathF.PI * 1320.0f * t) * 0.15f;

                buffer[i] = tone * env * 0.7f;
            }

            return SoundGenerator.CreateWav(buffer);
        }

        public static byte[] GenerateGrinderLoaded(float duration = 1.0f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];
            var rng = new Random(666);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;

                // Bogged down motor tone under heavy steel friction load (~280 Hz)
                float motor = MathF.Sin(2.0f * MathF.PI * 280.0f * t) * 0.35f +
                              MathF.Sin(2.0f * MathF.PI * 560.0f * t) * 0.20f;

                // High-frequency abrasive grinding grit noise
                float grit = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.35f;

                buffer[i] = (motor + grit) * 0.75f;
            }

            return SoundGenerator.CreateWav(buffer);
        }

        public static byte[] GenerateWelderArcStrike(float duration = 0.8f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];
            var rng = new Random(777);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;

                // Initial HF spark pop in first 40ms
                float pop = t < 0.04f ? MathF.Sin(2.0f * MathF.PI * 1800.0f * t) * MathF.Exp(-t * 80.0f) * 0.8f : 0f;

                // Rectified 120 Hz plasma arc buzz with randomized sizzle
                float buzz = MathF.Sin(2.0f * MathF.PI * 120.0f * t) * 0.4f +
                             MathF.Sin(2.0f * MathF.PI * 240.0f * t) * 0.25f;
                float sizzle = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.25f;

                buffer[i] = (pop + buzz + sizzle) * 0.7f;
            }

            return SoundGenerator.CreateWav(buffer);
        }
    }
}
