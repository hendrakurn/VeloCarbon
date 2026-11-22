using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using VeloCarbon.Core.Models;
using VeloCarbon.Core.Services;

namespace VeloCarbon.App.Infrastructure;

/// <summary>Satu scope DI (dan DbContext) per operasi, agar aplikasi berumur panjang tidak menahan state basis data.</summary>
public class ScopeRunner
{
    private readonly IServiceScopeFactory _factory;
    public ScopeRunner(IServiceScopeFactory factory) => _factory = factory;
    public async Task<T> RunAsync<T>(Func<IServiceProvider, Task<T>> work) { using var s = _factory.CreateScope(); return await work(s.ServiceProvider); }
    public async Task RunAsync(Func<IServiceProvider, Task> work) { using var s = _factory.CreateScope(); await work(s.ServiceProvider); }
}

/// <summary>State bersama: profil & bulan terpilih, serta pesan status.</summary>
public partial class Shell : ObservableObject
{
    [ObservableProperty] private string _statusText = "Siap.";
    [ObservableProperty] private bool _isError;
    [ObservableProperty] private UserProfile? _selectedProfile;
    [ObservableProperty] private DateTime _selectedMonth = MonthHelper.MonthStart(DateTime.Today);

    public Guid? ProfileId => SelectedProfile?.Id;

    public void Info(string msg) { StatusText = msg; IsError = false; }
    public void Error(string msg) { StatusText = msg; IsError = true; }

    /// <summary>Menjalankan aksi; pelanggaran aturan bisnis ditampilkan sebagai pesan jelas, bukan crash.</summary>
    public async Task<bool> TryAsync(Func<Task> action, string? success = null)
    {
        try { await action(); if (success != null) Info(success); return true; }
        catch (DomainException ex) { Error(ex.Message); return false; }
        catch (Exception ex) { Error("Terjadi kesalahan: " + (ex.InnerException?.Message ?? ex.Message)); return false; }
    }

    public Guid RequireProfile() => ProfileId ?? throw new DomainException("Pilih atau buat profil terlebih dahulu.");

    public bool Confirm(string message) =>
        MessageBox.Show(message, "VeloCarbon", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}

public static class Num
{
    /// <summary>Menerima "12,5" maupun "12.5".</summary>
    public static bool TryDecimal(string? s, out decimal v) =>
        decimal.TryParse((s ?? "").Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out v);

    public static decimal Decimal(string? s, string field) =>
        TryDecimal(s, out var v) ? v : throw new DomainException($"{field} harus berupa angka.");

    public static int Int(string? s, string field) =>
        int.TryParse((s ?? "").Trim(), out var v) ? v : throw new DomainException($"{field} harus berupa bilangan bulat.");

    public static string Kg(decimal v) => v.ToString("N2", CultureInfo.GetCultureInfo("id-ID")) + " kgCO2e";
}
