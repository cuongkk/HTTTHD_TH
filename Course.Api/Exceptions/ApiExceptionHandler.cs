using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CourseApi.Exceptions;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int status;
        string title;
        string detail;

        switch (exception)
        {
            case CourseValidationException:
                status = StatusCodes.Status400BadRequest;
                title = "Dữ liệu không hợp lệ.";
                detail = exception.Message;
                break;
            case CourseNotFoundException:
                status = StatusCodes.Status404NotFound;
                title = "Không tìm thấy môn học.";
                detail = exception.Message;
                break;
            case CourseInactiveException:
                status = StatusCodes.Status409Conflict;
                title = "Không thể đăng ký môn học.";
                detail = exception.Message;
                break;
            default:
                logger.LogError(exception, "Unhandled API exception");
                status = StatusCodes.Status500InternalServerError;
                title = "Đã xảy ra lỗi máy chủ.";
                detail = "Yêu cầu chưa thể được xử lý.";
                break;
        }

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";
        await JsonSerializer.SerializeAsync(httpContext.Response.Body, new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{status}"
        }, cancellationToken: cancellationToken);
        return true;
    }
}
