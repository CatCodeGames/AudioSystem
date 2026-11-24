#nullable enable
using CatCode.EventPrimitives;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace CatCode.Audio.Demo
{
    public sealed class MixerGroupPanel : MonoBehaviour
    {
        [SerializeField] private AudioMixerGroup _group = null!;
        [SerializeField] private string _volumeParameter = "";
        [SerializeField] private Toggle _pause = null!;
        [SerializeField] private Toggle _mute = null!;
        [SerializeField] private Slider _volume = null!;
        [SerializeField] private TMP_Text _volumeValue = null!;
        [SerializeField] private TMP_Text _state = null!;

        private AudioMixerGroupMirror? _mirror;
        private ObservableSubscription _pauseSubscription;
        private ObservableSubscription _muteSubscription;
        private float _defaultVolume;

        public AudioMixerGroup Group => _group;

        public void Initialize(AudioService audio)
        {
            _mirror = audio.GetAudioMixerGroupMirror(_group);
            _defaultVolume = _volume.value;
            _pause.onValueChanged.AddListener(SetPause);
            _mute.onValueChanged.AddListener(SetMute);
            _volume.onValueChanged.AddListener(SetVolume);
            _pauseSubscription = _mirror.Pause.Subscribe(OnGroupChanged);
            _muteSubscription = _mirror.Mute.Subscribe(OnGroupChanged);
            SetVolume(_defaultVolume);
            RefreshState();
        }

        public void ResetSettings()
        {
            _mirror!.Pause.SetValue(false);
            _mirror.Mute.SetValue(false);
            _volume.SetValueWithoutNotify(_defaultVolume);
            SetVolume(_defaultVolume);
            RefreshState();
        }

        private void OnDestroy()
        {
            _pauseSubscription.Dispose();
            _muteSubscription.Dispose();
            _pause.onValueChanged.RemoveListener(SetPause);
            _mute.onValueChanged.RemoveListener(SetMute);
            _volume.onValueChanged.RemoveListener(SetVolume);
        }

        private void SetPause(bool value)
        {
            _mirror!.Pause.SetValue(value);
            RefreshState();
        }

        private void SetMute(bool value)
        {
            _mirror!.Mute.SetValue(value);
            RefreshState();
        }

        private void SetVolume(float value)
        {
            var decibels = value > 0f ? 20f * Mathf.Log10(value) : -80f;
            _group.audioMixer.SetFloat(_volumeParameter, decibels);
            _volumeValue.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }

        private void OnGroupChanged(bool _)
            => RefreshState();

        private void RefreshState()
        {
            var pause = _mirror!.Pause;
            var mute = _mirror.Mute;
            _pause.SetIsOnWithoutNotify(pause.LocalValue);
            _mute.SetIsOnWithoutNotify(mute.LocalValue);

            var pauseState = pause.LocalValue ? "Paused here" : "Paused by Master";
            var muteState = mute.LocalValue ? "Muted here" : "Muted by Master";
            _state.text = (pause.TotalValue, mute.TotalValue) switch
            {
                (true, true) => $"{pauseState} / {muteState}",
                (true, false) => pauseState,
                (false, true) => muteState,
                _ => "Open"
            };
        }
    }
}
