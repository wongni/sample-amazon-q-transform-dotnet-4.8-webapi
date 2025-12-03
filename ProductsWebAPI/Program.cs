using Microsoft.EntityFrameworkCore;
using ProductsWebAPI.Repository;
using ProductsWebAPI.Service;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application services
builder.Services.AddScoped<IProductsService, ProductsService>();

// Configure database connection with RDS credentials from Secrets Manager
var secretArn = Environment.GetEnvironmentVariable("DB_SECRET");
if (!string.IsNullOrEmpty(secretArn))
{
    var client = new AmazonSecretsManagerClient();
    var request = new GetSecretValueRequest { SecretId = secretArn };
    var response = await client.GetSecretValueAsync(request);
    
    var secret = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);
    var connectionString = $"Host={secret!["host"]};Port={secret["port"]};Database={secret["dbname"]};Username={secret["username"]};Password={secret["password"]}";
    
    builder.Services.AddDbContext<ProductsContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Fallback for local development
    builder.Services.AddDbContext<ProductsContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("ProductsContext")));
}

var app = builder.Build();

// Run database migrations automatically on startup
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductsContext>();
    await dbContext.Database.MigrateAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database");
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/", () => Results.Ok(new { status = "healthy", service = "ProductsWebAPI" }));

app.Run();
