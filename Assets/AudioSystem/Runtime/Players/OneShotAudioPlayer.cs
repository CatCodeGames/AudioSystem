using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace CatCode.Audio
{
    internal sealed class OneShotAudioPlayer
    {
        private readonly GameObject _audioSourceOwner;
        private readonly AudioSource _defaultAudioSource;
        private readonly Dictionary<AudioMixerGroup, (AudioSource AudioSource, AudioMixerGroupMirror Mirror)> _audioSources = new();

        public OneShotAudioPlayer(GameObject audioSourceOwner, AudioMixerMirror audioMixer)
        {
            _audioSourceOwner = audioSourceOwner;
            _defaultAudioSource = CreateAudioSource(null);

            foreach (var kvp in audioMixer.GroupStates)
            {
                var audioSource = CreateAudioSource(kvp.Key);
                BindMirror(audioSource, kvp.Value);
                _audioSources.Add(kvp.Key, (audioSource, kvp.Value));
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

        private static void BindMirror(AudioSource audioSource, AudioMixerGroupMirror mirror)
        {
            audioSource.mute = mirror.Mute.TotalValue;

            mirror.Mute.Subscribe(value =>
            {
                if (audioSource != null)
                    audioSource.mute = value;
            });

            mirror.Pause.Subscribe(value =>
            {
                if (audioSource == null)
                    return;

                if (value)
                    audioSource.Pause();
                else
                    audioSource.UnPause();
            });
        }

        public void PlayOneShot(AudioClip clip, AudioMixerGroup audioMixerGroup, float volume)
        {
            if (audioMixerGroup == null)
            {
                _defaultAudioSource.PlayOneShot(clip, volume);
                return;
            }

            if (!_audioSources.TryGetValue(audioMixerGroup, out var entry))
            {
                Debug.LogWarning($"AudioMixerGroup '{audioMixerGroup.name}' belongs to an AudioMixer that is not registered in AudioService. One-shot is not played.");
                return;
            }

            // One-shots started while the group is paused are dropped instead of being queued.
            if (entry.Mirror.Pause.TotalValue)
                return;

            entry.AudioSource.PlayOneShot(clip, volume);
        }

        public void Pause()
        {
            _defaultAudioSource.Pause();
            foreach (var entry in _audioSources.Values)
                entry.AudioSource.Pause();
        }

        public void StopAll()
        {
            if (_defaultAudioSource != null)
                _defaultAudioSource.Stop();

            foreach (var entry in _audioSources.Values)
            {
                if (entry.AudioSource != null)
                    entry.AudioSource.Stop();
            }
        }
    }
}
