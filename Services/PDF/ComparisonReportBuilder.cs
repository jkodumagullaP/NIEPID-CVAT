using CAT.AID.Models;
using CAT.AID.Models.DTO;
using System.Text.Json;

namespace CAT.AID.Web.Services.PDF
{
    public static class ComparisonReportBuilder
    {
        public static ComparisonReportDTO Build(List<Assessment> assessments)
        {
            var dto = new ComparisonReportDTO();

            // ================= Candidate =================

            dto.CandidateName =
                assessments.First().Candidate.FullName;

            // ================= Assessment Summary =================

            foreach (var a in assessments)
            {
                var score =
                    string.IsNullOrWhiteSpace(a.ScoreJson)
                    ? new AssessmentScoreDTO()
                    : JsonSerializer.Deserialize<AssessmentScoreDTO>(a.ScoreJson);

                dto.Assessments.Add(new AssessmentDTO
                {
                    Id = a.Id,                     // needed by PDF

                    AssessmentId = a.Id,

                    AssessmentDate = a.CreatedAt,

                    CreatedAt = a.CreatedAt,

                    TotalScore =
                        Convert.ToInt32(score?.TotalScore ?? 0)
                });
            }

            // ================= Questions =================

            var sections =
                JsonSerializer.Deserialize<List<AssessmentSection>>(
                    File.ReadAllText(
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "data",
                            "assessment_questions.json")))
                ?? new();

            foreach (var sec in sections)
            {
                foreach (var q in sec.Questions)
                {
                    var row = new ComparisonRowDTO
                    {
                        // Domain not available → use Category
                        Domain = sec.Category,

                        Section = sec.Category,

                        QuestionText = q.Text
                    };

                    foreach (var a in assessments)
                    {
                        var answers =
                            string.IsNullOrWhiteSpace(a.AssessmentResultJson)
                            ? new Dictionary<string, string>()
                            : JsonSerializer.Deserialize
                                <Dictionary<string, string>>(a.AssessmentResultJson);

                        var ansKey = $"ANS_{q.Id}";
                        var scoreKey = $"SCORE_{q.Id}";

                        // Answer text
                        if (answers != null &&
                            answers.ContainsKey(ansKey))
                        {
                            row.Values.Add(answers[ansKey]);
                        }
                        else
                        {
                            row.Values.Add("-");
                        }

                        // Score numeric
                        if (answers != null &&
                            answers.ContainsKey(scoreKey) &&
                            int.TryParse(answers[scoreKey], out int s))
                        {
                            row.Scores.Add(s);
                        }
                        else
                        {
                            row.Scores.Add(0);
                        }
                    }

                    dto.Rows.Add(row);
                }
            }

            return dto;
        }
    }
}