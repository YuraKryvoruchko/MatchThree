namespace Core.Infrastructure.UI
{
    public interface IWindow
    {
        string Path { get; }
        bool IsPopup { get; }

        void Show();
        void Hide();
        void Focus();
        void Unfocus();

        void Push();
        void Back();
    }
}
