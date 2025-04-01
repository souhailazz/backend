using System.Text.Json.Serialization;

namespace AppartementReservationAPI.Models
{
    public class AppartementPhotos
    {
        public int Id { get; set; }
        public int appartement_id { get; set; }
        public string  photo_url { get; set; }
       
       [JsonIgnore]
    public Appartement Appartement { get; set; }
        
    }
}