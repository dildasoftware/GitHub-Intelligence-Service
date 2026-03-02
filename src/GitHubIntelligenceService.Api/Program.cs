using GitHubIntelligenceService.Infrastructure;
using GitHubIntelligenceService.Application;
using GitHubIntelligenceService.Api.Middlewares; // YENİ: Middleware
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Setup Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TANIYALIM: Exception Handler (Hata Yönetimi)
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// TANIYALIM: CORS - Frontend (React) ile Backend (API) konuşsun diye izin veriyoruz.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// 3. Layer Dependencies
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// 4. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global Exception Handler'ı Dev değil her ortamda kullanmalıyız
app.UseExceptionHandler();

app.UseSerilogRequestLogging();

app.UseCors("AllowAll"); // CORS (Artık localhost'ta aynı portta olacağı için kritik değil ama dursun)

// FRONTEND SUNUMU (SPA)
app.UseStaticFiles(); // wwwroot klasöründeki dosyaları sunar 

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Tüm bilinmeyen istekleri index.html'e yönlendir (React Router için)
app.MapFallbackToFile("index.html");

// TANIYALIM: Otomatik Veritabanı Kurulumu (Auto-Migration)
// Uygulama her başladığında veritabanı yoksa oluşturur, tabloları hazırlar.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GitHubIntelligenceService.Infrastructure.Persistence.AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
