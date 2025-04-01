namespace AppartementReservationAPI.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string Mail { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string password { get; set; }
    }
}