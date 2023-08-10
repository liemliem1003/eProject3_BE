using Microsoft.EntityFrameworkCore;
using Server.Models;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;



var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4200").WithMethods("PUT", "DELETE", "GET", "POST");
                      });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    //c =>
    //{
    //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    //    // Configure file upload support for Swagger
    //    c.OperationFilter<FileUploadOperationFilter>();
    //}
);
builder.Services.AddDbContext<InsuranceContext>
    (options => options.UseSqlServer(builder.Configuration.GetConnectionString("mycon")));
builder.Services.AddDistributedMemoryCache(); //lưu trữ các phần tử dữ liệu trong bộ nhớ cache
builder.Services.AddSession(); //có sử dụng biến session
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
    //    c =>
    //{
    //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
    //}
    );
}
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();
app.UseSession(); //khai báo có sử dụng Session

app.MapControllers();

app.Run();