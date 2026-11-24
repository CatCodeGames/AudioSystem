#nullable enable
using UnityEngine;
using UnityEngine.Audio;

namespace CatCode.Audio.Demo
{
    public sealed class MixerPanel : MonoBehaviour
    {
        [SerializeField] private MixerGroupPanel _master = null!;
        [SerializeField] private MixerGroupPanel _music = null!;
        [SerializeField] private MixerGroupPanel _effects = null!;

        public AudioMixerGroup MusicGroup => _music.Group;
        public AudioMixerGroup EffectsGroup => _effects.Group;

        public void Initialize(AudioService audio)
        {
            _master.Initialize(audio);
            _music.Initialize(audio);
            _effects.Initialize(audio);
        }

        public void ResetSettings()
        {
            _master.ResetSettings();
            _music.ResetSettings();
            _effects.ResetSettings();
        }
    }
}
