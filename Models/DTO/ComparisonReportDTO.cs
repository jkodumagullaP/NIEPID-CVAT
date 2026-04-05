namespace CAT.AID.Models.DTO
{
    public class ComparisonReportDTO
    {
        public string CandidateName { get; set; } = "";

        public List<AssessmentDTO> Assessments { get; set; } = new();

        public List<ComparisonRowDTO> Rows { get; set; } = new();

        public List<string> AssessmentDates { get; set; } = new();

        public List<int> AssessmentIds { get; set; } = new();

        public List<int> TotalScores { get; set; } = new();

        public List<ComparisonSectionDTO> Sections { get; set; } = new();

    //    public PdfSignatureDTO Signature { get; set; } = new();
    }

    public class AssessmentDTO
    {
        public int Id { get; set; }

        public int AssessmentId { get; set; }

        public DateTime AssessmentDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public int TotalScore { get; set; }
    }

    public class ComparisonRowDTO
    {
        public string Domain { get; set; } = "";

        public string Section { get; set; } = "";

        public string QuestionText { get; set; } = "";

        public string Question { get; set; } = "";

        public List<string> Values { get; set; } = new();

        public List<int> Scores { get; set; } = new();
    }

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
