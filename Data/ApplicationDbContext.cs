using Microsoft.AspNetCore.Identity;
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

        public DbSet<Pedido> Pedidos { get; set; }

        public DbSet<ItemPedido> ItensPedido { get; set; }

        public DbSet<PerfilUsuario> PerfisUsuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUserLogin<string>>()
                .Property(login => login.LoginProvider)
                .HasMaxLength(128);

            builder.Entity<IdentityUserLogin<string>>()
                .Property(login => login.ProviderKey)
                .HasMaxLength(128);

            builder.Entity<IdentityUserToken<string>>()
                .Property(token => token.LoginProvider)
                .HasMaxLength(128);

            builder.Entity<IdentityUserToken<string>>()
                .Property(token => token.Name)
                .HasMaxLength(128);

            builder.Entity<Produto>()
                .HasOne(p => p.Farmacia)
                .WithMany()
                .HasForeignKey(p => p.FarmaciaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Pedido>()
                .HasIndex(p => p.CodigoPedido)
                .IsUnique();

            builder.Entity<Pedido>()
                .HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Pedido>()
                .HasMany(p => p.Itens)
                .WithOne(i => i.Pedido)
                .HasForeignKey(i => i.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ItemPedido>()
                .HasOne(i => i.Produto)
                .WithMany()
                .HasForeignKey(i => i.ProdutoId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<PerfilUsuario>()
                .HasKey(p => p.UsuarioId);

            builder.Entity<PerfilUsuario>()
                .HasOne(p => p.Usuario)
                .WithOne()
                .HasForeignKey<PerfilUsuario>(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
