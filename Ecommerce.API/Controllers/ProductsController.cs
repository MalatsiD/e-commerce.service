using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Ecommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(string? brand, string? type, string? sort)
        {
            var result = await _productRepository.GetProductsAsync(brand, type, sort);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct(Product product)
        {
            _productRepository.AddProduct(product);
            var isCreated = await _productRepository.SaveChangesAsync();

            if (!isCreated)
            {
                return BadRequest("Problem creating product");
            }

            return CreatedAtAction("GetProduct", new {id = product.Id}, product);  
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateProduct(int id, Product product)
        {
            if (product.Id != id || !ProductExists(id))
            {
                return BadRequest("Cannot update this product");
            }

            _productRepository.UpdateProduct(product);
            var isUpdated = await _productRepository.SaveChangesAsync();

            if (!isUpdated)
            {
                return BadRequest("Problem updating the product");
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _productRepository.DeleteProduct(product);
            var isDeleted = await _productRepository.SaveChangesAsync();

            if(!isDeleted)
            {
                return BadRequest("Ploblem deleting a product");
            }

            return NoContent();
        }

        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
        {
            var result = await _productRepository.GetBrandsAsync();

            return Ok(result);
        }

        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
        {
            var result = await _productRepository.GetTypesAsync();

            return Ok(result);
        }

        private bool ProductExists(int id)
        {
            return _productRepository.ProductExists(id);
        }
    }
}
