using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using QAgentWeb.Data;
using QAgentWeb.Data.SeedData;
using QAgentWeb.Models;
using QAgentWeb.Resources;
using QAgentWeb.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// Add Repository Pattern
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Add Authentication Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Add UC01 Services
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IUploadService, UploadService>();

// Add UC02 AI Analysis Services
builder.Services.AddScoped<IAIAnalysisService, AIAnalysisService>();
builder.Services.AddScoped<IGoogleVisionService, GoogleVisionService>();
builder.Services.AddScoped<IGoogleGeminiService, GoogleGeminiService>();
builder.Services.AddScoped<ITextExtractionService, TextExtractionService>();
builder.Services.AddScoped<IUIElementDetectionService, UIElementDetectionService>();
builder.Services.AddScoped<IScreenStandardizationService, ScreenStandardizationService>();

// Add UC03 ISTQB Rules Services
builder.Services.AddScoped<IRuleEngineService, RuleEngineService>();

// Add API Services
builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<IGoogleDriveService, GoogleDriveService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

// Add Database Migration Service
builder.Services.AddScoped<IDatabaseMigrationService, DatabaseMigrationService>();

// Add HttpClient
builder.Services.AddHttpClient();

// Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"),
        new CultureInfo("vi")
    };
    
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Add services to the container.
builder.Services.AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResource));
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Use Request Localization
app.UseRequestLocalization();

// Use Session
app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    
    // Seed ISTQB Rules data
    await ISTQBRulesSeedData.SeedAsync(context);
}

app.Run();
