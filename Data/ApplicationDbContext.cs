using ArtGallery.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ArtGallery.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Artist> Artist { get; set; }
        public DbSet<Biding> Biding { get; set; }
        public DbSet<Order> Orders { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Wishing> Wishlist { get; set; }

        public DbSet<Profile> Profile { get; set; }


        public DbSet<Exhibition> Exhibitions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Biding aur Artist ka relationship sahi tareeqay se map kiya
            modelBuilder.Entity<Biding>()
                .HasOne(b => b.Artwork)
                .WithMany(a => a.Biding) // Yahan "a.Biding" likhna zaroori tha
                .HasForeignKey(b => b.ArtworkId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ User aur Biding ka link
            modelBuilder.Entity<Biding>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
        .Property(o => o.TotalAmount)
        .HasColumnType("decimal(18,2)");

            // ✅ Cascade Delete Fix for Orders (Multiple paths error solve karne ke liye)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Restricted taake cycle na bany

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Artwork)
                .WithMany()
                .HasForeignKey(o => o.ArtworkId)
                .OnDelete(DeleteBehavior.Restrict); // Restricted
        }


    }
}
