namespace NganHangDeThi.Models;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageIndex { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    // Có 40 items, mỗi trang 20: 40 / 20.0 = 2.0  => Ceiling = 2 => 2 trang
    // Có 41 items, mỗi trang 20: 41 / 20.0 = 2.05 => Ceiling = 3 => 3 trang
    // Có 0  items, mỗi trang 20: 0 / 20.0  = 0    => Ceiling = 0 => Max(1,0) -> 1 trang
    public int TotalPages => PageSize <= 0 
        ? 0 
        : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    public bool HasPrevious => PageIndex > 1;
    public bool HasNext => PageIndex < TotalPages;

    public static PagedResult<T> Empty(int pageIndex, int pageSize) => new()
    {
        Items = [],
        TotalCount = 0,
        PageIndex = pageIndex,
        PageSize = pageSize
    };
}
