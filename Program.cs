using Microsoft.EntityFrameworkCore;
using PizzaTownDHA.Data;
using PizzaTownDHA.Interfaces;
using PizzaTownDHA.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Pizza_Town_DHA");

if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("Connection string 'Pizza_Town_DHA' not found in appsettings.json");
}
// ✅ Debug: Print available connection strings
Console.WriteLine($"Connection string: '{connectionString}'");
Console.WriteLine($"Is null or empty: {string.IsNullOrEmpty(connectionString)}");

builder.Services.AddDbContext<PizzaTownContext>(options => options.UseMySQL(connectionString)
);
// Add services to the container.
builder.Services.AddControllersWithViews();

// Add scoped for services here
builder.Services.AddScoped<IUnitService, UnitService>();

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
