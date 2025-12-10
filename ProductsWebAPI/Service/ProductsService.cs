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
    
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _context.Products.ToArrayAsync();
    }

    public async Task<Product?> GetProductAsync(int id)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task SaveProductAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProductAsync(int id, Product product)
    {
        var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (existingProduct is null)
            return;

        existingProduct.Name = product.Name;
        existingProduct.Price = product.Price;
        existingProduct.Category = product.Category;

        await _context.SaveChangesAsync();
    }
}
