# TH4 Application Architecture Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task by task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Tóm tắt để duyệt:** Giữ API và database TH3, đưa request/response qua DTO; chuyển truy vấn EF Core vào Repository; để Service quyết định validation và quy tắc nghiệp vụ; dùng một exception handler trả `ProblemDetails`. Thêm endpoint chỉ **kiểm tra khả năng đăng ký** Course Active/Inactive, chưa lưu Enrollment. Sau khi code và kiểm chứng, viết hướng dẫn video 4:40–4:50 dựa trên kết quả thực tế. Tài liệu này là kế hoạch, chưa phải phần triển khai.

**Goal:** Refactor the existing Course API into clear Controller → Service → Repository → EF Core boundaries, preserve TH2/TH3 behavior, demonstrate the Inactive registration rule, and prepare an accurate video guide under five minutes.

**Architecture:** Keep `Models/Course.cs` as the EF entity so the existing migrations and database remain valid; the assignment permits different folder names. HTTP endpoints use request/response DTOs. `CourseService` owns validation and business decisions, `CourseRepository` owns EF Core queries and writes, and one exception handler emits consistent `ProblemDetails` responses. A small registration eligibility endpoint proves the new rule without creating an Enrollment table or claiming to persist registrations.

**Tech Stack:** .NET 10, ASP.NET Core Web API, EF Core 10, Npgsql/PostgreSQL, Swashbuckle, existing console HTTP checks.

**Spec:** `C:\Users\84328\Downloads\Bài tập thực hành phần 01.pdf`, sections 5.1–5.3 and 7–8. The TH3 baseline is `source/README.md` and the current `Course.Api.Checks/Program.cs`.

## Global Constraints

- Preserve `GET/POST/PUT/DELETE /api/courses` and `GET /api/courses/search?keyword=...` with their current success and error status codes.
- Preserve JSON response fields `id`, `code`, `name`, `credits`, `isActive`, `createdAt`; retain `201 Created` plus `Location` for POST.
- Preserve search semantics: trim input, case insensitive substring over Code or Name, escape `%`, `_`, `\`; empty keyword returns 400 and no match returns `200 []`.
- Preserve `CreatedAt` and existing Course records; do not delete or recreate the database or edit the two TH3 migrations for this refactor.
- Keep connection secrets out of source. Use the existing `ConnectionStrings:CourseDb` configuration.
- The registration use case is an eligibility check only: `POST /api/courses/{id}/registration-check` returns 204 for Active, 409 for Inactive, 404 for missing Id. It does not create a student registration.
- Preserve the existing PostgreSQL backed HTTP checks, and extend them only where needed to prove the new architecture's outward behavior.

## File Map

| File | Responsibility |
| --- | --- |
| `Course.Api/DTOs/CourseUpsertRequest.cs` | Four client editable fields for POST/PUT. |
| `Course.Api/DTOs/CourseResponse.cs` | Public Course shape including DB generated Id and CreatedAt. |
| `Course.Api/Repositories/ICourseRepository.cs` | Data access contract used by Service. |
| `Course.Api/Repositories/CourseRepository.cs` | All EF Core Course queries, inserts, updates, deletes and saves. |
| `Course.Api/Exceptions/CourseValidationException.cs` | Domain input failure. |
| `Course.Api/Exceptions/CourseNotFoundException.cs` | Missing Course failure. |
| `Course.Api/Exceptions/CourseInactiveException.cs` | Registration rule failure. |
| `Course.Api/Exceptions/ApiExceptionHandler.cs` | Central exception to HTTP `ProblemDetails` mapping. |
| `Course.Api/Models/Course.cs` | Existing EF entity and its validation invariant; keep EF mapping and migrations stable. |
| `Course.Api/Controllers/CoursesController.cs` | Bind DTOs, call Service, return success responses. |
| `Course.Api/Services/ICourseService.cs`, `CourseService.cs` | DTO mapping, validation, CRUD and registration decision. |
| `Course.Api/Program.cs` | Scoped DI, exception handling and Swagger metadata. |
| `Course.Api.Checks/Program.cs` | HTTP regression and new rule/error scenarios. |
| `Course.Api/Course.Api.http` | Manual demo requests. |
| `README.md`, `HUONG_DAN_VIDEO_TH4.md` | Run instructions, decisions and timed presentation prompts. |

## Review Focus

1. A client sends `id` or `createdAt` in POST/PUT JSON: the DTO must not allow those values to overwrite server data; assert generated Id and unchanged CreatedAt in Task 1.
2. Empty, whitespace or malformed JSON: every 400 response must be understandable as `ProblemDetails`; assert response shape in Task 3.
3. A nonexistent Course on GET, PUT, DELETE or registration check: return 404 without turning it into 500; assert in Tasks 3–4.
4. Search input containing `%`, `_` or `\`: treat it as literal text, not a wildcard; retain an HTTP check in Task 2.
5. An Inactive Course cannot pass the registration check while an Active Course can; prove both cases in Task 4.

---

### Task 1: Introduce API DTOs without changing the public Course behavior

**Files:** Create `Course.Api/DTOs/CourseUpsertRequest.cs`, `Course.Api/DTOs/CourseResponse.cs`; modify Controller and Service interface/implementation; extend `Course.Api.Checks/Program.cs`.

**Interfaces:** `CourseUpsertRequest(string Code, string Name, int Credits, bool IsActive)` and `CourseResponse(int Id, string Code, string Name, int Credits, bool IsActive, DateTime CreatedAt)` (records or equivalent classes). `ICourseService` exposes `Task<List<CourseResponse>> GetAllAsync()`, `Task<CourseResponse?> GetByIdAsync(int id)`, `Task<CourseResponse> CreateAsync(CourseUpsertRequest request)`, `Task<bool> UpdateAsync(int id, CourseUpsertRequest request)`, `Task<bool> DeleteAsync(int id)`, and `Task<List<CourseResponse>> SearchAsync(string? keyword)` at this stage. Keep the entity inside Service/Repository boundaries.

- [ ] Add a Swagger schema check that POST/PUT requests expose only `code`, `name`, `credits`, `isActive`; add HTTP checks showing POST/PUT ignore incoming `id` and `createdAt`, POST returns the public Course shape and `Location`, and PUT leaves `CreatedAt` unchanged.
- [ ] Run the checks against the TH3 API. Confirm the Swagger request schema check fails because TH3 binds the Entity; keep existing passing HTTP regressions intact.
- [ ] Add DTOs and map Entity → response in Service. Bind request DTOs in Controller. Keep Code/Name/Credits/IsActive behavior and success codes.
- [ ] Run `dotnet build TH3.slnx` and the HTTP checks against the configured local PostgreSQL database; require all previous CRUD/search checks to pass.

### Task 2: Extract EF Core access behind a scoped Repository

**Files:** Create `Course.Api/Repositories/ICourseRepository.cs`, `CourseRepository.cs`; modify Service and `Program.cs`; extend HTTP search checks.

**Interfaces:** `ICourseRepository` supplies `GetAllAsync()`, `GetByIdAsync(int id)`, `GetTrackedByIdAsync(int id)`, `SearchAsync(string keyword)`, `Add(Course course)`, `Remove(Course course)`, and `SaveChangesAsync()`; all returned Course objects are internal entities. `CourseRepository` receives `CourseDbContext`. `CourseService` receives `ICourseRepository` only.

- [ ] Add a regression check for literal wildcard search and retain existing CRUD/search checks.
- [ ] Run the checks on the current implementation. This task is a behavior preserving extraction, so the existing passing checks characterize behavior; no new HTTP failure is expected.
- [ ] Move EF Core calls, query ordering, `AsNoTracking`, `ILike` and save operations into Repository. Register `AddScoped<ICourseRepository, CourseRepository>()` beside the scoped Service and DbContext.
- [ ] Build and run HTTP checks. Inspect Service and Controller to confirm neither references `CourseDbContext`; inspect Repository for the only application layer EF query logic.

### Task 3: Centralize validation and error handling

**Files:** Create the validation/not found exception types and `ApiExceptionHandler.cs`; modify Service, Controller, `Program.cs` and HTTP checks.

**Interfaces:** Service invokes `Course.Validate()` for Code not blank and Credits from 1 to 6 before persistence, and rejects blank search keyword. Change `Course.Validate()` to raise `CourseValidationException` so validation has one declared error type. Missing Course operations raise `CourseNotFoundException`. `ApiExceptionHandler : IExceptionHandler` maps validation to 400, missing to 404 and unexpected exceptions to 500. All mapped responses use `ProblemDetails` with status/title/detail; do not expose exception internals for 500. Configure `[ApiController]` model binding failures to the same ProblemDetails style.

- [ ] Add HTTP assertions for blank Code, Credits 0/7, missing/blank keyword, malformed body, missing Id and a consistent error shape; verify invalid PUT leaves the original DB row untouched.
- [ ] Run the checks before implementation and confirm the new error shape assertions fail for the expected reason.
- [ ] Move HTTP error decisions out of Controller, replace repeated `try/catch` with exceptions from Service, and configure one handler in `Program.cs`.
- [ ] Build and rerun the HTTP checks; require the earlier status codes and the new response shape assertions to pass.

### Task 4: Prove the Inactive Course registration rule

**Files:** Modify Service interface/implementation, Controller, exception handler, HTTP checks and `.http` demo file; create `CourseInactiveException.cs`.

**Requirement in plain language:** Before a new student registration, look up the Course. A missing Course fails; an Inactive Course is ineligible; an Active Course is eligible. The small endpoint checks eligibility and does not save an Enrollment.

**Interfaces:** `ICourseService.EnsureCanRegisterAsync(int courseId) : Task`; `POST /api/courses/{id}/registration-check` returns 204 for eligible, 404 if absent, 409 with ProblemDetails if Inactive. The rule belongs in Service because it is a business decision; Repository only retrieves the Course.

- [ ] Add HTTP scenarios for sample Active `C002` → 204, sample Inactive `C003` → 409, and unknown Id → 404; also assert the check does not add rows.
- [ ] Run them first and confirm the new route is absent.
- [ ] Implement the Service guard, exception mapping and Controller endpoint, with Swagger response metadata.
- [ ] Build and run all HTTP checks; manually inspect `Courses` count before/after the eligibility requests.

### Task 5: Document and rehearse a truthful under five minute video

**Files:** Modify `README.md` and `Course.Api/Course.Api.http`; create `HUONG_DAN_VIDEO_TH4.md`.

- [ ] Update README with structure, DI chain, configuration, unchanged migrations, endpoint table and exact eligibility semantics.
- [ ] Add `.http` examples for success and failure, using the Id actually returned by POST for CRUD demos.
- [ ] Write a 4:40–4:50 screen and voice outline: architecture/DI, DTO mapping, validation/central errors, Repository trade-off, one request from HTTP to PostgreSQL and back, CRUD/Search, Active/Inactive registration check. Mark it as prompts for the student's own explanation, not a speech to read verbatim.
- [ ] Compare each video checkpoint with source and an observed result; remove any claim that was not actually demonstrated.

## Final Verification During Execution

- [ ] `dotnet build TH3.slnx` exits 0 with no warnings/errors.
- [ ] Apply pending migrations with `dotnet ef database update --project Course.Api --startup-project Course.Api` on a configured **test** database; do not rollback or recreate an existing database. Confirm the two TH3 migrations and existing Course rows remain.
- [ ] Start API with `dotnet run --project Course.Api --launch-profile http`, then run `dotnet run --project Course.Api.Checks -- http://localhost:5082` in another terminal; require every check to pass.
- [ ] Inspect Swagger for all existing routes and the new eligibility route; observe both successful and invalid requests and their status/body.
- [ ] Confirm no password or connection secret was added to source and no additional database migration was introduced by the refactor.
- [ ] Recheck the final video outline against the code and time it aloud; the student records and explains it in their own words.

## Execution Boundary

This document is planning only. Do not edit application code, run migrations, or record a video until the user asks to execute the plan.
