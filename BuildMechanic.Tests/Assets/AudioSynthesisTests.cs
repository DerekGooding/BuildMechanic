using System;
using System.IO;
using BuildMechanic.Audio;
using Xunit;

namespace BuildMechanic.Tests.Assets
{
    public class AudioSynthesisTests
    {
        [Theory]
        [InlineData("DriverRun")]
        [InlineData("TorqueClick")]
        [InlineData("GrinderMetal")]
        [InlineData("GrinderSpin")]
        [InlineData("WelderArc")]
        [InlineData("PlateMount")]
        [InlineData("PlateDetach")]
        [InlineData("ScannerPing")]
        [InlineData("ContractSuccess")]
        public void SoundGenerator_ProducesValidPcmWaveHeaders(string soundType)
        {
            byte[] wavBytes = soundType switch
            {
                "DriverRun" => SoundGenerator.GenerateImpactDriverRun(0.2f),
                "TorqueClick" => SoundGenerator.GenerateImpactTorqueClick(0.2f),
                "GrinderMetal" => SoundGenerator.GenerateAngleGrinderMetal(0.2f),
                "GrinderSpin" => SoundGenerator.GenerateAngleGrinderSpin(0.2f),
                "WelderArc" => SoundGenerator.GenerateWeldingArc(0.2f),
                "PlateMount" => SoundGenerator.GeneratePlateMount(0.2f),
                "PlateDetach" => SoundGenerator.GeneratePlateDetach(0.2f),
                "ScannerPing" => SoundGenerator.GenerateScannerPing(0.2f),
                "ContractSuccess" => SoundGenerator.GenerateContractSuccess(0.3f),
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

            int subchunk2Size = reader.ReadInt32();
            Assert.Equal(wavBytes.Length - 44, subchunk2Size);
        }
    }
}
