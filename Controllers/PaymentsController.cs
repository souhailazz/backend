using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AppartementReservationAPI.Models;
using AppartementReservationAPI.Data;
using AppartementReservationAPI.Services;

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
        public async Task<ActionResult> CreateCheckoutSession(StripeCheckoutRequest request)
        {
            try
            {
                var reservation = await _context.Reservation
                    .Include(r => r.Appartement)
                    .Include(r => r.Client)
                    .FirstOrDefaultAsync(r => r.id_reservation == request.ReservationId);

                if (reservation == null)
                {
                    return NotFound("Reservation not found");
                }

                string description = $"Reservation for {reservation.Appartement.Titre ?? "Apartment"} " +
                                   $"from {reservation.date_depart:d} to {reservation.date_sortie:d}";

                string checkoutUrl = await _stripeService.CreateCheckoutSession(
                    request.ReservationId,
                    description,
                    request.Amount,
                    reservation.Client.Mail
                );

                return Ok(new { Url = checkoutUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating checkout session: {ex.Message}");
            }
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
            catch (Exception ex)  // Fixed the missing exception variable here
            {
                return StatusCode(500, $"Error retrieving session: {ex.Message}");
            }
        }
    }

    public class StripeCheckoutRequest
    {
        public int ReservationId { get; set; }
        public decimal Amount { get; set; }
    }
}