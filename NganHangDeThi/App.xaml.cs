using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NganHangDeThi.Common.Configs;
using NganHangDeThi.Data.DataContext;
using NganHangDeThi.MyUserControl;
using NganHangDeThi.Repository.UnitOfWorks;
using NganHangDeThi.Services;
using System.Windows;

namespace NganHangDeThi;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, service) =>
            {
                // Đăng ký cấu hình image folder
                service.Configure<ImageStorageOptions>(
                    hostContext.Configuration.GetSection("ImageStorage"));

                service.AddDbContext<AppDbContext>(options =>
                {
                    var sqlConnectionStringBuilder = new SqlConnectionStringBuilder
                    {
                        DataSource = "localhost",
                        InitialCatalog = "QuanLiDeThiDb",
                        UserID = "sa",
                        Password = "StrongP@ssw0rd123!",
                        IntegratedSecurity = false,
                        TrustServerCertificate = true,
                    };
                    options.UseSqlServer(sqlConnectionStringBuilder.ConnectionString);
                });

                service.AddTransient<ThemCauHoiTuFileWindow>();
                service.AddTransient<QuestionExtractorService>();

                service.AddScoped<RaDeControl>();
                service.AddScoped<NganHangCauHoiControl>();
                service.AddScoped<QuanTriHeThongControl>();

                service.AddScoped<IUnitOfWork, UnitOfWork>();
                service.AddScoped<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();

        using (var scope = AppHost.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            dbContext.Database.Migrate();
        }

        var startupWindow = AppHost.Services.GetRequiredService<MainWindow>();
        startupWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost!.StopAsync();

        base.OnExit(e);
    }
}
