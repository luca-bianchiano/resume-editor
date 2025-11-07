using ResumEditor.Services;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ResumEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public UserProfile Author { get; private set; } = null!;

        public MainWindow()
        {
            InitializeComponent();
            LoadAuthorInfo();
        }

        private void LoadAuthorInfo()
        {
            string userConfigPath = "Config/user.json";

            if (!File.Exists(userConfigPath))
            {
                MessageBox.Show("User config not found!");
                return;
            }

            string json = File.ReadAllText(userConfigPath, System.Text.Encoding.UTF8);

            var loaded = Newtonsoft.Json.JsonConvert.DeserializeObject<UserProfile>(json);
            if (loaded == null)
            {
                MessageBox.Show("Failed to parse user.json!");
                return;
            }

            Author = loaded;

            MessageBox.Show($"Loaded Author: {Author.Name}, {Author.Title}, {Author.Location}");
        }
    }
}