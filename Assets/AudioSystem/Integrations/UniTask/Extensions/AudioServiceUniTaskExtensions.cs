using CatCode.Audio.Promises;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

namespace CatCode.Audio
{
    public static class AudioServiceUniTaskExtensions
    {
        public static UniTask<PlaybackResult> PlayAsync(
            this AudioService audioService,
            AudioClip clip,
            AudioMixerGroup outputAudioMixerGroup,
            AudioPlayOptions options,
            out AudioHandle handle,
            CancellationToken cancellationToken = default)
        {
            var taskSource = AudioPlaybackPromise.Create(audioService, clip, outputAudioMixerGroup, options, cancellationToken, out handle, out var token);
            return new UniTask<PlaybackResult>(taskSource, token);
        }
    }
}
