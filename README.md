# Products Web API - Modernized for ECS

This application has been migrated from .NET Framework 4.8 to ASP.NET Core 10.0 and containerized for AWS ECS deployment.

## Migration Summary

- **Original Framework:** .NET Framework 4.8 (ASP.NET Web API 2)
- **Target Framework:** ASP.NET Core 10.0
- **Database:** Entity Framework 6.x → Entity Framework Core 10.0
- **Dependency Injection:** Autofac/SimpleInjector → Built-in ASP.NET Core DI
- **Container:** Multi-stage Docker build with optimized layers

## Key Changes

1. Converted to SDK-style project format
2. Migrated from System.Web.Http to Microsoft.AspNetCore.Mvc
3. Updated Entity Framework to EF Core with constructor injection
4. Added Swagger/OpenAPI documentation
5. Configured for port 8080 (ECS standard)
6. Created health check endpoint at `/health`

## Local Development

### Prerequisites
- .NET SDK 10.0 or higher
- Docker Desktop
- SQL Server (or use Docker container)

### Build and Run

```bash
cd ProductsWebAPI

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run locally
dotnet run
```

Access the API at `http://localhost:8080`
- Swagger UI: `http://localhost:8080/swagger`
- Health check: `http://localhost:8080/health`

### Database Setup

```bash
# Apply migrations
dotnet ef database update

# Create new migration
dotnet ef migrations add <MigrationName>
```

## Docker

### Build Image

```bash
cd ProductsWebAPI
docker build -t products-api:latest .
```

### Run with Docker

```bash
# Start SQL Server
docker run -d --name sqlserver \
  -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest

# Run API
docker run -d --name products-api \
  -p 8080:8080 \
  -e "ConnectionStrings__ProductsContext=Server=host.docker.internal,1433;Database=ProductsContext;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;" \
  products-api:latest
```

## Environment Variables

The application uses ASP.NET Core configuration with double underscore notation for nested settings:

```bash
# Connection string format
ConnectionStrings__ProductsContext="Server=<host>,1433;Database=ProductsContext;User Id=<user>;Password=<password>;TrustServerCertificate=True;"

# Example for ECS with RDS
ConnectionStrings__ProductsContext="Server=mydb.abc123.us-east-1.rds.amazonaws.com,1433;Database=ProductsContext;User Id=admin;Password=<password>;TrustServerCertificate=True;"
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | List all products |
| GET | `/api/products/{id}` | Get product by ID |
| POST | `/api/products` | Create new product |
| PUT | `/api/products/{id}` | Update product |
| DELETE | `/api/products/{id}` | Delete product |
| GET | `/health` | Health check |
| GET | `/swagger` | OpenAPI documentation |

### Example Requests

```bash
# Create product
curl -X POST http://localhost:8080/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Laptop","category":"Electronics","price":999.99}'

# Get all products
curl http://localhost:8080/api/products

# Update product
curl -X PUT http://localhost:8080/api/products/1 \
  -H "Content-Type: application/json" \
  -d '{"name":"Gaming Laptop","category":"Electronics","price":1299.99}'

# Delete product
curl -X DELETE http://localhost:8080/api/products/1
```

## AWS ECS Deployment

### Push to ECR

```bash
# Authenticate to ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com

# Tag image
docker tag products-api:latest <account-id>.dkr.ecr.us-east-1.amazonaws.com/products-api:latest

# Push image
docker push <account-id>.dkr.ecr.us-east-1.amazonaws.com/products-api:latest
```

### ECS Task Definition

```json
{
  "family": "products-api",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "256",
  "memory": "512",
  "containerDefinitions": [
    {
      "name": "products-api",
      "image": "<account-id>.dkr.ecr.us-east-1.amazonaws.com/products-api:latest",
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        },
        {
          "name": "ConnectionStrings__ProductsContext",
          "value": "Server=<rds-endpoint>,1433;Database=ProductsContext;User Id=admin;Password=<password>;TrustServerCertificate=True;"
        }
      ],
      "healthCheck": {
        "command": ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"],
        "interval": 30,
        "timeout": 5,
        "retries": 3,
        "startPeriod": 60
      },
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/products-api",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

### Database Migration in ECS

Run migrations as a one-time ECS task before deploying the service:

```bash
# Create migration task definition with command override
aws ecs run-task \
  --cluster <cluster-name> \
  --task-definition products-api-migration \
  --launch-type FARGATE \
  --network-configuration "awsvpcConfiguration={subnets=[<subnet-id>],securityGroups=[<sg-id>],assignPublicIp=ENABLED}" \
  --overrides '{"containerOverrides":[{"name":"products-api","command":["dotnet","ef","database","update"]}]}'
```

## Testing

The application includes MSTest unit tests. Run them with:

```bash
cd ProductsWebAPITest
dotnet test
```

## Troubleshooting

### Connection Issues
- Ensure SQL Server is accessible from the container
- Use `host.docker.internal` for local Docker Desktop
- For ECS, ensure security groups allow traffic on port 1433

### Migration Errors
- Verify connection string format
- Check database exists
- Ensure SQL Server is ready (wait 15-20 seconds after container start)

### Port Conflicts
- The application listens on port 8080
- Ensure no other services are using this port
- For local development, you can change the port in `appsettings.json`

## Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [AWS ECS Documentation](https://docs.aws.amazon.com/ecs/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
