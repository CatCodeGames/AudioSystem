using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace CatCode.Audio
{

    public sealed class TimeScaleController : MonoBehaviour
    {
        [Serializable]
        private class AudioMixerGroupData
        {
            public AudioMixerGroup AudioMixerGroup;
            public string PitchPropertyName;
        }

        private AudioMixerGroupMirror[] _rootGroupMirrors;

        [SerializeField] private AudioService _audioService;

        [Space]
        [SerializeField] private AudioMixerGroupData[] _rootGroups;
        [SerializeField] private string _pitchPropertyName;
        [SerializeField] private float _minPitch;

        [Space]
        [SerializeField] private Slider _timeScaleSlider;
        [SerializeField] private TextMeshProUGUI _timeScaleValue;

        private void Awake()
        {
            _timeScaleSlider.onValueChanged.AddListener(OnTimeScaleValueChanged);
            _timeScaleValue.text = _timeScaleSlider.value.ToString();
        }

        private void Start()
        {
            var count = _rootGroups.Length;
            _rootGroupMirrors = new AudioMixerGroupMirror[count];

            for (int i = 0; i < count; i++)
                _rootGroupMirrors[i] = _audioService.GetAudioMixerGroupMirror(_rootGroups[i].AudioMixerGroup);

            OnTimeScaleValueChanged(_timeScaleSlider.value);
        }

        private void OnTimeScaleValueChanged(float value)
        {
            _timeScaleValue.text = _timeScaleSlider.value.ToString()+"%";
            var normValue = value / 100;

            Time.timeScale = normValue;
            var pauseValue = value < _minPitch;

            for (int i = 0; i < _rootGroups.Length; i++)
            {
                _rootGroups[i].AudioMixerGroup.audioMixer.SetFloat(_pitchPropertyName, normValue);
                _rootGroupMirrors[i].Pause.SetValue(pauseValue);
            }
        }
    }
}