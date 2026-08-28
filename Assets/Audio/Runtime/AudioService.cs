using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

#if CATCODE_AUDIO_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using System.Threading;
#endif

namespace CatCode.Audio
{
    public sealed class AudioService : MonoBehaviour
    {
        private ObjectPool<AudioSource> _audioSourcePool;
        private ObjectPool<AudioPlaybackRunner> _playbackRunnersPool;

        private AudioMixerMirror _audioMixerMirror;

        private OneShotAudioPlayer _oneShotAudioPlayer;
        private HandleAudioPlayer _handleAudioPlayer;
#if CATCODE_AUDIO_UNITASK_SUPPORT
        private AsyncAudioPlayer _asyncAudioPlayer;
#endif

        [SerializeField] private AudioMixer[] _audioMixers;

        [Space]
        [SerializeField] private int _audioSourcesStartCount;
        [SerializeField] private int _audioSourceMaxCount;

        [Space]
        [SerializeField] private GameObject _audioPlayerObject;
        [SerializeField] private GameObject _oneShotPlayerObject;

        private void Awake()
        {
            _audioMixerMirror = AudioMixerMirror.Build(_audioMixers);

            _audioSourcePool = new ObjectPool<AudioSource>(
                createFunc: () =>
                {
                    var audioSource = _audioPlayerObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0f;
                    audioSource.enabled = false;
                    return audioSource;
                },
                actionOnGet: audioSource =>
                {
                    audioSource.enabled = true;
                },
                actionOnRelease: audioSource =>
                {
                    audioSource.enabled = false;
                    audioSource.Stop();
                },
                actionOnDestroy: audioSource =>
                {
                    GameObject.Destroy(audioSource);
                },
                collectionCheck: false,
                defaultCapacity: _audioSourcesStartCount,
                maxSize: _audioSourceMaxCount);

            _playbackRunnersPool = new ObjectPool<AudioPlaybackRunner>(
                createFunc: () => new AudioPlaybackRunner(PlayerLoops.PlayerLoopTiming.Update, PlayerLoops.PlayerLoopPhase.Early, _audioMixerMirror),
                actionOnRelease: instance => instance.Release());

            _oneShotAudioPlayer = new(_oneShotPlayerObject, _audioMixerMirror);
            _handleAudioPlayer = new(_audioSourcePool, _playbackRunnersPool);
#if CATCODE_AUDIO_UNITASK_SUPPORT
            _asyncAudioPlayer = new(_audioSourcePool, _playbackRunnersPool);
#endif
        }


        public AudioHandle Play(AudioClip clip, AudioMixerGroup outputAudioMixerGroup, AudioPlayOptions options, Callback<PlaybackResult> callback)
            => _handleAudioPlayer.Play(clip, outputAudioMixerGroup, options, callback);

        public AudioHandle Play(AudioClip clip, AudioMixerGroup outputAudioMixerGroup, AudioPlayOptions options, Action<PlaybackResult> callback)
            => Play(clip, outputAudioMixerGroup, options, Callback<PlaybackResult>.Create(callback));

        public AudioHandle Play<T>(AudioClip clip, AudioMixerGroup outputAudioMixerGroup, AudioPlayOptions options, T state, Action<T, PlaybackResult> callback) where T : class
            => Play(clip, outputAudioMixerGroup, options, Callback<PlaybackResult>.Create(state, callback));

#if CATCODE_AUDIO_UNITASK_SUPPORT
        public UniTask<PlaybackResult> PlayAsync(AudioClip clip, AudioMixerGroup outputAudioMixerGroup, AudioControl control, CancellationToken token)
            => _asyncAudioPlayer.PlayAsync(clip, outputAudioMixerGroup, control, token);
#endif
        public void PlayOneshot(AudioClip clip, AudioMixerGroup outputAudioMixerGroup, float volume)
            => _oneShotAudioPlayer.PlayOneShot(clip, outputAudioMixerGroup, volume);


        public void StopAll()
        {
#if CATCODE_AUDIO_UNITASK_SUPPORT
            _asyncAudioPlayer.StopAll();
#endif
            _handleAudioPlayer.StopAll();
            _oneShotAudioPlayer.StopAll();
        }

        public AudioMixerGroupMirror GetAudioMixerGroupMirror(AudioMixerGroup audioMixerGroup)
            => _audioMixerMirror.GetMirror(audioMixerGroup);

        public AudioServiceInfo GetInfo()
        {
            var handleAudioPlayerInfo = _handleAudioPlayer.GetInfo();
            var asyncAudioPlayerInfo = _asyncAudioPlayer.GetInfo();

            var audioSourcePoolInfo = PoolInfo.Create(_audioSourcePool);
            var playbackRunnerPoolInfo = PoolInfo.Create(_playbackRunnersPool);
#if CATCODE_AUDIO_UNITASK_SUPPORT
            return new AudioServiceInfo(handleAudioPlayerInfo, asyncAudioPlayerInfo, audioSourcePoolInfo, playbackRunnerPoolInfo);
#else
            return new AudioServiceInfo(handleAudioPlayerInfo, audioSourcePoolInfo, playbackRunnerPoolInfo);
#endif
        }
    }
}