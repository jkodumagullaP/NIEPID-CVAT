using System.Collections.Generic;

namespace CAT.AID.Models.DTO
{
    public class AssessmentSection
    {
        // REQUIRED because Razor uses sec.Id
        public int Id { get; set; }

        // Section name
        public string Category { get; set; } = string.Empty;

        // JSON-driven questions
        public List<AssessmentQuestion> Questions { get; set; } = new();

        // Default max score per question
        public int MaxScore { get; set; } = 3;
    }


    public class AssessmentQuestion
    {
        // REQUIRED because Razor uses q.Id
        public int Id { get; set; }

        // Question text
        public string Text { get; set; } = string.Empty;

        // Options for radio / checkbox
        public List<string> Options { get; set; } = new();

        // Correct answer (optional if scoring logic used)
        public string Correct { get; set; } = string.Empty;

        // Score weight per question
        public int ScoreWeight { get; set; } = 1;
    }
}
