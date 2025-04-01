using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppartementReservationAPI.Data;
using AppartementReservationAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace AppartementReservationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApartmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApartmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Apartments
        [HttpGet]
        public async Task<IActionResult> GetApartments(
            [FromQuery] string? location = null,
            [FromQuery] int? adults = null,
            [FromQuery] int? children = null,
            [FromQuery] bool? pets = null)
        {
            // Log the request
            Console.WriteLine("Received request for apartments");

            try
            {
                var query = _context.Appartements
                    .Include(a => a.Photos) // Ensure related photos are included
                    .AsQueryable();

                // Location filter (case-insensitive search)
                if (!string.IsNullOrEmpty(location))
                {
                    query = query.Where(a => a.Ville != null && EF.Functions.Like(a.Ville, $"%{location}%"));
                }

                // Adults filter
                if (adults.HasValue && adults.Value > 0)
                {
                    query = query.Where(a => a.NbrAdultes >= adults.Value);
                }

                // Children filter
                if (children.HasValue && children.Value > 0)
                {
                    query = query.Where(a => a.NbrEnfants >= children.Value);
                }

                // Pets filter
                if (pets.HasValue)
                {
                    query = query.Where(a => a.AccepteAnimaux == pets.Value);
                }

                // Log the final query
                Console.WriteLine($"Final query: {query.ToQueryString()}");

                // Execute the query
                var results = await query.ToListAsync();

                if (!results.Any())
                {
                    return NoContent();  // No apartments match the criteria, but it's not an error.
                }

                // Log the results to verify the Photos property
                Console.WriteLine($"Results: {JsonSerializer.Serialize(results)}");

                return Ok(results);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error fetching apartments: {ex.Message}");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // GET: api/Apartments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetApartmentById(int id)
        {
            var apartment = await _context.Appartements
                .Include(a => a.Photos)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apartment == null)
            {
                return NotFound($"Apartment with id {id} not found.");
            }

            // Transformer les données pour ne renvoyer que les URLs des photos
            var result = new
            {
                apartment.Id,
                apartment.Titre,
                apartment.Description,
                apartment.Adresse,
                apartment.Ville,
                apartment.Prix,
                apartment.Capacite,
                apartment.NbrAdultes,
                apartment.NbrEnfants,
                apartment.AccepteAnimaux,
                apartment.Latitude,
                apartment.Longitude,
                apartment.frais_menage,
                apartment.max_animaux,
                apartment.surface,
                apartment.balcon_surface,
                apartment.chauffage,
                apartment.wifi,
                apartment.television,
                apartment.lave_Linge,
                apartment.seche_cheveux,
                apartment.cuisine_equipee,
                apartment.parking_payant,
                apartment.petit_dejeuner_inclus,
                apartment.lit_parapluie,
                apartment.menage_disponible,
                apartment.nombre_min_nuits,
                apartment.remise_semaine,
                apartment.remise_mois,
                apartment.checkin_heure,
                apartment.checkout_heure,
                apartment.politique_annulation,
                apartment.depart_instructions,
                apartment.regles_maison,
                Photos = apartment.Photos.Select(p => new { p.photo_url }) // Ne renvoyer que les URLs
            };

            return Ok(result);
        }

       // GET: api/Apartments/{id}/availability
[HttpGet("GetApartmentAvailability/{id}")]
public async Task<ActionResult<IEnumerable<string>>> GetApartmentAvailability(int id)
{
    try
    {
        var reservations = await _context.Reservation
            .Where(r => r.id_appartement == id)
            .Select(r => new { r.date_depart, r.date_sortie })
            .ToListAsync();

        var unavailableDates = new HashSet<string>(); // Evite les doublons

        foreach (var reservation in reservations)
        {
            DateTime startDate = reservation.date_depart;
            DateTime endDate = reservation.date_sortie;

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                unavailableDates.Add(date.ToString("yyyy-MM-dd")); // Format standard
            }
        }

        return Ok(unavailableDates.ToList()); // Convertir HashSet en List avant de retourner
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error fetching availability for apartment {id}: {ex.Message}");
        return StatusCode(500, "Internal server error: " + ex.Message);
    }
}


        // GET: api/Apartments/city/{ville}
        [HttpGet("city/{ville}")]
        public async Task<IActionResult> GetApartmentsByVille(string ville)
        {
            // Log the request
            Console.WriteLine($"Received request for apartments in city: {ville}");

            var apartments = await _context.Appartements
                .Where(a => a.Ville != null && a.Ville.Equals(ville, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();

            if (!apartments.Any())
            {
                return NoContent();  // No apartments found in the specified city.
            }

            return Ok(apartments);
        }
        // POST: api/Apartments
// POST: api/Apartments
[HttpPost]
        public async Task<ActionResult<Appartement>> CreateApartment([FromBody] AppartementCreateDto dto)
        {
            try
            {
                // Validate the model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Log the creation attempt
                Console.WriteLine($"Attempting to create apartment: {JsonSerializer.Serialize(dto)}");

                // Map DTO to domain model
                var appartement = new Appartement
                {
                    Titre = dto.Titre,
                    Description = dto.Description,
                    Adresse = dto.Adresse,
                    Ville = dto.Ville,
                    Prix = dto.Prix,
                    Capacite = dto.Capacite,
                    NbrAdultes = dto.NbrAdultes,
                    NbrEnfants = dto.NbrEnfants,
                    AccepteAnimaux = dto.AccepteAnimaux,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    frais_menage = dto.frais_menage,
                    max_animaux = dto.max_animaux,
                    surface = dto.surface,
                    balcon_surface = dto.balcon_surface,
                    chauffage = dto.chauffage,
                    wifi = dto.wifi,
                    television = dto.television,
                    lave_Linge = dto.lave_Linge,
                    seche_cheveux = dto.seche_cheveux,
                    cuisine_equipee = dto.cuisine_equipee,
                    parking_payant = dto.parking_payant,
                    petit_dejeuner_inclus = dto.petit_dejeuner_inclus,
                    lit_parapluie = dto.lit_parapluie,
                    menage_disponible = dto.menage_disponible,
                    nombre_min_nuits = dto.nombre_min_nuits,
                    remise_semaine = dto.remise_semaine,
                    remise_mois = dto.remise_mois,
                    checkin_heure = TimeSpan.Parse(dto.checkin_heure),
                    checkout_heure = TimeSpan.Parse(dto.checkout_heure),
                    politique_annulation = dto.politique_annulation,
                    depart_instructions = dto.depart_instructions,
                    regles_maison = dto.regles_maison,
                    Photos = new List<AppartementPhotos>()
                };

                // Map photos if they exist
                if (dto.Photos != null && dto.Photos.Count > 0)
                {
                    foreach (var photoDto in dto.Photos)
                    {
                        appartement.Photos.Add(new AppartementPhotos
                        {
                            photo_url = photoDto.photo_url
                            // No need to set appartement_id here as EF Core will handle it
                        });
                    }
                }

                // Add the new apartment to the context
                _context.Appartements.Add(appartement);
                
                // Save changes to the database
                await _context.SaveChangesAsync();

                // Log successful creation
                Console.WriteLine($"Successfully created apartment with ID: {appartement.Id}");

                // Return a 201 Created response with the newly created resource
                return CreatedAtAction(nameof(GetApartmentById), new { id = appartement.Id }, appartement);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error creating apartment: {ex.Message}");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
// DELETE: api/Apartments/{id}
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteApartment(int id)
{
    try
    {
        // Log the delete attempt
        Console.WriteLine($"Attempting to delete apartment ID {id}");

        // Find the apartment
        var apartment = await _context.Appartements
            .Include(a => a.Photos) // Include photos to delete them too
            .FirstOrDefaultAsync(a => a.Id == id);

        if (apartment == null)
        {
            return NotFound($"Apartment with ID {id} not found.");
        }

        // Remove the apartment (and by cascade, its photos)
        _context.Appartements.Remove(apartment);
        
        // Save changes to the database
        await _context.SaveChangesAsync();

        // Log successful deletion
        Console.WriteLine($"Successfully deleted apartment ID {id} and its associated photos");

        return NoContent(); // 204 No Content is the standard response for successful DELETE
    }
    catch (Exception ex)
    {
        // Log the exception
        Console.WriteLine($"Error deleting apartment: {ex.Message}");
        return StatusCode(500, "Internal server error: " + ex.Message);
    }
}// PUT: api/Apartments/{id}
// PUT: api/Apartments/{id}
[HttpPut("{id}")]
public async Task<IActionResult> UpdateApartment(int id, [FromBody] AppartementCreateDto dto)
{
    try
    {
        // Validate the model
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Log the update attempt
        Console.WriteLine($"Attempting to update apartment ID {id}");

        // Check if the apartment exists
        var existingApartment = await _context.Appartements
            .Include(a => a.Photos)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (existingApartment == null)
        {
            return NotFound($"Apartment with ID {id} not found.");
        }

        // Update basic properties
        existingApartment.Titre = dto.Titre;
        existingApartment.Description = dto.Description;
        existingApartment.Adresse = dto.Adresse;
        existingApartment.Ville = dto.Ville;
        existingApartment.Prix = dto.Prix;
        existingApartment.Capacite = dto.Capacite;
        existingApartment.NbrAdultes = dto.NbrAdultes;
        existingApartment.NbrEnfants = dto.NbrEnfants;
        existingApartment.AccepteAnimaux = dto.AccepteAnimaux;
        existingApartment.Latitude = dto.Latitude;
        existingApartment.Longitude = dto.Longitude;
        existingApartment.frais_menage = dto.frais_menage;
        existingApartment.max_animaux = dto.max_animaux;
        existingApartment.surface = dto.surface;
        existingApartment.balcon_surface = dto.balcon_surface;
        existingApartment.chauffage = dto.chauffage;
        existingApartment.wifi = dto.wifi;
        existingApartment.television = dto.television;
        existingApartment.lave_Linge = dto.lave_Linge;
        existingApartment.seche_cheveux = dto.seche_cheveux;
        existingApartment.cuisine_equipee = dto.cuisine_equipee;
        existingApartment.parking_payant = dto.parking_payant;
        existingApartment.petit_dejeuner_inclus = dto.petit_dejeuner_inclus;
        existingApartment.lit_parapluie = dto.lit_parapluie;
        existingApartment.menage_disponible = dto.menage_disponible;
        existingApartment.nombre_min_nuits = dto.nombre_min_nuits;
        existingApartment.remise_semaine = dto.remise_semaine;
        existingApartment.remise_mois = dto.remise_mois;
        existingApartment.checkin_heure = TimeSpan.Parse(dto.checkin_heure);
        existingApartment.checkout_heure = TimeSpan.Parse(dto.checkout_heure);
        existingApartment.politique_annulation = dto.politique_annulation;
        existingApartment.depart_instructions = dto.depart_instructions;
        existingApartment.regles_maison = dto.regles_maison;

        // Handle photo updates
        if (dto.Photos != null)
        {
            // Remove all existing photos
            _context.RemoveRange(existingApartment.Photos);

            // Add new photos
            existingApartment.Photos = dto.Photos.Select(p => new AppartementPhotos
            {
                photo_url = p.photo_url,
                appartement_id = id
            }).ToList();
        }

        // Save changes to the database
        await _context.SaveChangesAsync();

        // Log successful update
        Console.WriteLine($"Successfully updated apartment ID {id}");

        return NoContent(); // 204 No Content is the standard response for successful PUT
    }
    catch (Exception ex)
    {
        // Log the exception
        Console.WriteLine($"Error updating apartment: {ex.Message}");
        return StatusCode(500, "Internal server error: " + ex.Message);
    }
}

// Helper method to check if an apartment exists
private bool ApartmentExists(int id)
{
    return _context.Appartements.Any(a => a.Id == id);
}
    }
    
}
