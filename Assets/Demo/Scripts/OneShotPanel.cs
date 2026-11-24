#nullable enable
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace CatCode.Audio.Demo
{
    public sealed class OneShotPanel : MonoBehaviour
    {
        [SerializeField] private AudioClip _clip = null!;
        [SerializeField] private Button _play = null!;
        [SerializeField] private TMP_Text _state = null!;

        private AudioService _audio = null!;
        private AudioMixerGroup _group = null!;
        private AudioMixerGroupMirror _mirror = null!;

        public void Initialize(AudioService audio, AudioMixerGroup group)
        {
            _audio = audio;
            _group = group;
            _mirror = audio.GetAudioMixerGroupMirror(group);
            _play.onClick.AddListener(Play);
        }

        public void ResetState() => _state.text = "Ready";

        private void OnDestroy() => _play.onClick.RemoveListener(Play);

        private void Play()
        {
            _audio.PlayOneShot(_clip, _group);
            _state.text = _mirror.Pause.TotalValue ? "Skipped: group paused"
                : _mirror.Mute.TotalValue ? "Triggered: muted" : "Triggered";
        }
    }
}
