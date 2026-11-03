using System;
using System.IO;

namespace BuildMechanic.Audio
{
    /// <summary>
    /// Generates authentic 16-bit PCM WAV audio samples procedurally.
    /// Used for workshop tools, mechanical impacts, electrical welding arcs, and UI chimes.
    /// </summary>
    public static class SoundGenerator
    {
        private const int SampleRate = 44100;

        public static byte[] CreateWav(float[] samples)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            int subChunk2Size = samples.Length * 2; // 16-bit = 2 bytes per sample
            int chunkSize = 36 + subChunk2Size;

            // RIFF header
            writer.Write(new char[] { 'R', 'I', 'F', 'F' });
            writer.Write(chunkSize);
            writer.Write(new char[] { 'W', 'A', 'V', 'E' });

            // "fmt " sub-chunk
            writer.Write(new char[] { 'f', 'm', 't', ' ' });
            writer.Write(16); // Subchunk1Size for PCM
            writer.Write((short)1); // AudioFormat 1 = PCM
            writer.Write((short)1); // NumChannels = 1 (Mono)
            writer.Write(SampleRate); // SampleRate
            writer.Write(SampleRate * 2); // ByteRate (SampleRate * NumChannels * BitsPerSample/8)
            writer.Write((short)2); // BlockAlign (NumChannels * BitsPerSample/8)
            writer.Write((short)16); // BitsPerSample

            // "data" sub-chunk
            writer.Write(new char[] { 'd', 'a', 't', 'a' });
            writer.Write(subChunk2Size);

            // Write PCM 16-bit samples
            foreach (float sample in samples)
            {
                float clamped = Math.Clamp(sample, -1.0f, 1.0f);
                short pcm = (short)(clamped * 32767f);
                writer.Write(pcm);
            }

            return stream.ToArray();
        }

        /// <summary>
        /// Generates impact driver motor running with rapid mechanical hammer pulses (~32 Hz).
        /// </summary>
        public static byte[] GenerateImpactDriverRun(float duration = 1.0f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];
            var rng = new Random(42);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;

                // Electric DC motor tone (~210 Hz with odd harmonics)
                float motor = (float)(
                    0.25 * Math.Sin(2.0 * Math.PI * 210.0 * t) +
                    0.15 * Math.Sin(2.0 * Math.PI * 420.0 * t) +
                    0.08 * Math.Sin(2.0 * Math.PI * 840.0 * t)
                );

                // Pneumatic/rotary impact hammer pulse (~32 impacts per sec)
                float pulsePhase = (t * 32.0f) % 1.0f;
                float pulseEnv = MathF.Pow(1.0f - pulsePhase, 4.0f);
                float impactNoise = ((float)rng.NextDouble() * 2.0f - 1.0f) * pulseEnv * 0.6f;
                float impactClack = MathF.Sin(2.0f * MathF.PI * 980.0f * t) * pulseEnv * 0.4f;

                buffer[i] = (motor + impactNoise + impactClack) * 0.7f;
            }

            return CreateWav(buffer);
        }

        /// <summary>
        /// Generates a sharp mechanical torque wrench / impact driver lock snap.
        /// </summary>
        public static byte[] GenerateImpactTorqueClick(float duration = 0.25f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];
            var rng = new Random(101);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = MathF.Exp(-t * 35.0f); // Fast exponential decay

                // Metallic snap frequencies (1800Hz, 3200Hz, 5100Hz)
                float metallic = MathF.Sin(2.0f * MathF.PI * 1850.0f * t) * 0.5f +
                                 MathF.Sin(2.0f * MathF.PI * 3400.0f * t) * 0.3f +
                                 MathF.Sin(2.0f * MathF.PI * 5200.0f * t) * 0.2f;

                float noiseBurst = ((float)rng.NextDouble() * 2.0f - 1.0f) * MathF.Exp(-t * 90.0f) * 0.7f;

                buffer[i] = (metallic + noiseBurst) * env * 0.9f;
            }

            return CreateWav(buffer);
        }

        /// <summary>
        /// Generates angle grinder grinding into steel plate (high friction scream + motor + sparks).
        /// </summary>
        public static byte[] GenerateAngleGrinderMetal(float duration = 1.0f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];
            var rng = new Random(555);

            float lowpassNoise = 0f;
            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;

                // 8500 RPM motor whine (~480 Hz + harsh harmonics)
                float motor = (float)(
                    0.20 * Math.Sin(2.0 * Math.PI * 480.0 * t) +
                    0.18 * Math.Sin(2.0 * Math.PI * 960.0 * t) +
                    0.12 * Math.Sin(2.0 * Math.PI * 1440.0 * t) +
                    0.10 * Math.Sin(2.0 * Math.PI * 2400.0 * t)
                );

                // High-frequency friction scream (metal abrasive contact ~3100Hz and ~4500Hz)
                float frictionScream = (float)(
                    0.15 * Math.Sin(2.0 * Math.PI * 3150.0 * t) +
                    0.12 * Math.Sin(2.0 * Math.PI * 4600.0 * t)
                );

                // Grit noise
                float rawNoise = (float)rng.NextDouble() * 2.0f - 1.0f;
                lowpassNoise = lowpassNoise * 0.4f + rawNoise * 0.6f;

                // Amplitude flutter simulating spinning wheel texture
                float flutter = 0.85f + 0.15f * MathF.Sin(2.0f * MathF.PI * 140.0f * t);

                buffer[i] = (motor + frictionScream + lowpassNoise * 0.35f) * flutter * 0.75f;
            }

            return CreateWav(buffer);
        }

        /// <summary>
        /// Generates angle grinder idle spin up (whirring abrasive disc).
        /// </summary>
        public static byte[] GenerateAngleGrinderSpin(float duration = 0.8f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;
                // Pitch rises up to operating RPM
                float freq = MathHelperLerp(200f, 520f, MathF.Sqrt(progress));
                float amp = MathF.Min(1.0f, progress * 4.0f) * (1.0f - progress * 0.1f);

                float motor = MathF.Sin(2.0f * MathF.PI * freq * t) * 0.4f +
                              MathF.Sin(2.0f * MathF.PI * (freq * 2.0f) * t) * 0.25f +
                              MathF.Sin(2.0f * MathF.PI * (freq * 3.0f) * t) * 0.15f;

                buffer[i] = motor * amp * 0.7f;
            }

            return CreateWav(buffer);
        }

        /// <summary>
        /// Generates electric welding arc loop (120Hz mains buzz + erratic plasma sizzle).
        /// </summary>
        public static byte[] GenerateWeldingArc(float duration = 1.0f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];
            var rng = new Random(777);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;

                // 60Hz and 120Hz transformer current buzz
                float mainsHum = (float)(
                    0.25 * Math.Sin(2.0 * Math.PI * 60.0 * t) +
                    0.35 * Math.Sin(2.0 * Math.PI * 120.0 * t) +
                    0.15 * Math.Sin(2.0 * Math.PI * 240.0 * t)
                );

                // Chaotic electrical arc spark pops
                float plasmaPops = 0f;
                if (rng.NextDouble() < 0.18)
                {
                    plasmaPops = ((float)rng.NextDouble() * 2.0f - 1.0f) * 0.65f;
                }

                float rawNoise = ((float)rng.NextDouble() * 2.0f - 1.0f) * 0.25f;

                buffer[i] = (mainsHum + plasmaPops + rawNoise) * 0.8f;
            }

            return CreateWav(buffer);
        }

        /// <summary>
        /// Generates heavy steel plate seating / mounting clunk.
        /// </summary>
        public static byte[] GeneratePlateMount(float duration = 0.6f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];
            var rng = new Random(222);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = MathF.Exp(-t * 9.0f);

                // Sub-bass thump (80Hz) + metal plate resonance (320Hz, 740Hz)
                float thud = MathF.Sin(2.0f * MathF.PI * 80.0f * t) * 0.6f +
                             MathF.Sin(2.0f * MathF.PI * 320.0f * t) * 0.35f * MathF.Exp(-t * 14.0f) +
                             MathF.Sin(2.0f * MathF.PI * 740.0f * t) * 0.2f * MathF.Exp(-t * 22.0f);

                float rattle = ((float)rng.NextDouble() * 2.0f - 1.0f) * MathF.Exp(-t * 30.0f) * 0.4f;

                buffer[i] = (thud + rattle) * env * 0.9f;
            }

            return CreateWav(buffer);
        }

        /// <summary>
        /// Generates plate unseating / pry release metallic ring.
        /// </summary>
        public static byte[] GeneratePlateDetach(float duration = 0.5f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float env = MathF.Exp(-t * 8.0f);

                // Sliding latch release + metallic ring (440Hz, 880Hz, 1320Hz)
                float tone = MathF.Sin(2.0f * MathF.PI * 440.0f * t) * 0.5f +
                             MathF.Sin(2.0f * MathF.PI * 880.0f * t) * 0.3f +
                             MathF.Sin(2.0f * MathF.PI * 1320.0f * t) * 0.15f;

                buffer[i] = tone * env * 0.7f;
            }

            return CreateWav(buffer);
        }

        /// <summary>
        /// Generates a sleek sci-fi diagnostic AR scanner chirp.
        /// </summary>
        public static byte[] GenerateScannerPing(float duration = 0.35f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;

                // Frequency sweep upward: 1100 Hz to 2400 Hz
                float freq = MathHelperLerp(1100f, 2400f, progress);
                float env = MathF.Sin(progress * MathF.PI); // Smooth rise and fall

                float chime = MathF.Sin(2.0f * MathF.PI * freq * t) * 0.5f +
                              MathF.Sin(2.0f * MathF.PI * (freq * 1.5f) * t) * 0.25f;

                buffer[i] = chime * env * 0.6f;
            }

            return CreateWav(buffer);
        }

        /// <summary>
        /// Generates contract completed triumphant major chord chime.
        /// </summary>
        public static byte[] GenerateContractSuccess(float duration = 1.2f)
        {
            int totalSamples = (int)(SampleRate * duration);
            float[] buffer = new float[totalSamples];

            // C5 (523Hz), E5 (659Hz), G5 (784Hz), C6 (1046Hz) arpeggiated
            float[] freqs = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f };

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / SampleRate;
                float sum = 0f;

                for (int n = 0; n < freqs.Length; n++)
                {
                    float noteStartTime = n * 0.12f;
                    if (t >= noteStartTime)
                    {
                        float noteT = t - noteStartTime;
                        float noteEnv = MathF.Exp(-noteT * 3.5f);
                        float noteSine = MathF.Sin(2.0f * MathF.PI * freqs[n] * noteT);
                        sum += noteSine * noteEnv * 0.25f;
                    }
                }

                buffer[i] = sum * 0.8f;
            }

            return CreateWav(buffer);
        }

        private static float MathHelperLerp(float a, float b, float t) => a + (b - a) * Math.Clamp(t, 0f, 1f);
    }
}
