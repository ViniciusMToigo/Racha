# GenAI usage log

How AI tools were used to build Racha, and where their output was validated, corrected or rejected.

| # | Step | What I asked | What the AI suggested | What I checked / changed |
|---|------|--------------|-----------------------|--------------------------|
| 1 | Planning | Plan a Splitwise-like expense app following the exercise requirements | Clean Architecture with 4 layers, PostgreSQL, two Web API projects | Chose PostgreSQL over SQL Server: the SQL Server image does not run well on Apple Silicon, which could break setup for reviewers |
| 2 | Scaffold | Generate the solution scaffold for .NET 8 | Script with `dotnet new` and `dotnet add package` | Pinned package versions: without `--version`, the CLI picks the newest major (e.g. EF Core 9/10), which doesn't target .NET 8 |
| 3 | Scaffold | Test libraries | FluentAssertions + NSubstitute | Pinned FluentAssertions to 6.x because v8+ changed to a commercial license |

## Prompting practices used

- Gave the AI the full exercise PDF as context instead of paraphrasing it.
- Split the work into small, reviewable steps (scaffold → domain → application → infrastructure → API → frontend).
- Asked for tests first for every business rule (TDD), then reviewed the test cases before accepting the implementation.
