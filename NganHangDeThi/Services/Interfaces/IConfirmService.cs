namespace NganHangDeThi.Services.Interfaces;

public interface IConfirmService
{
    bool Confirm(string message, string title = "Xác nhận");
}
