namespace Coursework.Data.AutoRunners
{
    public interface IBackgroundWorker
    {
        double Interval { get; set; }
        void Run();
        void Resume();
        void Pause();
        void Stop();
    }
}
