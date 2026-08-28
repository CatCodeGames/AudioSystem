using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace CatCode.Audio
{
    public sealed class OneShotAudioPlayer
    {
        private readonly GameObject _audioSourceOwner;
        private readonly Dictionary<AudioMixerGroup, AudioSource> _audioSources = new();

        public OneShotAudioPlayer(GameObject audioSourceOwner, AudioMixerMirror audioMixer)
        {
            _audioSourceOwner = audioSourceOwner;
            foreach (var kvp in audioMixer.GroupStates)
            {
                var audioMixerGroup = kvp.Key;

                var scaledAudioSource = CreateAudioSource(audioMixerGroup);
                _audioSources.TryAdd(audioMixerGroup, scaledAudioSource);
            }
        }

        private AudioSource CreateAudioSource(AudioMixerGroup mixerGroup)
        {
            var audioSource = _audioSourceOwner.AddComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.outputAudioMixerGroup = mixerGroup;

            return audioSource;
        }

        public void PlayOneShot(AudioClip clip, AudioMixerGroup audioMixerGroup, float volume)
        {
            if (_audioSources.TryGetValue(audioMixerGroup, out var audioSource))
                audioSource.PlayOneShot(clip, volume);
        }

        public void Pause()
        {
            foreach (var audioSource in _audioSources.Values)
                audioSource.Pause();
        }

        public void StopAll()
        {
            foreach (var audioSource in _audioSources.Values)
                audioSource.Stop();
        }
    }
}