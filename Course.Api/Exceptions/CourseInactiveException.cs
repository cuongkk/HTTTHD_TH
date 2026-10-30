namespace CourseApi.Exceptions;

public sealed class CourseInactiveException(int id)
    : Exception($"Course với Id = {id} đang Inactive nên không thể đăng ký mới.");
