using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CAT.AID.Models;
using CAT.AID.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace CAT.AID.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        // ================= DASHBOARD =================
        public IActionResult Dashboard()
        {
            var model = new DashboardViewModel
            {
                TotalCandidates = _context.Candidates.Count(),
                TotalAssessments = _context.Assessments.Count(),
                RecentAssessments = _context.Assessments
                    .OrderByDescending(a => a.Id)
                    .Take(10)
                    .ToList()
            };

            return View(model);
        }


        // ================= USERS LIST =================
        public IActionResult Users()
        {
            var users = _context.Users.ToList();

            return View(users);
        }



        // ================= CREATE USER =================

        // Load page
        public IActionResult Create()
        {
            return View();
        }


        // Save user
        [HttpPost]
        public async Task<IActionResult> Create(
            ApplicationUser model,
            string password,
            string role)
        {

            if (!ModelState.IsValid)
                return View(model);


            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Location = model.Location
            };


            var result = await _userManager.CreateAsync(user, password);


            if (result.Succeeded)
            {

                // create role if not exists
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(
                        new IdentityRole(role));
                }


                await _userManager.AddToRoleAsync(user, role);


                return RedirectToAction("Users");
            }


            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);


            return View(model);
        }



        // ================= DELETE USER =================

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction("Users");
        }



        // ================= LOCK USER =================

        public async Task<IActionResult> Lock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                user.LockoutEnd = System.DateTime.UtcNow.AddYears(100);

                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("Users");
        }



        // ================= UNLOCK USER =================

        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                user.LockoutEnd = null;

                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("Users");
        }


    }
}
