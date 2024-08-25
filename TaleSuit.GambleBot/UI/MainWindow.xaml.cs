using AdonisUI.Controls;
using TaleSuit.GambleBot.Context;

namespace TaleSuit.GambleBot.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : AdonisWindow
{
    public MainWindow()
    {
        DataContext = new MainWindowContext();
        InitializeComponent();
    }
}