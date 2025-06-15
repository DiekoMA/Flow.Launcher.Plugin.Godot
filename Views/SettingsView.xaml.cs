using System.Windows.Controls;
using Flow.Launcher.Plugin.Godot.ViewModels;

namespace Flow.Launcher.Plugin.Godot.Views;

public partial class SettingsView : UserControl
{
    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}