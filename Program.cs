using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using AppartementReservationAPI.Data;
using System.IO;
using AppartementReservationAPI.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get port for Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

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

// In Program.cs
builder.Services.AddScoped<IStripeService, StripeService>();
string connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
    "Server=AppartementReservationDB.mssql.somee.com;" +
    "Database=AppartementReservationDB;" +
    "User Id=souhailazzimani_SQLLogin_1;" +
    "Password=x7heeqrtjf;" +
    "TrustServerCertificate=True;" +
    "Encrypt=True;" +
    "MultipleActiveResultSets=True";
// Configure DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(5); 
    }));
// Configure Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();


// Only use HTTPS redirection in development (not on Railway)
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.UseSession();
app.MapControllers();

app.Run();