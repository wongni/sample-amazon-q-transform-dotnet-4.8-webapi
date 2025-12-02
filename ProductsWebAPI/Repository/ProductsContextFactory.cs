using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProductsWebAPI.Repository;

public class ProductsContextFactory : IDesignTimeDbContextFactory<ProductsContext>
{
    public ProductsContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProductsContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=ProductsContext");
        return new ProductsContext(optionsBuilder.Options);
    }
}
