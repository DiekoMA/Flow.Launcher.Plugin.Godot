namespace Flow.Launcher.Plugin.Godot;

public class GodotProject
{
    public string Version { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ProjectPath { get; set; }
    public bool Favorite { get; set; }
    public string IconPath { get; set; } // Added for project icon support
    public DateTime LastModified { get; set; } // Added for last modified support
}