using Microsoft.EntityFrameworkCore;
using Server.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<InsuranceContext>
    (options => options.UseSqlServer(builder.Configuration.GetConnectionString("mycon")));
builder.Services.AddDistributedMemoryCache(); //lưu trữ các phần tử dữ liệu trong bộ nhớ cache
builder.Services.AddSession(); //có sử dụng biến session
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseSession(); //khai báo có sử dụng Session

app.MapControllers();

app.Run();