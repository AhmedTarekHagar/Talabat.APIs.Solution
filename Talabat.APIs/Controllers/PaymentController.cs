using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.IO;
using System;
using System.Threading.Tasks;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.Core.Entities;
using Talabat.Core.IServices;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : BaseApiController
    {
        private readonly IPaymentService _paymentService;
        private const string _webhookSecret = "whsec_685a25c9d33ec67990c5fc1c06917721f3496774baae5faf414baae739dc9cb4";

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [Authorize]
        [HttpPost("{basketId}")]
        public async Task<ActionResult<CustomerBasketDto>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);

            if (basket == null) return BadRequest(new ApiResponse(400, "Basket Empty"));

            return Ok(basket);
        }

        [HttpPost("webhook")]
        public async Task<ActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"], _webhookSecret);

            PaymentIntent intent;

            Order order;

            // Handle the event
            switch (stripeEvent.Type)
            {
                case Events.PaymentIntentSucceeded:
                    intent = (PaymentIntent)stripeEvent.Data.Object;
                    order = await _paymentService.UpdatePaymentIntentToSucceededOrFailed(intent.Id, true);
                    break;
                case Events.PaymentIntentPaymentFailed:
                    intent = (PaymentIntent)stripeEvent.Data.Object;
                    order = await _paymentService.UpdatePaymentIntentToSucceededOrFailed(intent.Id, false);
                    break;
            }

            return new EmptyResult();
        }

    }
}
