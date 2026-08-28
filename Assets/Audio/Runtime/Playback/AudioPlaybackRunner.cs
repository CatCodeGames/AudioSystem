using CatCode.PlayerLoops;
using System;
using UnityEngine;
using UnityEngine.Audio;

namespace CatCode.Audio
{
    public sealed partial class AudioPlaybackRunner : IAudioPlaybackController
    {
        private PlaybackState _state;
        private WhileHandle _playbackHandle;

        private bool _loop;
        private AudioMixerGroup _outputAudioMixerGroup;
        private AudioSource _audioSource;

        private Callback<PlaybackResult> _onFinished;

        private readonly PlayerLoopTiming _timing;
        private readonly PlayerLoopPhase _phase;

        private readonly Func<bool> _cachedPlaybackPredicate;
        private readonly Action _cachedPlaybackOnCompleted;

        private readonly AudioMixerMirror _audioMixerMirror;
        private readonly AudioMixerGroupPropertyResolver _pauseState;
        private readonly AudioMixerGroupPropertyResolver _muteState;

        public AudioPlaybackRunner(PlayerLoopTiming timing, PlayerLoopPhase phase, AudioMixerMirror audioMixerMirror)
        {
            _timing = timing;
            _phase = phase;

            _cachedPlaybackPredicate = () => _audioSource.isPlaying;
            _cachedPlaybackOnCompleted = () => HandleTrigger(PlaybackTrigger.Complete);

            _audioMixerMirror = audioMixerMirror;
            _pauseState = new AudioMixerGroupPropertyResolver(OnPauseChanged);
            _muteState = new AudioMixerGroupPropertyResolver(OnMuteChanged);
        }

        public AudioMixerGroup OutputAudioMixerGroup
        {
            get => _outputAudioMixerGroup;
            set
            {
                if (_outputAudioMixerGroup == value)
                    return;
                _outputAudioMixerGroup = value;
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

        public void Initialize(AudioSource audioSource, Callback<PlaybackResult> onFinished)
        {
            _audioSource = audioSource;
            _loop = audioSource.loop;
            _outputAudioMixerGroup = _audioSource.outputAudioMixerGroup;
            _onFinished = onFinished;
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
        {
            _triggers.Clear();
            HandleTrigger(PlaybackTrigger.Stop);
        }

        public void ForcedStop()
        {
            _triggers.Clear();
            HandleTrigger(PlaybackTrigger.ForcedStop);
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