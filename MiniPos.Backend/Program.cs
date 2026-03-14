using Database.EfAppDbContextModels;
using Microsoft.EntityFrameworkCore;
using MiniPos.Backend.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")),
    ServiceLifetime.Transient,
    ServiceLifetime.Transient
);
builder.Services.AddErrorMappings();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();