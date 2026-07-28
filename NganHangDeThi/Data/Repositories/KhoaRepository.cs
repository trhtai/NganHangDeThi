using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Helpers;
using NganHangDeThi.Models;
using NganHangDeThi.Services.Interfaces;

namespace NganHangDeThi.Data.Repositories;

public class KhoaRepository(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IDateTimeService dateTime
) : IKhoaRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;
    private readonly IDateTimeService _dateTime = dateTime;

    public async Task<List<Khoa>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        return await db.Khoa
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Khoa>> GetPagedAsync(
        string? searchText, 
        KhoaSortColumn sortColumn, 
        SortDirection sortDirection, 
        int pageIndex, int pageSize, 
        CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 20;

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        IQueryable<Khoa> query = db.Khoa.AsNoTracking();

        // Searching.
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var keyword = StringHelper.ToUnSign(searchText.Trim());
            query = query.Where(x => EF.Functions.Like(x.TenKhoaUnSign, $"%{keyword}%"));
        }

        // Sorting.
        query = (sortColumn, sortDirection) switch
        {
            (KhoaSortColumn.TenKhoa, SortDirection.Ascending) => query.OrderBy(x => x.TenKhoaUnSign),
            (KhoaSortColumn.TenKhoa, SortDirection.Descending) => query.OrderByDescending(x => x.TenKhoaUnSign),
            (KhoaSortColumn.CreatedAt, SortDirection.Ascending) => query.OrderBy(x => x.CreatedAt),
            (KhoaSortColumn.CreatedAt, SortDirection.Descending) => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        // Query lần 1: lấy tổng số phân tử trước, nếu là 0 thì không cần tốn chi phí tải ds về.
        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0)
        {
            return PagedResult<Khoa>.Empty(pageIndex, pageSize);
        }

        // Query lần 2: lấy về ds các phần tử tương ứng.
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Khoa>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<Khoa?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        return await db.Khoa
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<bool> TenKhoaExistsAsync(string ten, int? excludeId, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        var normalized = ten.Trim();

        return await db.Khoa
            .AsNoTracking()
            .AnyAsync(x => x.TenKhoa == normalized && (excludeId == null || x.Id != excludeId), ct);
    }

    public async Task AddAsync(Khoa item, CancellationToken ct = default)
    {
        item.TenKhoaUnSign = StringHelper.ToUnSign(item.TenKhoa);
        item.CreatedAt = _dateTime.GetVietnamTime();

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        db.Khoa.Add(item);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Khoa item, CancellationToken ct = default)
    {
        item.TenKhoaUnSign = StringHelper.ToUnSign(item.TenKhoa);

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        await db.Khoa
                .Where(x => x.Id == item.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.TenKhoa, item.TenKhoa)
                    .SetProperty(x => x.TenKhoaUnSign, item.TenKhoaUnSign)
                    .SetProperty(x => x.UpdatedAt, _dateTime.GetVietnamTime()),
                    ct);
    }

    public async Task DeleteRangeAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0) return;

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        await db.Khoa
            .Where(x => ids.Contains(x.Id))
            .ExecuteDeleteAsync(ct);
    }
}
