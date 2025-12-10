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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> ListProducts()
    {
        try
        {
            var products = await _productsService.GetAllProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid product ID. ID must be greater than 0.");

        try
        {
            var product = await _productsService.GetProductAsync(id);
            return product is null ? NotFound() : Ok(product);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] Product value)
    {
        if (value is null)
            return BadRequest("Product data cannot be null");

        try
        {
            await _productsService.SaveProductAsync(value);
            return CreatedAtAction(nameof(GetProduct), new { id = value.Id }, value);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Product>> UpdateProduct(int id, [FromBody] Product newValue)
    {
        if (id <= 0)
            return BadRequest("Invalid product ID. ID must be greater than 0.");

        if (newValue is null)
            return BadRequest("Product data cannot be null");

        try
        {
            var existingProduct = await _productsService.GetProductAsync(id);
            if (existingProduct is null)
                return NotFound();

            await _productsService.UpdateProductAsync(id, newValue);
            return Ok(newValue);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid product ID. ID must be greater than 0.");

        try
        {
            await _productsService.DeleteProductAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}

