using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Helpers;
using NganHangDeThi.Models;
using NganHangDeThi.Services.Interfaces;

namespace NganHangDeThi.Data.Repositories;

public class NienKhoaRepository(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IDateTimeService dateTime
) : INienKhoaRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;
    private readonly IDateTimeService _dateTime = dateTime;

    public async Task<PagedResult<NienKhoa>> GetPagedAsync(
        string? searchText,
        NienKhoaSortColumn sortColumn,
        SortDirection sortDirection,
        int pageIndex, int pageSize,
        CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 20;

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        IQueryable<NienKhoa> query = db.NienKhoa.AsNoTracking();

        // Searching.
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var keyword = StringHelper.ToUnSign(searchText.Trim());
            query = query.Where(x => EF.Functions.Like(x.TenNienKhoa, $"%{keyword}%"));
        }

        // Sorting.
        query = (sortColumn, sortDirection) switch
        {
            (NienKhoaSortColumn.TenNienKhoa, SortDirection.Ascending) => query.OrderBy(x => x.TenNienKhoa),
            (NienKhoaSortColumn.TenNienKhoa, SortDirection.Descending) => query.OrderByDescending(x => x.TenNienKhoa),
            (NienKhoaSortColumn.CreatedAt, SortDirection.Ascending) => query.OrderBy(x => x.CreatedAt),
            (NienKhoaSortColumn.CreatedAt, SortDirection.Descending) => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        // Query lần 1: lấy tổng số phân tử trước, nếu là 0 thì không cần tốn chi phí tải ds về.
        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0)
        {
            return PagedResult<NienKhoa>.Empty(pageIndex, pageSize);
        }

        // Query lần 2: lấy về ds các phần tử tương ứng.
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<NienKhoa>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<NienKhoa?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        return await db.NienKhoa
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<bool> TenNienKhoaExistsAsync(string ten, int? excludeId, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        var normalized = ten.Trim();

        return await db.NienKhoa
            .AsNoTracking()
            .AnyAsync(x => x.TenNienKhoa == normalized && (excludeId == null || x.Id != excludeId), ct);
    }

    public async Task AddAsync(NienKhoa item, CancellationToken ct = default)
    {
        item.CreatedAt = _dateTime.GetVietnamTime();

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        db.NienKhoa.Add(item);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(NienKhoa item, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        await db.NienKhoa
                .Where(x => x.Id == item.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.TenNienKhoa, item.TenNienKhoa)
                    .SetProperty(x => x.UpdatedAt, _dateTime.GetVietnamTime()),
                    ct);
    }

    public async Task DeleteRangeAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0) return;

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        await db.NienKhoa
            .Where(x => ids.Contains(x.Id))
            .ExecuteDeleteAsync(ct);
    }
}
