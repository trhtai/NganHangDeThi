using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Helpers;
using NganHangDeThi.Models;
using NganHangDeThi.Services.Interfaces;

namespace NganHangDeThi.Data.Repositories;

public class ChuongRepository(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IDateTimeService dateTime
) : IChuongRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;
    private readonly IDateTimeService _dateTime = dateTime;

    public async Task<PagedResult<Chuong>> GetPagedAsync(
        int monHocId,
        string? searchText,
        ChuongSortColumn sortColumn,
        SortDirection sortDirection,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 20;

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        IQueryable<Chuong> query = db.Chuong.AsNoTracking().Where(x => x.MonHocId == monHocId);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var keyword = StringHelper.ToUnSign(searchText.Trim());
            query = query.Where(x => EF.Functions.Like(x.TenChuongUnsign, $"%{keyword}%"));
        }

        // Sorting.
        query = (sortColumn, sortDirection) switch
        {
            (ChuongSortColumn.TenChuong, SortDirection.Ascending) => query.OrderBy(x => x.TenChuongUnsign),
            (ChuongSortColumn.TenChuong, SortDirection.Descending) => query.OrderByDescending(x => x.TenChuongUnsign),
            (ChuongSortColumn.ThuTu, SortDirection.Ascending) => query.OrderBy(x => x.ThuTu),
            (ChuongSortColumn.ThuTu, SortDirection.Descending) => query.OrderByDescending(x => x.ThuTu),
            (ChuongSortColumn.CreatedAt, SortDirection.Ascending) => query.OrderBy(x => x.CreatedAt),
            (ChuongSortColumn.CreatedAt, SortDirection.Descending) => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderBy(x => x.ThuTu)
        };

        // Query lần 1: đếm tổng, tránh tốn chi phí tải ds nếu rỗng.
        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0)
        {
            return PagedResult<Chuong>.Empty(pageIndex, pageSize);
        }

        // Query lần 2: lấy về ds phần tử tương ứng.
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Chuong>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<Chuong?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        return await db.Chuong
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<bool> TenChuongExistsAsync(int monHocId, string ten, int? excludeId, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        var normalized = StringHelper.ToUnSign(ten.Trim());

        // Trùng tên chỉ tính khi trùng trong CÙNG 1 môn học (2 môn khác nhau có thể
        // đặt trùng tên chương, ví dụ "Chương 1 - Giới thiệu").
        return await db.Chuong
            .AsNoTracking()
            .AnyAsync(x => x.MonHocId == monHocId
                        && x.TenChuongUnsign == normalized
                        && (excludeId == null || x.Id != excludeId), ct);
    }

    public async Task<int> GetNextThuTuAsync(int monHocId, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        var maxThuTu = await db.Chuong
            .Where(x => x.MonHocId == monHocId)
            .Select(x => (int?)x.ThuTu)
            .MaxAsync(ct);

        return (maxThuTu ?? 0) + 1;
    }

    public async Task AddAsync(Chuong item, CancellationToken ct = default)
    {
        item.TenChuongUnsign = StringHelper.ToUnSign(item.TenChuong);
        item.CreatedAt = _dateTime.GetVietnamTime();

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        db.Chuong.Add(item);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Chuong item, CancellationToken ct = default)
    {
        item.TenChuongUnsign = StringHelper.ToUnSign(item.TenChuong);

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        await db.Chuong
                .Where(x => x.Id == item.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.TenChuong, item.TenChuong)
                    .SetProperty(x => x.TenChuongUnsign, item.TenChuongUnsign)
                    .SetProperty(x => x.ThuTu, item.ThuTu)
                    .SetProperty(x => x.UpdatedAt, _dateTime.GetVietnamTime()),
                    ct);
    }

    public async Task DeleteRangeAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0) return;

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        await db.Chuong
            .Where(x => ids.Contains(x.Id))
            .ExecuteDeleteAsync(ct);
    }
}
