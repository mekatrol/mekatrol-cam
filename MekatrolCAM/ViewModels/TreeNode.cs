using System.Collections.ObjectModel;

namespace MekatrolCAM.ViewModels;

public class TreeNode
{
    public string Name { get; }
    public ObservableCollection<TreeNode> Children { get; } = [];
    public TreeNode(string name, params TreeNode[] children)
    {
        Name = name;
        foreach (var c in children)
        {
            Children.Add(c);
        }
    }
}
