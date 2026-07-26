using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;

namespace NganHangDeThi.Data.Repositories;

public class KhoaRepository(IDbContextFactory<AppDbContext> dbContextFactory) : IKhoaRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;

    public async Task<List<Khoa>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        return await db.Khoa
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }
}
