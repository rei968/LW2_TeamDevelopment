namespace Shop.Shared.DTOs.Catalog;

public class ProductFilterParameters
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public string? Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; } = "name";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
