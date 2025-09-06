using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MekatrolCAM.Views;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;

namespace MekatrolCAM.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IWindowService _windowService;

    public ObservableCollection<TreeNode> Roots { get; } = [];

    public static string SelectedInfo => "Select an item from the tree.";

    public ReactiveCommand<Unit, Unit> ExitCommand { get; }

    public ReactiveCommand<Unit, Unit> FileOpenCommand { get; }

    public MainWindowViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Roots.Add(new TreeNode("Projects",
            new TreeNode("Alpha"),
            new TreeNode("Beta",
                new TreeNode("Docs"),
                new TreeNode("Src"))));

        Roots.Add(new TreeNode("Assets",
            new TreeNode("Images"),
            new TreeNode("Fonts")));

        ExitCommand = ReactiveCommand.Create(() =>
        {
            Dispatcher.UIThread.Post(() => _windowService.CloseMain());
        });

        FileOpenCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var win = _windowService.MainWindow!;
            var top = TopLevel.GetTopLevel(win)!;

            var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("All files"){ Patterns = ["*"] }
                    // new FilePickerFileType("GCode"){ Patterns = new[] { "*.gcode", "*.nc" } }
                ]
            });

            if (files.Count == 0)
            {
                return;
            }

            await using var stream = await files[0].OpenReadAsync();
            // use stream
        });
    }
}
