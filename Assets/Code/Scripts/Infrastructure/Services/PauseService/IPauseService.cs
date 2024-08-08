namespace Core.Infrastructure.Service.Pause
{
    public interface IPauseService : IPauseProvider
    {
        bool IsPause { get; }

        void SetPause(bool isPause);
    }
}
