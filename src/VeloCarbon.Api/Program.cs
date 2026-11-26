using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using VeloCarbon.Core.Models;
using VeloCarbon.Core.Services;
using VeloCarbon.Infrastructure;
using VeloCarbon.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, ContentRootPath = AppContext.BaseDirectory });
var connection = builder.Configuration.GetConnectionString("VeloCarbon")
    ?? throw new InvalidOperationException("ConnectionStrings:VeloCarbon belum diisi di appsettings.json.");

builder.Services.AddVeloCarbon(connection);
builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.SwaggerDoc("v1", new() { Title = "VeloCarbon API", Version = "v1",
    Description = "Layanan estimasi jejak karbon mobilitas & listrik. Hasil bersifat edukatif, bukan inventarisasi emisi resmi." }));
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

var app = builder.Build();

// Penanganan error: pelanggaran aturan bisnis -> 400 dengan pesan yang bisa ditampilkan.
app.Use(async (ctx, next) =>
{
    try { await next(); }
    catch (DomainException ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

// API key opsional: aktif bila Api:ApiKey diisi. Swagger & /health tetap terbuka.
var apiKey = app.Configuration["Api:ApiKey"];
if (!string.IsNullOrEmpty(apiKey))
    app.Use(async (ctx, next) =>
    {
        var p = ctx.Request.Path;
        if (p.StartsWithSegments("/api") && !p.StartsWithSegments("/api/health") && ctx.Request.Headers["X-Api-Key"] != apiKey)
        { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; await ctx.Response.WriteAsJsonAsync(new { error = "X-Api-Key tidak valid." }); return; }
        await next();
    });

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

using (var scope = app.Services.CreateScope())
    await DatabaseInitializer.InitializeAsync(scope.ServiceProvider, app.Configuration.GetValue<bool>("Seed:DemoData"));

static DateTime ParseMonth(string? month)
{
    if (string.IsNullOrWhiteSpace(month)) return MonthHelper.MonthStart(DateTime.Today);
    if (DateTime.TryParseExact(month, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out var d)) return d;
    throw new DomainException("Format bulan harus yyyy-MM, contoh 2026-09.");
}

var api = app.MapGroup("/api");

api.MapGet("/health", () => new { status = "ok", time = DateTime.UtcNow }).WithTags("Sistem");

// ---- Layanan nilai tambah: kalkulator cepat tanpa menyimpan data ----
api.MapPost("/estimate", async (EstimateRequest r, EmissionFactorService factors, IEmissionCalculatorService calc) =>
{
    ActivityRecord a = r.Kind switch
    {
        EstimateKind.Distance => new MobilityActivity { TransportMode = r.Mode ?? throw new DomainException("Mode wajib untuk Distance."), DistanceKm = r.Value, Frequency = r.Frequency ?? 1, ActivityDate = r.Date ?? DateTime.Today },
        EstimateKind.Fuel => new FuelPurchaseActivity { FuelType = r.FuelType ?? throw new DomainException("FuelType wajib untuk Fuel."), LitersPurchased = r.Value, PurchaseDate = r.Date ?? DateTime.Today },
        _ => new ElectricityUsage { UsageKWh = r.Value, PeriodStart = r.Date ?? DateTime.Today }
    };
    if (r.Value <= 0) throw new DomainException("Nilai harus lebih dari 0.");
    var f = EmissionFactorService.Resolve(await factors.GetAllAsync(), a);
    var c = calc.CalculateActivityEmission(a, f);
    return new { activityValue = Math.Round(c.ActivityValue, 2, MidpointRounding.AwayFromZero), unit = a.Unit, factor = f.Value, factorUnit = $"kgCO2e/{a.Unit}", factorSource = f.Source, factorVersion = f.Version, kgCO2e = Math.Round(c.ResultKgCO2e, 2, MidpointRounding.AwayFromZero),
                 note = "Estimasi edukatif berdasarkan faktor emisi lokal berversi; bukan inventarisasi emisi resmi atau dasar klaim sertifikasi." };
}).WithTags("Kalkulator").WithSummary("Hitung estimasi emisi sekali pakai (tidak disimpan)");

api.MapGet("/emission-factors", (EmissionFactorService s) => s.GetAllAsync()).WithTags("Faktor Emisi");
api.MapPost("/emission-factors", async (EmissionFactor f, EmissionFactorService s) =>
{
    f.Id = Guid.NewGuid();
    return Results.Created($"/api/emission-factors/{f.Id}", await s.AddNewVersionAsync(f));
}).WithTags("Faktor Emisi").WithSummary("Tambah versi faktor baru (versi lama ditutup, tidak ditimpa)");

// ---- Profil & kendaraan ----
api.MapGet("/profiles", (ProfileService s) => s.GetAllAsync()).WithTags("Profil");
api.MapPost("/profiles", async (CreateProfileRequest r, ProfileService s) =>
{
    var p = await s.CreateProfileAsync(r.Name);
    return Results.Created($"/api/profiles/{p.Id}", p);
}).WithTags("Profil");
api.MapGet("/profiles/{id:guid}", async (Guid id, ProfileService s) => await s.GetAsync(id) is { } p ? Results.Ok(p) : Results.NotFound(new { error = "Profil tidak ditemukan." })).WithTags("Profil");
api.MapPut("/profiles/{id:guid}", async (Guid id, CreateProfileRequest r, ProfileService s) => Results.Ok(await s.UpdateProfileAsync(id, r.Name))).WithTags("Profil");
api.MapPost("/profiles/{id:guid}/vehicles", async (Guid id, CreateVehicleRequest r, ProfileService s) =>
    Results.Created($"/api/profiles/{id}", await s.AddVehicleAsync(id, r.Name, r.VehicleType, r.FuelType))).WithTags("Profil");
api.MapPut("/profiles/{id:guid}/vehicles/{vehicleId:guid}", async (Guid id, Guid vehicleId, CreateVehicleRequest r, ProfileService s) =>
    Results.Ok(await s.UpdateVehicleAsync(id, vehicleId, r.Name, r.VehicleType, r.FuelType))).WithTags("Profil");
api.MapDelete("/profiles/{id:guid}/vehicles/{vehicleId:guid}", async (Guid id, Guid vehicleId, ProfileService s) =>
    { await s.DeleteVehicleAsync(id, vehicleId); return Results.NoContent(); }).WithTags("Profil");

// ---- Aktivitas ----
api.MapGet("/profiles/{id:guid}/activities", async (Guid id, string? month, ActivityService s) =>
    (await s.GetMonthAsync(id, ParseMonth(month))).Select(ActivityDto.From)).WithTags("Aktivitas");
api.MapPost("/activities/distance", async (DistanceInput i, ActivityService s) => ActivityDto.From(await s.AddDistanceAsync(i))).WithTags("Aktivitas");
api.MapPost("/activities/fuel", async (FuelPurchaseInput i, ActivityService s) => ActivityDto.From(await s.AddFuelPurchaseAsync(i))).WithTags("Aktivitas");
api.MapPost("/activities/electricity/meter", async (MeterElectricityInput i, ActivityService s) => ActivityDto.From(await s.AddMeterElectricityAsync(i))).WithTags("Aktivitas");
api.MapPost("/activities/electricity/appliances", async (ApplianceElectricityInput i, ActivityService s) => ActivityDto.From(await s.AddApplianceElectricityAsync(i))).WithTags("Aktivitas");
api.MapDelete("/activities/{id:guid}", async (Guid id, ActivityService s) => { await s.DeleteAsync(id); return Results.NoContent(); }).WithTags("Aktivitas");

// ---- Dashboard, tren, laporan ----
api.MapGet("/profiles/{id:guid}/dashboard", (Guid id, string? month, DashboardService s) => s.GetSummaryAsync(id, ParseMonth(month))).WithTags("Dashboard");
api.MapGet("/profiles/{id:guid}/trend", (Guid id, string? endMonth, int? months, DashboardService s) => s.GetTrendAsync(id, ParseMonth(endMonth), months ?? 6)).WithTags("Dashboard");
api.MapGet("/profiles/{id:guid}/report", async (Guid id, string? month, DashboardService d, ActivityService a) =>
{
    var m = ParseMonth(month);
    return new
    {
        unit = "kgCO2e",
        note = "Estimasi edukatif berdasarkan faktor emisi lokal berversi; bukan inventarisasi emisi resmi atau dasar klaim sertifikasi.",
        summary = await d.GetSummaryAsync(id, m),
        trend = await d.GetTrendAsync(id, m, 6),
        activities = (await a.GetMonthAsync(id, m)).Select(ActivityDto.From)
    };
}).WithTags("Dashboard").WithSummary("Laporan bulanan: ringkasan, tren 6 bulan, dan rincian aktivitas");

// ---- Skenario ----
api.MapPost("/scenarios", async (ScenarioRequest r, ScenarioService s) => ScenarioDto.From(await s.RunAndSaveAsync(r))).WithTags("Skenario");
api.MapGet("/profiles/{id:guid}/scenarios", async (Guid id, ScenarioService s) => (await s.GetByProfileAsync(id)).Select(ScenarioDto.From)).WithTags("Skenario");

app.Run();

public enum EstimateKind { Distance, Fuel, Electricity }
public record EstimateRequest(EstimateKind Kind, decimal Value, TransportMode? Mode, FuelType? FuelType, int? Frequency, DateTime? Date);
public record CreateProfileRequest(string Name);
public record CreateVehicleRequest(string Name, TransportMode VehicleType, FuelType FuelType);

public record ActivityDto(Guid Id, string Type, DateTime Date, string Label, decimal ActivityValue, ActivityUnit Unit, decimal? KgCO2e, string? FactorSource, string? FactorVersion)
{
    public static ActivityDto From(ActivityRecord a) => new(a.Id, a.GetType().Name, a.ReferenceDate, DashboardService.LabelOf(a), Math.Round(a.GetActivityValue(), 2, MidpointRounding.AwayFromZero), a.Unit,
        a.Calculation is { } c ? Math.Round(c.ResultKgCO2e, 2, MidpointRounding.AwayFromZero) : null, a.Calculation?.EmissionFactor?.Source, a.Calculation?.EmissionFactor?.Version);
}
public record RecommendationDto(int Priority, string Title, string RecommendedAction, decimal EstimatedReductionKgCO2e);
public record ScenarioDto(Guid Id, string Name, DateTime BaselineStart, decimal Baseline, decimal Scenario, decimal Reduction, decimal? ReductionPercent, IEnumerable<RecommendationDto> Recommendations)
{
    public static ScenarioDto From(Scenario s) => new(s.Id, s.Name, s.BaselineStart, s.Result!.BaselineKgCO2e, s.Result.ScenarioKgCO2e, s.Result.ReductionKgCO2e,
        s.Result.GetReductionPercentage(), s.Result.Recommendations.OrderBy(r => r.Priority).Select(r => new RecommendationDto(r.Priority, r.Title, r.RecommendedAction, r.EstimatedReductionKgCO2e)));
}
