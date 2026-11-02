namespace CatCode.Audio
{
    public sealed class AudioMixerGroupMirror
    {
        public readonly IAudioMixerGroupBoolProperty Pause;
        public readonly IAudioMixerGroupBoolProperty Mute;

        internal AudioMixerGroupMirror(IAudioMixerGroupBoolProperty pause, IAudioMixerGroupBoolProperty mute)
        {
            Pause = pause;
            Mute = mute;
        }
    }
}
