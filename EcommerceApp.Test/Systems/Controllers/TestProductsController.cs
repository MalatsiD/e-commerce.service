using Ecommerce.API.Controllers;
using Ecommerce.API.RequestHelpers;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Specifications;
using Ecommerce.Test.SampleData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ecommerce.Test.Systems.Controllers
{
    public class TestProductsController
    {
        private readonly Mock<ILogger<ProductsController>> _mockLogger;
        private readonly Mock<IGenericRepository<Product>> _mockRepo;
        private readonly ProductsController _controller;

        public TestProductsController()
        {
            _mockLogger = new Mock<ILogger<ProductsController>>();
            _mockRepo = new Mock<IGenericRepository<Product>>();
            _controller = new ProductsController(_mockRepo.Object, _mockLogger.Object);
        }

        #region Test Get Product List
        [Fact]
        public async Task Get_OnSuccess_ReturnListOfProducts()
        {
            // Arrange
            var specParams = new ProductSpecParams();
            var products = ProductData.GetProducts();

            _mockRepo
            .Setup(repo => repo.ListAsync(It.IsAny<ProductSpecification>()))
            .ReturnsAsync(products);

            _mockRepo
                .Setup(repo => repo.CountAsync(It.IsAny<ProductSpecification>()))
                .ReturnsAsync(products.Count);

            // Act
            var result = await _controller.GetProducts(specParams);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IReadOnlyList<Product>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedProducts = Assert.IsAssignableFrom<Pagination<Product>>(okResult.Value);

            Assert.Equal(products.Count, returnedProducts.Data.Count);
            Assert.Equal(products[0].Name, returnedProducts.Data[0].Name);
        }
        #endregion

        #region Test Get Product
        [Fact]
        public async Task Get_OnSuccess_ReturnProduct()
        {
            // Arrange
            int id = 1;
            var products = ProductData.GetProducts();
            var filteredProduct = products.Find(x => x.Id == id);

            _mockRepo.Setup(service => service.GetByIdAsync(id))
                .ReturnsAsync(filteredProduct);

            // Act
            var result = await _controller.GetProduct(id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedProduct = Assert.IsAssignableFrom<Product>(okResult.Value);

            Assert.Equal(filteredProduct!.Name, returnedProduct.Name);
        }

        [Fact]
        public async Task Get_OnFail_ReturnNotFoud()
        {
            // Arrange
            int id = 1;
            var product = ProductData.GetProducts().Find(x => x.Id == 3);

            _mockRepo.Setup(service => service.GetByIdAsync(id))
                .ReturnsAsync(product);

            // Act
            var result = await _controller.GetProduct(id);

            // Assert

            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }
        #endregion End Get Product

        #region Test Create Product
        [Fact]
        public async Task Add_OnSuccess_ReturnCreated()
        {
            // Arrange
            var product = ProductData.GetProduct(3, "New Product");

            _mockRepo.Setup(service => service.Add(product));

            _mockRepo.Setup(service => service.SaveChangesAsync())
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AddProduct(product);

            //Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var createdProduct = Assert.IsAssignableFrom<Product>(createdResult.Value);

            Assert.Equal(product.Id, createdProduct.Id);
        }

        [Fact]
        public async Task Add_OnFail_ReturnBadRequest()
        {
            // Arrange
            var product = ProductData.GetProduct(3, "Test New Prod");

            _mockRepo.Setup(service => service.Add(product));
            _mockRepo.Setup(service => service.SaveChangesAsync())
                .ReturnsAsync(false);

            // Act
            var result = await _controller.AddProduct(product);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }
        #endregion

        #region Test Update Product
        [Fact]
        public async Task Update_OnSuccess_ReturnUpdated()
        {
            // Arrange
            int id = 1;
            var product = ProductData.GetProducts().Find(x => x.Id == id);
            product!.Name = "Updated Name";

            _mockRepo.Setup(service => service.Exists(id))
                .Returns(true);
            _mockRepo.Setup(service => service.Update(product!));
            _mockRepo.Setup(service => service.SaveChangesAsync())
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateProduct(id, product);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_OnIdFail_ReturnBadRequest()
        {
            // Arrange
            int id = 1;
            var product = ProductData.GetProducts().Find(x => x.Id == 2);

            _mockRepo.Setup(service => service.Exists(id))
                 .Returns(true);

            // Act
            var result = await _controller.UpdateProduct(id, product!);

            // Assert
            var actionResult = Assert.IsAssignableFrom<ActionResult>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult);
        }

        [Fact]
        public async Task Update_OnRecordUpdateFail_ReturnBadRequest()
        {
            // Arrange
            int id = 1;
            var product = ProductData.GetProducts().Find(x => x.Id == 1);

            _mockRepo.Setup(service => service.Exists(id))
                 .Returns(true);
            _mockRepo.Setup(service => service.Update(product!));
            _mockRepo.Setup(service => service.SaveChangesAsync())
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateProduct(id, product!);

            // Assert
            var actionResult = Assert.IsAssignableFrom<ActionResult>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult);
        }
        #endregion

        #region Test Delete Product
        [Fact]
        public async Task Delete_OnSuccess_ReturnNoContent()
        {
            // Arrange
            int id = 1;
            var product = ProductData.GetProducts().Find(x => x.Id == id);

            _mockRepo.Setup(service => service.GetByIdAsync(id))
                .ReturnsAsync(product);
            _mockRepo.Setup(service => service.Delete(product!));
            _mockRepo.Setup(service => service.SaveChangesAsync())
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteProduct(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_OnFail_ResturnNotFound()
        {
            // Arrange
            int id = 1;
            var product = ProductData.GetProducts().Find(x => x.Id == 3);

            _mockRepo.Setup(service => service.GetByIdAsync(id))
                .ReturnsAsync(product);

            // Act 
            var result = await _controller.DeleteProduct(id);

            // Assert
            var actionResult = Assert.IsAssignableFrom<ActionResult>(result);
            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task Delete_OnFail_ResturnBadRequest()
        {
            // Arrange
            int id = 1;
            var product = ProductData.GetProducts().Find(x => x.Id == id);

            _mockRepo.Setup(service => service.GetByIdAsync(id))
                .ReturnsAsync(product);
            _mockRepo.Setup(service => service.Delete(product!));
            _mockRepo.Setup(service => service.SaveChangesAsync())
                .ReturnsAsync(false);

            // Act 
            var result = await _controller.DeleteProduct(id);

            // Assert
            var actionResult = Assert.IsAssignableFrom<ActionResult>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult);
        }
        #endregion

        #region Test Get Brands List
        [Fact]
        public async Task Get_OnSuccess_ReturnListOfBrands()
        {
            // Arrange
            var brands = ProductData.GetBrands();

            _mockRepo.Setup(repo => repo.ListAsync(It.IsAny<BrandListSpecification>()))
            .ReturnsAsync(brands);

            // Act
            var result = await _controller.GetBrands();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IReadOnlyList<string>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var brandsResponse = Assert.IsAssignableFrom<List<string>>(okResult.Value);

            Assert.Equal(brands.Count, brandsResponse.Count);
            Assert.Equal(brands, brandsResponse);
        }
        #endregion

        #region Test Get Types List
        [Fact]
        public async Task Get_OnSuccess_ReturnListOfTypes()
        {
            // Arrange
            var types = ProductData.GetTypes();

            _mockRepo.Setup(repo => repo.ListAsync(It.IsAny<TypeListSpecification>()))
            .ReturnsAsync(types);

            // Act
            var result = await _controller.GetTypes();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IReadOnlyList<string>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var typesResponse = Assert.IsAssignableFrom<List<string>>(okResult.Value);

            Assert.Equal(types.Count, typesResponse.Count);
            Assert.Equal(types, typesResponse);
        }
        #endregion
    }
}
