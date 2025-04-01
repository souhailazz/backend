using Microsoft.EntityFrameworkCore;
using AppartementReservationAPI.Models;

namespace AppartementReservationAPI.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Appartement> Appartements { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<Reservation> Reservation { get; set; }
        public DbSet<Paiement> Paiement { get; set; }
        public DbSet<Message> Message { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppartementPhotos>()
        .HasOne(ap => ap.Appartement)
        .WithMany(a => a.Photos)
        .HasForeignKey(ap => ap.appartement_id); 
            modelBuilder.Entity<Appartement>().ToTable("Appartement");
            modelBuilder.Entity<Appartement>().Property(a => a.Prix).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Appartement>().Property(a => a.Latitude).HasColumnType("decimal(9,6)");
            modelBuilder.Entity<Appartement>().Property(a => a.Longitude).HasColumnType("decimal(9,6)");
        }
    }
}