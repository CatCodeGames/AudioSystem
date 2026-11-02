using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace CatCode.Audio
{

    public sealed class AsyncAudioPlayerController : MonoBehaviour
    {
        private CancellationTokenSource _cts;
        private AudioHandle _handle = AudioHandle.None;

        [SerializeField] private AudioService _audioService;

        [Space]
        [SerializeField] private AudioClip _clip;
        [SerializeField] private AudioMixerGroup _outputMixerGroup;

        [Space]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _stopButton;

        [Space]
        [SerializeField] private Toggle _loopToggle;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Slider _pitchSlider;


        private void Awake()
        {
            _playButton.onClick.AddListener(OnPlayButtonClicked);
            _pauseButton.onClick.AddListener(OnPauseButtonClicked);
            _resumeButton.onClick.AddListener(OnResumeButtonClicked);
            _stopButton.onClick.AddListener(OnStopButtonClicked);

            _loopToggle.onValueChanged.AddListener(OnLoopToggleValueChanged);
            _volumeSlider.onValueChanged.AddListener(OnVolumeSliderValueChanged);
            _pitchSlider.onValueChanged.AddListener(OnPitchSliderValueChanged);
        }


        private void Start()
        {
            _resumeButton.interactable = false;
            _pauseButton.interactable = false;
            _stopButton.interactable = false;
        }

        public void OnPlayButtonClicked()
        {
            _cts ??= new CancellationTokenSource();
            PlayAsync(_cts.Token).Forget();
        }

        private void OnPauseButtonClicked()
        {
            _resumeButton.interactable = true;
            _pauseButton.interactable = false;
            _handle.Pause();
        }

        private void OnResumeButtonClicked()
        {
            _resumeButton.interactable = false;
            _pauseButton.interactable = true;
            _handle.Resume();
        }

        private void OnStopButtonClicked()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        private void OnVolumeSliderValueChanged(float value)
            => _handle.Volume = value;

        private void OnPitchSliderValueChanged(float value)
            => _handle.Pitch = value;

        private void OnLoopToggleValueChanged(bool value)
            => _handle.Loop = value;

        private async UniTaskVoid PlayAsync(CancellationToken token)
        {
            _playButton.interactable = false;

            _resumeButton.interactable = false;
            _pauseButton.interactable = true;
            _stopButton.interactable = true;

            var options = new AudioPlayOptions(_volumeSlider.value, _pitchSlider.value, _loopToggle.isOn);

            var task = _audioService.PlayAsync(_clip, _outputMixerGroup, options, out _handle, token);
            await task;

            _resumeButton.interactable = false;
            _pauseButton.interactable = false;
            _stopButton.interactable = false;

            _playButton.interactable = true;
        }
    }
}