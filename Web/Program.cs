using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SurveyApp.BLL;
using SurveyApp.DAL;
using SurveyApp.Web.Helpers; // Add this using for SessionExtensions
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure Session State
builder.Services.AddDistributedMemoryCache(); // Use memory cache for session state (simple)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Adjust timeout as needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Make session cookie essential
});

// Register application services (Dependency Injection)
// Scoped: One instance per HTTP request
builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
builder.Services.AddScoped<ISurveyService, SurveyService>();

// Add Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
// Add other providers like Application Insights if needed

// Add HttpContextAccessor if you need to access HttpContext outside of Razor Pages/Controllers (e.g., in services, though usually avoided)
// builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // Use detailed developer exception page in development
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Serve files from wwwroot (like CSS, JS)

app.UseRouting();

// IMPORTANT: Session middleware must come AFTER UseRouting and BEFORE UseAuthorization/MapRazorPages
app.UseSession();

app.UseAuthorization(); // Even if no auth is configured, it's good practice

app.MapRazorPages();

app.Run();