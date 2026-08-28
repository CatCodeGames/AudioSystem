using CatCode.Collections;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace CatCode.Audio
{

    public sealed class HandleAudioPlayer
    {
        private readonly Action<ActivePlaybackState, PlaybackResult> _cahchedOnFinished;

        private readonly IObjectPool<AudioSource> _audioSourcePool;
        private readonly IObjectPool<AudioPlaybackRunner> _runnersPool;

        private readonly IObjectPool<ActivePlaybackState> _statePool;
        private readonly ArrayBackedLinkedList<ActivePlaybackState> _stateRegistry;


        public HandleAudioPlayer(IObjectPool<AudioSource> audioSourcePool, IObjectPool<AudioPlaybackRunner> runnersPool)
        {
            _audioSourcePool = audioSourcePool;
            _runnersPool = runnersPool;

            _cahchedOnFinished = OnFinished;

            _statePool = new ObjectPool<ActivePlaybackState>(
                createFunc: static () => new ActivePlaybackState(),
                actionOnRelease: static instance => instance.Release(),
                collectionCheck: false);

            _stateRegistry = new ArrayBackedLinkedList<ActivePlaybackState>();
        }

        public AudioHandle Play(AudioClip clip, AudioMixerGroup outputAudioMixerGroup, AudioPlayOptions options, Callback<PlaybackResult> onFinished)
        {
            var audioSource = _audioSourcePool.Get();

            audioSource.clip = clip;
            audioSource.outputAudioMixerGroup = outputAudioMixerGroup;
            audioSource.volume = options.Volume;
            audioSource.pitch = options.Pitch;
            audioSource.volume = options.Volume;
            audioSource.loop = options.Loop;

            var runner = _runnersPool.Get();

            var state = _statePool.Get();
            var slotId = _stateRegistry.Add(state);

            runner.Initialize(audioSource, Callback<PlaybackResult>.Create(state, _cahchedOnFinished));
            
            state.Initiazlie(slotId, audioSource, runner, onFinished);

            runner.Play();

            return new AudioHandle(slotId, _stateRegistry, audioSource, runner);
        }

        public void StopAll()
        {
            foreach (var state in _stateRegistry)
            {
                _stateRegistry.RemoveDeferred(state.SlotId);

                state.Runner.ForcedStop();

                var audioSource = state.AudioSource;
                var runner = state.Runner;
                var callback = state.Callback;

                _audioSourcePool.Release(audioSource);
                _runnersPool.Release(runner);
                _statePool.Release(state);

                callback.Invoke(PlaybackResult.Interrupted);
            }
            _stateRegistry.ApplyRemove();
        }

        public HandleAudioPlayerInfo GetInfo()
            => new(_stateRegistry.Count);
        
        private void OnFinished(ActivePlaybackState state, PlaybackResult result)
        {
            _stateRegistry.Remove(state.SlotId);

            var audioSource = state.AudioSource;
            var runner = state.Runner;
            var callback = state.Callback;

            _audioSourcePool.Release(audioSource);
            _runnersPool.Release(runner);
            _statePool.Release(state);

            callback.Invoke(result);
        }
    }
}