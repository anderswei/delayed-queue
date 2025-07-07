using Microsoft.EntityFrameworkCore;
using DelayedQ.Models;
using System.Text.Json;

namespace DelayedQ.Data
{
    public class DelayedQDbContext : DbContext
    {
        public DelayedQDbContext(DbContextOptions<DelayedQDbContext> options) : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Job entity
            modelBuilder.Entity<Job>(entity =>
            {
                // Set composite primary key (EventId, Timestamp) for partitioning
                entity.HasKey(e => new { e.EventId, e.Timestamp });
                
                // Configure EventId
                entity.Property(e => e.EventId)
                    .HasMaxLength(255)
                    .IsRequired();

                // Configure CallbackPayload as JSON
                entity.Property(e => e.CallbackPayload)
                    .HasColumnType("jsonb")
                    .IsRequired()
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                        v => JsonSerializer.Deserialize<JsonElement>(v, (JsonSerializerOptions)null!)
                    );

                // Configure CallbackType as enum
                entity.Property(e => e.CallbackType)
                    .HasConversion<string>()
                    .IsRequired();

                // Configure CallbackUrl
                entity.Property(e => e.CallbackUrl)
                    .HasMaxLength(2048)
                    .IsRequired();

                // Configure Timestamp
                entity.Property(e => e.Timestamp)
                    .IsRequired();

                // Configure CreatedAt
                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                // Configure ExecutedAt
                entity.Property(e => e.ExecutedAt);

                // Configure Status
                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasDefaultValue("Pending");

                // Add indexes for performance
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}
