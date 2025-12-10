using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProductsWebAPI.Controllers;
using ProductsWebAPI.Models;
using ProductsWebAPI.Service;

namespace ProductsApp.Tests.Controllers;

[TestClass]
public class ProductsControllerTests
{
    private Mock<IProductsService> _mockProductService = null!;
    private ProductsController _controller = null!;
    private Product _testProduct = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockProductService = new Mock<IProductsService>();
        _controller = new ProductsController(_mockProductService.Object);
        _testProduct = new Product { Id = 1, Name = "Test Product", Category = "Test", Price = 10.0m };
    }

    #region Constructor Tests
    [TestMethod]
    public void Constructor_WithValidService_CreatesController()
    {
        Assert.IsNotNull(_controller);
    }

    [TestMethod]
    public void Constructor_WithNullService_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new ProductsController(null!));
    }
    #endregion

    #region ListProducts Tests
    [TestMethod]
    public async Task ListProducts_ReturnsOkResultWithProducts()
    {
        var expectedProducts = new List<Product>
        {
            new Product { Id = 1, Name = "Test Product 1", Category = "Test", Price = 10.0m },
            new Product { Id = 2, Name = "Test Product 2", Category = "Test", Price = 20.0m }
        };
        _mockProductService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(expectedProducts);

        var result = await _controller.ListProducts();

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(expectedProducts, okResult.Value);
    }

    [TestMethod]
    public async Task ListProducts_WhenExceptionOccurs_ReturnsInternalServerError()
    {
        _mockProductService.Setup(s => s.GetAllProductsAsync()).ThrowsAsync(new Exception("Test exception"));

        var result = await _controller.ListProducts();

        var statusResult = result.Result as ObjectResult;
        Assert.IsNotNull(statusResult);
        Assert.AreEqual(500, statusResult.StatusCode);
    }
    #endregion

    #region GetProduct Tests
    [TestMethod]
    public async Task GetProduct_WithValidId_ReturnsOkResult()
    {
        _mockProductService.Setup(s => s.GetProductAsync(1)).ReturnsAsync(_testProduct);

        var result = await _controller.GetProduct(1);

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(_testProduct, okResult.Value);
    }

    [TestMethod]
    public async Task GetProduct_WithInvalidId_ReturnsBadRequest()
    {
        var result = await _controller.GetProduct(0);

        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual("Invalid product ID. ID must be greater than 0.", badRequestResult.Value);
    }

    [TestMethod]
    public async Task GetProduct_WithNonexistentId_ReturnsNotFound()
    {
        _mockProductService.Setup(s => s.GetProductAsync(1)).ReturnsAsync((Product?)null);

        var result = await _controller.GetProduct(1);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
    }
    #endregion

    #region CreateProduct Tests
    [TestMethod]
    public async Task CreateProduct_WithValidProduct_ReturnsCreatedResult()
    {
        var result = await _controller.CreateProduct(_testProduct);

        var createdResult = result.Result as CreatedAtActionResult;
        Assert.IsNotNull(createdResult);
        Assert.AreEqual(_testProduct, createdResult.Value);
        _mockProductService.Verify(s => s.SaveProductAsync(_testProduct), Times.Once);
    }

    [TestMethod]
    public async Task CreateProduct_WithNullProduct_ReturnsBadRequest()
    {
        var result = await _controller.CreateProduct(null!);

        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual("Product data cannot be null", badRequestResult.Value);
    }
    #endregion

    #region UpdateProduct Tests
    [TestMethod]
    public async Task UpdateProduct_WithValidProduct_ReturnsOkResult()
    {
        _mockProductService.Setup(s => s.GetProductAsync(1)).ReturnsAsync(_testProduct);

        var result = await _controller.UpdateProduct(1, _testProduct);

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(_testProduct, okResult.Value);
    }
    #endregion

    #region DeleteProduct Tests
    [TestMethod]
    public async Task DeleteProduct_WithValidId_ReturnsOkResult()
    {
        _mockProductService.Setup(s => s.GetProductAsync(1)).ReturnsAsync(_testProduct);

        var result = await _controller.DeleteProduct(1);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        _mockProductService.Verify(s => s.DeleteProductAsync(1), Times.Once);
    }
    #endregion
}
