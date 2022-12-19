using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)// UNCOMMENT WHEN IMPLEMENTING ROLES -> .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Just to test if the LayoutRegister works
app.MapControllerRoute(
    name: "MyRegisterLogIn",
    pattern: "MyRegister/LogIn",
    defaults: new { controller = "Register", action = "LogIn" });

app.MapControllerRoute(
    name: "MyRegisterSignUp",
    pattern: "MyRegister/SignUp",
    defaults: new { controller = "Register", action = "SignUp" });

// Articles routes
// INDEX
app.MapControllerRoute(
    name: "ArticlesIndex",
    pattern: "articles/index/filter-sort/{categoryID?}/{sort?}",
    defaults: new { controller = "Articles", action = "Index"});

// SHOW
app.MapControllerRoute(
    name: "ArticlesShow",
    pattern: "articles/show/{articleID}",
    defaults: new { controller = "Articles", action = "Show" });

// NEW
app.MapControllerRoute(
    name: "ArticlesNew",
    pattern: "Articles/New",
    defaults: new { controller = "Articles", action = "New" });

// EDIT
app.MapControllerRoute(
    name: "ArticlesEdit",
    pattern: "Articles/Edit/{articleID}/{versionID}",
    defaults: new { controller = "Articles", action = "Edit" });

// DELETE
app.MapControllerRoute(
    name: "ArticlesDelete",
    pattern: "Articles/Delete/{articleID}",
    defaults: new { controller = "Articles", action = "Delete" });

// RESTRICT
app.MapControllerRoute(
    name: "ArticlesRestrict",
    pattern: "Articles/Restrict/{articleID}",
    defaults: new { controller = "Articles", action = "Restrict" });

// CHOOSE_VERSION
app.MapControllerRoute(
    name: "ArticlesChooseVersion",
    pattern: "Articles/Choose-Version/{articleID}",
    defaults: new { controller = "Articles", action = "ChooseVersion" });

// Search article
// TO DO 
// app.MapControllerRoute(
//    name: "ArticlesSearch",
//    pattern: "Articles/Search/Search-string/{query?}",
//    defaults: new { controller = "Articles", action = "Search" });

// Categories routes
// NEW
app.MapControllerRoute(
    name: "CategoriesNew",
    pattern: "Categories/New",
    defaults: new { controller = "Categories", action = "New" });

// EDIT
app.MapControllerRoute(
    name: "CategoriesEdit",
    pattern: "Categories/Edit/{CategoryID}",
    defaults: new { controller = "Categories", action = "Edit"});

// DELETE
app.MapControllerRoute(
    name: "CategoriesDelete",
    pattern: "Categories/Delete/{CategoryID}",
    defaults: new { controller = "Categories", action = "Delete" });

// Basic route
app.MapControllerRoute(
    name: "default",
    pattern: "/",
    defaults: new { controller = "Articles", action = "Index" });

// FOR COMMENTS
// Delete
app.MapControllerRoute(
    name: "default",
    pattern: "/Comments/Delete/{CommentID}",
    defaults: new { controller = "Articles", action = "Delete" });

// Edit
app.MapControllerRoute(
    name: "default",
    pattern: "/Comments/Edit/{CommentID}",
    defaults: new { controller = "Articles", action = "Edit" });

app.MapRazorPages();
app.Run();
