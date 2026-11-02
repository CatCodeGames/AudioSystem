using CatCode.Collections;
using CatCode.PlayerLoops;
using System;
using UnityEngine;
using UnityEngine.Audio;

namespace CatCode.Audio
{
    internal sealed partial class AudioPlaybackRunner : IDisposable
    {
        private PlaybackState _state;
        private WhileHandle _playbackHandle;

        private bool _loop;
        private AudioMixerGroup _outputAudioMixerGroup;
        private AudioSource _audioSource;

        private SlotId _slotId;
        private Callback<PlaybackResult> _callback;

        private readonly Action<AudioPlaybackRunner, PlaybackResult> _onFinished;

        private readonly PlayerLoopTiming _timing;
        private readonly PlayerLoopPhase _phase;

        private readonly Func<bool> _cachedPlaybackPredicate;
        private readonly Action _cachedPlaybackOnCompleted;

        private readonly AudioMixerMirror _audioMixerMirror;
        private readonly AudioMixerGroupPropertyResolver _pauseState;
        private readonly AudioMixerGroupPropertyResolver _muteState;

        public AudioPlaybackRunner(
            GameObject audioSourceOwner,
            AudioMixerMirror audioMixerMirror,
            PlayerLoopTiming timing,
            PlayerLoopPhase phase,
            Action<AudioPlaybackRunner, PlaybackResult> onFinished)
        {
            _audioSource = audioSourceOwner.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;
            _audioSource.enabled = false;

            _timing = timing;
            _phase = phase;
            _onFinished = onFinished;

            _cachedPlaybackPredicate = () => _audioSource.isPlaying;
            _cachedPlaybackOnCompleted = () => HandleTrigger(PlaybackTrigger.Complete);

            _audioMixerMirror = audioMixerMirror;
            _pauseState = new AudioMixerGroupPropertyResolver(OnPauseChanged);
            _muteState = new AudioMixerGroupPropertyResolver(OnMuteChanged);
        }

        public SlotId SlotId => _slotId;
        public Callback<PlaybackResult> Callback => _callback;

        public float Volume
        {
            get => _audioSource.volume;
            set => _audioSource.volume = value;
        }

        public float Pitch
        {
            get => _audioSource.pitch;
            set => _audioSource.pitch = value;
        }

        public bool Mute
        {
            get => _muteState.LocalValue;
            set => _muteState.LocalValue = value;
        }

        public AudioMixerGroup OutputAudioMixerGroup
        {
            get => _outputAudioMixerGroup;
            set
            {
                if (_outputAudioMixerGroup == value)
                    return;
                _outputAudioMixerGroup = value;
                _audioSource.outputAudioMixerGroup = value;
                HandleTrigger(PlaybackTrigger.GroupChanged);
            }
        }

        public bool Loop
        {
            get => _loop;
            set
            {
                if (_loop == value)
                    return;
                _audioSource.loop = value;
                _loop = value;
                HandleTrigger(PlaybackTrigger.LoopChanged);
            }
        }

        public void Initialize(SlotId slotId, AudioClip clip, AudioMixerGroup outputAudioMixerGroup, AudioPlayOptions options, Callback<PlaybackResult> callback)
        {
            _slotId = slotId;
            _callback = callback;

            _loop = options.Loop;
            _outputAudioMixerGroup = outputAudioMixerGroup;

            _audioSource.enabled = true;
            _audioSource.clip = clip;
            _audioSource.outputAudioMixerGroup = outputAudioMixerGroup;
            _audioSource.volume = options.Volume;
            _audioSource.pitch = options.Pitch;
            _audioSource.loop = options.Loop;
        }

        public void Release()
            => HandleTrigger(PlaybackTrigger.Release);

        public void Play()
            => HandleTrigger(PlaybackTrigger.Play);

        public void Resume()
            => _pauseState.LocalValue = false;

        public void Pause()
            => _pauseState.LocalValue = true;

        public void Stop()
            => HandleTrigger(PlaybackTrigger.Stop);

        public void Dispose()
        {
            if (_audioSource != null)
                UnityEngine.Object.Destroy(_audioSource);
        }


        private void OnPauseChanged(bool pause)
            => HandleTrigger(pause ? PlaybackTrigger.Pause : PlaybackTrigger.Resume);

        private void OnMuteChanged(bool mute)
            => HandleTrigger(PlaybackTrigger.MuteChanged);


        private void StartProcess()
        {
            _playbackHandle = UniLoop.Schedule(_cachedPlaybackPredicate, _timing, _phase)
                .SetOnCompleted(_cachedPlaybackOnCompleted);
        }

        private void StopProcess()
        {
            _playbackHandle.Dispose();
        }
    }
}
