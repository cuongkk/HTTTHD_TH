using CourseApi.DTOs;
using CourseApi.Exceptions;
using CourseApi.Models;
using CourseApi.Repositories;

namespace CourseApi.Services;

public sealed class CourseService(ICourseRepository repository) : ICourseService
{
    public async Task<List<CourseResponse>> GetAllAsync() =>
        (await repository.GetAllAsync()).Select(ToResponse).ToList();

    public async Task<CourseResponse?> GetByIdAsync(int id)
    {
        Course? course = await repository.GetByIdAsync(id);
        return course is null ? null : ToResponse(course);
    }

    public async Task<CourseResponse> CreateAsync(CourseUpsertRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Course course = new(0, request.Code, request.Name, request.Credits, request.IsActive);
        course.Validate();
        course.Code = course.Code.Trim();

        repository.Add(course);
        await repository.SaveChangesAsync();
        return ToResponse(course);
    }

    public async Task<bool> UpdateAsync(int id, CourseUpsertRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Course candidate = new(id, request.Code, request.Name, request.Credits, request.IsActive);
        candidate.Validate();

        Course? existing = await repository.GetTrackedByIdAsync(id);
        if (existing is null) return false;

        existing.Code = candidate.Code.Trim();
        existing.Name = candidate.Name;
        existing.Credits = candidate.Credits;
        existing.IsActive = candidate.IsActive;
        await repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Course? existing = await repository.GetTrackedByIdAsync(id);
        if (existing is null) return false;

        repository.Remove(existing);
        await repository.SaveChangesAsync();
        return true;
    }

    public async Task<List<CourseResponse>> SearchAsync(string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            throw new CourseValidationException("Từ khóa tìm kiếm không được rỗng.");

        List<Course> courses = await repository.SearchAsync(keyword.Trim());
        return courses.Select(ToResponse).ToList();
    }

    public async Task EnsureCanRegisterAsync(int courseId)
    {
        Course? course = await repository.GetByIdAsync(courseId);
        if (course is null) throw new CourseNotFoundException(courseId);
        if (!course.IsActive) throw new CourseInactiveException(courseId);
    }

    private static CourseResponse ToResponse(Course course) =>
        new(course.Id, course.Code, course.Name, course.Credits, course.IsActive, course.CreatedAt);
}
