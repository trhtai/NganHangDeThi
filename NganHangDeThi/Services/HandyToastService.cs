using HandyControl.Controls;
using NganHangDeThi.Services.Interfaces;

namespace NganHangDeThi.Services;

public sealed class HandyToastService : IToastService
{
    public void Success(string message) => Growl.SuccessGlobal(message);
    public void Warning(string message) => Growl.WarningGlobal(message);
    public void Error(string message) => Growl.ErrorGlobal(message);
    public void Info(string message) => Growl.InfoGlobal(message);
}
