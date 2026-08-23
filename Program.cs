using Microsoft.EntityFrameworkCore;
using PizzaTownDHA.Data;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Services;
using PizzaTownDHA.Utilities;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Pizza_Town_DHA");
//var connectionString = builder.Configuration.GetConnectionString("Pizza Town DHA");

if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("Connection string 'Pizza Town DHA' not found in appsettings.json");
}


builder.Services.AddDbContext<PizzaTownContext>(options => options.UseMySQL(connectionString)
);
// Add services to the container.
builder.Services.AddControllersWithViews();

// Scopedd FOr Servicees
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IIngredientService, IngredientService>();
builder.Services.AddScoped<IProductService, ProductService>(); 
builder.Services.AddScoped<IKitchenLogService, KitchenLogService>();
builder.Services.AddScoped<IStockAuditService, StockAuditService>();
builder.Services.AddScoped<IStockInService, StockInService>();
builder.Services.AddScoped<IUnitConvertorService, UnitConverterService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
