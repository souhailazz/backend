using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppartementReservationAPI.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required, ForeignKey("Reservation")]
        public int ReservationId { get; set; }

        [Required]
        public int SenderId { get; set; } // Either client or admin

        [Required]
        public int ReceiverId { get; set; } // Admin or client

        [Required]
        public string Content { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ReservationId")]
        public virtual Reservation Reservation { get; set; }
    }
}
public class MessageDto
{
    public int ReservationId { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Content { get; set; }
}
