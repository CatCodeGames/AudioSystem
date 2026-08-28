namespace CatCode.Audio
{
    public readonly struct HandleAudioPlayerInfo
    {
        public readonly int ActiveSoundCount;

        public HandleAudioPlayerInfo(int activeSoundCount)
        {
            ActiveSoundCount = activeSoundCount;
        }
    }
}