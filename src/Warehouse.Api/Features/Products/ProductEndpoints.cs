using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Data;
using Warehouse.Api.Domain;

namespace Warehouse.Api.Features.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", GetAll);

        return app;
    }

    private static async Task<IResult> GetAll(WarehouseDbContext db, CancellationToken ct)
    {
        var products = await db.Products
            .AsNoTracking()
            .OrderBy(p => p.SKU)
            .Select(p => MapToResponse(p))
            .ToListAsync(ct);

        return products is null ? TypedResults.NotFound() : TypedResults.Ok(products);
    }

    private static ProductResponse MapToResponse(Product p) => new(
        p.Id, p.SKU, p.Name, p.Description,
        p.StockLevel != null ? p.StockLevel.Quantity : 0,
        p.StockLevel != null ? p.StockLevel.ReorderThreshold : 0,
        p.StockLevel != null && p.StockLevel.Quantity <= p.StockLevel.ReorderThreshold
    );
}
