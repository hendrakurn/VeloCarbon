using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VeloCarbon.App.Infrastructure;
using VeloCarbon.App.ViewModels;
using VeloCarbon.Infrastructure;
using VeloCarbon.Infrastructure.Persistence;

namespace VeloCarbon.App;

public partial class App : Application
{
    private ServiceProvider? _provider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.local.json", optional: true) // override pengguna, tidak tertimpa saat update
                .Build();
            var cs = config.GetConnectionString("VeloCarbon") ?? throw new InvalidOperationException("ConnectionStrings:VeloCarbon belum diisi.");

            var services = new ServiceCollection();
            services.AddVeloCarbon(cs);
            services.AddSingleton<ScopeRunner>();
            services.AddSingleton<Shell>();
            services.AddSingleton<MainViewModel>();
            _provider = services.BuildServiceProvider();

            using (var scope = _provider.CreateScope())
                await DatabaseInitializer.InitializeAsync(scope.ServiceProvider, config.GetValue<bool>("Seed:DemoData"));

            var main = _provider.GetRequiredService<MainViewModel>();
            var window = new MainWindow { DataContext = main };
            window.Show();
            await main.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "VeloCarbon tidak dapat terhubung ke basis data PostgreSQL.\n\n" +
                "Periksa:\n 1. PostgreSQL berjalan (lihat INSTALASI.md)\n 2. Connection string di appsettings.json (atau appsettings.local.json)\n\n" +
                "Detail: " + ex.Message, "VeloCarbon - Gagal memulai", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e) { _provider?.Dispose(); base.OnExit(e); }
}
