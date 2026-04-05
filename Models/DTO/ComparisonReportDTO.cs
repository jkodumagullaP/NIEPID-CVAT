namespace CAT.AID.Models.DTO
{
    public class ComparisonReportDTO
    {
        public string CandidateName { get; set; } = "";

        // existing list used in PDF
        public List<AssessmentDTO> Assessments { get; set; } = new();

        public List<ComparisonRowDTO> Rows { get; set; } = new();

        // ================= ADD BELOW =================

        public List<string> AssessmentDates { get; set; } = new();

        public List<int> AssessmentIds { get; set; } = new();

        public List<int> TotalScores { get; set; } = new();

        public List<ComparisonSectionDTO> Sections { get; set; } = new();
<<<<<<< HEAD
=======
    }

    // existing
    public class AssessmentDTO
    {
        public int Id { get; set; }   // ADD

        public int AssessmentId { get; set; }

        public DateTime AssessmentDate { get; set; }
        public DateTime CreatedAt { get; set; }   // ADD


        public int TotalScore { get; set; }
    }

    public class ComparisonRowDTO
    {
        public string Domain { get; set; } = "";   // ADD

        public string Section { get; set; } = "";

        public string QuestionText { get; set; } = "";   // FIX NAME


        public string Question { get; set; } = "";

        public List<string> Values { get; set; } = new();

        public List<int> Scores { get; set; } = new();
    }

    // ================= ADD THESE CLASSES =================

    public class ComparisonSectionDTO
    {
        public string SectionName { get; set; } = "";

        public List<ComparisonQuestionDTO> Questions { get; set; }
            = new();
    }

    public class ComparisonQuestionDTO
    {
        public string QuestionText { get; set; } = "";

        public List<string> Values { get; set; }
            = new();
>>>>>>> 7238502f206e40816d86a9383647a02217754152
    }

    // existing
    public class AssessmentDTO
    {
        public int Id { get; set; }   // ADD

        public int AssessmentId { get; set; }

        public DateTime AssessmentDate { get; set; }
        public DateTime CreatedAt { get; set; }   // ADD


        public int TotalScore { get; set; }
    }

    public class ComparisonRowDTO
    {
        public string Domain { get; set; } = "";   // ADD

        public string Section { get; set; } = "";

        public string QuestionText { get; set; } = "";   // FIX NAME


        public string Question { get; set; } = "";

        public List<string> Values { get; set; } = new();

        public List<int> Scores { get; set; } = new();
    }

    // ================= ADD THESE CLASSES =================

    public class ComparisonSectionDTO
    {
        public string SectionName { get; set; } = "";

        public List<ComparisonQuestionDTO> Questions { get; set; }
            = new();
    }

    public class ComparisonQuestionDTO
    {
        public string QuestionText { get; set; } = "";

        public List<string> Values { get; set; }
            = new();
    }
}