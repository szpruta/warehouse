namespace Warehouse.Api.Domain;

public class Product
{
    public int Id { get; set; }
    public required string SKU { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    public StockLevel? StockLevel { get; set; }
}