using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppartementReservationAPI.Models;
using AppartementReservationAPI.Data;
using AppartementReservationAPI.Services;
using System.IO;
using Stripe;
using Stripe.Checkout;

namespace AppartementReservationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IStripeService _stripeService;
        private readonly AppDbContext _context;

        public PaymentsController(IStripeService stripeService, AppDbContext context)
        {
            _stripeService = stripeService;
            _context = context;
        }

        // GET: api/payments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Paiement>>> GetPayments()
        {
            return await _context.Paiement.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Paiement>> GetPayment(int id)
        {
            var paiement = await _context.Paiement.FindAsync(id);
            if (paiement == null)
            {
                return NotFound();
            }
            return paiement;
        }

        // POST: api/payments
        [HttpPost]
        public async Task<ActionResult<Paiement>> CreatePayment(Paiement payment)
        {
            if (payment == null)
            {
                return BadRequest("Invalid payment data.");
            }

            if (string.IsNullOrEmpty(payment.payment_code))
            {
                payment.payment_code = "PAY-" + Guid.NewGuid().ToString();
            }

            try
            {
                _context.Paiement.Add(payment);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPayment), new { id = payment.id }, payment);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"An error occurred while creating payment: {ex.InnerException?.Message}");
            }
        }

        [HttpPost("create-checkout-session")]
        public async Task<string> CreateCheckoutSession(
    int reservationId,
    string description,
    decimal amount,
    string customerEmail)
{
    var options = new SessionCreateOptions
    {
        PaymentMethodTypes = new List<string> { "card" },
        LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(amount * 100), // Convert to cents
                    Currency = "eur", // Set your currency
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Apartment Reservation",
                        Description = description
                    }
                },
                Quantity = 1
            }
        },
        Mode = "payment",
        SuccessUrl = "https://clientfrance.netlify.app/payment/success?session_id={CHECKOUT_SESSION_ID}",
        CancelUrl = "https://clientfrance.netlify.app/payment/cancel",
        CustomerEmail = customerEmail,
        Metadata = new Dictionary<string, string>
        {
            { "ReservationId", reservationId.ToString() }
        }
    };

    var service = new SessionService();
    var session = await service.CreateAsync(options);
    return session.Url;
}

        [HttpGet("session-status")]
        public async Task<ActionResult> GetSessionStatus(string sessionId)
        {
            try
            {
                var session = await _stripeService.GetSession(sessionId);
                return Ok(new { 
                    Status = session.PaymentStatus,
                    Amount = session.AmountTotal / 100m,
                    Currency = session.Currency,
                    CustomerEmail = session.CustomerEmail
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving session: {ex.Message}");
            }
        }

        // Handle Stripe webhook for payment confirmation
[HttpPost("webhook")]
public async Task<IActionResult> HandleStripeWebhook()
{
    var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
    try
    {
        var stripeEvent = EventUtility.ConstructEvent(
            json, 
            Request.Headers["Stripe-Signature"], 
            _stripeService.webhookSecret);

        // Handle the event
if (stripeEvent.Type == "checkout.session.completed")
        {
            var session = stripeEvent.Data.Object as Session;
            // Get the reservation ID from metadata
            int reservationId = 0;

            if (session.Metadata.TryGetValue("ReservationId", out string reservationIdStr) &&
    int.TryParse(reservationIdStr, out reservationId))
            {
                // Update reservation status only
                var reservation = await _context.Reservation
                    .Include(r => r.Paiement)
                    .FirstOrDefaultAsync(r => r.id_reservation == reservationId);

                if (reservation != null && reservation.Paiement != null)
                {
                    reservation.etat = "Confirmed";
                    // Update payment_code instead of status
                    reservation.Paiement.payment_code = session.PaymentIntentId;
                    await _context.SaveChangesAsync();
                }
            }
        }
        return Ok();
    }
    catch (Exception ex)
    {
        return BadRequest($"Webhook error: {ex.Message}");
    }
}
    }

    public class StripeCheckoutRequest
    {
        public int ReservationId { get; set; }
        public decimal Amount { get; set; }
    }
}