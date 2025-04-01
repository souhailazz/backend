// AppartementCreateDto.cs
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AppartementReservationAPI.Models
{
    public class AppartementCreateDto
    {
        public string Titre { get; set; }
        public string Description { get; set; }
        public string Adresse { get; set; }
        public string Ville { get; set; }
        public int Prix { get; set; }
        public int Capacite { get; set; }
        public int NbrAdultes { get; set; }
        public int NbrEnfants { get; set; }
        public bool? AccepteAnimaux { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        
        // New attributes based on the database schema
        public decimal frais_menage { get; set; }
        public int max_animaux { get; set; }
        public decimal surface { get; set; }
        public decimal balcon_surface { get; set; }
        public bool? chauffage { get; set; }
        public bool? wifi { get; set; }
        public bool? television { get; set; }
        public bool? lave_Linge { get; set; }
        public bool? seche_cheveux { get; set; }
        public bool? cuisine_equipee { get; set; }
        public bool? parking_payant { get; set; }
        public bool? petit_dejeuner_inclus { get; set; }
        public bool? lit_parapluie { get; set; }
        public bool? menage_disponible { get; set; }
        public int nombre_min_nuits { get; set; }
        public decimal remise_semaine { get; set; }
        public decimal remise_mois { get; set; }
        public string checkin_heure { get; set; } // Using string for TimeSpan to avoid JSON conversion issues
        public string checkout_heure { get; set; } // Using string for TimeSpan to avoid JSON conversion issues
        public string politique_annulation { get; set; }
        public string depart_instructions { get; set; }
        public string regles_maison { get; set; }
        
        public List<PhotoCreateDto> Photos { get; set; }
    }
}

// PhotoCreateDto.cs
namespace AppartementReservationAPI.Models
{
    public class PhotoCreateDto
    {
        public string photo_url { get; set; }
    }
}