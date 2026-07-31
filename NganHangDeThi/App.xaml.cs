using HandyControl.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NganHangDeThi.Data;
using NganHangDeThi.Data.Repositories;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Helpers;
using NganHangDeThi.Services;
using NganHangDeThi.Services.Interfaces;
using NganHangDeThi.ViewModels.Chapters.Factories;
using NganHangDeThi.ViewModels.Chapters.Factories.Interfaces;
using NganHangDeThi.ViewModels.Curriculum.Factories;
using NganHangDeThi.ViewModels.Curriculum.Factories.Interfaces;
using NganHangDeThi.ViewModels.KhoaPage;
using NganHangDeThi.ViewModels.KhoaPage.Factories;
using NganHangDeThi.ViewModels.KhoaPage.Factories.Interfaces;
using NganHangDeThi.ViewModels.LopPage;
using NganHangDeThi.ViewModels.LopPage.Factories;
using NganHangDeThi.ViewModels.LopPage.Factories.Interfaces;
using NganHangDeThi.ViewModels.NganHangCauHoiPage;
using NganHangDeThi.ViewModels.NienKhoaPage;
using NganHangDeThi.ViewModels.NienKhoaPage.Factories;
using NganHangDeThi.ViewModels.NienKhoaPage.Factories.Interfaces;
using NganHangDeThi.ViewModels.Semesters.Factories;
using NganHangDeThi.ViewModels.Semesters.Factories.Interfaces;
using NganHangDeThi.ViewModels.Settings;
using NganHangDeThi.ViewModels.Subjects;
using NganHangDeThi.ViewModels.Subjects.Factories;
using NganHangDeThi.ViewModels.Subjects.Factories.Interfaces;
using NganHangDeThi.Views;
using NganHangDeThi.Views.KhoaPage;
using NganHangDeThi.Views.LopPage;
using NganHangDeThi.Views.MonHocPage;
using NganHangDeThi.Views.NganHangCauHoiPage;
using NganHangDeThi.Views.NienKhoaPage;
using NganHangDeThi.Views.Settings;
using System.Windows;

namespace NganHangDeThi;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // SQLite / EF Core.
                services.AddDbContextFactory<AppDbContext>(options =>
                {
                    options.UseSqlite(DbPathProvider.ConnectionString);
                });

                // Repositories.
                services.AddSingleton<ICaiDatRepository, CaiDatRepository>();
                services.AddSingleton<IMonHocRepository, MonHocRepository>();
                services.AddSingleton<IKhoaRepository, KhoaRepository>();
                services.AddSingleton<ILopRepository, LopRepository>();
                services.AddSingleton<INienKhoaRepository, NienKhoaRepository>();
                services.AddSingleton<IChuongRepository, ChuongRepository>();
                services.AddSingleton<IHocKyRepository, HocKyRepository>();
                services.AddSingleton<IChuongTrinhHocRepository, ChuongTrinhHocRepository>();

                // Services.
                services.AddSingleton<IDateTimeService, DateTimeService>();
                services.AddSingleton<IConfirmService, HandyConfirmService>();
                services.AddSingleton<IToastService, HandyToastService>();

                // Views, ViewModels and Factories.
                // Cài đặt,
                services.AddTransient<SettingViewModel>();
                services.AddTransient<SettingView>();
                // Ngân hàng câu hỏi.
                services.AddTransient<NganHangCauHoiViewModel>();
                services.AddTransient<NganHangCauHoiView>();
                // Khoa.
                services.AddTransient<IChinhSuaKhoaViewModelFactory, ChinhSuaKhoaViewModelFactory>();
                services.AddTransient<KhoaViewModel>();
                services.AddTransient<KhoaView>();
                // Lớp.
                services.AddTransient<IChinhSuaLopViewModelFactory, ChinhSuaLopViewModelFactory>();
                services.AddTransient<LopViewModel>();
                services.AddTransient<LopView>();
                // Môn học.
                services.AddTransient<ISubjectEditViewModelFactory, SubjectEditViewModelFactory>();
                services.AddTransient<SubjectViewModel>();
                services.AddTransient<MonHocView>();
                // Niên khóa.
                services.AddTransient<IChinhSuaNienKhoaViewModelFactory, ChinhSuaNienKhoaViewModelFactory>();
                services.AddTransient<NienKhoaViewModel>();
                services.AddTransient<NienKhoaView>();
                // Chương.
                services.AddTransient<IChapterEditViewModelFactory, ChapterEditViewModelFactory>();
                services.AddTransient<IChapterViewModelFactory, ChapterViewModelFactory>();
                // Học kỳ.
                services.AddScoped<ISemesterViewModelFactory, SemesterViewModelFactory>();
                services.AddScoped<ISemesterEditViewModelFactory, SemesterEditViewModelFactory>();
                // Chuong trinh hoc.
                services.AddScoped<ICurriculumViewModelFactory, CurriculumViewModelFactory>();
                services.AddScoped<ICurriculumEditViewModelFactory, CurriculumEditViewModelFactory>();
                // Main view.
                services.AddSingleton<MainView>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();

        // Áp dụng Migration (pending) trước khi hiển thị MainWindow.
        await using (var dbContext = await AppHost.Services
                         .GetRequiredService<IDbContextFactory<AppDbContext>>()
                         .CreateDbContextAsync())
        {
            await dbContext.Database.MigrateAsync();
        }

        // Load DinhDangNgayGio.
        var caiDatRepo = AppHost!.Services.GetRequiredService<ICaiDatRepository>();

        if (caiDatRepo != null)
        {
            try
            {
                // Kéo dữ liệu từ DB (truyền CancellationToken.None vì app mới khởi động)
                string formatStr = await caiDatRepo.GetDinhDangNgayGioAsync(CancellationToken.None);

                // Gán vào biến toàn cục cho Converter dùng
                if (!string.IsNullOrWhiteSpace(formatStr))
                {
                    AppGlobalState.CurrentDateFormat = formatStr;
                }
            }
            catch (Exception)
            {
                // Bỏ qua hoặc ghi Log nếu lỗi kết nối DB. 
                // AppGlobalState vẫn sẽ giữ giá trị mặc định là "dd/MM/yyyy HH:mm"
            }
        }

        // Đặt ngôn ngữ mặc định của HandyControl sang tiếng Anh
        ConfigHelper.Instance.SetLang("en");

        // Hiển thị MainView.
        AppHost.Services.GetRequiredService<MainView>().Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost!.StopAsync();

        base.OnExit(e);
    }
}
