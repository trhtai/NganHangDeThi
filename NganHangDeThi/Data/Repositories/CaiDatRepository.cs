using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;

namespace NganHangDeThi.Data.Repositories;

public class CaiDatRepository(IDbContextFactory<AppDbContext> dbContextFactory) : ICaiDatRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;

    public async Task<bool> GetHienThiXacNhanThoatAsync(CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        var setting = await db.CaiDat.FirstOrDefaultAsync(ct);

        return setting?.HienThiXacNhanThoat ?? true;
    }

    public async Task SetHienThiXacNhanThoatAsync(bool value, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        var setting = await db.CaiDat.FirstOrDefaultAsync(ct);

        if (setting == null)
        {
            db.CaiDat.Add(new CaiDat
            {
                HienThiXacNhanThoat = value
            });
        }
        else
        {
            setting.HienThiXacNhanThoat = value;
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<string> GetDinhDangNgayGioAsync(CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        var caiDat = await db.CaiDat.FirstOrDefaultAsync(ct);

        if (caiDat != null && !string.IsNullOrWhiteSpace(caiDat.DinhDangNgayGio))
        {
            return caiDat.DinhDangNgayGio;
        }

        return "dd/MM/yyyy HH:mm";
    }

    public async Task SetDinhDangNgayGioAsync(string value, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        var caiDat = await db.CaiDat.FirstOrDefaultAsync(ct);

        if (caiDat == null)
        {
            // Trường hợp DB trắng tinh chưa có dòng cài đặt nào -> Tạo mới
            caiDat = new CaiDat
            {
                DinhDangNgayGio = value,
                HienThiXacNhanThoat = true
            };
            db.CaiDat.Add(caiDat);
        }
        else
        {
            // Đã có data -> Chỉ cập nhật trường định dạng ngày giờ
            caiDat.DinhDangNgayGio = value;
            db.CaiDat.Update(caiDat);
        }

        await db.SaveChangesAsync(ct);
    }
}
