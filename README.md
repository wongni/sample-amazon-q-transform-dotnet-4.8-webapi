# Products Web API - Modernized for ECS

This application has been migrated from .NET Framework 4.8 Web API to ASP.NET Core 10.0 for containerized deployment on AWS ECS.

## Migration Summary

**Original Stack:**
- .NET Framework 4.8
- ASP.NET Web API 2
- Entity Framework 6.5.1
- SimpleInjector DI
- LocalDB

**Modernized Stack:**
- ASP.NET Core 10.0
- EF Core 10.0
- Built-in Dependency Injection
- SQL Server (containerized)
- Swagger/OpenAPI documentation

## Key Changes

- Converted to SDK-style project format
- Migrated from `IHttpActionResult` to `IActionResult`
- Updated Entity Framework 6 to EF Core 10
- Replaced SimpleInjector with built-in DI
- Added Swagger/OpenAPI support
- Configured for port 8080 (ECS standard)
- Added health check endpoint
- Updated all tests to MSTest 3.6.3

## API Endpoints

- `GET /api/products` - List all products
- `GET /api/products/{id}` - Get single product
- `POST /api/products` - Create product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product
- `GET /health` - Health check
- `GET /swagger` - Swagger UI

## Local Development

### Prerequisites
- .NET SDK 10.0+
- Docker Desktop
- SQL Server (or use Docker)

### Run with Docker

```bash
# Start SQL Server
docker run -d --name sqlserver \
  -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest

# Apply migrations
cd ProductsWebAPI
dotnet ef database update

# Build and run API
docker build -t products-api .
docker run -d -p 8080:8080 \
  -e "ConnectionStrings__ProductsContext=Server=host.docker.internal,1433;Database=ProductsContext;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;" \
  products-api
```

### Run Tests

```bash
cd ProductsWebAPITest
dotnet test
```

## Environment Variables

The application uses ASP.NET Core configuration binding with double underscore notation for nested settings:

```bash
# Connection string format
ConnectionStrings__ProductsContext="Server=<host>,1433;Database=ProductsContext;User Id=<user>;Password=<password>;TrustServerCertificate=True;"

# Example for ECS
ConnectionStrings__ProductsContext="Server=mydb.abc123.us-east-1.rds.amazonaws.com,1433;Database=ProductsContext;User Id=admin;Password=<password>;TrustServerCertificate=True;"
```

## Database Migrations

EF Core migrations are included in the `Migrations/` folder. To apply:

```bash
# Local
dotnet ef database update

# In ECS (run as init container or startup task)
dotnet ef database update --connection "Server=<rds-endpoint>,1433;..."
```

## AWS ECS Deployment

### 1. Push Image to ECR

```bash
# Authenticate to ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com

# Tag and push
docker tag products-api:latest <account-id>.dkr.ecr.us-east-1.amazonaws.com/products-api:latest
docker push <account-id>.dkr.ecr.us-east-1.amazonaws.com/products-api:latest
```

### 2. Create RDS SQL Server Instance

- Engine: Microsoft SQL Server
- Version: SQL Server 2022
- Instance class: db.t3.small (or larger)
- Storage: 20 GB minimum
- Enable automatic backups
- Note the endpoint and credentials

### 3. ECS Task Definition

```json
{
  "family": "products-api",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "512",
  "memory": "1024",
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

### 4. ECS Service Configuration

- Launch type: Fargate
- Platform version: Latest
- Desired tasks: 2 (for high availability)
- Load balancer: Application Load Balancer
- Target group: Port 8080, health check path `/health`
- Auto-scaling: Target tracking (CPU 70%)

### 5. Security Groups

**ECS Tasks:**
- Inbound: Port 8080 from ALB security group
- Outbound: Port 1433 to RDS security group

**RDS:**
- Inbound: Port 1433 from ECS security group

## OpenAPI/Swagger

Swagger UI is available at `/swagger` in all environments. The OpenAPI specification is at `/swagger/v1/swagger.json`.

## Validation Results

✅ All 11 unit tests passed  
✅ Docker build successful  
✅ Health check endpoint working  
✅ Swagger/OpenAPI accessible  
✅ GET /api/products - Success  
✅ POST /api/products - Success  
✅ GET /api/products/{id} - Success  
✅ PUT /api/products/{id} - Success  
✅ DELETE /api/products/{id} - Success  

## Migration Date

Migrated on: December 1, 2025

## License

See LICENSE file for details.
