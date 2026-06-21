using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Warehouse.Api;
using Warehouse.Api.Data;
using Warehouse.Api.Features.Products;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Warehouse"))
);

builder.Services.AddOpenApi();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapProductEndpoints();

app.Run();
