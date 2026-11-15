using System;
using System.Collections.Generic;
using VeloCarbon.Core.Enums;

namespace VeloCarbon.Core.Models;

public class Vehicle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserProfileId { get; set; }
    public FuelType FuelType { get; set; }

    // Navigation properties
    public UserProfile? UserProfile { get; set; }
    public ICollection<FuelPurchaseActivity> FuelPurchaseActivities { get; set; } = new List<FuelPurchaseActivity>();
}
