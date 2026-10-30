namespace CourseApi.Exceptions;

public sealed class CourseValidationException(string message) : Exception(message);
