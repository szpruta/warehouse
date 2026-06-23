using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Data;
using Warehouse.Api.Domain;

namespace Warehouse.Api.Features.Products;

public static class ProductEndpoints
{
    private readonly static Expression<Func<Product, ProductResponse>> Projection =
        p => new ProductResponse(
            p.Id, p.Sku, p.Name, p.Description,
            p.StockLevel != null ? p.StockLevel.Quantity : 0,
            p.StockLevel != null ? p.StockLevel.ReorderThreshold : 0,
            p.StockLevel != null && p.StockLevel.Quantity <= p.StockLevel.ReorderThreshold);

    // Compiled once — for mapping entities already loaded in memory (Create/Update).
    private readonly static Func<Product, ProductResponse> MapToResponse = Projection.Compile();

    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:int}", Update);
        group.MapDelete("/{id:int}", Delete);

        return app;
    }

    private static async Task<IResult> GetById(int id, WarehouseDbContext db, CancellationToken ct)
    {
        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(Projection)
            .FirstOrDefaultAsync(ct);

        return product is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(product);
    }

    private static async Task<IResult> GetAll(WarehouseDbContext db, CancellationToken ct)
    {
        var products = await db.Products
            .AsNoTracking()
            .OrderBy(p => p.Sku)
            .Select(Projection)
            .ToListAsync(ct);

        return TypedResults.Ok(products);
    }

    private static async Task<IResult> Create(CreateProductRequest request, WarehouseDbContext db, CancellationToken ct)
    {
        var exists = await db.Products.AnyAsync(p => p.Sku == request.Sku, ct);
        if (exists)
            return TypedResults.Conflict($"A product with SKU '{request.Sku}' already exists");

        var now = DateTimeOffset.UtcNow;
        var product = new Product
        {
            Id = Guid.NewGuid().GetHashCode(),
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            CreatedAt = now,
            UpdatedAt = now,
            StockLevel = new StockLevel
            {
                Quantity = request.Quantity,
                ReorderThreshold = request.ReorderThreshold,
                UpdatedAt = now
            }
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created($"/api/products/{product.Id}", MapToResponse(product));
    }

    private static async Task<IResult> Update(int id, UpdateProductRequest request, WarehouseDbContext db, CancellationToken ct)
    {
        var product = await db.Products
            .Include(p => p.StockLevel)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
            return TypedResults.NotFound();

        product.Name = request.Name;
        product.Description = request.Description ?? product.Description;
        product.UpdatedAt = DateTimeOffset.UtcNow;
        product.StockLevel?.ReorderThreshold = request.ReorderThreshold;

        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(MapToResponse(product));
    }

    private static async Task<IResult> Delete(int id, WarehouseDbContext db, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
            return TypedResults.NotFound();

        db.Products.Remove(product);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok($"Product of ID {id} has been deleted");
    }
}
