using Ecommerce.API.RequestHelpers;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IGenericRepository<Product> _repo) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts([FromQuery] ProductSpecParams specParams)
        {
            var spec = new ProductSpecification(specParams);

            var result = await CreatePagedResult(_repo, spec, specParams.PageIndex, specParams.PageSize);

            return result;
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
            var spec = new BrandListSpecification();

            var result = await _repo.ListAsync(spec);

            return Ok(result);
        }

        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
        {
            var spec = new TypeListSpecification();

            var result = await _repo.ListAsync(spec);

            return Ok(result);
        }

        private bool ProductExists(int id)
        {
            return _repo.Exists(id);
        }
    }
}
