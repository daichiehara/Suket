using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Suket.Models;

namespace Suket.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserAccount>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        //public DbSet<Suket.Models.User> User { get; set; }
        //public DbSet<Suket.Models.UserAccount>? UserAccount { get; set; }
        public DbSet<Suket.Models.Post>? Post { get; set; }
        //public DbSet<Suket.Models.User> User { get; set; }
        public DbSet<Suket.Models.Subscription>? Subscription { get; set; }
        //public DbSet<Suket.Models.User> User { get; set; }
        public DbSet<Suket.Models.Adoption>? Adoption { get; set; }
        //public DbSet<Suket.Models.User> User { get; set; }
        public DbSet<Suket.Models.Confirm>? Confirm { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            /*
            modelBuilder.Entity<UserAccount>()
                .HasIndex(u => u.NickName)
                .IsUnique();
            */
            // Add uniqueness constraints on UserName and Email
            modelBuilder.Entity<UserAccount>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<UserAccount>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Review>()
            .HasOne(r => r.Reviewer)
            .WithMany(u => u.ReviewsWritten)
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict); // Restrict to prevent cascading delete

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewed)
                .WithMany(u => u.ReviewsReceived)
                .HasForeignKey(r => r.ReviewedId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict to prevent cascading delete
        }

        public DbSet<Suket.Models.Reply>? Reply { get; set; }

        public DbSet<Suket.Models.RollCall>? RollCall { get; set; }

        public DbSet<Suket.Models.Review>? Review { get; set; }

        public DbSet<Suket.Models.PaymentRecord>? PaymentRecord { get; set; }

        public DbSet<Suket.Models.Contact>? Contact { get; set; }

    }
}