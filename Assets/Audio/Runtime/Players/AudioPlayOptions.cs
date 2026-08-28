
namespace CatCode.Audio
{
    public struct AudioPlayOptions
    {
        public float Volume;
        public float Pitch;
        public bool Loop;

        public static AudioPlayOptions Default =>
            new()
            {
                Volume = 1.0f,
                Pitch = 1.0f,
                Loop = false
            };

    }
}