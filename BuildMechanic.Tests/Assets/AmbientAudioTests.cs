using System;
using System.IO;
using BuildMechanic.Audio;
using Xunit;

namespace BuildMechanic.Tests.Assets
{
    public class AmbientAudioTests
    {
        [Theory]
        [InlineData("HangarHVAC")]
        [InlineData("CraneEcho")]
        [InlineData("TransformerHum")]
        [InlineData("ArgonPurge")]
        [InlineData("GrinderLoaded")]
        [InlineData("WelderStrike")]
        public void AmbientAudio_ProducesValidPcmWaveHeaders(string soundType)
        {
            byte[] wavBytes = soundType switch
            {
                "HangarHVAC" => AmbientSoundGenerator.GenerateHangarHVAC(0.3f),
                "CraneEcho" => AmbientSoundGenerator.GenerateCraneEcho(0.3f),
                "TransformerHum" => AmbientSoundGenerator.GenerateTransformerHum(0.3f),
                "ArgonPurge" => AmbientSoundGenerator.GenerateArgonPurgeHiss(0.2f),
                "GrinderLoaded" => WorkshopSounds.GenerateGrinderLoaded(0.2f),
                "WelderStrike" => WorkshopSounds.GenerateWelderArcStrike(0.2f),
                _ => throw new ArgumentException()
            };

            Assert.NotNull(wavBytes);
            Assert.True(wavBytes.Length > 44, "WAV bytes must be longer than the standard 44-byte header.");

            using var ms = new MemoryStream(wavBytes);
            using var reader = new BinaryReader(ms);

            // RIFF header
            string riff = new string(reader.ReadChars(4));
            Assert.Equal("RIFF", riff);

            int chunkSize = reader.ReadInt32();
            Assert.Equal(wavBytes.Length - 8, chunkSize);

            string wave = new string(reader.ReadChars(4));
            Assert.Equal("WAVE", wave);

            // fmt sub-chunk
            string fmt = new string(reader.ReadChars(4));
            Assert.Equal("fmt ", fmt);

            int subchunk1Size = reader.ReadInt32();
            Assert.Equal(16, subchunk1Size);

            short audioFormat = reader.ReadInt16();
            Assert.Equal(1, audioFormat); // 1 = PCM

            short channels = reader.ReadInt16();
            Assert.Equal(1, channels); // Mono

            int sampleRate = reader.ReadInt32();
            Assert.Equal(44100, sampleRate);

            int byteRate = reader.ReadInt32();
            Assert.Equal(44100 * 2, byteRate);

            short blockAlign = reader.ReadInt16();
            Assert.Equal(2, blockAlign);

            short bitsPerSample = reader.ReadInt16();
            Assert.Equal(16, bitsPerSample);

            // data sub-chunk
            string data = new string(reader.ReadChars(4));
            Assert.Equal("data", data);

            int dataSize = reader.ReadInt32();
            Assert.Equal(wavBytes.Length - 44, dataSize);
        }
    }
}
