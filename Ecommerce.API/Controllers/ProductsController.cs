using Ecommerce.API.RequestHelpers;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    public class ProductsController(IUnitOfWork _unit, ILogger<ProductsController> _logger) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts([FromQuery] ProductSpecParams specParams)
        {
            _logger.LogInformation("Starting request {@RequestName}, {@DataRecieved}, {@DateTimeUct}",
                typeof(ProductsController).Name, specParams, DateTime.UtcNow);

            var spec = new ProductSpecification(specParams);

            var result = await CreatePagedResult(_unit.Repository<Product>(), spec, specParams.PageIndex, specParams.PageSize);

            _logger.LogInformation("Ending request {@RequestName}, {@DateTimeUct}",
                typeof(ProductsController).Name, DateTime.UtcNow);

            return result;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            _logger.LogInformation("Starting request {@RequestName}, {@DataRecieved}, {@DateTimeUct}",
                typeof(ProductsController).Name, id, DateTime.UtcNow);

            var product = await _unit.Repository<Product>().GetByIdAsync(id);

            if (product == null)
                return NotFound();

            _logger.LogInformation("Ending request {@RequestName}, {@DateTimeUct}",
                typeof(ProductsController).Name, DateTime.UtcNow);

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct(Product product)
        {
            _logger.LogInformation("Starting request {@RequestName}, {@DataRecieved}, {@DateTimeUct}",
                typeof(ProductsController).Name, product, DateTime.UtcNow);

            _unit.Repository<Product>().Add(product);
            var isCreated = await _unit.Complete();

            if (!isCreated)
            {
                return BadRequest("Problem creating product");
            }

            _logger.LogInformation("Ending request {@RequestName}, {@DateTimeUct}",
                typeof(ProductsController).Name, DateTime.UtcNow);

            return CreatedAtAction("GetProduct", new {id = product.Id}, product);  
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateProduct(int id, Product product)
        {
            _logger.LogInformation("Starting request {@RequestName}, {@DataRecieved}, {@DateTimeUct}",
                typeof(ProductsController).Name, new { id, product }, DateTime.UtcNow);

            if (product.Id != id || !ProductExists(id))
            {
                return BadRequest("Cannot update this product");
            }

            _unit.Repository<Product>().Update(product);
            var isUpdated = await _unit.Complete();

            if (!isUpdated)
            {
                return BadRequest("Problem updating the product");
            }

            _logger.LogInformation("Ending request {@RequestName}, {@DateTimeUct}",
                typeof(ProductsController).Name, DateTime.UtcNow);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation("Starting request {@RequestName}, {@DataRecieved}, {@DateTimeUct}",
                typeof(ProductsController).Name, id, DateTime.UtcNow);

            var product = await _unit.Repository<Product>().GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _unit.Repository<Product>().Delete(product);
            var isDeleted = await _unit.Complete();

            if(!isDeleted)
            {
                return BadRequest("Ploblem deleting a product");
            }

            _logger.LogInformation("Ending request {@RequestName}, {@DateTimeUct}",
                typeof(ProductsController).Name, DateTime.UtcNow);

            return NoContent();
        }

        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
        {
            _logger.LogInformation("Starting request {@RequestName}, {@DataRecieved}, {@DateTimeUct}",
                typeof(ProductsController).Name, string.Empty, DateTime.UtcNow);

            var spec = new BrandListSpecification();

            var result = await _unit.Repository<Product>().ListAsync(spec);

            _logger.LogInformation("Ending request {@RequestName}, {@DateTimeUct}",
                typeof(ProductsController).Name, DateTime.UtcNow);

            return Ok(result);
        }

        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
        {
            _logger.LogInformation("Starting request {@RequestName}, {@DataRecieved}, {@DateTimeUct}",
                typeof(ProductsController).Name, string.Empty, DateTime.UtcNow);

            var spec = new TypeListSpecification();

            var result = await _unit.Repository<Product>().ListAsync(spec);

            _logger.LogInformation("Ending request {@RequestName}, {@DateTimeUct}",
                typeof(ProductsController).Name, DateTime.UtcNow);

            return Ok(result);
        }

        private bool ProductExists(int id)
        {
            return _unit.Repository<Product>().Exists(id);
        }
    }
}
