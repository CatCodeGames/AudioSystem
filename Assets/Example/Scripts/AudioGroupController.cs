using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace CatCode.Audio
{
    public sealed class AudioGroupController : MonoBehaviour
    {
        [SerializeField] private AudioService _audioService;

        [Space]
        [SerializeField] private AudioMixerGroup _outputGroup;

        [Space]
        [SerializeField] private Toggle _pauseToggle;
        [SerializeField] private Toggle _muteToggle;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private string _volumePropertyName;

        private void Awake()
        {
            _pauseToggle.onValueChanged.AddListener(OnPauseChanged);
            _muteToggle.onValueChanged.AddListener(OnMuteChanged);
            _volumeSlider.onValueChanged.AddListener(OnVolumeSliderValueChanged);
        }

        private void Start()
        {
            OnVolumeSliderValueChanged(_volumeSlider.value);

            OnPauseChanged(_pauseToggle.isOn);
            OnMuteChanged(_muteToggle.isOn);
        }

        private void OnPauseChanged(bool value)
            => _audioService.GetAudioMixerGroupMirror(_outputGroup).Pause.SetValue(value);

        private void OnMuteChanged(bool value)
            => _audioService.GetAudioMixerGroupMirror(_outputGroup).Mute.SetValue(value);

        private void OnVolumeSliderValueChanged(float value)
            => _outputGroup?.audioMixer.SetFloat(_volumePropertyName, value <= 0f ? -80f : Mathf.Log10(value) * 20f);

    }
}