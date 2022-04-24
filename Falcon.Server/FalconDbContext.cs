using Falcon.Server.Features.Auth.Models;
using Falcon.Server.Features.Messages.Models;
using Falcon.Server.Features.Rooms.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Falcon.Server
{
    public class FalconDbContext : IdentityDbContext<ApplicationUser>
    {
        public FalconDbContext(DbContextOptions<FalconDbContext> options) : base(options)
        {
        }

        public override DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Room> Rooms { get; set; }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ShadowPropertiesSetup();
            return base.SaveChangesAsync(true, cancellationToken);
        }

        public override int SaveChanges()
        {
            ShadowPropertiesSetup();
            return base.SaveChanges();
        }

        private void ShadowPropertiesSetup()
        {
            ChangeTracker.DetectChanges();

            ChangeTracker.Entries()
                .Where(e => e.Metadata.FindProperty("CreationDate") != null && e.Metadata.FindProperty("LastModificationDate") != null)
                .ToList()
                .ForEach(e =>
                {
                    e.Property("LastModificationDate").CurrentValue = DateTimeOffset.UtcNow;

                    if (e.State == EntityState.Added)
                    {
                        e.Property("CreationDate").CurrentValue = DateTimeOffset.UtcNow;
                    }
                });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entities = modelBuilder.Model.GetEntityTypes()
                .Where(x => x.ClrType.Name != "IdentityUser")
                .ToList();

            entities.ForEach(entity =>
            {
                entity.AddProperty("CreationDate", typeof(DateTimeOffset));
                entity.AddProperty("LastModificationDate", typeof(DateTimeOffset));
            });

            base.OnModelCreating(modelBuilder);
            BuildModelForApplicationUsers(modelBuilder);
            BuildModelForMessages(modelBuilder);
            BuildModelForRooms(modelBuilder);
        }

        private static void BuildModelForApplicationUsers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>()
                .ToTable("Users")
                .HasKey(u => u.Id);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Messages)
                .WithOne(m => m.User);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.OwnedRooms)
                .WithOne(r => r.Owner);
        }

        private static void BuildModelForMessages(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .ToTable("Messages")
                .HasKey(m => new { m.Id });

            modelBuilder.Entity<Message>()
                .HasOne(m => m.User)
                .WithMany(u => u.Messages);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Room)
                .WithMany(r => r.Messages);
        }

        private static void BuildModelForRooms(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>()
                .ToTable("Rooms")
                .HasKey(r => new { r.Id });

            modelBuilder.Entity<Room>()
               .HasMany(r => r.Messages)
               .WithOne(m => m.Room);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.Owner)
                .WithMany(u => u.OwnedRooms);  // ?????
        }
    }
}