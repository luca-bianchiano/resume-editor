using System;
using System.Collections.Generic;

namespace ResumEditor.Services
{
    // Represents the dynamic content of a resume (from ChatGPT)
    public class DynamicResume
    {
        public string Summary { get; set; } = "";
        public SkillsSection Skills { get; set; } = new();
        public List<Experience> Experience { get; set; } = new();
        public List<string> Education { get; set; } = new();
        public List<string> Projects { get; set; } = new();
    }

    public class SkillsSection
    {
        public Dictionary<string, List<string>> Categories { get; set; } = new();
    }

    public class Experience
    {
        public string Role { get; set; } = "";
        public string Company { get; set; } = "";
        public string Dates { get; set; } = "";
        public List<string> Achievements { get; set; } = new();
    }
}
