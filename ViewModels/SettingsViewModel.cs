namespace Flow.Launcher.Plugin.Godot.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly Settings _settings;
    private readonly PluginInitContext _context;
    private string _godotPath;
    
    public string GodotPath
    {
        get => _godotPath;
        set
        {
            if (SetProperty(ref _godotPath, value)) 
            {
                _settings.GodotPath = value;
                OnPropertyChanged();
                _context.API.SavePluginSettings();
            }
        }
    }
    
    private string _godotProjectsConfigPath;
    
    public string GodotProjectsConfigPath
    {
        get => _godotProjectsConfigPath;
        set
        {
            if (SetProperty(ref _godotProjectsConfigPath, value))
            {
                _settings.ProjectsConfigPath = value;
                OnPropertyChanged();
                _context.API.SavePluginSettings();
            }
        }
    } 
    
    private string _godotNewProjectsPath;

    public string GodotNewProjectsPath
    {
        get => _godotNewProjectsPath;
        set
        {
            if (SetProperty(ref _godotNewProjectsPath, value))
            {
                _settings.GodotNewProjectsPath = value;
                OnPropertyChanged();
                _context.API.SavePluginSettings();
            }
        }
    }
    
    
    public SettingsViewModel(PluginInitContext context, Settings settings)
    {
        _context = context;
        _settings = settings;
        GodotPath = _settings.GodotPath;
        GodotNewProjectsPath = _settings.GodotNewProjectsPath;
    }
}