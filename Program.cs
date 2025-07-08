using DelayedQ.Services;
using DelayedQ.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add PostgreSQL database context
builder.Services.AddDbContext<DelayedQDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

// Register custom services
builder.Services.AddScoped<IJobService, DatabaseJobService>();
builder.Services.AddScoped<IPartitionService, PartitionService>();
builder.Services.AddScoped<ILowPrecisionJobService, InMemoryLowPrecisionJobService>(); // In-memory implementation for now

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
