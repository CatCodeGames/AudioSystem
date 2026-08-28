using System;
using System.Collections.Generic;

namespace CatCode.Audio
{
    public sealed partial class AudioPlaybackRunner
    {
        private enum PlaybackTrigger
        {
            Play,
            Pause,
            Resume,

            Stop,
            ForcedStop,
            Complete,

            MuteChanged,
            LoopChanged,
            GroupChanged,

            Release
        }

        private bool _isProcessingTriggers;
        private readonly Queue<PlaybackTrigger> _triggers = new();

        public bool IsPlaying
            => (_state == PlaybackState.Playing) ||
               (_state == PlaybackState.Paused && !_pauseState.LocalValue);

        public bool IsPaused
            => _state == PlaybackState.Paused && _pauseState.LocalValue;

        private void HandleTrigger(PlaybackTrigger trigger)
        {
            _triggers.Enqueue(trigger);

            if (_isProcessingTriggers)
                return;

            _isProcessingTriggers = true;
            while (_triggers.TryDequeue(out var currentTrigger))
                Transition(currentTrigger);
            _isProcessingTriggers = false;
        }

        private void Transition(PlaybackTrigger trigger)
        {
            switch (_state, trigger)
            {
                case (PlaybackState.Idle, PlaybackTrigger.Play):
                    StartPlayback();
                    break;

                case (PlaybackState.Playing, PlaybackTrigger.Pause):
                    PausePlayback();
                    return;

                case (PlaybackState.Paused, PlaybackTrigger.Resume):
                    ResumePlayback();
                    break;

                case (PlaybackState.Playing, PlaybackTrigger.MuteChanged):
                case (PlaybackState.Paused, PlaybackTrigger.MuteChanged):
                    UpdatePlaybackMute();
                    break;

                case (PlaybackState.Playing, PlaybackTrigger.LoopChanged):
                    UpdatePlaybackLoop();
                    break;

                case (PlaybackState.Playing, PlaybackTrigger.GroupChanged):
                case (PlaybackState.Paused, PlaybackTrigger.GroupChanged):
                    RebindGroupMirror();
                    break;

                case (PlaybackState.Playing, PlaybackTrigger.Complete):
                    CompletePlayback();
                    break;

                case (PlaybackState.Playing, PlaybackTrigger.Stop):
                case (PlaybackState.Paused, PlaybackTrigger.Stop):
                    StopPlayback(true);
                    break;

                case (PlaybackState.Playing, PlaybackTrigger.ForcedStop):
                case (PlaybackState.Paused, PlaybackTrigger.ForcedStop):
                    StopPlayback(false);
                    break;

                case (_, PlaybackTrigger.Release):
                    Release();
                    break;
            }

            void StartPlayback()
            {
                var audioMixerGroupMirror = _audioMixerMirror.GetMirror(_outputAudioMixerGroup);

                _pauseState.Bind(audioMixerGroupMirror.Pause);
                _muteState.Bind(audioMixerGroupMirror.Mute);

                _audioSource.mute = _muteState.ResultValue;

                if (_pauseState.ResultValue)
                {
                    _state = PlaybackState.Paused;
                    _audioSource.Play();
                    _audioSource.Pause();
                }
                else
                {
                    _state = PlaybackState.Playing;
                    _audioSource.Play();
                    if (!_loop)
                        StartProcess();
                }
            }

            void PausePlayback()
            {
                _state = PlaybackState.Paused;
                _audioSource.Pause();
                if (!_loop)
                    StopProcess();
            }

            void UpdatePlaybackLoop()
            {
                if (_loop)
                    StopProcess();
                else
                    StartProcess();
            }

            void UpdatePlaybackMute()
            {
                _audioSource.mute = _muteState.ResultValue;
            }

            void ResumePlayback()
            {
                _state = PlaybackState.Playing;
                _audioSource.UnPause();
                if (!_loop)
                    StartProcess();
            }

            void RebindGroupMirror()
            {
                var mirror = _audioMixerMirror.GetMirror(_outputAudioMixerGroup);

                _muteState.Bind(mirror.Mute);
                OnMuteChanged(_muteState.ResultValue);

                _pauseState.Bind(mirror.Pause);
                OnPauseChanged(_pauseState.ResultValue);
            }

            void CompletePlayback()
            {
                _state = PlaybackState.Finished;
                _triggers.Clear();

                _muteState.UnBind();
                _pauseState.UnBind();


                _onFinished.Invoke(PlaybackResult.Completed);
            }

            void StopPlayback(bool invokeCallback)
            {
                _state = PlaybackState.Finished;
                _triggers.Clear();

                _playbackHandle.Dispose();
                _audioSource.Stop();

                _muteState.UnBind();
                _pauseState.UnBind();

                if(invokeCallback)
                    _onFinished.Invoke(PlaybackResult.Interrupted);
            }

            void Release()
            {
                _state = PlaybackState.Idle;

                _playbackHandle.Dispose();

                _audioSource?.Stop();
                _audioSource = null;

                _pauseState.UnBind();
                _muteState.UnBind();

                _pauseState.LocalValue = false;
                _muteState.LocalValue = false;

                _loop = false;
                _onFinished = default;
            }
        }
    }
}
