using CatCode.Collections;
using UnityEngine.Audio;

namespace CatCode.Audio
{
    public readonly struct AudioHandle
    {
        public static AudioHandle None
            => default;


        private readonly SlotId _slotId;
        private readonly SlotMap<AudioPlaybackRunner> _registry;
        private readonly AudioPlaybackRunner _runner;

        internal AudioHandle(SlotId slotId, SlotMap<AudioPlaybackRunner> registry)
        {
            _slotId = slotId;
            _registry = registry;
            if(_registry.TryGet(slotId,out var runner))
                _runner = runner;
            else
                _runner = null;
        }

        public readonly bool IsValid => _registry != null && _registry.IsValid(_slotId);
        public readonly bool IsPlaying => IsValid && _runner.IsPlaying;
        public readonly bool IsPaused => IsValid && _runner.IsPaused;

        public readonly float Volume
        {
            set
            {
                if (IsValid)
                    _runner.Volume = value;
            }
        }

        public readonly float Pitch
        {
            set
            {
                if (IsValid)
                    _runner.Pitch = value;
            }
        }

        public readonly bool Loop
        {
            set
            {
                if (IsValid)
                    _runner.Loop = value;
            }
        }

        public readonly bool Mute
        {
            set
            {
                if (IsValid)
                    _runner.Mute = value;
            }
        }

        public readonly AudioMixerGroup OutputAudioMixerGroup
        {
            set
            {
                if (IsValid)
                    _runner.OutputAudioMixerGroup = value;
            }
        }


        public bool TryGetVolume(out float volume)
        {
            if (IsValid)
            {
                volume = _runner.Volume;
                return true;
            }
            else
            {
                volume = default;
                return false;
            }
        }

        public bool TryGetPitch(out float pitch)
        {
            if (IsValid)
            {
                pitch = _runner.Pitch;
                return true;
            }
            else
            {
                pitch = default;
                return false;
            }
        }

        public bool TryGetLoop(out bool loop)
        {
            if (IsValid)
            {
                loop = _runner.Loop;
                return true;
            }
            else
            {
                loop = default;
                return false;
            }
        }

        public bool TryGetMute(out bool mute)
        {
            if (IsValid)
            {
                mute = _runner.Mute;
                return true;
            }
            else
            {
                mute = default;
                return false;
            }
        }

        public bool TryGetOutputAudioMixerGroup(out AudioMixerGroup outputAudioMixerGroup)
        {
            if (IsValid)
            {
                outputAudioMixerGroup = _runner.OutputAudioMixerGroup;
                return true;
            }
            else
            {
                outputAudioMixerGroup = default;
                return false;
            }
        }


        public readonly void Resume()
        {
            if (IsValid)
                _runner.Resume();
        }

        public readonly void Pause()
        {
            if (IsValid)
                _runner.Pause();
        }

        public readonly void Stop()
        {
            if (IsValid)
                _runner.Stop();
        }
    }
}
