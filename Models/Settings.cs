namespace Flow.Launcher.Plugin.Godot;

public class Settings
{
    public string GodotPath{ get; set; }
    public string GodotNewProjectsPath{ get; set; }

    /// <summary>
    /// Gets or sets the path to the Godot projects configuration file.
    /// By default, this is set to the "projects.cfg" file located in the
    /// user's application data directory under the "Godot" folder.
    ///
    /// Some installation methods, like Steam, may place this file
    /// in a different location.
    /// </summary>
    public string ProjectsConfigPath { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
        "Godot",
        "projects.cfg"
    );
}
