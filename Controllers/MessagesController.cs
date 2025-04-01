using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppartementReservationAPI.Data; // Assuming your DbContext is here
using AppartementReservationAPI.Models; // Assuming your Reservation model is here
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/message")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly AppDbContext _context;

    public MessagesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] MessageDto messageDto)
    {
        try
        {
            var message = new Message
            {
                ReservationId = messageDto.ReservationId,
                SenderId = messageDto.SenderId,
                ReceiverId = messageDto.ReceiverId,
                Content = messageDto.Content
            };

            _context.Message.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message sent successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error sending message: " + ex.Message);
        }
    }

    [HttpGet("{reservationId}")]
    public async Task<IActionResult> GetMessages(int reservationId)
    {
        var messages = await _context.Message
            .Where(m => m.ReservationId == reservationId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return Ok(messages);
    }

    // Correction: sessionId doit être un paramètre de l'URL et non du JavaScript
    [HttpGet("sender/{senderId}")]
    public async Task<IActionResult> GetChatsBySender(int senderId)
    {
        try
        {
            var messages = await _context.Message
                .Where(m => m.SenderId == senderId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            if (!messages.Any())
            {
                return NotFound(new { message = "No messages found for this sender." });
            }

            return Ok(messages);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching messages: {ex.Message}");
            return StatusCode(500, "An error occurred while retrieving messages.");
        }
    }
[HttpGet("chats/{sessionId}")]
public async Task<IActionResult> GetUserChats(int sessionId)
{
    try
    {
        // Get all reservations where the user is either sender or receiver
        var reservationIds = await _context.Message
            .Where(m => m.SenderId == sessionId || m.ReceiverId == sessionId)
            .Select(m => m.ReservationId)
            .Distinct()
            .ToListAsync();

        var chats = new List<object>();
        foreach (var reservationId in reservationIds)
        {
            // Get first message of conversation to determine participants
            var firstMessage = await _context.Message
                .Where(m => m.ReservationId == reservationId)
                .OrderBy(m => m.SentAt)
                .FirstOrDefaultAsync();

            if (firstMessage != null)
            {
                chats.Add(new
                {
                    ReservationId = reservationId,
                    ClientId = firstMessage.SenderId,
                    AdminId = firstMessage.ReceiverId,
                    // Include last message timestamp for sorting (optional)
                    LastMessageTime = await _context.Message
                        .Where(m => m.ReservationId == reservationId)
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => m.SentAt)
                        .FirstOrDefaultAsync()
                });
            }
        }

        // Sort by most recent activity
        chats = chats.OrderByDescending(c => ((dynamic)c).LastMessageTime).ToList();

        return Ok(chats);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error fetching chats: {ex.Message}");
        return StatusCode(500, "An error occurred while retrieving chats.");
    }
}

}
