using ProductsWebAPI.Models;

namespace ProductsWebAPI.Service;

public interface IProductsService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductAsync(int id);
    Task SaveProductAsync(Product product);
    Task DeleteProductAsync(int id);
    Task UpdateProductAsync(int id, Product product);
}

