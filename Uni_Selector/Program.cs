using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using Uni_Selector.Data;
using Uni_Selector.Hubs;
using Uni_Selector.Middleware;
using Uni_Selector.Models;
using Uni_Selector.Service;
using Uni_Selector.Service.BackgorundService;
using Uni_Selector.Service.Implementation;
using Uni_Selector.Service.Interface;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()
    ));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;

    options.SignIn.RequireConfirmedEmail = false; 
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy =
        builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Student", policy => policy.RequireRole(UserRoles.Student));
    options.AddPolicy("UniversityRep", policy => policy.RequireRole(UserRoles.UniversityRep));
    options.AddPolicy("PlatformAdmin", policy => policy.RequireRole(UserRoles.PlatformAdmin));
    options.AddPolicy("BtecAuthority", policy => policy.RequireRole(UserRoles.BtecAuthority));
});

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddSingleton<IRecommendationUpdateQueue, RecommendationUpdateQueue>();
builder.Services.AddScoped<IBtecStandardsNotifier, BtecStandardsNotifier>();


builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddMemoryCache();

builder.Services.AddSignalR();

builder.Services.AddHostedService<CpuMonitoringService>();
builder.Services.AddHostedService<SmartRecommendationService>();


builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

var supportedCultures = new[] { "en-US", "ar-JO" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddDistributedMemoryCache();



builder.Services.AddScoped<RecommendationService>();




var app = builder.Build();


if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseMiddleware<ResponseTimeMiddleware>();

app.UseRouting();

app.UseRequestLocalization(localizationOptions);
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/hubs/notification");


app.Run();
