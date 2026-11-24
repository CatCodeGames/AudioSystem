#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace CatCode.Audio.Demo
{
    public sealed class AudioDemo : MonoBehaviour
    {
        [SerializeField] private AudioService _audio = null!;
        [SerializeField] private Button _reset = null!;
        [SerializeField] private MixerPanel _mixer = null!;
        [SerializeField] private MusicPlayerPanel _music = null!;
        [SerializeField] private EffectsPlayerPanel _effects = null!;
        [SerializeField] private OneShotPanel _oneShot = null!;

        private void Start()
        {
            _mixer.Initialize(_audio);
            _music.Initialize(_audio, _mixer.MusicGroup);
            _effects.Initialize(_audio, _mixer.EffectsGroup);
            _oneShot.Initialize(_audio, _mixer.EffectsGroup);
            _reset.onClick.AddListener(ResetDemo);
        }

        private void OnDestroy()
            => _reset.onClick.RemoveListener(ResetDemo);

        private void ResetDemo()
        {
            _audio.StopAll();
            _mixer.ResetSettings();
            _music.ResetSettings();
            _effects.ResetSettings();
            _oneShot.ResetState();
        }
    }
}
