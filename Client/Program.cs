using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Client.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ClientContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ClientContext") ?? throw new InvalidOperationException("Connection string 'ClientContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Login}/{id?}");

app.Run();