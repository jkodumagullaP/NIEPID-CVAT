using CAT.AID.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Text.Json;

namespace CAT.AID.Controllers
{
    public class QuestionBankController : Controller
    {
        private readonly IWebHostEnvironment _env;

        private string FilePath =>
            Path.Combine(_env.WebRootPath, "data", "assessment_questions.json");

        public QuestionBankController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Ensure JSON exists
        private List<AssessmentSection> LoadData()
        {
            var folder = Path.Combine(_env.WebRootPath, "data");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            if (!System.IO.File.Exists(FilePath))
            {
                var sample = new List<AssessmentSection>
                {
                    new AssessmentSection
                    {
                        Category = "Sample Section",
                        Questions = new List<AssessmentQuestion>
                        {
                            new AssessmentQuestion
                            {
                                Id = 1,
                                Text = "Sample question?",
                                Options = new List<string>{"Yes","No"},
                                Correct = "Yes",
                                ScoreWeight = 1
                            }
                        }
                    }
                };

                System.IO.File.WriteAllText(
                    FilePath,
                    JsonSerializer.Serialize(sample, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    })
                );
            }

            return JsonSerializer.Deserialize<List<AssessmentSection>>(
                System.IO.File.ReadAllText(FilePath)
            ) ?? new();
        }

        private void SaveData(List<AssessmentSection> data)
        {
            System.IO.File.WriteAllText(
                FilePath,
                JsonSerializer.Serialize(data,
                new JsonSerializerOptions { WriteIndented = true })
            );
        }

        // ----------------------
        // LIST
        // ----------------------
        public IActionResult Index()
        {
            return View(LoadData());
        }

        // ----------------------
        // ADD / EDIT (GET)
        // ----------------------
        public IActionResult Edit(int? id)
        {
            var all = LoadData();

            AssessmentQuestion q = new();
            string section = "";

            if (id.HasValue)
            {
                q = all.SelectMany(x => x.Questions)
                       .FirstOrDefault(x => x.Id == id) ?? new();

                section = all
                    .FirstOrDefault(x => x.Questions.Any(q => q.Id == id))
                    ?.Category ?? "";
            }

            ViewBag.Sections = all.Select(x => x.Category).Distinct();

            return View((section, q));
        }

        // ----------------------
        // ADD / EDIT (POST)
        // ----------------------
        [HttpPost]
        public IActionResult Edit(string section, AssessmentQuestion q)
        {
            var all = LoadData();

            var sec = all.FirstOrDefault(x => x.Category == section);

            if (sec == null)
            {
                sec = new AssessmentSection
                {
                    Category = section,
                    Questions = new()
                };

                all.Add(sec);
            }

            var existing = sec.Questions.FirstOrDefault(x => x.Id == q.Id);

            if (existing == null)
            {
                q.Id = all.SelectMany(x => x.Questions).Any()
                    ? all.SelectMany(x => x.Questions).Max(x => x.Id) + 1
                    : 1;

                sec.Questions.Add(q);
            }
            else
            {
                existing.Text = q.Text;
                existing.Options = q.Options;
                existing.Correct = q.Correct;
                existing.ScoreWeight = q.ScoreWeight;
            }

            SaveData(all);

            TempData["msg"] = "Saved successfully";
            return RedirectToAction(nameof(Index));
        }

        // ----------------------
        // DELETE
        // ----------------------
        public IActionResult Delete(int id)
        {
            var all = LoadData();

            foreach (var sec in all)
                sec.Questions.RemoveAll(x => x.Id == id);

            SaveData(all);

            return RedirectToAction(nameof(Index));
        }

        // ----------------------
        // DOWNLOAD EXCEL
        // ----------------------
        public IActionResult Download()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var data = LoadData();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Questions");

            ws.Cells[1, 1].Value = "Section";
            ws.Cells[1, 2].Value = "Id";
            ws.Cells[1, 3].Value = "Question";
            ws.Cells[1, 4].Value = "Options";
            ws.Cells[1, 5].Value = "Correct";
            ws.Cells[1, 6].Value = "Score";

            int r = 2;

            foreach (var sec in data)
            foreach (var q in sec.Questions)
            {
                ws.Cells[r, 1].Value = sec.Category;
                ws.Cells[r, 2].Value = q.Id;
                ws.Cells[r, 3].Value = q.Text;
                ws.Cells[r, 4].Value = string.Join(",", q.Options);
                ws.Cells[r, 5].Value = q.Correct;
                ws.Cells[r, 6].Value = q.ScoreWeight;

                r++;
            }

            return File(
                package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "QuestionBank.xlsx"
            );
        }

        // ----------------------
        // UPLOAD EXCEL
        // ----------------------
        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            if (file == null || !file.FileName.EndsWith(".xlsx"))
            {
                TempData["msg"] = "Upload valid Excel file";
                return RedirectToAction(nameof(Index));
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var data = LoadData();

            using var package = new ExcelPackage(file.OpenReadStream());
            var ws = package.Workbook.Worksheets[0];

            int row = 2;

            while (ws.Cells[row, 1].Value != null)
            {
                string section = ws.Cells[row, 1].Text;
                string text = ws.Cells[row, 3].Text;

                var options = ws.Cells[row, 4]
                    .Text.Split(',')
                    .Select(x => x.Trim())
                    .ToList();

                string correct = ws.Cells[row, 5].Text;

                int.TryParse(ws.Cells[row, 6].Text, out int score);

                var sec = data.FirstOrDefault(x => x.Category == section);

                if (sec == null)
                {
                    sec = new AssessmentSection
                    {
                        Category = section,
                        Questions = new()
                    };

                    data.Add(sec);
                }

                int id = data.SelectMany(x => x.Questions).Any()
                    ? data.SelectMany(x => x.Questions).Max(x => x.Id) + 1
                    : 1;

                sec.Questions.Add(new AssessmentQuestion
                {
                    Id = id,
                    Text = text,
                    Options = options,
                    Correct = correct,
                    ScoreWeight = score
                });

                row++;
            }

            SaveData(data);

            return RedirectToAction(nameof(Index));
        }
    }
}
