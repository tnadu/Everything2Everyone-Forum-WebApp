using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static System.Formats.Asn1.AsnWriter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true).AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}

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
    pattern: "articles/index/{categoryID?}/{sort?}/{userSpecificMode?}",
    defaults: new { controller = "Articles", action = "Index"});

// SHOW
app.MapControllerRoute(
    name: "ArticlesShow",
    pattern: "articles/show/{articleID}",
    defaults: new { controller = "Articles", action = "Show" });

// NEW
app.MapControllerRoute(
    name: "ArticlesNew",
    pattern: "articles/new",
    defaults: new { controller = "Articles", action = "New" });

// EDIT
app.MapControllerRoute(
    name: "ArticlesEdit",
    pattern: "articles/edit/{articleID}/{versionID}",
    defaults: new { controller = "Articles", action = "Edit" });

app.MapControllerRoute(
    name: "ArticlesEdit",
    pattern: "articles/edit",
    defaults: new { controller = "Articles", action = "Edit" });

// DELETE
app.MapControllerRoute(
    name: "ArticlesDelete",
    pattern: "articles/delete/{articleID}",
    defaults: new { controller = "Articles", action = "Delete" });

// RESTRICT
app.MapControllerRoute(
    name: "ArticlesRestrict",
    pattern: "articles/restrict/{articleID}",
    defaults: new { controller = "Articles", action = "Restrict" });

// CHOOSE_VERSION
app.MapControllerRoute(
    name: "ArticlesChooseVersion",
    pattern: "articles/choose-version/{articleID}",
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
    pattern: "categories/new",
    defaults: new { controller = "Categories", action = "New" });

// EDIT
app.MapControllerRoute(
    name: "CategoriesEdit",
    pattern: "categories/edit/{CategoryID}",
    defaults: new { controller = "Categories", action = "Edit"});

// DELETE
app.MapControllerRoute(
    name: "CategoriesDelete",
    pattern: "categories/delete/{CategoryID}",
    defaults: new { controller = "Categories", action = "Delete" });

// FOR COMMENTS
// Delete
app.MapControllerRoute(
    name: "CommentsDelete",
    pattern: "/comments/delete/{CommentID}",
    defaults: new { controller = "Comments", action = "Delete" });

// Edit
app.MapControllerRoute(
    name: "CommentsEdit",
    pattern: "/comments/edit/{CommentID}",
    defaults: new { controller = "Comments", action = "Edit" });

//MyComments
app.MapControllerRoute(
    name: "MyComments",
    pattern: "/comments/my-comments",
    defaults: new { controller = "Comments", action = "Index" });

// USERS
// Index - Manage users
app.MapControllerRoute(
    name: "ManageUsers",
    pattern: "/users/index",
    defaults: new { controller = "Users", action = "Index" });

// Delete
app.MapControllerRoute(
    name: "UsersDelete",
    pattern: "/users/delete/{UserID}",
    defaults: new { controller = "Users", action = "Delete" });

// Edit
app.MapControllerRoute(
    name: "UsersEdit",
    pattern: "/users/edit/{UserID}",
    defaults: new { controller = "Users", action = "Edit" });

// Change Email
app.MapControllerRoute(
    name: "UsersChangeEmail",
    pattern: "/users/change-email/{UserID}",
    defaults: new { controller = "Users", action = "ChangeEmail" });

// Change Password
app.MapControllerRoute(
    name: "UsersChangePassword",
    pattern: "/users/change-password/{UserID}",
    defaults: new { controller = "Users", action = "ChangePassword" });

app.MapControllerRoute(
    name: "ForwardedLogin",
    pattern: "/login",
    defaults: new { controller = "Authentication", action = "LogIn" });

app.MapControllerRoute(
    name: "ForwardedSignup",
    pattern: "/signup",
    defaults: new { controller = "Authentication", action = "SignUp" });

//////////////
// Basic route
app.MapControllerRoute(
    name: "default",
    pattern: "/",
    defaults: new { controller = "Authentication", action = "LogIn" });

app.MapRazorPages();
app.Run();
