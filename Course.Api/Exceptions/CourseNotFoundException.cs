namespace CourseApi.Exceptions;

public sealed class CourseNotFoundException(int id)
    : Exception($"Không có Course với Id = {id}.");
