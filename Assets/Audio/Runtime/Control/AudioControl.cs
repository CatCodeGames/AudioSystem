using CatCode.Collections;
using System;
using UnityEngine;

namespace CatCode.Audio
{
    public sealed class AudioControl
    {
        private readonly ArrayBackedLinkedList<IAudioPlaybackController> _runners;
        private readonly ArrayBackedLinkedList<AudioSource> _audioSources;

        private float _volume;
        private float _pitch;
        private bool _loop;

        public AudioControl() : this(4)
        { }

        public AudioControl(int capacity)
        {
            _runners = new ArrayBackedLinkedList<IAudioPlaybackController>(capacity);
            _audioSources = new ArrayBackedLinkedList<AudioSource>(capacity);
        }

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                foreach (var audioSource in _audioSources)
                    audioSource.volume = _volume;
            }
        }

        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = value;
                foreach (var audioSource in _audioSources)
                    audioSource.pitch = _pitch;
            }
        }

        public bool Loop
        {
            get => _loop;
            set
            {
                _loop = value;
                foreach (var runner in _runners)
                    runner.Loop = _loop;
            }
        }

        public void Pause()
        {
            foreach (var operation in _runners)
                operation.Pause();
        }

        public void Resume()
        {
            foreach (var operation in _runners)
                operation.Resume();
        }

        public AudioControlRegistration Register(IAudioPlaybackController playback, AudioSource audioSource)
        {
            audioSource.volume = _volume;
            audioSource.pitch = _pitch;
            playback.Loop = _loop;
            
            var audioSourceItemId = _audioSources.Add(audioSource);
            var playbackItemId = _runners.Add(playback);

            var audioSourceRegistration = new RegistrationHandle(audioSourceItemId, _audioSources);
            var playbackRegistration = new RegistrationHandle(playbackItemId, _runners);

            return new AudioControlRegistration(audioSourceRegistration, playbackRegistration);
        }
    }

    public readonly struct RegistrationHandle : IDisposable
    {
        private readonly SlotId _slotID;
        private readonly ISlotStorage _register;

        public RegistrationHandle(SlotId itemId, ISlotStorage register)
        {
            _slotID = itemId;
            _register = register;
        }

        public void Dispose()
        {
            _register?.Remove(_slotID);
        }
    }

    public readonly struct AudioControlRegistration : IDisposable
    {
        private readonly RegistrationHandle _audioSourceRegistration;
        private readonly RegistrationHandle _playbackRegistration;

        public AudioControlRegistration(RegistrationHandle audioSourceRegistration, RegistrationHandle playbackRegistration)
        {
            _audioSourceRegistration = audioSourceRegistration;
            _playbackRegistration = playbackRegistration;
        }
        public void Dispose()
        {
            _audioSourceRegistration.Dispose();
            _playbackRegistration.Dispose();
        }
    }
}