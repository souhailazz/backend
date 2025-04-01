using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppartementReservationAPI.Data; // Assuming your DbContext is here
using AppartementReservationAPI.Models; // Assuming your Reservation model is here
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ReservationController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReservationController(AppDbContext context)
    {
        _context = context; // Inject the DbContext
    }

    // GET: api/reservations/{userId}
    [HttpGet("reservations/{userId}")]
    public async Task<IActionResult> GetReservations(int userId)
    {
        // Fetch reservations for the specified user
        var reservations = await _context.Reservation
            .Where(r => r.id_client == userId) // Filter by userId
            .Include(r => r.Client) // Include related Client data
            .Include(r => r.Appartement) // Include related Appartement data
            .Include(r => r.Paiement) // Include related Paiement data
            .ToListAsync();

        // Check if any reservations were found
        if (reservations == null || !reservations.Any())
        {
            return NotFound("No reservations found for this user.");
        }

        // Return the list of reservations
        return Ok(reservations);
    }

      [HttpPost]
public async Task<IActionResult> CreateReservation([FromBody] ReservationDto reservationDto)
{
    try
    {
        // Basic validation
        if (reservationDto == null)
        {
            return BadRequest("Invalid reservation data");
        }

        // Create a new reservation from DTO
        var newReservation = new Reservation
        {
            id_client = reservationDto.id_client,
            id_appartement = reservationDto.id_appartement,
            date_depart = reservationDto.date_depart,
            date_sortie = reservationDto.date_sortie,
            etat = "Pending",
            nbr_adultes = reservationDto.nbr_adultes,
            nbr_enfants = reservationDto.nbr_enfants,
            animaux = reservationDto.animaux,
            id_paiement = reservationDto.id_paiement,  // Set default payment ID
        };

        // Validate client exists
        var client = await _context.Client.FindAsync(newReservation.id_client);
        if (client == null)
        {
            return BadRequest($"Client with ID {newReservation.id_client} not found");
        }

        var hasConflict = await _context.Reservation
            .AnyAsync(r => 
                r.id_appartement == newReservation.id_appartement &&
                r.date_depart <= newReservation.date_sortie &&
                r.date_sortie >= newReservation.date_depart);

        if (hasConflict)
        {
            return BadRequest("The selected dates are not available for this apartment");
        }

        // Add the new reservation
        _context.Reservation.Add(newReservation);
        await _context.SaveChangesAsync();

        // Return success response
        return Ok(new
        {
            message = "Reservation created successfully",
            reservation = new
            {
                id = newReservation.id_reservation,
                checkIn = newReservation.date_depart,
                checkOut = newReservation.date_sortie,
                status = newReservation.etat
            }
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating reservation: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
        return StatusCode(500, "An error occurred while creating the reservation. Please try again later.");
    }
}
    
    [HttpGet("{id}")]
public async Task<IActionResult> GetReservation(int id)
{
    try
    {
        var reservation = await _context.Reservation.FindAsync(id);
        
        if (reservation == null)
        {
            return NotFound(new { message = "Reservation not found" });
        }
        
        return Ok(reservation);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Error fetching reservation: " + ex.Message });
    }
}
[HttpPut("{id}/status")]
public async Task<IActionResult> UpdateReservationStatus(int id, [FromBody] ReservationStatusDto statusDto)
{
    try
    {
        var reservation = await _context.Reservation.FindAsync(id);
        
        if (reservation == null)
        {
            return NotFound(new { message = "Reservation not found" });
        }
        
        reservation.etat = statusDto.Etat;
        
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Reservation status updated successfully" });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Error updating reservation status: " + ex.Message });
    }
}

[HttpGet("recent/{userId}")]
public async Task<IActionResult> GetRecentReservation(string userId)
{
    try
    {
        // Convert userId to int
        if (!int.TryParse(userId, out int userIdInt))
        {
            return BadRequest(new { message = "Invalid user ID format" });
        }

        var reservation = await _context.Reservation
            .Where(r => r.id_client == userIdInt) // Use the converted int value
            .OrderByDescending(r => r.id_reservation) // Use the correct property name
            .FirstOrDefaultAsync();
        
        if (reservation == null)
        {
            return NotFound(new { message = "No reservation found for this user" });
        }
        
        return Ok(reservation);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "An error occurred", details = ex.Message });
    }
}


}