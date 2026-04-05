using CAT.AID.Models.DTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CAT.AID.Web.Services.PDF
{
    public class ComparisonPdfDocument : IDocument
    {
        private readonly ComparisonReportDTO _model;

        public ComparisonPdfDocument(ComparisonReportDTO model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata()
            => DocumentMetadata.Default;

        private string ScoreText(int? score)
        {
            return score switch
            {
                3 => "Independent",
                2 => "Verbal Prompt",
                1 => "Physical Prompt",
                0 => "Dependent",
                _ => "-"
            };
        }

        private string DiffText(int? diff)
        {
            if (!diff.HasValue)
                return "-";

            if (diff > 0)
                return $"Improved (+{diff})";

            if (diff < 0)
                return $"Reduced ({diff})";

            return "No Change";
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);

                page.Margin(25);

                page.DefaultTextStyle(x =>
                    x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("NIEPID – CVAT Assessment Report")
                        .FontSize(16)
                        .Bold()
                        .AlignCenter();

                    col.Item().Text($"Candidate: {_model.CandidateName}")
                        .FontSize(12)
                        .Bold();

                    col.Item().Text($"Assessments Compared: {string.Join(", ", _model.Assessments.Select(x => x.Id))}");
                });

                page.Content().Column(col =>
                {
                    col.Spacing(12);

                    var grouped =
                        _model.Rows
                        .GroupBy(x => x.Domain);

                    foreach (var domain in grouped)
                    {
                        col.Item().Border(1).Padding(8).Column(section =>
                        {
                            section.Item()
                                .Text(domain.Key)
                                .FontSize(13)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            section.Item().Table(table =>
                            {
                                int columns =
                                    2 + _model.Assessments.Count;

                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(4);

                                    foreach (var a in _model.Assessments)
                                        c.RelativeColumn(2);
                                });

                                // HEADER

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Section").Bold();

                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Question").Bold();

                                    foreach (var a in _model.Assessments)
                                    {
                                        header.Cell()
                                            .Background(Colors.Grey.Lighten2)
                                            .Padding(4)
                                            .Text(a.CreatedAt.ToString("dd-MMM-yyyy"))
                                            .Bold()
                                            .AlignCenter();
                                    }
                                });

                                // ROWS

                                foreach (var row in domain)
                                {
                                    table.Cell().Padding(3).Text(row.Domain);

                                    table.Cell().Padding(3).Text(row.QuestionText);

                                    foreach (var score in row.Scores)
                                    {
                                        table.Cell()
                                            .Padding(3)
                                            .AlignCenter()
                                            .Text(ScoreText(score));
                                    }
                                }
                            });
                        });
                    }

                    // TOTAL SUMMARY

                    col.Item().PaddingTop(10).BorderTop(1).Column(summary =>
                    {
                        summary.Item()
                            .Text("Total Score Summary")
                            .FontSize(13)
                            .Bold();

                        summary.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                foreach (var a in _model.Assessments)
                                    c.RelativeColumn();
                            });

                            t.Header(h =>
                            {
                                foreach (var a in _model.Assessments)
                                {
                                    h.Cell()
                                        .Background(Colors.Grey.Lighten2)
                                        .Padding(4)
                                        .Text(a.CreatedAt.ToString("dd-MMM-yyyy"))
                                        .Bold();
                                }
                            });

                            t.Cell().Row(row =>
                            {
                                foreach (var a in _model.Assessments)
                                {
                                    var total =
                                        _model.Rows
                                        .Select(r => r.Scores
                                        .ElementAt(_model.Assessments
                                        .IndexOf(a)))
                                        .Sum();

                                    row.RelativeItem()
                                        .Padding(4)
                                        .Text(total.ToString());
                                }
                            });
                        });
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated on ");
                    x.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm"));
                });
            });
        }
    }
}