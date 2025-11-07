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
            this.DataContext = this;
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

        }
        private void ParseResume_Click(object sender, RoutedEventArgs e)
        {
            string rawText = ResumeInput.Text;
            var dynamicResume = ResumeParser.ParseDynamicResume(rawText);

            // Preview structured output
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(dynamicResume.Summary))
                sb.AppendLine("Summary: " + dynamicResume.Summary + "\n");

            sb.AppendLine("Skills:");
            foreach (var cat in dynamicResume.Skills.Categories)
                sb.AppendLine($"{cat.Key}: {string.Join(", ", cat.Value)}");
            sb.AppendLine();

            sb.AppendLine("Experience:");
            foreach (var exp in dynamicResume.Experience)
            {
                sb.AppendLine($"{exp.Role} at {exp.Company} ({exp.Dates})");
                foreach (var ach in exp.Achievements)
                    sb.AppendLine($" - {ach}");
            }
            sb.AppendLine();

            if (dynamicResume.Education.Count > 0)
            {
                sb.AppendLine("Education:");
                foreach (var edu in dynamicResume.Education)
                    sb.AppendLine(" - " + edu);
                sb.AppendLine();
            }

            if (dynamicResume.Projects.Count > 0)
            {
                sb.AppendLine("Projects:");
                foreach (var proj in dynamicResume.Projects)
                    sb.AppendLine(" - " + proj);
            }

            ParseResult.Text = sb.ToString();
        }
    }
}