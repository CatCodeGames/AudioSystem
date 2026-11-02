using System;
using UnityEngine;
using UnityEngine.Audio;

namespace CatCode.Audio
{
    public sealed class AudioService : MonoBehaviour
    {
        private AudioMixerMirror _audioMixerMirror;

        private OneShotAudioPlayer _oneShotAudioPlayer;
        private HandleAudioPlayer _handleAudioPlayer;

        [SerializeField] private AudioMixer[] _audioMixers;

        [Space]
        [Tooltip("Number of AudioSources created in Awake.")]
        [SerializeField] private int _audioSourcesStartCount = 8;
        [Tooltip("Maximum number of inactive AudioSources kept in the pool. It does not limit the number of simultaneous sounds.")]
        [SerializeField] private int _audioSourceMaxCount = 32;

        [Space]
        [SerializeField] private GameObject _audioPlayerObject;
        [SerializeField] private GameObject _oneShotPlayerObject;

        private void Awake()
        {
            _audioMixerMirror = AudioMixerMirror.Build(_audioMixers);

            _oneShotAudioPlayer = new(_oneShotPlayerObject, _audioMixerMirror);
            _handleAudioPlayer = new(
                _audioPlayerObject,
                _audioMixerMirror,
                initialSize: Mathf.Max(0, _audioSourcesStartCount),
                maxSize: Mathf.Max(1, _audioSourceMaxCount));
        }

        private void OnDestroy()
        {
            if (_handleAudioPlayer == null)
                return;

            StopAll();
            _handleAudioPlayer.Dispose();
        }


        public AudioHandle Play(AudioClip clip, AudioMixerGroup outputAudioMixerGroup, AudioPlayOptions options = default)
            => Play(clip, outputAudioMixerGroup, options, Callback<PlaybackResult>.Empty);

        public AudioHandle Play(AudioClip clip, AudioMixerGroup outputAudioMixerGroup, AudioPlayOptions options, Callback<PlaybackResult> callback)
            => _handleAudioPlayer.Play(clip, outputAudioMixerGroup, options, callback);

        public AudioHandle Play(AudioClip clip, AudioMixerGroup outputAudioMixerGroup, AudioPlayOptions options, Action<PlaybackResult> callback)
            => Play(clip, outputAudioMixerGroup, options, Callback<PlaybackResult>.Create(callback));

        public AudioHandle Play<T>(AudioClip clip, AudioMixerGroup outputAudioMixerGroup, AudioPlayOptions options, T state, Action<T, PlaybackResult> callback) where T : class
            => Play(clip, outputAudioMixerGroup, options, Callback<PlaybackResult>.Create(state, callback));

        public void PlayOneShot(AudioClip clip, AudioMixerGroup outputAudioMixerGroup, float volume = 1f)
            => _oneShotAudioPlayer.PlayOneShot(clip, outputAudioMixerGroup, volume);


        public void StopAll()
        {
            _handleAudioPlayer.StopAll();
            _oneShotAudioPlayer.StopAll();
        }

        public AudioMixerGroupMirror GetAudioMixerGroupMirror(AudioMixerGroup audioMixerGroup)
            => _audioMixerMirror.GetMirror(audioMixerGroup);

        public AudioServiceInfo GetInfo()
            => new(_handleAudioPlayer.ActiveCount, _handleAudioPlayer.GetPoolInfo());
    }
}
