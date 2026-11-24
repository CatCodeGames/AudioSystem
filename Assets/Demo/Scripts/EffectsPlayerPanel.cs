#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace CatCode.Audio.Demo
{
    public sealed class EffectsPlayerPanel : MonoBehaviour
    {
        [SerializeField] private PlayerControls _controls = null!;
        [SerializeField] private AudioClip _clip = null!;

        private AudioService _audio = null!;
        private AudioMixerGroup _group = null!;
        private CancellationTokenSource? _cancellation;

        public void Initialize(AudioService audio, AudioMixerGroup group)
        {
            _audio = audio;
            _group = group;
            _controls.Initialize(audio.GetAudioMixerGroupMirror(group), false, Play, Stop);
        }

        public void ResetSettings() => _controls.ResetSettings();

        private void Play() => WaitForPlayback().Forget();

        private void Stop() => _cancellation?.Cancel();

        private void OnDestroy() => Stop();

        private async UniTask WaitForPlayback()
        {
            using var cancellation = new CancellationTokenSource();
            _cancellation = cancellation;
            PlaybackResult result;
            try
            {
                var playback = _audio.PlayAsync(_clip, _group, _controls.Options, out var handle, cancellation.Token);
                _controls.ShowPlayback(handle);
                result = await playback;
            }
            finally
            {
                _cancellation = null;
            }

            if (_controls != null)
                _controls.ShowResult(result);
        }
    }
}
