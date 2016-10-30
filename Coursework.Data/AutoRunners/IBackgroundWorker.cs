namespace Coursework.Data.AutoRunners
{
    public interface IBackgroundWorker
    {
        bool IsActive { get; }
        void Run();
        void Resume();
        void Pause();
        void Stop();
    }
}
