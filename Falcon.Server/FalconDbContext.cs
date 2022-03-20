﻿using Falcon.Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Falcon.Server
{
    public class FalconDbContext : IdentityDbContext
    {
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
        }

        private static void BuildModelForMessages(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .ToTable("Messages")
                .HasKey(m => new { m.Id });
        }
    }
}