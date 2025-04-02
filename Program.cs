using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using AppartementReservationAPI.Data;

// For Railway deployment, use environment variable PORT or default to 8080
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
// Configure ASPNETCORE_URLS environment variable
Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://+:{port}");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable CORS
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

// Get connection string from environment variable or appsettings.json
string connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
    builder.Configuration.GetConnectionString("DefaultConnection");

// Configure DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Always use Swagger in all environments for Railway
app.UseSwagger();
app.UseSwaggerUI();

// Use CORS
app.UseCors("AllowAll");

// Skip HTTPS redirection on Railway (as Railway handles HTTPS)
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.UseSession();
app.MapControllers();

// Add a simple health check endpoint that Railway can use to verify the app is running
app.MapGet("/", () => "API is running!");

app.Run();