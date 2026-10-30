using CourseApi.Exceptions;

namespace CourseApi.Models;

public class Course
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Credits { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Dùng khi khởi tạo bằng object initializer: new Course { ... }.
    public Course() { }

    // Constructor này chỉ gán dữ liệu. Validate được gọi riêng trước khi lưu.
    public Course(int id, string code, string name, int credits, bool isActive)
    {
        Id = id;
        Code = code;
        Name = name;
        Credits = credits;
        IsActive = isActive;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Code))
            throw new CourseValidationException("Mã môn học (Code) không được rỗng.");

        if (Credits < 1 || Credits > 6)
            throw new CourseValidationException("Số tín chỉ (Credits) phải nằm trong khoảng 1–6.");
    }

    public override string ToString()
    {
        string status = IsActive ? "Active" : "Inactive";
        return $"{Id} | {Code} | {Name} | {Credits} tín chỉ | {status}";
    }
}
