using CatCode.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace CatCode.Audio
{
    public readonly struct AudioHandle
    {
        private readonly static EmptySlotStorage<ActivePlaybackState> _emptyStorage = new();

        public static AudioHandle None
            => new (new SlotId(-1, -1), _emptyStorage, null, null);


        private readonly SlotId _slotId;
        private readonly ISlotStorage<ActivePlaybackState> _registry;
        private readonly AudioSource _audioSource;
        private readonly AudioPlaybackRunner _runner;

        public AudioHandle(SlotId slotId, ISlotStorage<ActivePlaybackState> registry, AudioSource audioSource, AudioPlaybackRunner runner)
        {
            _slotId = slotId;
            _registry = registry;
            _audioSource = audioSource;
            _runner = runner;
        }

        public readonly bool IsValid => _registry != null && _registry.IsValid(_slotId);
        public readonly bool IsPlaying => _registry.IsValid(_slotId) && _runner.IsPlaying;
        public readonly bool IsPaused => _registry.IsValid(_slotId) && _runner.IsPaused;

        public readonly float Volume
        {
            set
            {
                if (_registry.IsValid(_slotId))
                    _audioSource.volume = value;
            }
        }

        public readonly float Pitch
        {
            set
            {
                if (_registry.IsValid(_slotId))
                    _audioSource.pitch = value;
            }
        }

        public readonly bool Loop
        {
            set
            {
                if (_registry.IsValid(_slotId))
                    _runner.Loop = value;
            }
        }

        public readonly AudioMixerGroup OutputAudioMixerGroup
        {
            set
            {
                if (_registry.IsValid(_slotId))
                    _runner.OutputAudioMixerGroup = value;
            }
        }


        public bool TryGetVolume(out float volume)
        {
            if (_registry.IsValid(_slotId))
            {
                volume = _audioSource.volume;
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
            if (_registry.IsValid(_slotId))
            {
                pitch = _audioSource.pitch;
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
            if (_registry.IsValid(_slotId))
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

        public bool TryGetOutputAudioMixerGroup(out bool outputAudioMixerGroup)
        {
            if (_registry.IsValid(_slotId))
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
            if (_registry.IsValid(_slotId))
                _runner.Resume();
        }

        public readonly void Pause()
        {
            if (_registry.IsValid(_slotId))
                _runner.Pause();
        }

        public readonly void Stop()
        {
            if (_registry.IsValid(_slotId))
                _runner.Stop();
        }


        internal bool TryGetStatePlaybackState(out ActivePlaybackState state)
            => _registry.TryGet(_slotId, out state);
    }
}