using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Data;
using ProductCatalog.Api.Models;
var builder = WebApplication.CreateBuilder(args);
// Configuration de la connexion SQL Server
var connectionString =
builder.Configuration.GetConnectionString("CatalogDbConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(connectionString));
// Activation d'Endpoints API Explorer & Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
// Swagger activé pour tester l'API localement et dans le conteneur
app.UseSwagger();
app.UseSwaggerUI();
// Auto-création de la base de données au démarrage
using (var scope = app.Services.CreateScope())
{
var db =
scope.ServiceProvider.GetRequiredService<AppDbContext>();
// Attend que la base soit disponible et crée le schéma
db.Database.EnsureCreated();
}
// 1. Endpoint d'observabilité / Healthcheck applicatif (Measurement- CALMS)
app.MapGet("/health", () => Results.Ok(new { status = "Healthy",
timestamp = DateTime.UtcNow }))
.WithName("HealthCheck");
// 2. Endpoints (listing des produits et ajout d’un produit)
app.MapGet("/api/products", async (AppDbContext db) =>
await db.Products.AsNoTracking().ToListAsync())
.WithName("GetAllProducts");
app.MapPost("/api/products", async (Product product, AppDbContext
db) =>
{
db.Products.Add(product);
await db.SaveChangesAsync();
return Results.Created($"/api/products/{product.Id}", product);
}).WithName("CreateProduct");
app.Run();