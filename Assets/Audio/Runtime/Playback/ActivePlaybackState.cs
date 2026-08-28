using CatCode.Collections;
using UnityEngine;

namespace CatCode.Audio
{
    public sealed class ActivePlaybackState
    {
        public SlotId SlotId;
        public AudioSource AudioSource;
        public AudioPlaybackRunner Runner;
        public Callback<PlaybackResult> Callback;

        public void Initiazlie(SlotId slotId, AudioSource audioSource, AudioPlaybackRunner runner, Callback<PlaybackResult> callback)
        {
            SlotId = slotId;
            AudioSource = audioSource;
            Runner = runner;
            Callback = callback;
        }

        public void Release()
        {
            SlotId = default;
            AudioSource = null;
            Runner = null;
            Callback = default;
        }
    }
}