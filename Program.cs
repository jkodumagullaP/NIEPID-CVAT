using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using QuestPDF.Infrastructure;

using CAT.AID.Web.Data;
using CAT.AID.Web.Models;

var builder = WebApplication.CreateBuilder(args);


// --------------------
// QuestPDF License
// --------------------
QuestPDF.Settings.License = LicenseType.Community;


// --------------------
// SERVICES
// --------------------

// MVC
builder.Services.AddControllersWithViews();


// PostgreSQL
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));


// Identity
builder.Services
.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


// cookie redirect
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});


// CORS (optional)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


var app = builder.Build();


// --------------------
// PIPELINE
// --------------------

// show detailed error on Render logs
app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();


// --------------------
// ROUTES
// --------------------

// default route → login page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");


// dashboard route
app.MapControllerRoute(
    name: "dashboard",
    pattern: "Dashboard",
    defaults: new { controller = "Dashboard", action = "Index" });


// candidates route
app.MapControllerRoute(
    name: "candidates",
    pattern: "Candidates",
    defaults: new { controller = "Candidates", action = "Index" });


app.Run();
