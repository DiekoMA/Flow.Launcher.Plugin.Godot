namespace Flow.Launcher.Plugin.Godot.Views;

public partial class CustomPreviewPanelView : UserControl
{
    public CustomPreviewPanelView(CustomPreviewPanelViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}