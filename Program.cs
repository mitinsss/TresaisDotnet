using Microsoft.EntityFrameworkCore;
using PraktiskaisDarbs3.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();

var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "inventory.db");
var connectionString = $"Data Source={dbPath}";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
