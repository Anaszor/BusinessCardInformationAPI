using BusinessCardInformationAPI.Application.Interfaces;
using BusinessCardInformationAPI.Application.Mappings;
using BusinessCardInformationAPI.Application.Services;
using BusinessCardInformationAPI.Domain.Interfaces;
using BusinessCardInformationAPI.Infrastructure.Persistence.Context;
using BusinessCardInformationAPI.Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence; // <- DataSeeder namespace
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database connection (adjust connection string in appsettings.json)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped<IBusinessCardRepository, BusinessCardRepository>();
builder.Services.AddScoped<IBusinessCardService, BusinessCardService>();

// Register the seeder
builder.Services.AddTransient<DataSeeder>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<BusinessCardProfile>());

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS to allow Angular dev server(s)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDevServer", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:4200",
            "http://localhost:5173"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// IMPORTANT: Enable CORS before authentication/authorization and before mapping controllers
app.UseCors("AllowAngularDevServer");

app.UseAuthorization();

// Map controllers
app.MapControllers();

// Apply migrations and seed database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var seeder = services.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        // decide whether to rethrow or continue; rethrowing will stop the app start
        // throw;
    }
}

// Serve Angular static files in production
app.UseDefaultFiles(); // Serves index.html by default
app.UseStaticFiles();  // Serves wwwroot files (Angular build output)

app.Run();