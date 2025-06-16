namespace Flow.Launcher.Plugin.Godot.ViewModels;

public class CustomPreviewPanelViewModel : ObservableObject
{
    private GodotProject _selectedGodotProject;

    public GodotProject SelectedGodotProject
    {
        get => _selectedGodotProject;
        set
        {
            if (SetProperty(ref _selectedGodotProject, value))
            {
                OnPropertyChanged();
            }
        }
    }
    public CustomPreviewPanelViewModel(GodotProject project)
    {
        SelectedGodotProject = project;
    }
}