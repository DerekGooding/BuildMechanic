using System;

namespace BuildMechanic.Audio
{
    public static class AmbientSoundGenerator
    {
        private const int SampleRate = 44100;

        public static byte[] GenerateHangarHVAC(float duration = 2.5f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];
            var rng = new Random(333);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;

                // Seamless looping envelope (sine ramp in & out)
                float loopEnv = MathF.Sin((float)i / totalSamples * MathF.PI);

                // Deep industrial ventilation drone (55 Hz fundamental + 110 Hz harmonic)
                float fundamental = MathF.Sin(2.0f * MathF.PI * 55.0f * t) * 0.35f;
                float subBass = MathF.Sin(2.0f * MathF.PI * 27.5f * t) * 0.25f;
                float harmonic = MathF.Sin(2.0f * MathF.PI * 110.0f * t) * 0.15f;

                // Air duct turbulence noise
                float airNoise = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.12f;

                buffer[i] = (fundamental + subBass + harmonic + airNoise) * loopEnv * 0.5f;
            }

            return SoundGenerator.CreateWav(buffer);
        }

        public static byte[] GenerateCraneEcho(float duration = 1.2f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];
            var rng = new Random(444);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = MathF.Exp(-t * 3.5f);

                // Metallic chain clink frequencies (1250 Hz, 2400 Hz)
                float clink = MathF.Sin(2.0f * MathF.PI * 1250.0f * t) * 0.4f * MathF.Exp(-t * 12.0f) +
                              MathF.Sin(2.0f * MathF.PI * 2400.0f * t) * 0.25f * MathF.Exp(-t * 18.0f);

                // Gantry trolley wheel squeak (820 Hz with slight FM flutter)
                float fm = MathF.Sin(2.0f * MathF.PI * 14.0f * t) * 35.0f;
                float squeak = MathF.Sin(2.0f * MathF.PI * (820.0f + fm) * t) * 0.2f * env;

                // Reverb diffusion noise
                float reverb = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.08f * env;

                buffer[i] = (clink + squeak + reverb) * 0.6f;
            }

            return SoundGenerator.CreateWav(buffer);
        }

        public static byte[] GenerateTransformerHum(float duration = 2.0f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float loopEnv = MathF.Sin((float)i / totalSamples * MathF.PI);

                // 60 Hz electrical mains hum with 120 Hz and 180 Hz rectifier harmonics
                float h60 = MathF.Sin(2.0f * MathF.PI * 60.0f * t) * 0.4f;
                float h120 = MathF.Sin(2.0f * MathF.PI * 120.0f * t) * 0.35f;
                float h180 = MathF.Sin(2.0f * MathF.PI * 180.0f * t) * 0.15f;

                buffer[i] = (h60 + h120 + h180) * loopEnv * 0.35f;
            }

            return SoundGenerator.CreateWav(buffer);
        }

        public static byte[] GenerateArgonPurgeHiss(float duration = 0.5f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];
            var rng = new Random(555);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = MathF.Exp(-t * 5.0f);
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.45f;
                buffer[i] = noise * env * 0.5f;
            }

            return SoundGenerator.CreateWav(buffer);
        }
    }
}
