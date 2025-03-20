using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController (ICartService cartService) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<ShoppingCart>> GetCart(string id)
        {
            var cart = await cartService.GetCartAsync(id);

            return Ok(cart ?? new ShoppingCart { Id = id});
        }

        [HttpPost]
        public async Task<ActionResult<ShoppingCart>> UpdateCart(ShoppingCart shoppingCart)
        {
            var cart = await cartService.SetCartAsync(shoppingCart);

            if (cart == null) return BadRequest("Problem with cart");

            return Ok(cart);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteCart(string id)
        {
            var isDeleted = await cartService.DeleteCartAsync(id);

            if (!isDeleted) return BadRequest("Problem deleting cart");

            return Ok();
        }
    }
}
