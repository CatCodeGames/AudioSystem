using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace CatCode.Audio
{

    public sealed class AudioMixerGroupController : MonoBehaviour
    {
        private AudioMixer _audioMixer;
        private AudioMixerGroupMirror _groupMirror;

        [SerializeField] private AudioService _service;
        [SerializeField] private AudioMixerGroup _group;
        [Space]
        [SerializeField] private Toggle _pauseToggle;
        [SerializeField] private Toggle _muteToggle;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private string _volumeName;

        private void Start()
        {
            _pauseToggle.onValueChanged.AddListener(OnPauseChanged);
            _muteToggle.onValueChanged.AddListener(OnMuteChanged);
            _volumeSlider.onValueChanged.AddListener(OnVolumeSliderValueChanged);

            if (_group != null)
            {
                _audioMixer = _group.audioMixer;
                _audioMixer.SetFloat(_volumeName, _volumeSlider.value <= 0f ? -80f : Mathf.Log10(_volumeSlider.value) * 20f);
            }
            _groupMirror = _service.GetAudioMixerGroupMirror(_group);
            _groupMirror.Pause.SetValue(_pauseToggle.isOn);
            _groupMirror.Mute.SetValue(_muteToggle.isOn);
        }

        private void OnPauseChanged(bool value)
            => _groupMirror.Pause.SetValue(value);

        private void OnMuteChanged(bool value)
            => _groupMirror.Mute.SetValue(value);

        private void OnVolumeSliderValueChanged(float value)
            => _audioMixer?.SetFloat(_volumeName, value <= 0f ? -80f : Mathf.Log10(value) * 20f);

    }
}