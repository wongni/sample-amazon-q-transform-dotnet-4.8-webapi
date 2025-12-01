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

    [TestMethod]
    public void ListProducts_ReturnsOkResultWithProducts()
    {
        var expectedProducts = new List<Product>
        {
            new Product { Id = 1, Name = "Test Product 1", Category = "Test", Price = 10.0m },
            new Product { Id = 2, Name = "Test Product 2", Category = "Test", Price = 20.0m }
        };
        _mockProductService.Setup(s => s.GetAllProducts()).Returns(expectedProducts);

        var result = _controller.ListProducts() as OkObjectResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
    }

    [TestMethod]
    public void ListProducts_WhenExceptionOccurs_ReturnsInternalServerError()
    {
        _mockProductService.Setup(s => s.GetAllProducts()).Throws(new Exception());

        var result = _controller.ListProducts() as ObjectResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(500, result.StatusCode);
    }

    [TestMethod]
    public void GetProduct_WithValidId_ReturnsOkResult()
    {
        _mockProductService.Setup(s => s.GetProduct(1)).Returns(_testProduct);

        var result = _controller.GetProduct(1) as OkObjectResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(_testProduct, result.Value);
    }

    [TestMethod]
    public void GetProduct_WithInvalidId_ReturnsBadRequest()
    {
        var result = _controller.GetProduct(0) as BadRequestObjectResult;

        Assert.IsNotNull(result);
        Assert.AreEqual("Invalid product ID. ID must be greater than 0.", result.Value);
    }

    [TestMethod]
    public void GetProduct_WithNonexistentId_ReturnsNotFound()
    {
        _mockProductService.Setup(s => s.GetProduct(1)).Returns((Product?)null);

        var result = _controller.GetProduct(1) as NotFoundResult;

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void CreateProduct_WithValidProduct_ReturnsCreatedResult()
    {
        var result = _controller.CreateProduct(_testProduct) as CreatedResult;

        Assert.IsNotNull(result);
        _mockProductService.Verify(s => s.SaveProduct(_testProduct), Times.Once);
    }

    [TestMethod]
    public void CreateProduct_WithNullProduct_ReturnsBadRequest()
    {
        var result = _controller.CreateProduct(null!) as BadRequestObjectResult;

        Assert.IsNotNull(result);
        Assert.AreEqual("Product data cannot be null", result.Value);
    }

    [TestMethod]
    public void UpdateProduct_WithValidProduct_ReturnsOkResult()
    {
        _mockProductService.Setup(s => s.GetProduct(1)).Returns(_testProduct);

        var result = _controller.UpdateProduct(1, _testProduct) as OkObjectResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(_testProduct, result.Value);
    }

    [TestMethod]
    public void DeleteProduct_WithValidId_ReturnsOkResult()
    {
        _mockProductService.Setup(s => s.GetProduct(1)).Returns(_testProduct);

        var result = _controller.DeleteProduct(1) as OkResult;

        Assert.IsNotNull(result);
        _mockProductService.Verify(s => s.DeleteProduct(1), Times.Once);
    }
}
