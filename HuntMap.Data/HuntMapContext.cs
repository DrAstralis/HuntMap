using System;
using HuntMap.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HuntMap.Data;

public class ApplicationUser : IdentityUser<Guid> { }

public class HuntMapContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public HuntMapContext(DbContextOptions<HuntMapContext> options) : base(options) { }

    public DbSet<Pin> Pins => Set<Pin>();
    public DbSet<Share> Shares => Set<Share>();
    public DbSet<TierDefinition> TierDefinitions => Set<TierDefinition>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Pin>(e =>
        {
            e.HasIndex(p => new { p.OwnerId, p.IsDeleted });
            e.Property(p => p.X).HasPrecision(9,6);
            e.Property(p => p.Y).HasPrecision(9,6);
        });
        builder.Entity<Share>(e =>
        {
            e.HasIndex(s => new { s.OwnerId, s.RecipientId }).IsUnique();
        });
        builder.Entity<TierDefinition>().HasData(
            new TierDefinition { Tier = 1, ColorHex = "#FF69B4", DisplayName = "Tier 1" },
            new TierDefinition { Tier = 2, ColorHex = "#FF69B4", DisplayName = "Tier 2" },
            new TierDefinition { Tier = 3, ColorHex = "#FF69B4", DisplayName = "Tier 3" },
            new TierDefinition { Tier = 4, ColorHex = "#FF69B4", DisplayName = "Tier 4" },
            new TierDefinition { Tier = 5, ColorHex = "#FF69B4", DisplayName = "Tier 5" },
            new TierDefinition { Tier = 6, ColorHex = "#FF69B4", DisplayName = "Tier 6" },
            new TierDefinition { Tier = 7, ColorHex = "#FF69B4", DisplayName = "Tier 7" },
            new TierDefinition { Tier = 8, ColorHex = "#FF69B4", DisplayName = "Tier 8" },
            new TierDefinition { Tier = 9, ColorHex = "#FF69B4", DisplayName = "Tier 9" },
            new TierDefinition { Tier = 10, ColorHex = "#FF69B4", DisplayName = "Tier 10" }
        );
    }
}