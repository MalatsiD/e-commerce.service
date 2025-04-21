using Ecommerce.API.Extensions;
using Ecommerce.API.SignalR;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Entities.OrderAggregate;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stripe;

namespace Ecommerce.API.Controllers
{
    public class PaymentsController (IPaymentService _paymentService, 
        IUnitOfWork _unit, ILogger<PaymentsController> _logger, 
        IConfiguration _config, IHubContext<NotificationHub> _hubContext) : BaseApiController
    {
        private readonly string _whSecret = _config["StripeSettings:WhSecret"]!;

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

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = ConstructStripeEvent(json);

                if(stripeEvent.Data.Object is not PaymentIntent intent)
                {
                    return BadRequest("Invalid event data");
                }

                await HandlePaymentIntentSucceeded(intent);

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError("Stripe webhook error");
                return StatusCode(StatusCodes.Status500InternalServerError, "Webhook error");
            }
            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occured");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occured");
            }
        }

        private async Task HandlePaymentIntentSucceeded(PaymentIntent intent)
        {
            if (intent.Status == "succeeded")
            {
                var spec = new OrderSpecification(intent.Id, true);
                var order = await _unit.Repository<Order>().GetEntityWithSpecAsync(spec)
                    ?? throw new Exception("Order not found");

                if((long)order.GetTotal() * 100 != intent.Amount)
                {
                    order.Status = OrderStatus.PaymentMismatch;
                }
                else
                {
                    order.Status = OrderStatus.PaymentReceived;
                }

                await _unit.Complete();

                var connectionId = NotificationHub.GetConnectionIdByEmail(order.BuyerEmail);

                if (!string.IsNullOrEmpty(connectionId))
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("OrderCompleteNotification", order.ToDto());
                }
            }
        }

        private Event ConstructStripeEvent(string json)
        {
            try
            {
                return EventUtility.ConstructEvent(
                    json, 
                    Request.Headers["Stripe-Signature"], 
                    _whSecret,
                    throwOnApiVersionMismatch: false
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to construct stripe event");
                throw new StripeException("Invalid signature");
            }
        }
    }
}
