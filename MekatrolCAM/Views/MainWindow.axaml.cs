using Avalonia.Controls;
using MekatrolCAM.ViewModels;

namespace MekatrolCAM.Views;

public partial class MainWindow : Window
{
    // Required for runtime loader / designer
    public MainWindow()               
    {
        InitializeComponent();
        if (Design.IsDesignMode)
        {
            DataContext = new MainWindowViewModel(new WindowService());
        }
    }


    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }    
}
