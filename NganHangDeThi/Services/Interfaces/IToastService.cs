namespace NganHangDeThi.Services.Interfaces;

public interface IToastService
{
    void Success(string message);
    void Warning(string message);
    void Error(string message);
    void Info(string message);
}
