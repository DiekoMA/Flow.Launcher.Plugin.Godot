using Flow.Launcher.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.Godot
{
    public class Godot : IPlugin, ISettingProvider, IContextMenu
    {
        private const string GodotIconPath = "Assets\\Godot.png";
        private PluginInitContext _context;
        private List<GodotProject> projects;
        private Settings _settings;

        public Control CreateSettingPanel()
        {
            return new GodotLauncherSettings(_context, _settings);
        }

        public void Init(PluginInitContext context)
        {
            _context = context;
            _settings = context.API.LoadSettingJsonStorage<Settings>();
        }

        private void GetProjects()
        {
            projects = new List<GodotProject>();
            string currentSection = null;
            using StreamReader reader = new StreamReader(_settings.ProjectsConfigPath);
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
                    string[] parts = line.Split('=');
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    projects.Add(new GodotProject
                    {
                        ProjectPath = currentSection,
                        Name = Path.GetFileName(currentSection),
                        Favorite = bool.Parse(value)
                    });

                 
                }
            }
        }

        public List<Result> LoadContextMenus(Result selectedResult)
        {
            List<Result> contextResults = new List<Result>
            {
                new ()
                {
                    Title = "Open containing folder",
                    IcoPath = GodotIconPath,
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
                    IcoPath = GodotIconPath,
                    Action = x =>
                    {
                        Clipboard.SetText(selectedResult.SubTitle);
                        return true;
                    },
                },

                new ()
                {
                    Title = "Open with shell",
                    IcoPath = GodotIconPath,
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
            List<Result> results = new List<Result>();
            try
            {
                foreach (var project in projects)
                {
                    var result = new Result();
                    result.Title = project.Favorite ? $"{project.Name} \u2665" : project.Name;
                    result.SubTitle = project.ProjectPath;
                    result.IcoPath = GodotIconPath;
                    result.Action = x =>
                    {
                        Process godotProc = new Process()
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = _settings.GodotExecutablePath,
                                Arguments = Path.Combine(Path.GetFullPath(project.ProjectPath), "project.godot"),
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
            return results;
        }
    }
}

public class GodotProject
{
    public string Name { get; set; }
    public string ProjectPath { get; set; }
    public bool Favorite { get; set; }
}
public class Settings
{
    public string GodotExecutablePath { get; set; } = string.Empty;
    public string ProjectsConfigPath { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Godot",
        "projects.cfg"
    );
}