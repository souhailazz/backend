using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using AppartementReservationAPI.Data;
using System.IO;

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

// Configure DbContext with SQL Server
// Get connection string from environment variable or fall back to hardcoded one only for development
string connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
    "workstation id=AppartementReservationDB.mssql.somee.com;packet size=4096;user id=souhailazzimani_SQLLogin_1;pwd=x7heeqrtjf;data source=AppartementReservationDB.mssql.somee.com;persist security info=False;initial catalog=AppartementReservationDB;TrustServerCertificate=True";

// Configure DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(5); // Increase command timeout to 30 seconds
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