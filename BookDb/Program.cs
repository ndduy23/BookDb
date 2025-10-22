using BookDb.ExtendMethos;
using BookDb.Models;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using BookDb.Repository.Interfaces;
using BookDb.Services.Implementations;
using BookDb.Services.Interfaces;
using BookDb.Repositories.Interfaces;
using BookDb.Repositories.Implementations;
using BookDb.Hubs;
using BookDb.Services;

var builder = WebApplication.CreateBuilder(args);

// ====== Cấu hình dịch vụ (DI) ======
builder.Services.AddDbContext<AppDbContext>(options =>
{
    string connectString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectString);
});

builder.Services.AddSingleton<FileStorageService>();

// Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();
builder.Services.AddScoped<IDocumentPageRepository, DocumentPageRepository>();

// Services
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<IDocumentPageService, DocumentPageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Cấu hình Razor View Engine
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Add("/MyViews/{1}/{0}" + RazorViewEngine.ViewExtension);
});

// Add CORS for SignalR (if needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ====== Build app ======
var app = builder.Build();

// ====== Middleware pipeline ======
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Tùy biến lỗi từ 400–599
app.AddStatusCodePage();

app.UseRouting();

// CORS (if needed)
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// ====== Định tuyến ======
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Map SignalR hub
app.MapHub<NotificationHub>("/notify");

// ====== Chạy app ======
app.Run();