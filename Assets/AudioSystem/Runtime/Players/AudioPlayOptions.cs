namespace CatCode.Audio
{
    public readonly struct AudioPlayOptions
    {
        private readonly bool _isCreated;
        private readonly float _volume;
        private readonly float _pitch;

        public readonly bool Loop;

        public AudioPlayOptions(float volume = 1f, float pitch = 1f, bool loop = false)
        {
            _isCreated = true;
            _volume = volume;
            _pitch = pitch;
            Loop = loop;
        }

        // default(AudioPlayOptions) means Volume = 1 and Pitch = 1, not silence and a frozen playback.
        public float Volume => _isCreated ? _volume : 1f;
        public float Pitch => _isCreated ? _pitch : 1f;

        public static AudioPlayOptions Default => default;
    }
}
