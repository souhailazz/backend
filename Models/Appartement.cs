namespace AppartementReservationAPI.Models
{
    public class Appartement
{
    public int Id { get; set; }
    public string Titre { get; set; }
    public string Description { get; set; }
    public string Adresse { get; set; }
    public string Ville { get; set; }
    public int Prix { get; set; }
    public int Capacite { get; set; }
    public int NbrAdultes { get; set; } // Max number of adults
    public int NbrEnfants { get; set; } // Max number of children
    public bool? AccepteAnimaux { get; set; } // Pets allowed (true/false, nullable)
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public List<AppartementPhotos>? Photos { get; set; } // List of photo URLs

    // New attributes based on the database schema
    public decimal frais_menage { get; set; }
    public int max_animaux { get; set; }
    public decimal surface { get; set; }
    public decimal balcon_surface { get; set; }
    public bool? chauffage { get; set; } // Nullable
    public bool? wifi { get; set; } // Nullable
    public bool? television { get; set; } // Nullable
    public bool? lave_Linge { get; set; } // Nullable
    public bool? seche_cheveux { get; set; } // Nullable
    public bool? cuisine_equipee { get; set; } // Nullable
    public bool? parking_payant { get; set; } // Nullable
    public bool? petit_dejeuner_inclus { get; set; } // Nullable
    public bool? lit_parapluie { get; set; } // Nullable
    public bool? menage_disponible { get; set; } // Nullable
    public int nombre_min_nuits { get; set; }
    public decimal remise_semaine { get; set; }
    public decimal remise_mois { get; set; }
    public TimeSpan checkin_heure { get; set; }
    public TimeSpan checkout_heure { get; set; }

    public string politique_annulation { get; set; }
    public string depart_instructions { get; set; }
    public string regles_maison { get; set; }
}

}
