# GenAI usage log

How AI tools were used to build Racha, and where their output was validated, corrected or rejected.

| # | Step | What I asked | What the AI suggested | What I checked / changed |
|---|------|--------------|-----------------------|--------------------------|
| 1 | Planning | Plan a Splitwise-like expense app following the exercise requirements | Clean Architecture with 4 layers, PostgreSQL, two Web API projects | Chose PostgreSQL over SQL Server: the SQL Server image does not run well on Apple Silicon, which could break setup for reviewers |
| 2 | Scaffold | Generate the solution scaffold for .NET 8 | Script with `dotnet new` and `dotnet add package` | Pinned package versions: without `--version`, the CLI picks the newest major (e.g. EF Core 9/10), which doesn't target .NET 8 |
| 3 | Scaffold | Test libraries | FluentAssertions + NSubstitute | Pinned FluentAssertions to 6.x because v8+ changed to a commercial license |
| 4 | Scaffold | Run the API in Rider | Template kept an IIS Express launch profile | Removed it (Windows-only, not installed); kept Kestrel profiles with fixed ports 5080/5090 |
| 5 | Domain (red) | Write failing tests for split and balance rules first | Tests using C# 12 collection expressions inside `BeEquivalentTo([...])` | Rewrote as `new[] { ... }`: FluentAssertions has both `params T[]` and `IEnumerable<T>` overloads, so the collection expression is ambiguous |
| 6 | Domain (red) | Theories with money values | `[InlineData(10.001m)]` | Decimals aren't valid attribute arguments; passed them as strings parsed with `InvariantCulture` (a pt-BR machine would otherwise parse "10.001" as ten thousand) |
| 7 | Domain (green) | Re-split shares when an expense is edited | Clear the shares list and re-add | Reconciled shares in place instead, so EF Core sees updates rather than delete + insert of the same composite key |
| 8 | Domain (green) | Split R$ 100 among 3 | Divide and round each share | Worked in whole cents and gave leftover cents to the lowest ids, so shares always add up exactly to the total |
| 9 | Infrastructure | Pick a JWT library | `System.IdentityModel.Tokens.Jwt 8.x` | Switched to `Microsoft.IdentityModel.JsonWebTokens 7.1.2`: `JwtBearer 8.0.x` depends on IdentityModel 7.x, and mixing 7.x and 8.x compiles but breaks token validation at runtime |
| 10 | Infrastructure | PBKDF2 iteration count | 100,000 (ASP.NET Identity default) | Raised to 600,000 following the current OWASP recommendation for PBKDF2-HMAC-SHA256, and capped the count read from stored hashes to avoid a denial-of-service vector |

## Prompting practices used

- Gave the AI the full exercise PDF as context instead of paraphrasing it.
- Split the work into small, reviewable steps (scaffold → domain → application → infrastructure → API → frontend).
- Asked for tests first for every business rule (TDD), then reviewed the test cases before accepting the implementation.
