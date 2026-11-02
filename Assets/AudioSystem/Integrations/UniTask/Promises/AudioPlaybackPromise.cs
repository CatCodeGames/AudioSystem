using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace CatCode.Audio.Promises
{
    internal sealed class AudioPlaybackPromise : IUniTaskSource<PlaybackResult>
    {
        private static readonly ObjectPool<AudioPlaybackPromise> s_pool = new(() => new());

        static AudioPlaybackPromise()
        {
            TaskPool.RegisterSizeGetter(typeof(AudioPlaybackPromise), () => s_pool.CountAll);
        }

        private readonly Action<PlaybackResult> _handler;
        private readonly Action _cancellationHandler;

        private AudioHandle _handle;
        private CancellationTokenRegistration _cancellationTokenRegistration;
        private UniTaskCompletionSourceCore<PlaybackResult> _core;

        private AudioPlaybackPromise()
        {
            _handler = OnFinished;
            _cancellationHandler = OnCanceled;
        }

        public static IUniTaskSource<PlaybackResult> Create(
            AudioService audioService,
            AudioClip clip,
            AudioMixerGroup outputAudioMixerGroup,
            AudioPlayOptions options,
            CancellationToken cancellationToken,
            out AudioHandle handle,
            out short token)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                handle = AudioHandle.None;
                return AutoResetUniTaskCompletionSource<PlaybackResult>.CreateFromResult(PlaybackResult.Interrupted, out token);
            }

            var promise = s_pool.Get();

            promise._core.Reset();
            promise._handle = audioService.Play(clip, outputAudioMixerGroup, options, promise._handler);

            if (cancellationToken.CanBeCanceled)
                promise._cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(promise._cancellationHandler);

            TaskTracker.TrackActiveTask(promise, 3);

            handle = promise._handle;
            token = promise._core.Version;
            return promise;
        }

        private void OnFinished(PlaybackResult result)
        {
            if (_core.UnsafeGetStatus() != UniTaskStatus.Pending)
                return;

            StopListening();
            _core.TrySetResult(result);
        }

        private void OnCanceled()
        {
            if (_core.UnsafeGetStatus() != UniTaskStatus.Pending)
                return;

            if (PlayerLoopHelper.IsMainThread)
            {
                _handle.Stop();
                return;
            }

            // The token was cancelled off the main thread (for example by CancelAfter).
            // By the time the posted action runs, this promise may already serve another playback,
            // so the version is checked before stopping.
            var version = _core.Version;
            UniTask.Post(() =>
            {
                if (_core.Version == version && _core.UnsafeGetStatus() == UniTaskStatus.Pending)
                    _handle.Stop();
            });
        }

        private void StopListening()
        {
            _cancellationTokenRegistration.Dispose();
        }

        public PlaybackResult GetResult(short token)
        {
            try
            {
                return _core.GetResult(token);
            }
            finally
            {
                TaskTracker.RemoveTracking(this);
                ReturnToPool();
            }
        }

        void IUniTaskSource.GetResult(short token)
            => GetResult(token);

        public UniTaskStatus GetStatus(short token)
            => _core.GetStatus(token);

        public UniTaskStatus UnsafeGetStatus()
            => _core.UnsafeGetStatus();

        public void OnCompleted(Action<object> continuation, object state, short token)
            => _core.OnCompleted(continuation, state, token);

        private void ReturnToPool()
        {
            _handle = default;
            _cancellationTokenRegistration = default;

            s_pool.Release(this);
        }
    }
}
