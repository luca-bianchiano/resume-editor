using Newtonsoft.Json;
using ResumEditor.Services;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Document.NET;
using Xceed.Words.NET;
using IOPath = System.IO.Path;

namespace ResumEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public UserProfile Author { get; private set; } = null!;
        private string templatesFolder = "Templates"; // relative to app root

        public MainWindow()
        {
            InitializeComponent();
            LoadAuthorInfo();
            LoadTemplates();
            this.DataContext = this;
        }
        private void LoadTemplates()
        {
            TemplateDropdown.Items.Clear();

            if (!Directory.Exists(templatesFolder))
            {
                MessageBox.Show("Templates folder not found!");
                return;
            }

            // Get all .docx or template files
            var templateFiles = Directory.GetFiles(templatesFolder, "*.docx");

            foreach (var file in templateFiles)
            {
                TemplateDropdown.Items.Add(IOPath.GetFileName(file));
            }

            if (TemplateDropdown.Items.Count > 0)
                TemplateDropdown.SelectedIndex = 0; // select first template by default
        }

        private void TemplateDropdown_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedTemplate = TemplateDropdown.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedTemplate))
            {
                // TODO: Store the selected template path for later use
                string fullPath = IOPath.Combine(templatesFolder, selectedTemplate);
                Console.WriteLine($"Selected Template: {fullPath}");
            }
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

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Get the selected template
            string? selectedTemplate = TemplateDropdown.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTemplate))
            {
                MessageBox.Show("Please select a template.");
                return;
            }

            string templatePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", selectedTemplate);
            if (!File.Exists(templatePath))
            {
                MessageBox.Show("Template file not found!");
                return;
            }

            // 2. Parse the resume input
            var resumeText = ResumeInput.Text; // assuming you have a TextBox for input
            var resumeData = ResumeParser.ParseDynamicResume(resumeText);

            // 3. Merge resumeData into template (example using DocX)
            string outputFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            string outputFile = System.IO.Path.Combine(outputFolder, "Resume.docx");

            using (var doc = DocX.Load(templatePath))
            {
                doc.ReplaceText(new StringReplaceTextOptions { SearchValue = "{name}", NewValue = Author.Name ?? "" });
                doc.ReplaceText(new StringReplaceTextOptions { SearchValue = "{title}", NewValue = Author.Title ?? "" });
                doc.ReplaceText(new StringReplaceTextOptions { SearchValue = "{linkedin}", NewValue = Author.LinkedIn ?? "" });
                doc.ReplaceText(new StringReplaceTextOptions { SearchValue = "{email}", NewValue = Author.Email ?? "" });
                doc.ReplaceText(new StringReplaceTextOptions { SearchValue = "{phone}", NewValue = Author.Phone ?? "" });
                // --- Save final file ---
                doc.SaveAs(outputFile);
            }


            MessageBox.Show($"Resume generated successfully:\n{outputFile}");
        }
    }
}