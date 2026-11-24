#nullable enable
using UnityEngine;
using UnityEngine.Audio;

namespace CatCode.Audio.Demo
{
    public sealed class MusicPlayerPanel : MonoBehaviour
    {
        [SerializeField] private PlayerControls _controls = null!;
        [SerializeField] private AudioClip _clip = null!;

        private AudioService _audio = null!;
        private AudioMixerGroup _group = null!;
        private AudioHandle _handle;

        public void Initialize(AudioService audio, AudioMixerGroup group)
        {
            _audio = audio;
            _group = group;
            _controls.Initialize(audio.GetAudioMixerGroupMirror(group), true, Play, Stop);
        }

        public void ResetSettings() => _controls.ResetSettings();

        private void Play()
        {
            _handle = _audio.Play(_clip, _group, _controls.Options, OnFinished);
            if (_handle.IsValid)
                _controls.ShowPlayback(_handle);
        }

        private void Stop() => _handle.Stop();

        private void OnDestroy() => Stop();

        private void OnFinished(PlaybackResult result)
        {
            _handle = AudioHandle.None;
            if (_controls != null)
                _controls.ShowResult(result);
        }
    }
}
