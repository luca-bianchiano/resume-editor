using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ResumEditor.Services
{
    public static class ResumeParser
    {
        // Flexible section extractor using regex (ignores spaces & case)
        public static string ExtractSection(string text, string tag)
        {
            var pattern = $@"\{{\s*{tag}\s*\}}(.*?)\{{\s*:{tag}\s*\}}";
            var match = Regex.Match(text, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        // Flexible skills parser
        public static SkillsSection ParseSkills(string skillsText)
        {
            var skills = new SkillsSection();

            foreach (var line in skillsText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Contains(":"))
                {
                    var parts = line.Split(':', 2);
                    string category = parts[0].Trim();
                    var items = parts[1].Split(',')
                                        .Select(s => s.Trim())
                                        .Where(s => !string.IsNullOrEmpty(s))
                                        .ToList();
                    skills.Categories[category] = items;
                }
            }

            return skills;
        }

        // Flexible experience parser
        public static List<Experience> ParseExperience(string text)
        {
            var experiences = new List<Experience>();
            Experience current = null;
            bool inAchievement = false;

            foreach (var line in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                // Start achievements block
                if (Regex.IsMatch(trimmed, @"^\{achievement\}", RegexOptions.IgnoreCase))
                {
                    inAchievement = true;
                    current.Achievements = new List<string>();
                    continue;
                }

                // End achievements block
                if (Regex.IsMatch(trimmed, @"^\{\/?:achievement\}", RegexOptions.IgnoreCase))
                {
                    inAchievement = false;
                    continue;
                }

                // Add achievement line
                if (inAchievement && (trimmed.StartsWith("-") || trimmed.StartsWith("–")))
                {
                    current.Achievements.Add(trimmed.Substring(1).Trim());
                    continue;
                }

                // Match flexible experience line: optional numbering, optional role
                // Examples it can match:
                // 1. Senior Staff Machine Learning Engineer, [Buynomics], (2021 - Present)
                // 1. [Buynomics], (2021 - Present)
                // , at Buynomics (2021 - Present)
                var expPattern = @"^(?:\d+\.\s*)?(?:(.*?),\s*)?\[(.*?)\],\s*\((.*?)\)|, at (.*?) \((.*?)\)";
                var match = Regex.Match(trimmed, expPattern);
                if (match.Success)
                {
                    if (current != null)
                        experiences.Add(current);

                    current = new Experience();
                    // Check which group matched
                    if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        current.Role = match.Groups[1].Value?.Trim() ?? "";
                        current.Company = match.Groups[2].Value.Trim();
                        current.Dates = match.Groups[3].Value.Trim();
                    }
                    else
                    {
                        current.Role = "";
                        current.Company = match.Groups[4].Value.Trim();
                        current.Dates = match.Groups[5].Value.Trim();
                    }
                }
            }

            if (current != null)
                experiences.Add(current);

            return experiences;
        }

        // Flexible list parser (Education / Projects)
        public static List<string> ParseListSection(string text)
        {
            return text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.TrimStart('-', '–', ' ').Trim())
                       .Where(s => !string.IsNullOrEmpty(s))
                       .ToList();
        }

        // Master parser
        public static DynamicResume ParseDynamicResume(string text)
        {
            var resume = new DynamicResume
            {
                Summary = ExtractSection(text, "summary"),
                Skills = ParseSkills(ExtractSection(text, "skills")),
                Experience = ParseExperience(ExtractSection(text, "experience")),
                Education = ParseListSection(ExtractSection(text, "education")),
                Projects = ParseListSection(ExtractSection(text, "projects"))
            };

            return resume;
        }
    }
}
