

namespace CatCode.Audio
{
    public readonly struct AudioServiceInfo
    {
        public readonly HandleAudioPlayerInfo HandleAudioPlayerInfo;
#if CATCODE_AUDIO_UNITASK_SUPPORT
        public readonly AsyncAudioPlayerInfo AsyncAudioPlayerInfo;
#endif

        public readonly PoolInfo AudioSourcePoolInfo;
        public readonly PoolInfo PlaybackRunnerPoolInfo;

#if CATCODE_AUDIO_UNITASK_SUPPORT
        public AudioServiceInfo(HandleAudioPlayerInfo handleAudioPlayerInfo, AsyncAudioPlayerInfo asyncAudioPlayerInfo, PoolInfo audioSourcePoolInfo, PoolInfo playbackRunnerPoolInfo)
        {
            HandleAudioPlayerInfo = handleAudioPlayerInfo;
            AsyncAudioPlayerInfo = asyncAudioPlayerInfo;
            AudioSourcePoolInfo = audioSourcePoolInfo;
            PlaybackRunnerPoolInfo = playbackRunnerPoolInfo;
        }
#else
        public AudioServiceInfo(HandleAudioPlayerInfo handleAudioPlayerInfo, PoolInfo audioSourcePoolInfo, PoolInfo playbackRunnerPoolInfo)
        {
            HandleAudioPlayerInfo = handleAudioPlayerInfo;
            AudioSourcePoolInfo = audioSourcePoolInfo;
            PlaybackRunnerPoolInfo = playbackRunnerPoolInfo;
        }
#endif
    }
}