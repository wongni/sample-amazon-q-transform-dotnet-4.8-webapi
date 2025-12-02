using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.EntityFrameworkCore;
using ProductsWebAPI.Repository;
using ProductsWebAPI.Service;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Retrieve RDS credentials from Secrets Manager
var secretArn = Environment.GetEnvironmentVariable("DB_SECRET");
if (!string.IsNullOrEmpty(secretArn))
{
    using var client = new AmazonSecretsManagerClient();
    var request = new GetSecretValueRequest { SecretId = secretArn };
    var response = await client.GetSecretValueAsync(request);
    var secret = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);

    if (secret != null)
    {
        var host = secret["host"];
        var port = secret["port"];
        var username = secret["username"];
        var password = secret["password"];
        var dbname = secret["dbname"];

        var connectionString = $"Host={host};Port={port};Database={dbname};Username={username};Password={password}";
        builder.Services.AddDbContext<ProductsContext>(options => options.UseNpgsql(connectionString));
    }
}
else
{
    builder.Services.AddDbContext<ProductsContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("ProductsContext") ?? "Host=localhost;Database=ProductsContext"));
}

builder.Services.AddScoped<IProductsService, ProductsService>();

var app = builder.Build();

// Apply migrations automatically
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ProductsContext>();
    context.Database.Migrate();
}
catch (Exception ex)
{
    Console.WriteLine($"Migration error: {ex.Message}");
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();
app.Run();
