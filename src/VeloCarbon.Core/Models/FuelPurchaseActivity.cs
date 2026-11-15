using System;
using VeloCarbon.Core.Enums;

namespace VeloCarbon.Core.Models;

public class FuelPurchaseActivity : ActivityRecord
{
    public Guid VehicleId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public FuelType FuelType { get; set; }
    private decimal _litersPurchased;
    public decimal LitersPurchased
    {
        get => _litersPurchased;
        set => _litersPurchased = value > 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(LitersPurchased), value, "Liter pembelian harus lebih dari 0.");
    }
    public decimal? TotalCost { get; set; }
    public Vehicle? Vehicle { get; set; }
    public FuelPurchaseActivity()
    {
        Unit = ActivityUnit.Liter;
    }

    public override decimal GetActivityValue()
    {
        return LitersPurchased;
    }
}
