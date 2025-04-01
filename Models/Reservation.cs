using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppartementReservationAPI.Models
{
public class Reservation
{
    [Key]
    public int id_reservation { get; set; }
    
    [Required, ForeignKey("Client")]
    public int id_client { get; set; }
    
    [Required, ForeignKey("Appartement")]
    public int id_appartement { get; set; }
    
    [Required]
    public DateTime date_depart { get; set; } = DateTime.UtcNow;

    
    [Required]
    public DateTime date_sortie { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(50)]
    public string? etat { get; set; }
    
    [Required]
    public int nbr_adultes { get; set; }
    
    [Required]
    public int nbr_enfants { get; set; }
    
    [Required]
    public bool animaux { get; set; }
    
    [Required, ForeignKey("Paiement")]
    public int id_paiement { get; set; }

    // Navigation properties
    [ForeignKey("id_client")]
    public virtual Client Client { get; set; }
        [ForeignKey("id_appartement")]

    public virtual Appartement Appartement { get; set; }
        [ForeignKey("id_paiement")]

    public virtual Paiement Paiement { get; set; } 
}
  public class ReservationStatusDto
    {
        public string Etat { get; set; }
    }
}