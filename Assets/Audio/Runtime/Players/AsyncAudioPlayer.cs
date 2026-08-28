#if CATCODE_AUDIO_UNITASK_SUPPORT

using CatCode.Collections;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace CatCode.Audio
{
    public sealed class AsyncAudioPlayer
    {
        private bool _isStoppingAll;

        private readonly Action<AutoResetUniTaskCompletionSource<PlaybackResult>, PlaybackResult> _cachedOnFinished;
        private readonly ArrayBackedLinkedList<AudioPlaybackRunner> _activeRunners;

        private readonly IObjectPool<AudioSource> _audioSourcePool;
        private readonly IObjectPool<AudioPlaybackRunner> _runnersPool;

        public AsyncAudioPlayer(IObjectPool<AudioSource> audioSourcePool, IObjectPool<AudioPlaybackRunner> runnersPool)
        {
            _activeRunners = new();
            _cachedOnFinished = (tcs, result) => tcs.TrySetResult(result);

            _audioSourcePool = audioSourcePool;
            _runnersPool = runnersPool;
        }

        public async UniTask<PlaybackResult> PlayAsync(AudioClip clip, AudioMixerGroup outputAudioMixerGroup, AudioControl control, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return PlaybackResult.Interrupted;

            var tcs = AutoResetUniTaskCompletionSource<PlaybackResult>.Create();

            using var audioSourceLease = _audioSourcePool.Get(out var audioSource);
            audioSource.clip = clip;
            audioSource.outputAudioMixerGroup = outputAudioMixerGroup;

            using var runnerLease = _runnersPool.Get(out var runner);
            runner.Initialize(audioSource, Callback<PlaybackResult>.Create(tcs, _cachedOnFinished));
            var slotId = _activeRunners.Add(runner);

            using var ctsRegistration = token.Register(s => ((AudioPlaybackRunner)s).Stop(), runner);
            using var controlRegistration = control.Register(runner, audioSource);

            runner.Play();

            try
            {
                return await tcs.Task;
            }
            finally
            {
                if (_isStoppingAll)
                    _activeRunners.RemoveDeferred(slotId);
                else
                    _activeRunners.Remove(slotId);
            }
        }

        public void StopAll()
        {
            _isStoppingAll = true;
            foreach (var runner in _activeRunners)
                runner.Stop();
            _isStoppingAll = false;
            _activeRunners.ApplyRemove();
        }

        public AsyncAudioPlayerInfo GetInfo()
            => new(_activeRunners.Count);

    }
}
#endif