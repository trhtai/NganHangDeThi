namespace NganHangDeThi.Data.Repositories.Interfaces;

public interface ICaiDatRepository
{
    Task<bool> GetHienThiXacNhanThoatAsync(CancellationToken ct = default);
    Task SetHienThiXacNhanThoatAsync(bool value, CancellationToken ct = default);

    public Task<string> GetDinhDangNgayGioAsync(CancellationToken ct = default);
    public Task SetDinhDangNgayGioAsync(string value, CancellationToken ct = default);
}
