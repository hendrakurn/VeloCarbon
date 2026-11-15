using System;
using System.Collections.Generic;

namespace VeloCarbon.Core.Models;

public class UserProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<ActivityRecord> ActivityRecords { get; set; } = new List<ActivityRecord>();
    public ICollection<Scenario> Scenarios { get; set; } = new List<Scenario>();
}
