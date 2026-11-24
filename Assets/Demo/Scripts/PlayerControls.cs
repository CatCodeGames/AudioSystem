#nullable enable
using CatCode.EventPrimitives;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CatCode.Audio.Demo
{
    public sealed class PlayerControls : MonoBehaviour
    {
        [SerializeField] private Button _play = null!;
        [SerializeField] private Button _pause = null!;
        [SerializeField] private Button _stop = null!;
        [SerializeField] private Toggle _loop = null!;
        [SerializeField] private Toggle _mute = null!;
        [SerializeField] private Slider _volume = null!;
        [SerializeField] private TMP_Text _volumeValue = null!;
        [SerializeField] private TMP_Text _pauseLabel = null!;
        [SerializeField] private TMP_Text _state = null!;

        private AudioMixerGroupMirror? _mirror;
        private ObservableSubscription _pauseSubscription;
        private ObservableSubscription _muteSubscription;
        private UnityAction _playRequested = null!;
        private UnityAction _stopRequested = null!;
        private AudioPlayOptions _defaults;
        private bool _defaultMute;
        private AudioHandle _handle;
        private PlaybackResult? _result;

        public AudioPlayOptions Options => new(volume: _volume.value, loop: _loop.isOn);

        public void Initialize(AudioMixerGroupMirror mirror, bool loopOnStart, UnityAction play, UnityAction stop)
        {
            _mirror = mirror;
            _defaults = new AudioPlayOptions(volume: _volume.value, loop: loopOnStart);
            _defaultMute = _mute.isOn;
            _playRequested = play;
            _stopRequested = stop;
            _play.onClick.AddListener(_playRequested);
            _stop.onClick.AddListener(_stopRequested);
            _pause.onClick.AddListener(TogglePause);
            _loop.onValueChanged.AddListener(SetLoop);
            _mute.onValueChanged.AddListener(SetMute);
            _volume.onValueChanged.AddListener(SetVolume);
            _pauseSubscription = mirror.Pause.Subscribe(OnGroupChanged);
            _muteSubscription = mirror.Mute.Subscribe(OnGroupChanged);
            ResetSettings();
        }

        public void ShowPlayback(AudioHandle handle)
        {
            _result = null;
            _handle = handle;
            _handle.Mute = _mute.isOn;
            Refresh();
        }

        public void ShowResult(PlaybackResult result)
        {
            _handle = AudioHandle.None;
            _result = result;
            Refresh();
        }

        public void ResetSettings()
        {
            _result = null;
            _volume.SetValueWithoutNotify(_defaults.Volume);
            _loop.SetIsOnWithoutNotify(_defaults.Loop);
            _mute.SetIsOnWithoutNotify(_defaultMute);
            SetVolume(_defaults.Volume);
            Refresh();
        }

        private void OnDestroy()
        {
            if (_mirror == null)
                return;

            _mirror = null;
            _pauseSubscription.Dispose();
            _muteSubscription.Dispose();
            _play.onClick.RemoveListener(_playRequested);
            _stop.onClick.RemoveListener(_stopRequested);
            _pause.onClick.RemoveListener(TogglePause);
            _loop.onValueChanged.RemoveListener(SetLoop);
            _mute.onValueChanged.RemoveListener(SetMute);
            _volume.onValueChanged.RemoveListener(SetVolume);
        }

        private void TogglePause()
        {
            if (_handle.IsPaused)
                _handle.Resume();
            else
                _handle.Pause();

            Refresh();
        }

        private void SetLoop(bool value) => _handle.Loop = value;

        private void SetMute(bool value)
        {
            _handle.Mute = value;
            Refresh();
        }

        private void SetVolume(float value)
        {
            _handle.Volume = value;
            _volumeValue.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }

        private void OnGroupChanged(bool _) => Refresh();

        private void Refresh()
        {
            if (_mirror == null)
                return;

            var active = _handle.IsValid;
            var pausedHere = _handle.IsPaused;
            _play.interactable = !active;
            _pause.interactable = active;
            _stop.interactable = active;
            _pauseLabel.text = pausedHere ? "[ Resume ]" : "[ Pause ]";

            if (!active)
            {
                _state.text = _result?.ToString() ?? "Ready";
                return;
            }

            var status = pausedHere ? "Paused here"
                : _mirror.Pause.TotalValue ? "Paused by mixer" : "Playing";
            var mutedBy = _mute.isOn ? " / Muted here"
                : _mirror.Mute.TotalValue ? " / Muted by mixer" : "";
            _state.text = status + mutedBy;
        }
    }
}
