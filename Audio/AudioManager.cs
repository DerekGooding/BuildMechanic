using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace BuildMechanic.Audio
{
    public class AudioManager : IDisposable
    {
        public static AudioManager Instance { get; private set; }

        private SoundEffect _driverRunSfx;
        private SoundEffect _driverTorqueSfx;
        private SoundEffect _grinderMetalSfx;
        private SoundEffect _grinderSpinSfx;
        private SoundEffect _welderArcSfx;
        private SoundEffect _plateMountSfx;
        private SoundEffect _plateDetachSfx;
        private SoundEffect _scannerPingSfx;
        private SoundEffect _contractSuccessSfx;

        // Loop instances for sustained tool interactions
        private SoundEffectInstance _driverLoopInstance;
        private SoundEffectInstance _grinderLoopInstance;
        private SoundEffectInstance _welderLoopInstance;

        public float MasterVolume { get; set; } = 0.85f;

        public AudioManager()
        {
            Instance = this;
        }

        public void Initialize()
        {
            try
            {
                // Generate and load all audio clips dynamically from procedural PCM streams
                _driverRunSfx = LoadFromBytes(SoundGenerator.GenerateImpactDriverRun(1.0f));
                _driverTorqueSfx = LoadFromBytes(SoundGenerator.GenerateImpactTorqueClick(0.25f));
                _grinderMetalSfx = LoadFromBytes(SoundGenerator.GenerateAngleGrinderMetal(1.0f));
                _grinderSpinSfx = LoadFromBytes(SoundGenerator.GenerateAngleGrinderSpin(0.8f));
                _welderArcSfx = LoadFromBytes(SoundGenerator.GenerateWeldingArc(1.0f));
                _plateMountSfx = LoadFromBytes(SoundGenerator.GeneratePlateMount(0.6f));
                _plateDetachSfx = LoadFromBytes(SoundGenerator.GeneratePlateDetach(0.5f));
                _scannerPingSfx = LoadFromBytes(SoundGenerator.GenerateScannerPing(0.35f));
                _contractSuccessSfx = LoadFromBytes(SoundGenerator.GenerateContractSuccess(1.2f));

                // Initialize loopers
                if (_driverRunSfx != null)
                {
                    _driverLoopInstance = _driverRunSfx.CreateInstance();
                    _driverLoopInstance.IsLooped = true;
                }

                if (_grinderMetalSfx != null)
                {
                    _grinderLoopInstance = _grinderMetalSfx.CreateInstance();
                    _grinderLoopInstance.IsLooped = true;
                }

                if (_welderArcSfx != null)
                {
                    _welderLoopInstance = _welderArcSfx.CreateInstance();
                    _welderLoopInstance.IsLooped = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Audio initialization warning: {ex.Message}");
            }
        }

        private SoundEffect LoadFromBytes(byte[] wavBytes)
        {
            using var ms = new MemoryStream(wavBytes);
            return SoundEffect.FromStream(ms);
        }

        public void PlayDriverLoop(bool active)
        {
            if (_driverLoopInstance == null) return;
            if (active)
            {
                if (_driverLoopInstance.State != SoundState.Playing)
                {
                    _driverLoopInstance.Volume = MasterVolume * 0.9f;
                    _driverLoopInstance.Play();
                }
            }
            else
            {
                if (_driverLoopInstance.State == SoundState.Playing)
                    _driverLoopInstance.Pause();
            }
        }

        public void PlayGrinderLoop(bool active)
        {
            if (_grinderLoopInstance == null) return;
            if (active)
            {
                if (_grinderLoopInstance.State != SoundState.Playing)
                {
                    _grinderLoopInstance.Volume = MasterVolume * 0.85f;
                    _grinderLoopInstance.Play();
                }
            }
            else
            {
                if (_grinderLoopInstance.State == SoundState.Playing)
                    _grinderLoopInstance.Pause();
            }
        }

        public void PlayWelderLoop(bool active)
        {
            if (_welderLoopInstance == null) return;
            if (active)
            {
                if (_welderLoopInstance.State != SoundState.Playing)
                {
                    _welderLoopInstance.Volume = MasterVolume * 0.9f;
                    _welderLoopInstance.Play();
                }
            }
            else
            {
                if (_welderLoopInstance.State == SoundState.Playing)
                    _welderLoopInstance.Pause();
            }
        }

        public void PlayTorqueClick()
        {
            _driverTorqueSfx?.Play(MasterVolume, 0f, 0f);
        }

        public void PlayPlateMount()
        {
            _plateMountSfx?.Play(MasterVolume * 0.9f, 0f, 0f);
        }

        public void PlayPlateDetach()
        {
            _plateDetachSfx?.Play(MasterVolume * 0.85f, 0f, 0f);
        }

        public void PlayScannerPing()
        {
            _scannerPingSfx?.Play(MasterVolume * 0.7f, 0f, 0f);
        }

        public void PlayContractSuccess()
        {
            _contractSuccessSfx?.Play(MasterVolume, 0f, 0f);
        }

        public void StopAllLoops()
        {
            _driverLoopInstance?.Stop();
            _grinderLoopInstance?.Stop();
            _welderLoopInstance?.Stop();
        }

        public void Dispose()
        {
            StopAllLoops();
            _driverLoopInstance?.Dispose();
            _grinderLoopInstance?.Dispose();
            _welderLoopInstance?.Dispose();

            _driverRunSfx?.Dispose();
            _driverTorqueSfx?.Dispose();
            _grinderMetalSfx?.Dispose();
            _grinderSpinSfx?.Dispose();
            _welderArcSfx?.Dispose();
            _plateMountSfx?.Dispose();
            _plateDetachSfx?.Dispose();
            _scannerPingSfx?.Dispose();
            _contractSuccessSfx?.Dispose();
        }
    }
}
