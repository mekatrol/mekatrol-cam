using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace MekatrolCAM.Views;

public sealed class WindowService : IWindowService
{
    public Window? MainWindow =>
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

    public void CloseMain() =>
        Dispatcher.UIThread.Post(() => MainWindow?.Close());
}
