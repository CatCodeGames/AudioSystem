using CatCode.Collections;
using CatCode.PlayerLoops;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace CatCode.Audio
{
    internal sealed class HandleAudioPlayer : IDisposable
    {
        private readonly ObjectPool<AudioPlaybackRunner> _runnersPool;
        private readonly SlotMap<AudioPlaybackRunner> _activeRunners = new();

        public HandleAudioPlayer(GameObject audioSourceOwner, AudioMixerMirror audioMixerMirror, int initialSize, int maxSize)
        {
            var defaultCapacity = Mathf.Clamp(initialSize, 1, maxSize);
            Action<AudioPlaybackRunner, PlaybackResult> onFinished = OnFinished;

            _runnersPool = new ObjectPool<AudioPlaybackRunner>(
                createFunc: () => new AudioPlaybackRunner(audioSourceOwner, audioMixerMirror, PlayerLoopTiming.Update, PlayerLoopPhase.Early, onFinished),
                actionOnRelease: static runner => runner.Release(),
                actionOnDestroy: static runner => runner.Dispose(),
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);

            Prewarm(Mathf.Min(initialSize, maxSize));
        }

        public int ActiveCount => _activeRunners.Count;

        public AudioHandle Play(AudioClip clip, AudioMixerGroup outputAudioMixerGroup, AudioPlayOptions options, Callback<PlaybackResult> onFinished)
        {
            var runner = _runnersPool.Get();
            var slotId = _activeRunners.Add(runner);

            runner.Initialize(slotId, clip, outputAudioMixerGroup, options, onFinished);
            runner.Play();

            return new AudioHandle(slotId, _activeRunners);
        }

        public void StopAll()
        {
            foreach (var runner in _activeRunners)
                runner.Stop();
        }

        public PoolInfo GetPoolInfo()
            => PoolInfo.Create(_runnersPool);

        public void Dispose()
            => _runnersPool.Clear();

        private void Prewarm(int count)
        {
            var runners = new AudioPlaybackRunner[count];
            for (var i = 0; i < count; i++)
                runners[i] = _runnersPool.Get();
            foreach (var runner in runners)
                _runnersPool.Release(runner);
        }

        private void OnFinished(AudioPlaybackRunner runner, PlaybackResult result)
        {
            var callback = runner.Callback;

            _activeRunners.Remove(runner.SlotId);
            _runnersPool.Release(runner);

            callback.Invoke(result);
        }
    }
}
