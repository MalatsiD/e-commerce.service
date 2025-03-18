using Ecommerce.API.RequestHelpers;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IGenericRepository<Product> _repo, ILogger<ProductsController> _logger) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts([FromQuery] ProductSpecParams specParams)
        {
            _logger.LogInformation("Starting request {@RequestName}, {@DataRecieved}, {@DateTimeUct}",
                typeof(ProductsController).Name, specParams, DateTime.UtcNow);

            var spec = new ProductSpecification(specParams);

            var result = await CreatePagedResult(_repo, spec, specParams.PageIndex, specParams.PageSize);

            _logger.LogInformation("Ending request {@RequestName}, {@DateTimeUct}",
                typeof(ProductsController).Name, DateTime.UtcNow);

            return result;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            _logger.LogInformation("Starting request {@RequestName}, {@DataRecieved}, {@DateTimeUct}",
                typeof(ProductsController).Name, id, DateTime.UtcNow);

            var product = await _repo.GetByIdAsync(id);

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

            _repo.Add(product);
            var isCreated = await _repo.SaveChangesAsync();

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

            _repo.Update(product);
            var isUpdated = await _repo.SaveChangesAsync();

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

            var product = await _repo.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _repo.Delete(product);
            var isDeleted = await _repo.SaveChangesAsync();

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

            var result = await _repo.ListAsync(spec);

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

            var result = await _repo.ListAsync(spec);

            _logger.LogInformation("Ending request {@RequestName}, {@DateTimeUct}",
                typeof(ProductsController).Name, DateTime.UtcNow);

            return Ok(result);
        }

        private bool ProductExists(int id)
        {
            return _repo.Exists(id);
        }
    }
}
