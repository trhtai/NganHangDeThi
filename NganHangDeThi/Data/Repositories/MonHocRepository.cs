using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Helpers;
using NganHangDeThi.Models;
using NganHangDeThi.Services.Interfaces;

namespace NganHangDeThi.Data.Repositories;

public class MonHocRepository(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IDateTimeService dateTime
    ) : IMonHocRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;
    private readonly IDateTimeService _dateTime = dateTime;

    public async Task<PagedResult<MonHoc>> GetPagedAsync(
        string? searchText, 
        MonHocSortColumn sortColumn, 
        SortDirection sortDirection, 
        int pageIndex, 
        int pageSize, 
        CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 20;

        await using var db =  await _dbContextFactory.CreateDbContextAsync(ct);

        IQueryable<MonHoc> query = db.MonHoc.AsNoTracking();

        // Searching.
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var keyword = StringHelper.ToUnSign(searchText.Trim());
            query = query.Where(x => EF.Functions.Like(x.TenMonUnSign, $"%{keyword}%"));
        }

        // Sorting.
        query = (sortColumn, sortDirection) switch
        {
            (MonHocSortColumn.TenMon, SortDirection.Ascending) => query.OrderBy(x => x.TenMonUnSign),
            (MonHocSortColumn.TenMon, SortDirection.Descending) => query.OrderByDescending(x => x.TenMonUnSign),
            (MonHocSortColumn.CreatedAt, SortDirection.Ascending) => query.OrderBy(x => x.CreatedAt),
            (MonHocSortColumn.CreatedAt, SortDirection.Descending) => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        // Query lần 1: lấy tổng số phân tử trước, nếu là 0 thì không cần tốn chi phí tải ds về.
        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0)
        {
            return PagedResult<MonHoc>.Empty(pageIndex, pageSize);
        }

        // Query lần 2: lấy về ds các phần tử tương ứng.
        var items = await query
            .Include(x => x.Khoa)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<MonHoc>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<MonHoc?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        return await db.MonHoc
            .AsNoTracking()
            .Include(x => x.Khoa)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<bool> TenMonHocExistsAsync(string tenMonHoc, int? excludeId, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        var normalized = tenMonHoc.Trim();

        return await db.MonHoc
            .AsNoTracking()
            .AnyAsync(x => x.TenMon == normalized && (excludeId == null || x.Id != excludeId), ct);
    }

    public async Task AddAsync(MonHoc monHoc, CancellationToken ct = default)
    {
        monHoc.TenMonUnSign = StringHelper.ToUnSign(monHoc.TenMon);

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        db.MonHoc.Add(monHoc);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(MonHoc monHoc, CancellationToken ct = default)
    {
        monHoc.TenMonUnSign = StringHelper.ToUnSign(monHoc.TenMon);

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        await db.MonHoc
                .Where(x => x.Id == monHoc.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.TenMon, monHoc.TenMon)
                    .SetProperty(x => x.TenMonUnSign, monHoc.TenMonUnSign)
                    .SetProperty(x => x.KhoaId, monHoc.KhoaId)
                    .SetProperty(x => x.UpdatedAt, _dateTime.GetVietnamTime()),
                    ct);
    }

    public async Task DeleteRangeAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0) return;

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        await db.MonHoc
            .Where(x => ids.Contains(x.Id))
            .ExecuteDeleteAsync(ct);
    }
}
