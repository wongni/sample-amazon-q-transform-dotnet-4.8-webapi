using System;
using Microsoft.AspNetCore.Mvc;
using ProductsWebAPI.Models;
using ProductsWebAPI.Service;

namespace ProductsWebAPI.Controllers
{
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
        public IActionResult ListProducts()
        {
            try
            {
                var products = _productsService.GetAllProducts();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult GetProduct(int id)
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
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public IActionResult CreateProduct([FromBody] Product value)
        {
            if (value == null)
            {
                return BadRequest("Product data cannot be null");
            }

            try
            {
                _productsService.SaveProduct(value);
                return CreatedAtAction(nameof(GetProduct), new { id = value.Id }, value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product newValue)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid product ID. ID must be greater than 0.");
            }

            if (newValue == null)
            {
                return BadRequest("Product data cannot be null");
            }

            try
            {
                var existingProduct = _productsService.GetProduct(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                _productsService.UpdateProduct(id, newValue);
                return Ok(newValue);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public IActionResult DeleteProduct(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid product ID. ID must be greater than 0.");
            }

            try
            {
                _productsService.DeleteProduct(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

