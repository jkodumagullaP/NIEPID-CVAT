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


        // ================= LOAD JSON =================
        private List<AssessmentSection> LoadData()
        {
            var folder = Path.Combine(_env.WebRootPath, "data");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            if (!System.IO.File.Exists(FilePath))
            {
                System.IO.File.WriteAllText(
                    FilePath,
                    JsonSerializer.Serialize(new List<AssessmentSection>(),
                    new JsonSerializerOptions { WriteIndented = true })
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
                JsonSerializer.Serialize(
                    data,
                    new JsonSerializerOptions { WriteIndented = true }
                )
            );
        }



        // ================= LIST =================
        public IActionResult Index()
        {
            return View(LoadData());
        }



        // ================= LOAD ADD PAGE =================
        public IActionResult Edit(int? id)
        {
            var all = LoadData();

            var model = new QuestionEditVM();

            if (id.HasValue)
            {
                var question = all
                    .SelectMany(x => x.Questions)
                    .FirstOrDefault(x => x.Id == id);

                if (question != null)
                {
                    model.Question = question;

                    model.Section =
                        all.First(x => x.Questions.Any(q => q.Id == id))
                           .Category;
                }
            }

            model.AllSections =
                all.Select(x => x.Category)
                   .Distinct()
                   .ToList();

            return View(model);
        }



        // ================= SAVE =================
        [HttpPost]
        public IActionResult Edit(QuestionEditVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Section))
            {
                ModelState.AddModelError("", "Section required");
                return View(vm);
            }

            var all = LoadData();

            var sec = all.FirstOrDefault(
                x => x.Category == vm.Section.Trim()
            );

            if (sec == null)
            {
                sec = new AssessmentSection
                {
                    Category = vm.Section.Trim(),
                    Questions = new()
                };

                all.Add(sec);
            }


            vm.Question.Options ??= new List<string>();


            var existing = sec.Questions
                .FirstOrDefault(x => x.Id == vm.Question.Id);


            if (existing == null)
            {
                vm.Question.Id =
                    all.SelectMany(x => x.Questions).Any()
                    ? all.SelectMany(x => x.Questions).Max(x => x.Id) + 1
                    : 1;

                sec.Questions.Add(vm.Question);
            }
            else
            {
                existing.Text = vm.Question.Text;
                existing.Options = vm.Question.Options;
                existing.Correct = vm.Question.Correct;
                existing.ScoreWeight = vm.Question.ScoreWeight;
            }


            SaveData(all);

            TempData["msg"] = "Question saved";

            return RedirectToAction(nameof(Index));
        }



        // ================= DELETE =================
        public IActionResult Delete(int id)
        {
            var all = LoadData();

            foreach (var s in all)
                s.Questions.RemoveAll(x => x.Id == id);

            SaveData(all);

            return RedirectToAction(nameof(Index));
        }



        // ================= EXPORT =================
        public IActionResult Download()
        {
            ExcelPackage.LicenseContext =
                LicenseContext.NonCommercial;

            var data = LoadData();

            using var pkg = new ExcelPackage();

            var ws =
                pkg.Workbook.Worksheets.Add("Questions");


            ws.Cells[1, 1].Value = "Section";
            ws.Cells[1, 2].Value = "Question";
            ws.Cells[1, 3].Value = "Options";
            ws.Cells[1, 4].Value = "Correct";
            ws.Cells[1, 5].Value = "Score";


            int r = 2;


            foreach (var s in data)
            foreach (var q in s.Questions)
            {
                ws.Cells[r, 1].Value = s.Category;
                ws.Cells[r, 2].Value = q.Text;
                ws.Cells[r, 3].Value =
                    string.Join(",", q.Options ?? new());
                ws.Cells[r, 4].Value = q.Correct;
                ws.Cells[r, 5].Value = q.ScoreWeight;

                r++;
            }


            return File(
                pkg.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "QuestionBank.xlsx"
            );
        }



        // ================= IMPORT =================
        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            if (file == null || !file.FileName.EndsWith(".xlsx"))
            {
                TempData["msg"] = "Upload Excel file";
                return RedirectToAction(nameof(Index));
            }


            ExcelPackage.LicenseContext =
                LicenseContext.NonCommercial;


            var data = LoadData();


            using var pkg =
                new ExcelPackage(file.OpenReadStream());

            var ws = pkg.Workbook.Worksheets[0];


            int row = 2;


            while (ws.Cells[row, 1].Value != null)
            {
                var section = ws.Cells[row, 1].Text.Trim();

                var text = ws.Cells[row, 2].Text;

                var options =
                    ws.Cells[row, 3].Text
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToList();

                var correct = ws.Cells[row, 4].Text;

                int.TryParse(
                    ws.Cells[row, 5].Text,
                    out int score
                );


                var sec =
                    data.FirstOrDefault(x => x.Category == section);


                if (sec == null)
                {
                    sec = new AssessmentSection
                    {
                        Category = section,
                        Questions = new()
                    };

                    data.Add(sec);
                }


                int id =
                    data.SelectMany(x => x.Questions).Any()
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


            TempData["msg"] = "Imported successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}
