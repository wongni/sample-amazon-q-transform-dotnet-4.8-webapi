using Microsoft.EntityFrameworkCore;
using ProductsWebAPI.Models;

namespace ProductsWebAPI.Repository
{
    public class ProductsContext : DbContext
    {
        public ProductsContext(DbContextOptions<ProductsContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
