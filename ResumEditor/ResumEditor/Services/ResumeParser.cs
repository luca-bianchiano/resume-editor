using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ResumEditor.Services
{

    public static class ResumeParser
    {
        // Extract a section with tags, fully adaptive
        public static string ExtractSection(string text, string tag)
        {
            var pattern = $@"\{{\s*{tag}\s*\}}(.*?)\{{\s*:{tag}\s*\}}";
            var match = Regex.Match(text, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        // Parse Skills section, fully adaptive
        public static SkillsSection ParseSkills(string skillsText)
        {
            var skills = new SkillsSection();
            foreach (var line in skillsText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                if (!trimmed.Contains(":")) continue;

                var parts = trimmed.Split(new[] { ':' }, 2);
                var category = parts[0].Trim();
                var items = parts[1].Split(',')
                                    .Select(s => s.Trim())
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .ToList();
                skills.Categories[category] = items;
            }
            return skills;
        }

        // Parse experience section, fully adaptive
        public static List<Experience> ParseExperience(string text)
        {
            var experiences = new List<Experience>();
            Experience? current = null;
            bool inAchievement = false;

            foreach (var line in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                // Achievement start
                if (Regex.IsMatch(trimmed, @"^\{achievement\}", RegexOptions.IgnoreCase))
                {
                    inAchievement = true;
                    if (current != null && current.Achievements == null)
                        current.Achievements = new List<string>();
                    continue;
                }

                // Achievement end
                if (Regex.IsMatch(trimmed, @"^\{\/?:achievement\}", RegexOptions.IgnoreCase))
                {
                    inAchievement = false;
                    continue;
                }

                // Achievement line
                if (inAchievement)
                {
                    var achievement = trimmed.TrimStart('-', '–', ' ').Trim();
                    if (!string.IsNullOrEmpty(achievement) && current != null)
                        current.Achievements.Add(achievement);
                    continue;
                }

                // Detect experience line
                var expPattern = @"^(?:\d+\.\s*)?(.*?)(?:,?\s*\[([^\]]+)\])?\s*(?:\(([^)]+)\))?$|, at (.*?) \((.*?)\)";
                var match = Regex.Match(trimmed, expPattern);
                if (match.Success)
                {
                    if (current != null)
                        experiences.Add(current);

                    current = new Experience();

                    if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        current.Role = match.Groups[1].Value?.Trim() ?? "";
                        current.Company = match.Groups[2].Value.Trim();
                        current.Dates = match.Groups[3].Value.Trim();
                    }
                    else if (!string.IsNullOrEmpty(match.Groups[4].Value))
                    {
                        current.Role = match.Groups[1].Value?.Trim() ?? "";
                        current.Company = match.Groups[4].Value.Trim();
                        current.Dates = match.Groups[5].Value.Trim();
                    }
                    else
                    {
                        // Fallback: entire line as role if parsing fails
                        current.Role = trimmed;
                        current.Company = "";
                        current.Dates = "";
                    }
                }
            }

            if (current != null)
                experiences.Add(current);

            return experiences;
        }

        // Parse list sections (Education / Projects)
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
            return new DynamicResume
            {
                Summary = ExtractSection(text, "summary"),
                Skills = ParseSkills(ExtractSection(text, "skills")),
                Experience = ParseExperience(ExtractSection(text, "experience")),
                Education = ParseListSection(ExtractSection(text, "education")),
                Projects = ParseListSection(ExtractSection(text, "projects"))
            };
        }
    }
}
