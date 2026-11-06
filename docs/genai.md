# Use of GenAI

This document covers the **Generative AI tools** part of the exercise:

1. [How I used GenAI to build Racha](#1-how-i-used-genai-to-build-racha): workflow and prompting practices.
2. [The task-management API exercise](#2-the-task-management-api-exercise): the prompt, the generated code and how I reviewed it.
3. [Real corrections made in Racha](#3-real-corrections-made-in-racha): where AI output was wrong, risky or incomplete, and who caught it.
4. [Takeaways](#4-takeaways).

---

## 1. How I used GenAI to build Racha

**Tool:** Claude (Anthropic), used as a pair programmer with access to the repository. I compiled, ran and tested everything locally in JetBrains Rider; the AI could read and write files but could not run the .NET build on my machine, so **every backend change was compiled and tested by me** before it was committed.

### Workflow

| Practice | Why |
|---|---|
| Gave the AI the **full exercise PDF** instead of paraphrasing it | Requirements stay exact (e.g. "a *second* API", "seeded credentials", "no console warnings"). |
| **Planned before coding**: user story, data model, architecture and endpoints, approved by me first | Cheap to change a plan, expensive to change code. |
| **Small steps**: scaffold → Domain → Application → Infrastructure → APIs → frontend → docs | Each step small enough to review line by line. |
| **TDD with the AI**: tests and stubs first (`test:` commit, all red), implementation second (`feat:` commit, all green) | I reviewed the *tests* before any implementation existed, so the AI had to satisfy my expectations, not its own. |
| Asked for the **reasoning behind each decision** | I need to defend every line in the code review; "the AI did it" is not an answer. |
| **Manual end-to-end testing** after each step (Swagger, the UI) | Found a bug that every automated test had missed (see [§3](#3-real-corrections-made-in-racha)). |
| CI on every push | Nothing depends on "it works on my machine". |

### Prompting practices

- **Context first**: stack and versions (.NET 8, EF Core 8, Angular 22), architecture rules ("Domain has no dependencies"), conventions.
- **Explicit constraints**: "pin package versions compatible with .NET 8", "no business logic in controllers", "return 404, not 403, to non-members".
- **Ask for tests and edge cases explicitly**: AI output defaults to the happy path.
- **One concern per prompt**: smaller outputs are easier to verify.
- **Challenge the output**: "what can go wrong with this?", "what would a security reviewer flag?".

---

## 2. The task-management API exercise

> Generate a RESTful API for a simple task management system: CRUD on tasks; each task has `title`, `description`, `status` and `due_date`; tasks belong to a user (assume a basic User model exists).

I used C# / ASP.NET Core 8, the same stack as Racha.

### 2.1 First attempt: a one-line prompt

```text
Create a .NET REST API for tasks with title, description, status and due date. Tasks belong to a user.
```

This is representative of what that prompt produces. It compiles and the happy path works, which is exactly why it is dangerous:

```csharp
public class Task                                     // (1)
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }                // (2)
    public DateTime DueDate { get; set; }             // (3)
    public int UserId { get; set; }
}

[ApiController, Route("api/tasks")]
public class TasksController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int userId) =>          // (4)
        Ok(await db.Tasks.Where(t => t.UserId == userId).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) =>
        Ok(await db.Tasks.FindAsync(id));                            // (5)

    [HttpPost]
    public async Task<IActionResult> Create(Task task)               // (6)
    {
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
        return Ok(task);                                             // (7)
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Task task)
    {
        try
        {
            db.Entry(task).State = EntityState.Modified;             // (8)
            await db.SaveChangesAsync();
            return Ok(task);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);                      // (9)
        }
    }
}
```

**What my review found:**

| # | Problem | Impact |
|---|---|---|
| 1 | A class called `Task` collides with `System.Threading.Tasks.Task` (implicit usings) | Ambiguous references everywhere; the "fix" the AI suggests is usually fully qualified names, which is just noise. |
| 2 | `Status` is a free string | `"done"`, `"Done"` and `"DONE "` are three different states. |
| 3 | `DateTime` for a due *date* | Time zones shift the day when serialized. A due date is a calendar date. |
| 4 | **The client chooses `userId`** | Anyone can list anyone's tasks: **Broken Object Level Authorization**, #1 in the OWASP API Security Top 10. |
| 5 | `Get` doesn't check the owner, returns `200 null` for unknown ids | Same BOLA hole; wrong status code. |
| 6 | Binds the **entity** from the body | Mass assignment: the client can set `Id` or `UserId`. |
| 7 | `Ok` instead of `201 Created` + `Location` | Not RESTful. |
| 8 | Attaches the client's object as `Modified` | Overwrites every column, including the owner; no ownership check; no concurrency control. |
| 9 | Returns `ex.Message` with a 500 | Leaks internal details (SQL, stack traces). |
| – | No authentication, no validation, no pagination, no tests | |

### 2.2 The prompt I would actually use

```text
You are a senior .NET engineer. Build a REST API for a task management system.

CONTEXT
- .NET 8, ASP.NET Core minimal APIs, EF Core 8 with PostgreSQL, FluentValidation 11.
- Authentication already exists: requests carry a JWT whose "sub" claim is the user's Guid id.
  A basic User entity exists (Guid Id, string Email).
- Pin NuGet versions compatible with .NET 8 and list them. Do not invent versions.

DOMAIN
- Entity "TaskItem" (do NOT name it "Task": it collides with System.Threading.Tasks.Task).
- Fields: Id (Guid), OwnerId (Guid), Title (required, max 200), Description (optional, max 2000),
  Status (enum: Todo, InProgress, Done; serialized as strings), DueDate (DateOnly, optional).
- New tasks start as Todo. On create, DueDate cannot be in the past (use an injected TimeProvider).

ENDPOINTS (all require authentication)
- GET    /api/tasks?status=&page=&pageSize=   only the caller's tasks; pageSize max 100
- GET    /api/tasks/{id}
- POST   /api/tasks                            201 Created + Location
- PUT    /api/tasks/{id}                       full update of title, description, status, due date
- DELETE /api/tasks/{id}                       204

SECURITY AND CORRECTNESS RULES
- The owner always comes from the token, never from the request.
- A task owned by someone else must be indistinguishable from a missing one: return 404.
- Use request/response DTOs; never bind or return the entity.
- Errors as RFC 9457 ProblemDetails via a global exception handler; never return exception messages.
- Optimistic concurrency on update (PostgreSQL xmin); a conflict returns 409.

TESTS
- xUnit + FluentAssertions + WebApplicationFactory against a Testcontainers PostgreSQL.
- Cover: unauthenticated 401; another user's task 404 on GET/PUT/DELETE; validation 400 per field;
  past due date 400; list scoped to the caller and paginated; create returns 201 with Location.

OUTPUT
- Write the tests first, then the implementation.
- Then list the assumptions you made and anything you are unsure about.
```

**Why it is written this way:** context and versions remove guesswork; the security rules name the exact failure modes from §2.1 instead of hoping the model avoids them; requiring tests first gives me something to review before any implementation; asking for assumptions surfaces the places where the model guessed.

### 2.3 Representative output (after review)

> This sample illustrates the exercise. It is **not part of the Racha solution** and is not compiled by CI; the same patterns (DTOs, ownership enforced in queries, ProblemDetails, Testcontainers tests) are implemented and tested for real in Racha.

**Model and contracts**

```csharp
public enum TaskItemStatus { Todo, InProgress, Done }

public sealed class TaskItem
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 2000;

    private TaskItem() { }   // for EF Core

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public uint Version { get; private set; }           // mapped to PostgreSQL xmin (concurrency token)

    public static TaskItem Create(Guid ownerId, string title, string? description, DateOnly? dueDate) =>
        new()
        {
            OwnerId = ownerId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            DueDate = dueDate,
            Status = TaskItemStatus.Todo,
        };

    public void Update(string title, string? description, TaskItemStatus status, DateOnly? dueDate)
    {
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Status = status;
        DueDate = dueDate;
    }
}

public sealed record CreateTaskRequest(string Title, string? Description, DateOnly? DueDate);
public sealed record UpdateTaskRequest(string Title, string? Description, TaskItemStatus Status, DateOnly? DueDate);
public sealed record TaskResponse(Guid Id, string Title, string? Description, TaskItemStatus Status, DateOnly? DueDate);
```

**Validation**

```csharp
public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator(TimeProvider clock)
    {
        RuleFor(r => r.Title).NotEmpty().MaximumLength(TaskItem.MaxTitleLength);
        RuleFor(r => r.Description).MaximumLength(TaskItem.MaxDescriptionLength);
        RuleFor(r => r.DueDate)
            .Must(due => due is null || due >= DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime))
            .WithMessage("Due date cannot be in the past.");
    }
}

public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(r => r.Title).NotEmpty().MaximumLength(TaskItem.MaxTitleLength);
        RuleFor(r => r.Description).MaximumLength(TaskItem.MaxDescriptionLength);
        RuleFor(r => r.Status).IsInEnum();
        // No "not in the past" rule here: editing an overdue task must stay possible.
    }
}
```

**Endpoints**

```csharp
public static class TaskEndpoints
{
    public static RouteGroupBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var tasks = app.MapGroup("/api/tasks").RequireAuthorization().WithTags("Tasks");

        tasks.MapGet("/", async (
            ClaimsPrincipal user, AppDbContext db, TaskItemStatus? status,
            int page = 1, int pageSize = 20, CancellationToken ct = default) =>
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = db.Tasks.AsNoTracking().Where(t => t.OwnerId == user.GetUserId());
            if (status is not null)
            {
                query = query.Where(t => t.Status == status);
            }

            var items = await query
                .OrderBy(t => t.DueDate == null).ThenBy(t => t.DueDate).ThenBy(t => t.Title)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(t => new TaskResponse(t.Id, t.Title, t.Description, t.Status, t.DueDate))
                .ToListAsync(ct);

            return TypedResults.Ok(items);
        });

        tasks.MapGet("/{id:guid}", async Task<Results<Ok<TaskResponse>, NotFound>> (
            Guid id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            // Filtering by owner in the query: another user's task is simply "not found".
            var task = await db.Tasks.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == user.GetUserId(), ct);

            return task is null ? TypedResults.NotFound() : TypedResults.Ok(task.ToResponse());
        });

        tasks.MapPost("/", async (
            CreateTaskRequest request, IValidator<CreateTaskRequest> validator,
            ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            await validator.ValidateAndThrowAsync(request, ct);   // global handler turns this into a 400

            var task = TaskItem.Create(user.GetUserId(), request.Title, request.Description, request.DueDate);
            db.Tasks.Add(task);
            await db.SaveChangesAsync(ct);

            return TypedResults.Created($"/api/tasks/{task.Id}", task.ToResponse());
        });

        tasks.MapPut("/{id:guid}", async Task<Results<Ok<TaskResponse>, NotFound, Conflict>> (
            Guid id, UpdateTaskRequest request, IValidator<UpdateTaskRequest> validator,
            ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            await validator.ValidateAndThrowAsync(request, ct);

            var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == user.GetUserId(), ct);
            if (task is null)
            {
                return TypedResults.NotFound();
            }

            task.Update(request.Title, request.Description, request.Status, request.DueDate);
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                return TypedResults.Conflict();   // someone changed it since it was loaded
            }

            return TypedResults.Ok(task.ToResponse());
        });

        tasks.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (
            Guid id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var deleted = await db.Tasks
                .Where(t => t.Id == id && t.OwnerId == user.GetUserId())
                .ExecuteDeleteAsync(ct);

            return deleted == 0 ? TypedResults.NotFound() : TypedResults.NoContent();
        });

        return tasks;
    }

    private static TaskResponse ToResponse(this TaskItem t) =>
        new(t.Id, t.Title, t.Description, t.Status, t.DueDate);
}

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id)
            ? id
            : throw new UnauthorizedAccessException("The token does not identify a user.");
}
```

**Supporting configuration** (abridged): `TaskItem.Version` mapped with `.IsRowVersion()` to PostgreSQL's `xmin`; `JsonStringEnumConverter` registered so statuses are `"Todo"`, not `0`; a global `IExceptionHandler` maps `ValidationException` → 400 `ValidationProblemDetails` and anything unexpected → a generic 500 (the same pattern as `Racha.Web.Common/Errors/ApiExceptionHandler.cs`).

**Tests** (excerpt)

```csharp
[Fact]
public async Task Get_TaskOwnedByAnotherUser_Returns404()
{
    var alice = await factory.CreateUserClientAsync();
    var bob = await factory.CreateUserClientAsync();
    var created = await alice.PostAsJsonAsync("/api/tasks", new CreateTaskRequest("Pay rent", null, null));
    var task = await created.Content.ReadFromJsonAsync<TaskResponse>();

    var response = await bob.GetAsync($"/api/tasks/{task!.Id}");

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}

[Fact]
public async Task Create_WithDueDateInThePast_Returns400OnDueDate()
{
    var alice = await factory.CreateUserClientAsync();

    var response = await alice.PostAsJsonAsync("/api/tasks",
        new CreateTaskRequest("Old task", null, new DateOnly(2000, 1, 1)));

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!
        .Errors.Should().ContainKey("dueDate");
}
```

### 2.4 How I validate AI output

The checklist I apply to generated code, here and throughout Racha:

1. **Read every line** against the requirements and the prompt's rules. A plausible-looking line is not a reviewed line.
2. **Security review against the OWASP API Security Top 10**: ownership taken from the token and enforced *inside the query* (BOLA, #1), DTOs instead of entities (mass assignment), no internal details in errors, pagination capped (resource consumption).
3. **Tests for the edge cases before trusting the implementation**: another user's task on every verb, invalid input per field, pagination bounds, unauthenticated calls.
4. **Dependencies**: checked that every package version exists and targets .NET 8, and checked licences (see the FluentAssertions case below).
5. **Run it like a user**: in Racha, every endpoint was exercised in Swagger with different users and every screen was clicked through by hand, which is how the most subtle bug was found (§3, row 14).

### 2.5 Edge cases, authentication and validation

| Case | Handling |
|---|---|
| Another user's task id | 404 on GET, PUT and DELETE (filtered in the query, so there is no "load then check" gap). |
| Unauthenticated request | 401 (`RequireAuthorization` on the whole route group). |
| Blank or too long title, too long description | 400 with per-field errors. |
| Due date in the past | Rejected on create; allowed on update, so overdue tasks can still be edited. |
| Unknown status value | 400: rejected by the enum converter / `IsInEnum`. |
| Concurrent edits | 409 via the `xmin` concurrency token instead of silently losing an update. |
| Huge `pageSize` | Clamped to 100. |
| Delete of an already deleted task | 404 (idempotent from the client's point of view). |
| Unexpected exception | Generic 500 ProblemDetails; details only in the server log. |

---

## 3. Real corrections made in Racha

Each row happened while building this project. "Caught by" is honest: some issues the AI flagged in its own output, some only showed up when **I** compiled, ran or tested the code.

| # | Area | What the AI first produced or assumed | Problem | Fix | Caught by |
|---|---|---|---|---|---|
| 1 | Planning | SQL Server as the database | The SQL Server image runs poorly on Apple Silicon: a reviewer on a Mac might not be able to run the project. | PostgreSQL in Docker. | AI, when I asked for the easier option |
| 2 | Scaffold | `dotnet add package` without versions | The CLI picks the newest major (EF Core 9/10), which doesn't target .NET 8. | Every version pinned. | AI self-review |
| 3 | Scaffold | FluentAssertions, latest version | v8+ changed to a **commercial licence**. | Pinned to 6.12.2. | AI self-review |
| 4 | Scaffold | `global.json` pinned to SDK 8 | My machine only had SDK 9: every `dotnet` command failed and the setup script stopped halfway. | Installed SDK 8, made the script re-runnable. | **Me** (setup failed) |
| 5 | Scaffold | Template's IIS Express launch profile | Windows-only and not installed; Rider couldn't start the API. | Removed it; Kestrel profiles on fixed ports. | **Me** (run failed) |
| 6 | Domain tests | `BeEquivalentTo([...])` with C# 12 collection expressions | Ambiguous between FluentAssertions overloads. | `new[] { ... }`. | AI self-review before I compiled |
| 7 | Domain tests | `[InlineData(10.001m)]` | Decimals aren't valid attribute arguments; and on my pt-BR machine `"10.001"` would parse as ten thousand. | Strings parsed with `InvariantCulture`. | AI self-review |
| 8 | Domain | Re-split an edited expense by clearing and re-adding its shares | EF Core would delete and insert rows with the same composite key. | Reconcile shares in place. | AI self-review |
| 9 | Domain | Divide and round each share | Shares could add up to R$ 99.99 or R$ 100.01. | Integer cents; leftover cents to the lowest ids; tests prove the sum is exact. | AI, with the test written first |
| 10 | Red phase | Stub services with unused constructor parameters | Warning CS9113 + `TreatWarningsAsErrors` = build broken. | Scoped `#pragma` only during the red phase, removed in green. | AI self-review |
| 11 | Infrastructure | `System.IdentityModel.Tokens.Jwt` **8.x** | `JwtBearer 8.0.x` depends on IdentityModel **7.x**. Mixing them compiles but breaks token validation **at runtime**. | `Microsoft.IdentityModel.JsonWebTokens 7.1.2`. | AI self-review |
| 12 | Infrastructure | PBKDF2 with 100,000 iterations (ASP.NET Identity default) | Below the current OWASP recommendation. | 600,000; the count stored in the hash is capped to prevent a DoS. | AI self-review |
| 13 | API tests | `groups!.Select(...)` then `groups.Single(...)` | The `!` only applies to one expression; nullable warning → build error. | Apply `!` where the variable is assigned. | **Compiler on my machine** |
| 14 | Frontend | `<form (ngSubmit)>` around a standalone `[formControl]` | `ngSubmit` only exists with a form directive. The button did a native submit and **reloaded the page**. "Create group", "Add member" and "Rename" did nothing. It compiled, every existing test passed and the screenshots looked right. | Wrapped the controls in a `FormGroup`; added 8 tests that click the real buttons and assert the HTTP call, and verified they fail with the old code. | **Me**, testing the UI by hand |
| 15 | Frontend | Logout button using Material button tokens | Dark text on the blue toolbar (unreadable). | Toolbar theme tokens. | AI, from its own screenshots |

---

## 4. Takeaways

- **The AI is fast at producing plausible code, so plausibility is not a quality signal.** The most serious defects above (the BOLA hole, the runtime JWT version clash, the form that reloaded the page) all *compiled* and most passed the tests that existed at the time.
- **Tests written first are the best prompt.** When I reviewed the tests before the implementation existed, the AI had to meet my definition of correct.
- **Constraints beat hopes.** Naming the failure modes in the prompt ("owner from the token, never from the request", "404 for other users' data") prevents them; a generic "make it secure" doesn't.
- **Run it like a user.** The `ngSubmit` bug was invisible to the compiler, the unit tests and the screenshots; one manual click found it. Now a test covers it.
- **I own the code.** Every decision here is one I can explain and defend, and I changed or rejected the AI's suggestion whenever I couldn't.
