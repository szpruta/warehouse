namespace Warehouse.Api.Features.Products;

public record CreateProductRequest(
    string Sku, string Name, string? Description, int Quantity, int ReorderThreshold
);

public record UpdateProductRequest(
    string Name, string? Description, int ReorderThreshold
);

public record ProductResponse(
    int Id, string Sku, string Name, string? Description, int Quantity, int ReorderThreshold, bool NeedsReorder
);
