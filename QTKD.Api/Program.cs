using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Repositories;
using QTKD.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("QtkdDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Thiếu ConnectionStrings:QtkdDb. Hãy cấu hình PostgreSQL theo README_PHASE2.md.");
}

builder.Services.AddDbContext<QtkdDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IQtkdRepository, QtkdRepository>();
builder.Services.AddScoped<IQtkdReadService, QtkdReadService>();
builder.Services.AddScoped<IQtkdWriteService, QtkdWriteService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
