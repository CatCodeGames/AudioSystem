
namespace CatCode.Audio
{
    public interface IAudioPlaybackController
    {
        bool Loop { set; }
        void Resume();
        void Pause();
        void Stop();
    }
}