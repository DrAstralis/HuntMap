using System;
using System.ComponentModel.DataAnnotations;

namespace HuntMap.Domain;

public enum PinSymbol
{
    Dot = 0
}

public class TierDefinition
{
    [Key] public int Tier { get; set; } // 1..10
    [Required, MaxLength(16)] public string ColorHex { get; set; } = "#FF69B4"; // pink default
    [Required, MaxLength(64)] public string DisplayName { get; set; } = "Tier";
}

public class Pin
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(64)] public string Name { get; set; } = "";
    [Range(1, 10)] public int Tier { get; set; }
    [Range(0, 9999)] public int Quantity { get; set; }
    [Required, MaxLength(16)] public string Color { get; set; } = "#FF69B4";
    public PinSymbol Symbol { get; set; } = PinSymbol.Dot;
    [Range(0,1)] public double X { get; set; } // normalized 0..1
    [Range(0,1)] public double Y { get; set; } // normalized 0..1
    public Guid OwnerId { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }
    public bool IsDeleted { get; set; }
}

public enum ShareStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Blocked = 3
}

public class Share
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public Guid RecipientId { get; set; }
    public ShareStatus Status { get; set; } = ShareStatus.Pending;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}