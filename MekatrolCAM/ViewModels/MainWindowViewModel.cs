using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Mekatrol.CAM.Core.Data;
using Mekatrol.CAM.Core.Parsers.Svg;
using MekatrolCAM.Views;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

namespace MekatrolCAM.ViewModels;

public class MainWindowViewModel : ViewModelBase, IActivatableViewModel
{
    private static readonly DataSnapshot Empty = new([]);

    private readonly IWindowService _windowService;

    private ObservableAsPropertyHelper<DataSnapshot> _data;

    public DataSnapshot Data => _data.Value;

    public ViewModelActivator Activator { get; } = new();

    public ObservableCollection<TreeNode> Roots { get; } = [];

    public ReactiveCommand<Unit, Unit> ExitCommand { get; }

    public ReactiveCommand<Unit, Unit> FileOpenCommand { get; }

    public MainWindowViewModel(IWindowService windowService, ISvgParser svgParser, IDataStore store)
    {
        _windowService = windowService;

        store.Snapshot
             .StartWith(Empty)
             .ObserveOn(RxApp.MainThreadScheduler)
             .ToProperty(this, vm => vm.Data, out _data, initialValue: Empty);

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
                    new FilePickerFileType("SVG files"){ Patterns = ["*.svg"] },
                    new FilePickerFileType("All files"){ Patterns = ["*"] }
                ]
            });

            if (files.Count == 0)
            {
                return;
            }

            await using var stream = await files[0].OpenReadAsync();
            using var reader = new StreamReader(
                stream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 8192,
                leaveOpen: false
            );

            var entities = svgParser.Parse(reader, true);
            await store.UpdateDataAsync(entities!);
        });
    }
}
