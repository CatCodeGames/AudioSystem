#if CATCODE_AUDIO_UNITASK_SUPPORT

namespace CatCode.Audio
{
    public readonly struct AsyncAudioPlayerInfo
    {
        public readonly int ActiveSoundCount;

        public AsyncAudioPlayerInfo(int activeSoundCount)
        {
            ActiveSoundCount = activeSoundCount;
        }
    }
}
#endif