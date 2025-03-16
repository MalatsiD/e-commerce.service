using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Specifications;
using Ecommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IGenericRepository<Product> _repo) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(string? brand, string? type, string? sort)
        {
            var spec = new ProductSpecification(brand, type, sort);

            var result = await _repo.ListAsync(spec);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _repo.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct(Product product)
        {
            _repo.Add(product);
            var isCreated = await _repo.SaveChangesAsync();

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

            _repo.Update(product);
            var isUpdated = await _repo.SaveChangesAsync();

            if (!isUpdated)
            {
                return BadRequest("Problem updating the product");
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
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

            return NoContent();
        }

        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
        {
            // To Do

            return Ok();
        }

        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
        {
            // To Do

            return Ok();
        }

        private bool ProductExists(int id)
        {
            return _repo.Exists(id);
        }
    }
}
