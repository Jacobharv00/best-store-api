using ecommerce.Models;
using ecommerce.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

var app = builder.Build();

app.MapGet("/", (ApplicationDbContext context) =>
{
    var product = new Product()
    {
        Name = "IPhone 15 Pro Max",
        Brand = "Apple",
        Category = "Phones",
        Price = 1200,
        Description = "Cool new phone",
        ImageFileName = "www.example-png.com",
        CreatedAt = DateTime.Now
    };

    context.Products.Add(product);
    context.SaveChanges();

    var products = context.Products.ToList();

    return products;
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
