﻿using Microsoft.EntityFrameworkCore;
using Server.Models;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4200").WithMethods("PUT", "DELETE", "GET", "POST").WithHeaders("Content-Type", "Authorization");
                      });
});

builder.Services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(0, new Base64FileModelBinderProvider());
});
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



//authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "http://localhost:3000/api", 
            ValidAudience = "Server", 
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ZWxzZXdpbmRvd3Rvd2VyZ2FtZWZyZWVrZXlwbGFudHN0cmlwbGVmdGF0dGVudGlvbmg=")) 
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(MyAllowSpecificOrigins);
app.UseStaticFiles();
app.UseSession(); //khai báo có sử dụng Session
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();