using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

// Đây là client kiểm tra HTTP thật, không phải project khởi động API.
// Chạy Course.Api ở terminal khác trước. Chỉ cho kiểm tra địa chỉ loopback.
string baseUrl = args.Length > 0 ? args[0] : "http://localhost:5082";
if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? address)
    || !address.IsLoopback || (address.Scheme != "http" && address.Scheme != "https"))
{
    Console.Error.WriteLine("Chỉ dùng URL local hợp lệ, ví dụ http://localhost:5082.");
    return 2;
}

using HttpClient client = new() { BaseAddress = address, Timeout = TimeSpan.FromSeconds(15) };
List<int> createdIds = new();
int passed = 0;
int exitCode = 0;
string uniqueCode = "KT_" + Guid.NewGuid().ToString("N")[..8];

void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
    passed++;
    Console.WriteLine($"PASS {passed:00}: {message}");
}

async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null)
{
    using HttpRequestMessage request = new(method, path);
    if (body is not null) request.Content = JsonContent.Create(body);
    return await client.SendAsync(request);
}

async Task CheckStatusAsync(HttpMethod method, string path, HttpStatusCode expected, object? body = null)
{
    using HttpResponseMessage response = await SendAsync(method, path, body);
    string content = await response.Content.ReadAsStringAsync();
    Check(response.StatusCode == expected,
        $"{method} {path}: {(int)expected}; thực tế {(int)response.StatusCode}. {content}");
    if ((int)expected >= 400)
    {
        using JsonDocument problem = JsonDocument.Parse(content);
        JsonElement root = problem.RootElement;
        Check(root.TryGetProperty("status", out JsonElement status) && status.GetInt32() == (int)expected,
            $"{method} {path}: lỗi có trường ProblemDetails.status.");
        Check(root.TryGetProperty("title", out JsonElement title) && !string.IsNullOrWhiteSpace(title.GetString()),
            $"{method} {path}: lỗi có ProblemDetails.title dễ hiểu.");
        Check(root.TryGetProperty("detail", out JsonElement detail) && !string.IsNullOrWhiteSpace(detail.GetString()),
            $"{method} {path}: lỗi có ProblemDetails.detail.");
    }
    if (expected == HttpStatusCode.NoContent)
        Check(content.Length == 0, "204 không có response body.");
}

async Task<List<CourseResult>> SearchAsync(string keyword)
{
    string path = "/api/courses/search?keyword=" + Uri.EscapeDataString(keyword);
    using HttpResponseMessage response = await client.GetAsync(path);
    Check(response.StatusCode == HttpStatusCode.OK, $"Tìm '{keyword}' trả 200.");
    return await response.Content.ReadFromJsonAsync<List<CourseResult>>()
        ?? throw new InvalidOperationException("Search không trả JSON array.");
}

try
{
    using (HttpResponseMessage ui = await client.GetAsync("/swagger/index.html"))
    {
        Check(ui.StatusCode == HttpStatusCode.OK, "Swagger UI truy cập được trong Development.");
        Check((await ui.Content.ReadAsStringAsync()).Contains("swagger-ui"), "Response có giao diện Swagger UI.");
    }
    using (HttpResponseMessage spec = await client.GetAsync("/swagger/v1/swagger.json"))
    {
        Check(spec.StatusCode == HttpStatusCode.OK, "OpenAPI JSON trả 200.");
        using JsonDocument json = JsonDocument.Parse(await spec.Content.ReadAsStringAsync());
        JsonElement paths = json.RootElement.GetProperty("paths");
        Check(paths.GetProperty("/api/courses").TryGetProperty("get", out _), "Swagger có GET danh sách.");
        Check(paths.GetProperty("/api/courses").TryGetProperty("post", out _), "Swagger có POST.");
        Check(paths.GetProperty("/api/courses/{id}").TryGetProperty("get", out _), "Swagger có GET theo Id.");
        Check(paths.GetProperty("/api/courses/{id}").TryGetProperty("put", out _), "Swagger có PUT.");
        Check(paths.GetProperty("/api/courses/{id}").TryGetProperty("delete", out _), "Swagger có DELETE.");
        Check(paths.GetProperty("/api/courses/search").TryGetProperty("get", out _), "Swagger có Search.");
        JsonElement schemas = json.RootElement.GetProperty("components").GetProperty("schemas");
        JsonElement requestFields = schemas.GetProperty("CourseUpsertRequest").GetProperty("properties");
        Check(requestFields.TryGetProperty("code", out _) && requestFields.TryGetProperty("name", out _)
            && requestFields.TryGetProperty("credits", out _) && requestFields.TryGetProperty("isActive", out _),
            "DTO request chỉ có các trường client được phép sửa.");
        Check(!requestFields.TryGetProperty("id", out _) && !requestFields.TryGetProperty("createdAt", out _),
            "DTO request không cho client đặt Id hoặc CreatedAt.");
        Check(paths.GetProperty("/api/courses/{id}/registration-check").TryGetProperty("post", out _),
            "Swagger có registration-check.");
    }
    await CheckStatusAsync(HttpMethod.Get, "/api/courses", HttpStatusCode.OK);
    using (HttpResponseMessage beforeEligibility = await client.GetAsync("/api/courses"))
    {
        List<CourseResult> before = await beforeEligibility.Content.ReadFromJsonAsync<List<CourseResult>>() ?? [];
        await CheckStatusAsync(HttpMethod.Post, "/api/courses/2/registration-check", HttpStatusCode.NoContent);
        await CheckStatusAsync(HttpMethod.Post, "/api/courses/3/registration-check", HttpStatusCode.Conflict);
        await CheckStatusAsync(HttpMethod.Post, "/api/courses/2147483647/registration-check", HttpStatusCode.NotFound);
        using HttpResponseMessage afterEligibility = await client.GetAsync("/api/courses");
        List<CourseResult> after = await afterEligibility.Content.ReadFromJsonAsync<List<CourseResult>>() ?? [];
        Check(before.Count == after.Count, "Registration check không tạo Enrollment hay thay đổi Course.");
    }
    using (HttpResponseMessage existing = await client.GetAsync("/api/courses/2"))
    {
        Check(existing.StatusCode == HttpStatusCode.OK, "Môn mẫu C002 có trong database.");
        CourseResult? sample = await existing.Content.ReadFromJsonAsync<CourseResult>();
        Check(sample?.CreatedAt.Year >= 2020, "Môn mẫu có CreatedAt sau migration.");
    }
    await CheckStatusAsync(HttpMethod.Get, "/api/courses/2147483647", HttpStatusCode.NotFound);

    List<CourseResult> byCode = await SearchAsync("  c002  ");
    Check(byCode.Any(c => c.Code == "C002"), "Search trim và không phân biệt hoa thường theo Code.");
    List<CourseResult> byName = await SearchAsync("mạng");
    Check(byName.Any(c => c.Code == "C003"), "Search theo Name vẫn tìm được môn inactive C003.");
    Check((await SearchAsync("NO_MATCH_" + Guid.NewGuid().ToString("N"))).Count == 0,
        "Không khớp: 200 và mảng rỗng, không phải 404.");
    await CheckStatusAsync(HttpMethod.Get, "/api/courses/search", HttpStatusCode.BadRequest);
    await CheckStatusAsync(HttpMethod.Get, "/api/courses/search?keyword=", HttpStatusCode.BadRequest);
    await CheckStatusAsync(HttpMethod.Get, "/api/courses/search?keyword=%20%20", HttpStatusCode.BadRequest);

    object valid = new { id = 2147483647, createdAt = "2000-01-01T00:00:00Z", code = uniqueCode, name = "Môn kiểm tra HTTP", credits = 4, isActive = true };
    using HttpResponseMessage post = await SendAsync(HttpMethod.Post, "/api/courses", valid);
    Check(post.StatusCode == HttpStatusCode.Created, "POST hợp lệ trả 201.");
    CourseResult created = await post.Content.ReadFromJsonAsync<CourseResult>()
        ?? throw new InvalidOperationException("POST thiếu Course trong body.");
    createdIds.Add(created.Id);
    Check(created.Id > 0 && created.Id != 2147483647, "Id do service cấp, không dùng Id đầu vào.");
    Check(created.CreatedAt.Year > 2000, "CreatedAt do database cấp, không dùng giá trị từ request.");
    Check(created.Code == uniqueCode && created.Credits == 4, "POST trả đúng dữ liệu đã tạo.");
    Check(created.CreatedAt.Year >= 2020, "POST trả CreatedAt do database sinh.");
    Check(post.Headers.Location is not null, "POST có Location.");
    using (HttpResponseMessage byLocation = await client.GetAsync(post.Headers.Location!))
    {
        Check(byLocation.StatusCode == HttpStatusCode.OK, "GET theo Location trả 200 ở request khác.");
        CourseResult? fetched = await byLocation.Content.ReadFromJsonAsync<CourseResult>();
        Check(fetched?.Id == created.Id && fetched.Code == uniqueCode, "Dữ liệu được giữ giữa các request.");
    }

    foreach (int credits in new[] { 0, 7 })
    {
        await CheckStatusAsync(HttpMethod.Post, "/api/courses", HttpStatusCode.BadRequest,
            new { code = "SAI_" + uniqueCode, name = "Sai tín chỉ", credits, isActive = true });
    }
    await CheckStatusAsync(HttpMethod.Post, "/api/courses", HttpStatusCode.BadRequest,
        new { code = "   ", name = "Code trống", credits = 3, isActive = true });
    foreach (string raw in new[] { "null", "{broken-json", "{\"code\":\"TEST\",\"credits\":\"abc\"}" })
    {
        using StringContent content = new(raw, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await client.PostAsync("/api/courses", content);
        Check(response.StatusCode == HttpStatusCode.BadRequest, $"Body sai/null bị chặn bằng 400: {raw}");
    }

    object updated = new { id = 0, code = uniqueCode, name = "Đã cập nhật", credits = 6, isActive = false };
    await CheckStatusAsync(HttpMethod.Put, $"/api/courses/{created.Id}", HttpStatusCode.NoContent, updated);
    using (HttpResponseMessage get = await client.GetAsync($"/api/courses/{created.Id}"))
    {
        Check(get.StatusCode == HttpStatusCode.OK, "GET sau PUT trả 200.");
        CourseResult? result = await get.Content.ReadFromJsonAsync<CourseResult>();
        Check(result?.Id == created.Id && result.Name == "Đã cập nhật" && result.Credits == 6 && !result.IsActive,
            "PUT cập nhật đúng, giữ Id từ URL.");
        Check(result is not null && result.CreatedAt == created.CreatedAt, "PUT không thay CreatedAt.");
    }
    await CheckStatusAsync(HttpMethod.Put, $"/api/courses/{created.Id}", HttpStatusCode.BadRequest,
        new { code = uniqueCode, name = "Không được lưu", credits = 7, isActive = true });
    using (HttpResponseMessage get = await client.GetAsync($"/api/courses/{created.Id}"))
    {
        CourseResult? result = await get.Content.ReadFromJsonAsync<CourseResult>();
        Check(result?.Credits == 6 && result.Name == "Đã cập nhật", "PUT không hợp lệ không sửa dữ liệu cũ.");
    }
    await CheckStatusAsync(HttpMethod.Put, "/api/courses/2147483647", HttpStatusCode.NotFound, updated);
    await CheckStatusAsync(HttpMethod.Delete, $"/api/courses/{created.Id}", HttpStatusCode.NoContent);
    createdIds.Remove(created.Id);
    await CheckStatusAsync(HttpMethod.Get, $"/api/courses/{created.Id}", HttpStatusCode.NotFound);
    await CheckStatusAsync(HttpMethod.Delete, $"/api/courses/{created.Id}", HttpStatusCode.NotFound);
    Check((await SearchAsync(uniqueCode)).Count == 0, "Search không còn môn vừa xóa.");

    Console.WriteLine($"\nHoàn thành: {passed} phép kiểm tra đạt.");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"\nFAIL sau {passed} phép kiểm tra đạt: {ex.Message}");
    Console.Error.WriteLine("Dừng tại lỗi đầu tiên. Các phép kiểm tra phía sau CHƯA chạy.");
    Console.Error.WriteLine("Kiểm tra API đang chạy, profile http, cổng và dữ liệu C001/C002/C003.");
    exitCode = 1;
}
finally
{
    foreach (int id in createdIds)
    {
        try
        {
            using HttpResponseMessage cleanup = await client.DeleteAsync($"/api/courses/{id}");
            if (cleanup.StatusCode != HttpStatusCode.NoContent && cleanup.StatusCode != HttpStatusCode.NotFound)
                Console.Error.WriteLine($"Lưu ý: chưa dọn được dữ liệu kiểm tra Id {id}.");
        }
        catch (HttpRequestException)
        {
            Console.Error.WriteLine($"Lưu ý: không kết nối được API để dọn Id {id}.");
        }
        catch (TaskCanceledException)
        {
            Console.Error.WriteLine($"Lưu ý: hết thời gian dọn Id {id}.");
        }
    }
}
return exitCode;

internal sealed record CourseResult(int Id, string Code, string Name, int Credits, bool IsActive, DateTime CreatedAt);
