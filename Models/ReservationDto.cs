using System.Reflection;

public class ReservationDto
{
    public int id_client { get; set; }
    public int id_appartement { get; set; }
    public DateTime date_depart { get; set; }
    public DateTime date_sortie { get; set; }
    public int nbr_adultes { get; set; }
    public int nbr_enfants { get; set; }
    public bool animaux { get; set; }
    public int id_paiement { get; set; }
}
