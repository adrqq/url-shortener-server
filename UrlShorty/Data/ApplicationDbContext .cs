using Microsoft.EntityFrameworkCore;
using UrlShorty.Models;

namespace UrlShorty.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<ApplicationUserModel> User { get; set; }
        public DbSet<TokenModel> Token { get; set; }
        public DbSet<ShortUrlModel> ShortUrl { get; set; }
        public DbSet<RedirectionModel> Redirection { get; set; }
        public DbSet<AboutTextModel> About { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUserModel>()
                .HasIndex(u => u.Nickname)
                .IsUnique();

            modelBuilder.Entity<ApplicationUserModel>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<ShortUrlModel>()
                .HasIndex(u => u.Url)
                .IsUnique();

            modelBuilder.Entity<ShortUrlModel>()
                .HasIndex(u => u.Slug)
                .IsUnique();
        }
    }
}
