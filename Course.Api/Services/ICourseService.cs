using CourseApi.DTOs;

namespace CourseApi.Services;

public interface ICourseService
{
    Task<List<CourseResponse>> GetAllAsync();
    Task<CourseResponse?> GetByIdAsync(int id);
    Task<CourseResponse> CreateAsync(CourseUpsertRequest request);
    Task<bool> UpdateAsync(int id, CourseUpsertRequest request);
    Task<bool> DeleteAsync(int id);
    Task<List<CourseResponse>> SearchAsync(string? keyword);
    Task EnsureCanRegisterAsync(int courseId);
}
