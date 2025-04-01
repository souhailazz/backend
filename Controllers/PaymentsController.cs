using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AppartementReservationAPI.Models;
using AppartementReservationAPI.Data;

namespace AppartementReservationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
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

    // Ensure payment_code is unique
    if (string.IsNullOrEmpty(payment.payment_code))
    {
        payment.payment_code = "PAYMENT-" + Guid.NewGuid().ToString(); // Generate a unique payment code
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

        

    }
    
    
}
