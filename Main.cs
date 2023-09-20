using Flow.Launcher.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.Godot
{
    public class Godot : IPlugin, ISettingProvider
    {
        private const string GodotFileExtension = ".godot";
        private const string GodotIconPath = "Assets\\Godot.png";
        private PluginInitContext _context;
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

        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();
            try
            {
                var godotProjectsDirectoryInfo = new DirectoryInfo(_settings.GodotProjectsPath);
                var projects = godotProjectsDirectoryInfo.GetDirectories();
                for (int i = 0; i < projects.Length; i++)
                {
                    var singleProject = projects[i].GetFiles();
                    singleProject.ToList().ForEach(delegate (FileInfo file)
                    {
                        if (file.FullName.EndsWith(GodotFileExtension))
                        {
                            results.Add(new Result
                            {
                                Title = file.Directory?.Name ?? "Path not specified",
                                IcoPath = GodotIconPath,
                                Action = x =>
                                {
                                    Process godotProc = new Process()
                                    {
                                        StartInfo = new ProcessStartInfo
                                        {
                                            FileName = _settings.GodotExecutablePath,
                                            Arguments = file.FullName,
                                            UseShellExecute = true,
                                            CreateNoWindow = true
                                        }
                                    };

                                    godotProc.Start();
                                    return true;
                                }
                            });
                        }
                    });
                }
            }
            catch (Exception)
            {
                _context.API.ShowMsgError("Invalid Paths", "Please ensure you are using the correct paths");
                _context.API.ShowMainWindow();
            }
            return results;
        }
    }
}