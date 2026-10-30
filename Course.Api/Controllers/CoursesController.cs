using CourseApi.DTOs;
using CourseApi.Exceptions;
using CourseApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseApi.Controllers;

[ApiController]
[Route("api/courses")]
public sealed class CoursesController(ICourseService service) : ControllerBase
{
    /// <summary>Lấy danh sách tất cả môn học.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CourseResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CourseResponse>>> GetAll() =>
        Ok(await service.GetAllAsync());

    /// <summary>Lấy một môn học theo Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseResponse>> GetById([FromRoute] int id)
    {
        CourseResponse? course = await service.GetByIdAsync(id);
        if (course is null) throw new CourseNotFoundException(id);
        return Ok(course);
    }

    /// <summary>Tạo môn học mới. Database cấp Id và CreatedAt.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CourseResponse>> Create([FromBody] CourseUpsertRequest request)
    {
        CourseResponse created = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Cập nhật Code, Name, Credits và IsActive; Id lấy từ URL.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CourseUpsertRequest request)
    {
        if (!await service.UpdateAsync(id, request)) throw new CourseNotFoundException(id);
        return NoContent();
    }

    /// <summary>Xóa môn học theo Id.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!await service.DeleteAsync(id)) throw new CourseNotFoundException(id);
        return NoContent();
    }

    /// <summary>Tìm theo Code hoặc Name, không phân biệt hoa thường.</summary>
    /// <remarks>Keyword trống trả 400; không khớp trả 200 và mảng rỗng.</remarks>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<CourseResponse>>> Search([FromQuery] string? keyword) =>
        Ok(await service.SearchAsync(keyword));

    /// <summary>Kiểm tra Course có đủ điều kiện đăng ký mới hay không; không tạo Enrollment.</summary>
    [HttpPost("{id:int}/registration-check")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckRegistration([FromRoute] int id)
    {
        await service.EnsureCanRegisterAsync(id);
        return NoContent();
    }
}
