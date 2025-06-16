global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.IO;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Windows;
global using CommunityToolkit.Mvvm.ComponentModel;
global using System.Windows.Controls;
global using Flow.Launcher.Plugin.Godot.ViewModels;
global using Flow.Launcher.Plugin.Godot.Views;
using System.Text;

namespace Flow.Launcher.Plugin.Godot
{
    public class Godot : IPlugin, ISettingProvider, IContextMenu
    {
        private const string GodotIconPath = "Assets\\Godot.png";
        private string ProjectsConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Godot", "projects.cfg");
        private PluginInitContext _context;
        private List<GodotProject> projects;
        private Settings _settings;
        private SettingsViewModel _viewModel;

        public Control CreateSettingPanel()
        {
            return new SettingsView(_viewModel);
        }

        public void Init(PluginInitContext context)
        {
            _context = context;
            _settings = context.API.LoadSettingJsonStorage<Settings>();
            _viewModel = new SettingsViewModel(_context, _settings);
        }

        /*private void CreateProject(string projectName)
        {
            var projectPath = Path.Combine(_settings.GodotNewProjectsPath);
            if (!Directory.Exists(projectPath))
            {
                Directory.CreateDirectory(projectPath);
            }
            
            var newProjectPath = Path.Combine(projectPath, projectName);
            if (Directory.Exists(newProjectPath))
            {
                _context.API.ShowMsgError($"Project '{projectName}' already exists in '{projectPath}'.");
                return;
            }

            var newProjectDirectoryInfo = Directory.CreateDirectory(newProjectPath);
            using StreamWriter projectFile = new StreamWriter(Path.Combine(newProjectPath, "project.godot"));
            var projectContent = new StringBuilder();
            # region Half asleep, will fix later with something else
            projectContent.AppendLine("; Engine configuration file.");
            projectContent.AppendLine("; It's best edited using the editor UI and not directly,");
            projectContent.AppendLine("; since the parameters that go here are not all obvious.");
            projectContent.AppendLine(";");
            projectContent.AppendLine("; Format:");
            projectContent.AppendLine(";   [section] ; section goes between []");
            projectContent.AppendLine(";   param=value ; assign values to parameters");
            projectContent.AppendLine("");
            projectContent.AppendLine("config_version=5");
            projectContent.AppendLine("");
            projectContent.AppendLine("[application]");
            projectContent.AppendLine("");
            projectContent.AppendLine($"config/name={projectName}");
            projectContent.AppendLine("config/features=PackedStringArray(\"4.4\", \"GL Compatibility\")");
            projectContent.AppendLine("config/icon=\"res://icon.svg\"");
            projectContent.AppendLine("");
            projectContent.AppendLine("[rendering]");
            projectContent.AppendLine("");
            projectContent.AppendLine("renderer/rendering_method=\"gl_compatibility\"");
            projectContent.AppendLine("renderer/rendering_method.mobile=\"gl_compatibility\"");
            projectFile.WriteAsync(projectContent.ToString());
            var godotIndexFile = File.AppendText(ProjectsConfigPath);
            godotIndexFile.WriteLine($"" +
                                     $"[{newProjectDirectoryInfo.FullName}]" +
                                     "favorite=false" +
                                     "");
            # endregion
        }*/
        
        private void GetProjects()
        {
            projects = new List<GodotProject>();
            string currentSection = null;
            using var reader = new StreamReader(ProjectsConfigPath);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.StartsWith("["))
                {
                    currentSection = line.Substring(1, line.Length - 2);
                }
                else if (!string.IsNullOrEmpty(currentSection) && line.Contains("="))
                {
                    var parts = line.Split('=');
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    var projectInfo = ParseProjectFile(currentSection);
                    string iconPath = projectInfo.IconPath ?? GodotIconPath;
                    projects.Add(new GodotProject
                    {
                        ProjectPath = currentSection,
                        Name = projectInfo.Name ?? Path.GetFileName(currentSection),
                        IconPath = iconPath,
                        Favorite = bool.Parse(value),
                        LastModified = GetProjectLastModified(currentSection)
                    });
                }
            }
        }

        private (string Name, string IconPath) ParseProjectFile(string projectPath)
        {
            string projectFilePath = Path.Combine(projectPath, "project.godot");
            string projectName = null;
            string iconPath = null;

            if (!File.Exists(projectFilePath))
            {
                return (null, null);
            }

            try
            {
                string currentSection = null;
                using var reader = new StreamReader(projectFilePath);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (line.StartsWith("["))
                    {
                        currentSection = line.Substring(1, line.Length - 2);
                    }
                    else if (currentSection == "application" && line.Contains("="))
                    {
                        var parts = line.Split('=', 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim();
                            var value = parts[1].Trim().Trim('"');

                            if (key == "config/name")
                            {
                                projectName = value;
                            }
                            else if (key == "config/icon")
                            {
                                // Convert Godot resource path to actual file path
                                if (value.StartsWith("res://"))
                                {
                                    string relativePath = value.Substring(6);
                                    string fullIconPath = Path.Combine(projectPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
                                    if (File.Exists(fullIconPath) && IsSupportedIconFormat(fullIconPath))
                                    {
                                        iconPath = fullIconPath;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore parse errors
            }
            return (projectName, iconPath);
        }

        private bool IsSupportedIconFormat(string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath))
                return false;

            string extension = Path.GetExtension(iconPath).ToLowerInvariant();
            return extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".bmp";
        }

        private DateTime GetProjectLastModified(string projectPath)
        {
            string projectFilePath = Path.Combine(projectPath, "project.godot");

            try
            {
                if (File.Exists(projectFilePath))
                {
                    return File.GetLastWriteTime(projectFilePath);
                }
            }
            catch (Exception)
            {
                // Ignore errors
            }

            return DateTime.MinValue;
        }

        public List<Result> LoadContextMenus(Result selectedResult)
        {
            // Use the icon from the selected result, fallback to plugin icon if not available
            string iconPath = selectedResult.IcoPath ?? GodotIconPath;

            List<Result> contextResults = new List<Result>
            {
                new ()
                {
                    Title = "Open containing folder",
                    IcoPath = iconPath,
                    Action = x =>
                    {
                        Process explorerProc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = selectedResult.SubTitle,
                                Verb = "open",
                                CreateNoWindow = true,
                                UseShellExecute = true
                            }
                        };
                        explorerProc.Start();
                        return true;
                    },
                },

                new ()
                {
                    Title = "Copy Path",
                    IcoPath = iconPath,
                    Action = x =>
                    {
                        Clipboard.SetText(selectedResult.SubTitle);
                        return true;
                    },
                },

                new ()
                {
                    Title = "Open with shell",
                    IcoPath = iconPath,
                    Action = x =>
                    {
                        Process shellProc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "CMD.exe",
                                WorkingDirectory = selectedResult.SubTitle,
                                CreateNoWindow = false,
                                UseShellExecute = true
                            }
                        };
                        shellProc.Start();
                        return true;
                    },
                },
            };
            return contextResults;
        }

        public List<Result> Query(Query query)
        {
            GetProjects();
            var results = new List<Result>();
            try
            {
                // Filter projects based on search query
                var filteredProjects = projects.Where(project =>
                {
                    if (string.IsNullOrWhiteSpace(query.Search))
                        return true;

                    string searchTerm = query.Search.ToLowerInvariant();
                    return project.Name?.ToLowerInvariant().Contains(searchTerm) == true ||
                           Path.GetFileName(project.ProjectPath).ToLowerInvariant().Contains(searchTerm);
                })
                .OrderByDescending(project => project.Favorite) // Favorites first
                .ThenByDescending(project => project.LastModified) // Then by most recently modified
                .ThenBy(project => project.Name); // Then by name alphabetically

                foreach (var project in filteredProjects)
                {
                    var result = new Result();
                    result.Title = project.Favorite ? $"{project.Name} \u2665" : project.Name;
                    result.SubTitle = project.ProjectPath;
                    result.IcoPath = project.IconPath ?? GodotIconPath;

                    // Calculate score for better ranking
                    if (!string.IsNullOrWhiteSpace(query.Search))
                    {
                        string searchTerm = query.Search.ToLowerInvariant();
                        string projectName = project.Name?.ToLowerInvariant() ?? "";

                        if (projectName.StartsWith(searchTerm))
                            result.Score = 100; // Exact prefix match gets highest score
                        else if (projectName.Contains(searchTerm))
                            result.Score = 80; // Contains match gets high score
                        else
                            result.Score = 60; // Other matches get lower score

                        // Boost score for favorites
                        if (project.Favorite)
                            result.Score += 20;
                    }
                    else
                    {
                        // When no search query, favorites get higher score
                        result.Score = project.Favorite ? 100 : 80;
                    }

                    result.Action = x =>
                    {
                        var projectPath = Path.GetFullPath(project.ProjectPath);
                        var projectFile = Path.Combine(projectPath, "project.godot");
                        var godotProc = new Process()
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = _settings.GodotPath,
                                Arguments =  $"--editor \"{projectFile}\"",
                                UseShellExecute = true,
                                CreateNoWindow = true
                            }
                        };
                        godotProc.Start();
                        return true;
                    };
                    results.Add(result);
                }
            }
            catch (Exception ex)
            {
                _context.API.ShowMsgError("Error reading config file: " + ex.Message);
                throw;
            }
            
            # region For Later
            /*if (query.Search.Length < 2)
            {
                results.Add(new Result()
                {
                    Title = "gd create <project name>",
                    SubTitle = string.IsNullOrEmpty(_settings.GodotNewProjectsPath) ? "You need to set a path in the settings" : "Creates a new project in the project folder assigned in the settings.",
                });
                results.Add(new Result()
                {
                    Title = "gd list <project name>",
                    SubTitle = "Lists all Godot projects in the configured projects folder.",
                });
            }
            else
            {
                switch (query.Search.ToLower())
                {
                    case "create": 
                        results.Add(new Result()
                        {
                            Title = query.ThirdSearch,
                            SubTitle = string.IsNullOrEmpty(_settings.GodotNewProjectsPath) ? "You need to set a path in the settings" : "Creates a new project in the project folder assigned in the settings.",
                            Action = c =>
                            {
                                if (string.IsNullOrEmpty(_settings.GodotNewProjectsPath))
                                {
                                    _context.API.OpenSettingDialog();
                                }
                                CreateProject(query.ThirdSearch);
                                return true;
                            }
                        });
                        break;
                    
                    case "list":
                        break;
                }
            }*/
            # endregion
            return results;
        }
    }
}
