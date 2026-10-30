namespace CourseApi.DTOs;

/// <summary>Client-editable fields for creating or replacing a Course.</summary>
public sealed record CourseUpsertRequest(string Code, string Name, int Credits, bool IsActive);
