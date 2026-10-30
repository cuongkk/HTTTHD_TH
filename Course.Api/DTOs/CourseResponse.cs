namespace CourseApi.DTOs;

/// <summary>Public Course representation returned by the API.</summary>
public sealed record CourseResponse(int Id, string Code, string Name, int Credits, bool IsActive, DateTime CreatedAt);
