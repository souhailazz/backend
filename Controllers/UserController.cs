using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppartementReservationAPI.Data; // Assuming your DbContext is here
using AppartementReservationAPI.Models; // Assuming your Client model is here
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context; // Inject the DbContext
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] Client client)
    {
        if (client == null || string.IsNullOrEmpty(client.Mail) || string.IsNullOrEmpty(client.password))
        {
            return BadRequest("Invalid client data.");
        }

        var existingClient = await _context.Client.FirstOrDefaultAsync(c => c.Mail == client.Mail);
        if (existingClient != null)
        {
            return Conflict("Email is already registered.");
        }

        try
        {
            // Add the new client to the database
            _context.Client.Add(client);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log the error (consider using a logging framework)
            Console.WriteLine($"Error during signup: {ex.Message}");
            return StatusCode(500, "Internal server error. Please try again later.");
        }

        return Ok("User registered successfully.");
    }
[HttpGet("email")]
public async Task<IActionResult> GetUserEmail()
{
    // Try to get the client ID from the session
    int? clientId = HttpContext.Session.GetInt32("ClientId");
    
    if (!clientId.HasValue)
    {
        return Unauthorized("User not logged in.");
    }

    try
    {
        var client = await _context.Client.FindAsync(clientId.Value);
        
        if (client == null)
        {
            return NotFound("User not found.");
        }

        return Ok(new { email = client.Mail });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving user email: {ex.Message}");
        return StatusCode(500, "Internal server error while retrieving user data.");
    }
}
    [HttpPost("login")]
public async Task<IActionResult> Login([FromBody] Client client)
{
    if (client == null || string.IsNullOrEmpty(client.Mail) || string.IsNullOrEmpty(client.password))
    {
        return BadRequest("Invalid login data.");
    }

    var existingClient = await _context.Client
        .FirstOrDefaultAsync(c => c.Mail == client.Mail && c.password == client.password);

    if (existingClient == null)
    {
        return Unauthorized("Invalid credentials.");
    }

    HttpContext.Session.SetInt32("ClientId", existingClient.Id);

    return Ok(new { id = existingClient.Id, message = "Login successful." });
}
}