using Avalonia.Controls;

namespace MekatrolCAM.Views;

public interface IWindowService
{
    Window? MainWindow { get; }
    void CloseMain();
}
