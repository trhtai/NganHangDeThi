using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Helpers;
using NganHangDeThi.Models;

namespace NganHangDeThi.Data.Repositories;

public class ChuongTrinhHocRepository(
    IDbContextFactory<AppDbContext> dbContextFactory
) : IChuongTrinhHocRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;

    public async Task<PagedResult<ChuongTrinhHoc>> GetPagedAsync(
        int lopId,
        string? searchText,
        CurriculumSortColumn sortColumn,
        SortDirection sortDirection,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 20;

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        // Luôn giới hạn trong phạm vi 1 lớp học, kèm theo thông tin Môn học để hiển thị.
        IQueryable<ChuongTrinhHoc> query = db.ChuongTrinhHoc
            .AsNoTracking()
            .Include(x => x.MonHoc)
            .Where(x => x.LopId == lopId);

        // Searching theo tên môn học.
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var keyword = StringHelper.ToUnSign(searchText.Trim());
            query = query.Where(x => EF.Functions.Like(x.MonHoc.TenMonUnSign, $"%{keyword}%"));
        }

        // Sorting.
        query = (sortColumn, sortDirection) switch
        {
            (CurriculumSortColumn.TenMon, SortDirection.Ascending) => query.OrderBy(x => x.MonHoc.TenMonUnSign),
            (CurriculumSortColumn.TenMon, SortDirection.Descending) => query.OrderByDescending(x => x.MonHoc.TenMonUnSign),
            (CurriculumSortColumn.NamHoc, SortDirection.Ascending) => query.OrderBy(x => x.NamHoc),
            (CurriculumSortColumn.NamHoc, SortDirection.Descending) => query.OrderByDescending(x => x.NamHoc),
            _ => query.OrderByDescending(x => x.NamHoc).ThenBy(x => x.MonHoc.TenMonUnSign)
        };

        // Query lần 1: đếm tổng, tránh tốn chi phí tải ds nếu rỗng.
        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0)
        {
            return PagedResult<ChuongTrinhHoc>.Empty(pageIndex, pageSize);
        }

        // Query lần 2: lấy về ds phần tử tương ứng.
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<ChuongTrinhHoc>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<ChuongTrinhHoc?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        return await db.ChuongTrinhHoc
            .Include(x => x.MonHoc)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<bool> ExistsAsync(int lopId, int monHocId, int namHoc, int? excludeId, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        // 1 lớp không được gán trùng (môn học + năm học), tránh 2 bản ghi giống hệt nhau.
        return await db.ChuongTrinhHoc
            .AsNoTracking()
            .AnyAsync(x => x.LopId == lopId
                        && x.MonHocId == monHocId
                        && x.NamHoc == namHoc
                        && (excludeId == null || x.Id != excludeId), ct);
    }

    public async Task<List<MonHoc>> GetMonHocOptionsAsync(CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        return await db.MonHoc
            .AsNoTracking()
            .OrderBy(x => x.TenMonUnSign)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ChuongTrinhHoc item, CancellationToken ct = default)
    {
        // Không có Audit (IAuditable) nên không cần set CreatedAt.
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        db.ChuongTrinhHoc.Add(item);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ChuongTrinhHoc item, CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);
        await db.ChuongTrinhHoc
                .Where(x => x.Id == item.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.MonHocId, item.MonHocId)
                    .SetProperty(x => x.NamHoc, item.NamHoc),
                    ct);
    }

    public async Task DeleteRangeAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0) return;

        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        await db.ChuongTrinhHoc
            .Where(x => ids.Contains(x.Id))
            .ExecuteDeleteAsync(ct);
    }
}