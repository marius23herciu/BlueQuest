using BlueQuest.BusinessLayer;
using BlueQuest.Data;
using BlueQuest.Services.UserService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json.Converters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<UsersBusinessLayer>();
builder.Services.AddScoped<ToDTOs>();
builder.Services.AddScoped<QuestsBusinessLayer>();


builder.Services.AddSwaggerGen();


builder.Services.AddHttpContextAccessor();


builder.Services.AddDbContext<BlueQuestDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("BlueQuestConnectionString")));

/// show enums as string in Swagger
builder.Services
    .AddControllersWithViews()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.Converters.Add(new StringEnumConverter()));
builder.Services.AddSwaggerGenNewtonsoftSupport();



var app = builder.Build();

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
