namespace HotelManagement.Application.Common.Models;

/// <summary>
/// Paginated result wrapper
/// </summary>
public record PaginatedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int Size { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Size);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}
