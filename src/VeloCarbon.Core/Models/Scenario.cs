using System;
using System.Collections.Generic;

namespace VeloCarbon.Core.Models;

public class Scenario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserProfileId { get; set; }
    public DateTime BaselineStart { get; set; }
    public DateTime BaselineEnd { get; set; }

    // Navigation properties
    public UserProfile? UserProfile { get; set; }
    public ICollection<ScenarioChange> ScenarioChanges { get; set; } = new List<ScenarioChange>();
    public ScenarioResult? ScenarioResult { get; set; }
}
