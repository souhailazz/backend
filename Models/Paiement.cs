using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppartementReservationAPI.Models{
 public class Paiement
    {
        [Key]
        public int id { get; set; }

        public decimal? total { get; set; }  // Allow null for total

        [MaxLength(50)]
        public string? methode_de_paiement { get; set; }  // Allow null for methode_de_paiement

        [MaxLength(100)]
        public string? payment_code { get; set; }  // Allow null for payment_code
    }
}