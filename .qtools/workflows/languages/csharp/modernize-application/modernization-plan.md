# C# Modernization Plan: ProductsWebAPI
## .NET Framework 4.8 → .NET 10.0

---

## Executive Summary

**Current State:**
- Framework: .NET Framework 4.8
- Project Type: ASP.NET Web API (System.Web)
- Total Files: 11 source files + 2 test files
- Dependencies: Entity Framework 6.5.1, SimpleInjector 5.5.0, ASP.NET Web API 5.2.9

**Target State:**
- Framework: .NET 10.0
- Project Type: ASP.NET Core Web API (Minimal API or Controller-based)
- Modern patterns: Async/await, nullable reference types, file-scoped namespaces

**Effort Estimate:** 24-32 hours
**Risk Assessment:** Medium-High
- High risk: Framework migration (breaking changes in Web API → ASP.NET Core)
- Medium risk: Entity Framework 6 → EF Core migration
- Low risk: Language modernization

---

## Prioritized Modernization Tasks

| Priority | Category | Description | Files Affected | Effort | Risk |
|----------|----------|-------------|----------------|--------|------|
| **CRITICAL** | Framework | Migrate to .NET 10.0 SDK-style project | All | 8h | High |
| **CRITICAL** | Framework | ASP.NET Web API → ASP.NET Core | Controllers, Startup | 6h | High |
| **CRITICAL** | Framework | Entity Framework 6 → EF Core 10 | Repository, Context | 4h | Medium |
| **CRITICAL** | Framework | SimpleInjector → Built-in DI | DI Setup | 2h | Low |
| High | Async | Convert synchronous methods to async | Service, Controller | 3h | Low |
| High | Language | Enable nullable reference types | All | 2h | Low |
| High | Language | File-scoped namespaces | All | 1h | Low |
| Medium | Configuration | Web.config → appsettings.json | Config | 2h | Low |
| Medium | Language | Records for DTOs | Models | 1h | Low |
| Medium | Testing | MSTest → xUnit/NUnit | Tests | 2h | Low |
| Low | Language | Pattern matching improvements | Controllers | 1h | Low |
| Low | Language | Init-only properties | Models | 0.5h | Low |

---

## Detailed Changes by File

### 1. ProductsWebAPI.csproj
**Current Issues:**
- Old-style .csproj format (non-SDK style)
- References packages.config
- Targets .NET Framework 4.8
- Contains legacy Web Application targets

**Modernization Actions:**
1. Convert to SDK-style project format (Effort: 2h, Risk: High)
2. Change TargetFramework to net10.0 (Effort: 0.5h, Risk: High)
3. Migrate packages.config to PackageReference (Effort: 1h, Risk: Medium)
4. Update all NuGet packages to .NET 10.0 compatible versions (Effort: 2h, Risk: High)

**Code Examples:**

Before:
```xml
<Project ToolsVersion="15.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
    <ProjectTypeGuids>{349c5851-65df-11da-9384-00065b846f21};{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</ProjectTypeGuids>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="EntityFramework">
      <HintPath>..\packages\EntityFramework.6.5.1\lib\net45\EntityFramework.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

After:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0" />
  </ItemGroup>
</Project>
```

---

### 2. Controllers/ProductsController.cs
**Current Issues:**
- Inherits from ApiController (System.Web.Http)
- Synchronous methods (no async/await)
- Returns IHttpActionResult (legacy)
- No nullable reference types
- Traditional namespace declaration

**Modernization Actions:**
1. Change base class to ControllerBase (ASP.NET Core) (Effort: 1h, Risk: High)
2. Convert all methods to async (Effort: 2h, Risk: Low)
3. Update return types to ActionResult<T> (Effort: 1h, Risk: Medium)
4. Enable nullable reference types (Effort: 0.5h, Risk: Low)
5. Apply file-scoped namespaces (Effort: 0.25h, Risk: Low)
6. Use pattern matching for validation (Effort: 0.5h, Risk: Low)

**Code Examples:**

Before:
```csharp
using System;
using System.Web.Http;
using ProductsWebAPI.Models;
using ProductsWebAPI.Service;

namespace ProductsWebAPI.Controllers
{
    public class ProductsController : ApiController
    {
        private readonly IProductsService _productsService;
        
        [Route("api/products/{id:int}")]
        [HttpGet]
        public IHttpActionResult GetProduct(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid product ID. ID must be greater than 0.");
            }
            
            try
            {
                var product = _productsService.GetProduct(id);
                if (product == null)
                {
                    return NotFound();
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
```

After:
```csharp
using Microsoft.AspNetCore.Mvc;
using ProductsWebAPI.Models;
using ProductsWebAPI.Service;

namespace ProductsWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductsService _productsService;
    
    public ProductsController(IProductsService productsService)
    {
        _productsService = productsService ?? throw new ArgumentNullException(nameof(productsService));
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid product ID. ID must be greater than 0.");
        
        var product = await _productsService.GetProductAsync(id);
        return product is null ? NotFound() : Ok(product);
    }
}
```

---

### 3. Service/ProductsService.cs
**Current Issues:**
- Synchronous database operations
- Using statement with DbContext (not async)
- LINQ query syntax (verbose)
- No nullable reference types
- Traditional namespace

**Modernization Actions:**
1. Convert all methods to async (Effort: 2h, Risk: Low)
2. Use async DbContext operations (Effort: 1h, Risk: Low)
3. Simplify LINQ queries with method syntax (Effort: 0.5h, Risk: Low)
4. Enable nullable reference types (Effort: 0.5h, Risk: Low)
5. Apply file-scoped namespaces (Effort: 0.25h, Risk: Low)

**Code Examples:**

Before:
```csharp
using System.Collections.Generic;
using System.Linq;
using ProductsWebAPI.Models;
using ProductsWebAPI.Repository;

namespace ProductsWebAPI.Service
{
    public class ProductsService : IProductsService
    {
        public Product GetProduct(int id)
        {
            using (var db = new ProductsContext())
            {
                Product query = (from p in db.Products
                                 where p.Id == id
                                 select p).FirstOrDefault();
                
                if (query == null)
                {
                    return null;
                }
                
                return query;
            }
        }
    }
}
```

After:
```csharp
using Microsoft.EntityFrameworkCore;
using ProductsWebAPI.Models;
using ProductsWebAPI.Repository;

namespace ProductsWebAPI.Service;

public class ProductsService : IProductsService
{
    private readonly ProductsContext _context;
    
    public ProductsService(ProductsContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task<Product?> GetProductAsync(int id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
```

---

### 4. Service/IProductsService.cs
**Current Issues:**
- Synchronous method signatures
- No nullable annotations

**Modernization Actions:**
1. Convert to async method signatures (Effort: 0.5h, Risk: Low)
2. Add nullable reference types (Effort: 0.25h, Risk: Low)
3. Apply file-scoped namespace (Effort: 0.1h, Risk: Low)

**Code Examples:**

Before:
```csharp
namespace ProductsWebAPI.Service
{
    public interface IProductsService
    {
        IEnumerable<Product> GetAllProducts();
        Product GetProduct(int id);
        void SaveProduct(Product product);
        void UpdateProduct(int id, Product product);
        void DeleteProduct(int id);
    }
}
```

After:
```csharp
namespace ProductsWebAPI.Service;

public interface IProductsService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductAsync(int id);
    Task SaveProductAsync(Product product);
    Task UpdateProductAsync(int id, Product product);
    Task DeleteProductAsync(int id);
}
```

---

### 5. Models/Product.cs
**Current Issues:**
- Mutable properties (set)
- No nullable reference types
- Traditional namespace
- Could use records for immutability

**Modernization Actions:**
1. Enable nullable reference types (Effort: 0.25h, Risk: Low)
2. Apply file-scoped namespace (Effort: 0.1h, Risk: Low)
3. Consider converting to record (Effort: 0.5h, Risk: Low)
4. Use init-only properties where appropriate (Effort: 0.25h, Risk: Low)

**Code Examples:**

Before:
```csharp
namespace ProductsWebAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
    }
}
```

After (Option 1 - Class with init):
```csharp
namespace ProductsWebAPI.Models;

public class Product
{
    public int Id { get; init; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public decimal Price { get; set; }
}
```

After (Option 2 - Record):
```csharp
namespace ProductsWebAPI.Models;

public record Product
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public decimal Price { get; init; }
}
```

---

### 6. Repository/ProductsContext.cs
**Current Issues:**
- Inherits from DbContext (Entity Framework 6)
- No constructor for dependency injection
- Traditional namespace

**Modernization Actions:**
1. Migrate to EF Core DbContext (Effort: 2h, Risk: Medium)
2. Add constructor accepting DbContextOptions (Effort: 0.5h, Risk: Low)
3. Apply file-scoped namespace (Effort: 0.1h, Risk: Low)
4. Enable nullable reference types (Effort: 0.25h, Risk: Low)

**Code Examples:**

Before:
```csharp
using System.Data.Entity;
using ProductsWebAPI.Models;

namespace ProductsWebAPI.Repository
{
    public class ProductsContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
    }
}
```

After:
```csharp
using Microsoft.EntityFrameworkCore;
using ProductsWebAPI.Models;

namespace ProductsWebAPI.Repository;

public class ProductsContext : DbContext
{
    public ProductsContext(DbContextOptions<ProductsContext> options) 
        : base(options)
    {
    }
    
    public DbSet<Product> Products => Set<Product>();
}
```

---

### 7. App_Start/DependencyInjectionSetup.cs
**Current Issues:**
- Uses SimpleInjector (third-party DI)
- Manual registration required
- Traditional namespace

**Modernization Actions:**
1. Remove SimpleInjector, use built-in DI (Effort: 1h, Risk: Low)
2. Move to Program.cs/Startup.cs (Effort: 0.5h, Risk: Low)
3. This file will be deleted (Effort: 0.1h, Risk: Low)

**Code Examples:**

Before:
```csharp
using System.Web.Http;
using ProductsWebAPI.Service;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using SimpleInjector.Lifestyles;

namespace ProductsWebAPI.App_Start
{
    public static class DependencyInjectionSetup
    {
        public static void Configure()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.Register<IProductsService, ProductsService>(Lifestyle.Scoped);
            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);
            GlobalConfiguration.Configuration.DependencyResolver =
                new SimpleInjectorWebApiDependencyResolver(container);
            container.Verify();
        }
    }
}
```

After (in Program.cs):
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddDbContext<ProductsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductsContext")));
builder.Services.AddScoped<IProductsService, ProductsService>();

var app = builder.Build();

app.MapControllers();
app.Run();
```

---

### 8. Global.asax.cs
**Current Issues:**
- Legacy ASP.NET application lifecycle
- Will be replaced by Program.cs

**Modernization Actions:**
1. Delete Global.asax and Global.asax.cs (Effort: 0.1h, Risk: Low)
2. Move initialization to Program.cs (Effort: 0.5h, Risk: Low)

**Code Examples:**

Before:
```csharp
using System.Web.Http;
using ProductsWebAPI.App_Start;

namespace ProductsWebAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            DependencyInjectionSetup.Configure();
        }
    }
}
```

After (Program.cs):
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<ProductsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductsContext")));
builder.Services.AddScoped<IProductsService, ProductsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

### 9. Web.config
**Current Issues:**
- XML-based configuration
- Connection strings in Web.config
- Legacy appSettings

**Modernization Actions:**
1. Create appsettings.json (Effort: 1h, Risk: Low)
2. Migrate connection strings (Effort: 0.5h, Risk: Low)
3. Delete Web.config (Effort: 0.1h, Risk: Low)

**Code Examples:**

Before (Web.config):
```xml
<configuration>
  <connectionStrings>
    <add name="ProductsContext" 
         connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ProductsContext;Integrated Security=True;Connect Timeout=30;Encrypt=False;" 
         providerName="System.Data.SqlClient" />
  </connectionStrings>
  <appSettings>
    <add key="webpages:Version" value="3.0.0.0" />
  </appSettings>
</configuration>
```

After (appsettings.json):
```json
{
  "ConnectionStrings": {
    "ProductsContext": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ProductsContext;Integrated Security=True;Connect Timeout=30;Encrypt=False;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

### 10. ProductsWebAPITest/ProductsControllerTests.cs
**Current Issues:**
- Uses MSTest framework
- Synchronous test methods
- Traditional namespace

**Modernization Actions:**
1. Convert to async test methods (Effort: 1h, Risk: Low)
2. Update mocks for async methods (Effort: 0.5h, Risk: Low)
3. Apply file-scoped namespace (Effort: 0.1h, Risk: Low)
4. Optional: Migrate to xUnit (Effort: 1h, Risk: Low)

**Code Examples:**

Before:
```csharp
[TestMethod]
public void GetProduct_WithValidId_ReturnsOkResult()
{
    // Arrange
    _mockProductService.Setup(s => s.GetProduct(1)).Returns(_testProduct);
    
    // Act
    var result = _controller.GetProduct(1) as OkNegotiatedContentResult<Product>;
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(_testProduct, result.Content);
}
```

After:
```csharp
[TestMethod]
public async Task GetProduct_WithValidId_ReturnsOkResult()
{
    // Arrange
    _mockProductService
        .Setup(s => s.GetProductAsync(1))
        .ReturnsAsync(_testProduct);
    
    // Act
    var result = await _controller.GetProduct(1);
    
    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    Assert.Equal(_testProduct, okResult.Value);
}
```

---

### 11. ProductsWebAPITest.csproj
**Current Issues:**
- Old-style project format
- References packages.config
- Targets .NET Framework 4.8

**Modernization Actions:**
1. Convert to SDK-style project (Effort: 1h, Risk: Medium)
2. Update to net10.0 (Effort: 0.5h, Risk: Medium)
3. Update test packages (Effort: 0.5h, Risk: Low)

**Code Examples:**

Before:
```xml
<Project ToolsVersion="15.0" DefaultTargets="Build">
  <PropertyGroup>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="Microsoft.VisualStudio.TestPlatform.TestFramework">
      <HintPath>..\packages\MSTest.TestFramework.2.2.10\lib\net45\...</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

After:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="MSTest.TestAdapter" Version="3.7.0" />
    <PackageReference Include="MSTest.TestFramework" Version="3.7.0" />
    <PackageReference Include="Moq" Version="4.20.72" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\ProductsWebAPI\ProductsWebAPI.csproj" />
  </ItemGroup>
</Project>
```

---

## Implementation Order

### Phase 1: Framework Migration (CRITICAL - MUST BE FIRST)
**Duration:** 12-16 hours
**Risk:** High

1. **Backup current project** (0.5h)
   - Create git branch for modernization
   - Document current functionality

2. **Convert project files to SDK-style** (3h)
   - ProductsWebAPI.csproj → SDK-style
   - ProductsWebAPITest.csproj → SDK-style
   - Update TargetFramework to net10.0

3. **Migrate packages.config to PackageReference** (2h)
   - Remove packages.config
   - Add PackageReference entries to .csproj
   - Update all packages to .NET 10.0 compatible versions

4. **Migrate Entity Framework 6 to EF Core 10** (4h)
   - Replace EntityFramework with Microsoft.EntityFrameworkCore.SqlServer
   - Update ProductsContext to use EF Core
   - Update ProductsService to use EF Core APIs
   - Test database connectivity

5. **Migrate ASP.NET Web API to ASP.NET Core** (6h)
   - Create Program.cs
   - Remove Global.asax, Web.config
   - Update ProductsController to inherit from ControllerBase
   - Update routing attributes
   - Replace IHttpActionResult with ActionResult<T>
   - Configure built-in DI (remove SimpleInjector)
   - Create appsettings.json

6. **Fix compilation errors** (2h)
   - Resolve namespace changes
   - Fix API differences
   - Update test project references

### Phase 2: Async/Await Conversion
**Duration:** 4-5 hours
**Risk:** Low

1. **Update service layer** (2h)
   - Convert IProductsService to async methods
   - Update ProductsService implementation
   - Use async EF Core methods

2. **Update controller layer** (2h)
   - Convert all controller actions to async
   - Update return types

3. **Update tests** (1h)
   - Convert test methods to async
   - Update mock setups

### Phase 3: Language Modernization
**Duration:** 4-5 hours
**Risk:** Low

1. **Enable nullable reference types** (2h)
   - Add `<Nullable>enable</Nullable>` to .csproj
   - Add nullable annotations to all files
   - Fix nullable warnings

2. **Apply file-scoped namespaces** (1h)
   - Convert all files to file-scoped namespaces

3. **Modernize models** (1h)
   - Add required keyword
   - Consider init-only properties
   - Evaluate record types

4. **Apply pattern matching** (1h)
   - Simplify null checks
   - Use is null/is not null

### Phase 4: Configuration & Testing
**Duration:** 4-5 hours
**Risk:** Low

1. **Configuration modernization** (2h)
   - Finalize appsettings.json
   - Add appsettings.Development.json
   - Configure logging

2. **Testing & validation** (3h)
   - Run all unit tests
   - Manual API testing
   - Database migration testing
   - Performance comparison

---

## Testing Strategy

### Unit Tests to Verify
1. **Controller Tests**
   - All HTTP methods (GET, POST, PUT, DELETE)
   - Validation logic
   - Error handling
   - Async behavior

2. **Service Tests**
   - CRUD operations
   - Database interactions
   - Async operations

3. **Integration Tests** (New)
   - End-to-end API calls
   - Database connectivity
   - DI container resolution

### Manual Testing Checklist
- [ ] GET /api/products - List all products
- [ ] GET /api/products/{id} - Get single product
- [ ] POST /api/products - Create product
- [ ] PUT /api/products/{id} - Update product
- [ ] DELETE /api/products/{id} - Delete product
- [ ] Validation errors return 400
- [ ] Not found returns 404
- [ ] Server errors return 500
- [ ] Database connection works
- [ ] Connection string from appsettings.json

### Performance Testing
- Compare response times before/after
- Monitor memory usage
- Check database query performance

---

## Functionality Preservation

**CRITICAL: All existing behavior must be preserved**

### API Contract Preservation
- All endpoints must remain functional
- Request/response formats unchanged
- HTTP status codes consistent
- Error messages preserved

### Database Compatibility
- Existing database schema unchanged
- Data migration not required
- Connection string format compatible

### Backward Compatibility Requirements
- API consumers should not require changes
- Database queries produce same results
- Business logic unchanged

### Validation Rules
- All validation logic preserved
- Error messages identical
- Status codes consistent

---

## Breaking Changes to Address

### ASP.NET Web API → ASP.NET Core
1. **Routing changes**
   - `[Route]` attributes work differently
   - Route constraints syntax changed

2. **Return types**
   - `IHttpActionResult` → `ActionResult<T>`
   - `Ok()`, `BadRequest()`, etc. work similarly

3. **Dependency Injection**
   - Built-in DI instead of SimpleInjector
   - Constructor injection works the same

### Entity Framework 6 → EF Core
1. **DbContext constructor**
   - Must accept `DbContextOptions<T>`

2. **Async methods**
   - All database operations should be async

3. **LINQ differences**
   - Most queries work the same
   - Some advanced features may differ

---

## Risk Mitigation

### High-Risk Items
1. **Framework migration**
   - Mitigation: Thorough testing, staged rollout
   - Rollback plan: Keep .NET Framework version in separate branch

2. **API breaking changes**
   - Mitigation: Comprehensive integration tests
   - Validation: Contract testing

### Medium-Risk Items
1. **EF Core migration**
   - Mitigation: Test all database operations
   - Validation: Compare query results

### Low-Risk Items
1. **Language features**
   - Mitigation: Compiler catches most issues
   - Validation: Unit tests

---

## Success Criteria

1. ✅ All unit tests pass
2. ✅ All API endpoints functional
3. ✅ Database operations work correctly
4. ✅ No breaking changes to API contract
5. ✅ Performance equal or better
6. ✅ Code compiles without warnings
7. ✅ Nullable reference types enabled
8. ✅ All methods async where appropriate

---

## Post-Migration Enhancements (Optional)

These can be done after successful migration:

1. **Add Swagger/OpenAPI** (2h)
   - Document API endpoints
   - Interactive testing UI

2. **Add health checks** (1h)
   - Database connectivity
   - Application health

3. **Add structured logging** (2h)
   - Serilog or similar
   - Structured log output

4. **Add API versioning** (2h)
   - Support multiple API versions
   - Graceful deprecation

5. **Add response caching** (1h)
   - Cache GET responses
   - Improve performance

6. **Add rate limiting** (1h)
   - Protect against abuse
   - Throttle requests

---

## References

- [Migrate from ASP.NET Web API to ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/migration/webapi)
- [Migrate from Entity Framework 6 to EF Core](https://learn.microsoft.com/en-us/ef/efcore-and-ef6/porting/)
- [.NET 10 Breaking Changes](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- [C# 13 What's New](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13)

---

**Plan Created:** 2025-12-09
**Target Completion:** 24-32 hours of development effort
**Next Step:** Begin Phase 1 - Framework Migration
