namespace QTKD.Api.Services;

public sealed record ServiceResult<T>(T? Value, int StatusCode, string? Code = null, string? Message = null)
    where T : class
{
    public static ServiceResult<T> Ok(T value) => new(value, 200);
    public static ServiceResult<T> Created(T value) => new(value, 201);
    public static ServiceResult<T> BadRequest(string code, string message) => new(null, 400, code, message);
    public static ServiceResult<T> NotFound(string message) => new(null, 404, "NotFound", message);
    public static ServiceResult<T> Conflict(string code, string message) => new(null, 409, code, message);
}
