using CourseApi.Models;

namespace CourseApi.Repositories;

public interface ICourseRepository
{
    Task<List<Course>> GetAllAsync();
    Task<Course?> GetByIdAsync(int id);
    Task<Course?> GetTrackedByIdAsync(int id);
    Task<List<Course>> SearchAsync(string keyword);
    void Add(Course course);
    void Remove(Course course);
    Task SaveChangesAsync();
}
