namespace CatCode.Audio
{
    public readonly struct AudioServiceInfo
    {
        public readonly int ActivePlaybackCount;
        public readonly PoolInfo PlaybackRunnerPool;

        public AudioServiceInfo(int activePlaybackCount, PoolInfo playbackRunnerPool)
        {
            ActivePlaybackCount = activePlaybackCount;
            PlaybackRunnerPool = playbackRunnerPool;
        }
    }
}
