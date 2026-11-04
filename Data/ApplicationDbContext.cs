using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Point_Pharma.Models;

namespace Point_Pharma.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Farmacias> Farmacias { get; set; }

        public DbSet<Produto> Produtos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Produto>()
                .HasOne(p => p.Farmacia)
                .WithMany()
                .HasForeignKey(p => p.FarmaciaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}