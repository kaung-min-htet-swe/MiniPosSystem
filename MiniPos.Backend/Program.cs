using Database.EfAppDbContextModels;
using Microsoft.EntityFrameworkCore;
using MiniPos.Backend.Extensions;
using MiniPos.Backend.Features.Branches;
using MiniPos.Backend.Features.Categories;
using MiniPos.Backend.Features.Orders;
using MiniPos.Backend.Features.Products;
using MiniPos.Backend.Features.Users;

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

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();