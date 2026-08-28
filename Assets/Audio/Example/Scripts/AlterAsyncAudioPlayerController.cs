using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace CatCode.Audio
{
    public sealed class AlterAsyncAudioPlayerController : MonoBehaviour
    {
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
            PlayAndWaitAsync().Forget();
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
            => _handle.Stop();

        private void OnVolumeSliderValueChanged(float value)
            => _handle.Volume = value;

        private void OnPitchSliderValueChanged(float value)
            => _handle.Pitch = value;

        private void OnLoopToggleValueChanged(bool value)
            => _handle.Loop = value;


        private async UniTaskVoid PlayAndWaitAsync()
        {
            _playButton.interactable = false;

            _resumeButton.interactable = false;
            _pauseButton.interactable = true;
            _stopButton.interactable = true;

            var options = new AudioPlayOptions
            {
                Volume = _volumeSlider.value,
                Pitch = _pitchSlider.value,
                Loop = _loopToggle.isOn
            };

            var tcs = AutoResetUniTaskCompletionSource<PlaybackResult>.Create();
            _handle = _audioService.Play(_clip, _outputMixerGroup, options, tcs, (tcs, result) => tcs.TrySetResult(result));

            await tcs.Task;

            _resumeButton.interactable = false;
            _pauseButton.interactable = false;
            _stopButton.interactable = false;

            _playButton.interactable = true;
        }
    }
}