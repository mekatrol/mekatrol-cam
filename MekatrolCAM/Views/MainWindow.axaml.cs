using Avalonia.Controls;
using Mekatrol.CAM.Core.Data;
using Mekatrol.CAM.Core.Parsers.Svg;
using MekatrolCAM.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;

namespace MekatrolCAM.Views;

public partial class MainWindow : Window
{
    // Required for runtime loader / designer
    public MainWindow()
    {
        InitializeComponent();

        WindowState = WindowState.Maximized;

        if (Design.IsDesignMode)
        {
            DataContext = new MainWindowViewModel(new WindowService(), new SvgParser(NullLogger<SvgParser>.Instance), new DataStore());
        }
    }

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        WindowState = WindowState.Maximized;
    }
}
