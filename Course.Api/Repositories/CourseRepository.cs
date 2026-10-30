using CourseApi.Data;
using CourseApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseApi.Repositories;

public sealed class CourseRepository(CourseDbContext db) : ICourseRepository
{
    public Task<List<Course>> GetAllAsync() =>
        db.Courses.AsNoTracking().OrderBy(course => course.Id).ToListAsync();

    public Task<Course?> GetByIdAsync(int id) =>
        db.Courses.AsNoTracking().FirstOrDefaultAsync(course => course.Id == id);

    public Task<Course?> GetTrackedByIdAsync(int id) => db.Courses.FindAsync(id).AsTask();

    public Task<List<Course>> SearchAsync(string keyword)
    {
        string term = keyword.Trim()
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
        string pattern = $"%{term}%";

        return db.Courses.AsNoTracking()
            .Where(course => EF.Functions.ILike(course.Code, pattern, "\\") ||
                EF.Functions.ILike(course.Name, pattern, "\\"))
            .OrderBy(course => course.Id)
            .ToListAsync();
    }

    public void Add(Course course) => db.Courses.Add(course);

    public void Remove(Course course) => db.Courses.Remove(course);

    public async Task SaveChangesAsync() => await db.SaveChangesAsync();
}
