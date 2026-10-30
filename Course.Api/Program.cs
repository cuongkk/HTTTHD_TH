using CourseApi.Data;
using CourseApi.Exceptions;
using CourseApi.Repositories;
using CourseApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký MVC Controller và thông tin endpoint cho Swagger.
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        string detail = string.Join(" ", context.ModelState.Values
            .SelectMany(value => value.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                ? "Request body không hợp lệ."
                : error.ErrorMessage));
        return new BadRequestObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Request không hợp lệ.",
            Detail = detail
        });
    };
});
builder.Services.AddEndpointsApiExplorer();

string? connectionString = builder.Configuration.GetConnectionString("CourseDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Thiếu ConnectionStrings:CourseDb. Xem README để cấu hình PostgreSQL.");
}
builder.Services.AddDbContext<CourseDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TH4 - Course Management API",
        Version = "v1",
        Description = "CRUD Course qua Controller, Service, Repository và EF Core/PostgreSQL."
    });
    string xmlPath = Path.Combine(AppContext.BaseDirectory, "Course.Api.xml");
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();
app.UseExceptionHandler();

// Giao diện thử API chỉ bật khi dùng môi trường Development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Course.Api v1"));
}

// Đưa các route [HttpGet], [HttpPost]... của Controller vào ứng dụng.
app.MapControllers();
app.Run();
