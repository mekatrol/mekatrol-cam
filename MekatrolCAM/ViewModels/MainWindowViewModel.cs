using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;

namespace MekatrolCAM.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<TreeNode> Roots { get; } = [];

    public static string SelectedInfo => "Select an item from the tree.";

    public ReactiveCommand<Unit, Unit> ExitCommand { get; }

    public MainWindowViewModel()
    {
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
            // optional: close via message bus or window service
        });
    }
}
