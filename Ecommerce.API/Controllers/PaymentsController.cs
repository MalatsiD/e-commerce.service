using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    public class PaymentsController (IPaymentService _paymentService, IUnitOfWork _unit) : BaseApiController
    {
        [Authorize]
        [HttpPost("{cardId}")]
        public async Task<ActionResult<ShoppingCart>> CreateOrUpdatePaymentIntent(string cardId)
        {
            var cart = await _paymentService.CreateOrUpdatePaymentIntent(cardId);

            if (cart == null) return BadRequest("Problem with your cart");

            return Ok(cart);
        }

        [HttpGet("delivery-methods")]
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
        {
            return Ok(await _unit.Repository<DeliveryMethod>().GetAllAsync());
        }
    }
}
