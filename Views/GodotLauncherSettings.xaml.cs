using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Flow.Launcher.Plugin.Godot
{
    /// <summary>
    /// Interaction logic for GodotLauncherSettings.xaml
    /// </summary>
    public partial class GodotLauncherSettings : UserControl
    {
        private readonly Settings _settings;
        private readonly PluginInitContext _context;
        public GodotLauncherSettings(PluginInitContext context, Settings settings)
        {
            InitializeComponent();
            _context = context;
            _settings = settings;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GodotDirectoryTextBox.Text = _settings.GodotExecutablePath ?? "Not Set";
            GodotProjectsDirectoryTextBox.Text = _settings.GodotProjectsPath ?? "Not Set";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _settings.GodotExecutablePath = GodotDirectoryTextBox.Text;
            _settings.GodotProjectsPath = GodotProjectsDirectoryTextBox.Text;
            _context.API.SaveSettingJsonStorage<Settings>();
        }
    }
}
