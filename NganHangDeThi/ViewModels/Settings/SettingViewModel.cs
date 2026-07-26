using CommunityToolkit.Mvvm.ComponentModel;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Helpers;
using System.Collections.ObjectModel;

namespace NganHangDeThi.ViewModels.Settings;

public partial class SettingViewModel : ObservableObject
{
    private readonly ICaiDatRepository _caiDatRepo;

    // Cờ đánh dấu trạng thái "Đang tải dữ liệu".
    // Tránh việc gán giá trị ban đầu từ DB bị hiểu nhầm là người dùng vừa click đổi setting trên UI,
    // dẫn đến việc ghi đè ngược lại Database ngay khi vừa mở trang.
    [ObservableProperty]
    private bool isLoading = true;

    // Dùng để hủy tiến trình load dữ liệu.
    private CancellationTokenSource? _loadCts;
    // Dùng để hủy tiến trình save dữ liệu.
    private CancellationTokenSource? _saveCts;

    [ObservableProperty]
    private bool showExitConfirm = true;

    #region DinhDangNgayGio
    public record DateFormatOption(string Format, string DisplayName);

    public ObservableCollection<DateFormatOption> AvailableDateFormats { get; } = new()
    {
        new DateFormatOption("dd/MM/yyyy",          "Ngày/Tháng/Năm (31/12/2026)"),
        new DateFormatOption("dd/MM/yyyy HH:mm",    "Ngày/Tháng/Năm 24h (31/12/2026 23:59)"),
        new DateFormatOption("dd/MM/yyyy hh:mm tt", "Ngày/Tháng/Năm 12h (31/12/2026 11:59 PM)"),
        new DateFormatOption("MM/dd/yyyy",          "Tháng/Ngày/Năm (12/31/2026)"),
        new DateFormatOption("yyyy-MM-dd HH:mm",    "Năm-Tháng-Ngày (2026-12-31 23:59)"),
        new DateFormatOption("HH:mm dd/MM/yyyy",    "Giờ:Phút Ngày/Tháng/Năm")
    };

    [ObservableProperty]
    private string selectedDateFormat = "dd/MM/yyyy HH:mm";
    #endregion

    public SettingViewModel(ICaiDatRepository caiDatRepo)
    {
        _caiDatRepo = caiDatRepo;

        /* Dấu '_' là biến loại bỏ (Discard variable).
         * Vì constructor không thể dùng 'await', ta chủ động gọi hàm chạy ngầm (fire-and-forget).
         * Việc gán vào '_' như một lời nhắn nhủ với Compiler: "Tôi biết đây là hàm async, 
         * tôi cố tình không await và vứt cái Task trả về đi, đừng báo Warning cảnh báo tôi nữa!".*/
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        // Hủy tiến trình cũ.
        _loadCts?.Cancel();

        // Tạo cts cho tiến tiến tình mới.
        var cts = new CancellationTokenSource();
        _loadCts = cts;

        try
        {
            ShowExitConfirm = await _caiDatRepo.GetHienThiXacNhanThoatAsync(cts.Token);
            SelectedDateFormat = await _caiDatRepo.GetDinhDangNgayGioAsync(cts.Token);

            AppGlobalState.CurrentDateFormat = SelectedDateFormat;
        }
        catch (OperationCanceledException)
        {
            // Request này đã bị hủy (do người dùng chuyển trang quá nhanh, hàm load khác đè lên).
            // Ta bắt lỗi này để app không bị crash, và lặng lẽ bỏ qua.
        }
        finally
        {
            // Nếu tiến trình hiện tại đúng là tiến trình cuối cùng (không bị ai đè lên), thì mới tắt cờ loading.
            if (_loadCts == cts)
            {
                IsLoading = false;
            }
        }
    }

    /* ====================================================================================
     * HÀM NÀY Ở ĐÂU CHUI RA? (PHÉP THUẬT SOURCE GENERATOR)
     * ====================================================================================
     * Nhờ có [ObservableProperty] ở trên, bộ MVVM Toolkit đã âm thầm sinh ra một hàm 
     * có cấu trúc tên là "On{TênProperty}Changed".
     * Từ khóa 'partial' giúp ta "bắt sóng" (hook) vào cái hàm ẩn đó.
     * Cứ mỗi khi người dùng click Checkbox/Toggle trên giao diện làm thay đổi ShowExitConfirm, 
     * hàm này sẽ tự động được kích hoạt chạy ngay lập tức.
     * ĐOẠN CODE ẨN DO THƯ VIỆN TỰ SINH RA
        public bool ShowExitConfirm
        {
            get => showExitConfirm;
            set
            {
                if (EqualityComparer<bool>.Default.Equals(showExitConfirm, value))
                    return;

                OnShowExitConfirmChanging(value); // Báo hiệu TRƯỚC khi đổi giá trị
        
                showExitConfirm = value;          // Đổi giá trị
                OnPropertyChanged("ShowExitConfirm"); // Báo cho giao diện (WPF) biết để vẽ lại
        
                OnShowExitConfirmChanged(value);  // Báo hiệu SAU KHI ĐÃ ĐỔI GIÁ TRỊ XONG <--- NÓ NẰM Ở ĐÂY
            }
        }
     * ==================================================================================== */
    partial void OnShowExitConfirmChanged(bool value)
    {
        // Nếu thay đổi này là do code đang tải dữ liệu lúc mở trang -> Bỏ qua, không lưu DB
        if (IsLoading)
        {
            return;
        }

        SaveSettingAsync(value);
    }

    private async void SaveSettingAsync(bool value)
    {
        _saveCts?.Cancel();

        _saveCts = new CancellationTokenSource();

        try
        {
            await _caiDatRepo.SetHienThiXacNhanThoatAsync(value, _saveCts.Token);
        }
        catch(OperationCanceledException) { }
    }

    // Auto-save khi user chọn ComboBox
    partial void OnSelectedDateFormatChanged(string value)
    {
        if (IsLoading || value == null) return;

        AppGlobalState.CurrentDateFormat = value;
        SaveDateFormatAsync(value);
    }

    private async void SaveDateFormatAsync(string format)
    {
        _saveCts?.Cancel();
        _saveCts = new CancellationTokenSource();
        try
        {
            await _caiDatRepo.SetDinhDangNgayGioAsync(format, _saveCts.Token);
        }
        catch (OperationCanceledException) { }
    }
}
