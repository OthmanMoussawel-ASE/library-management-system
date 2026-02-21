namespace LibraryManagement.Application.Common.Models;

public class QueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "asc";
    public Dictionary<string, string>? Filters { get; set; }
    public string? SearchTerm { get; set; }
}
