# Racha

> *"Rachar a conta"* — Brazilian slang for splitting the bill.

Racha is a simplified Splitwise: members of a group record shared expenses and see who owes whom.

## User story

> As a member of a shared household or trip, I want to record expenses paid by any member of my group and see who owes whom, so that we can settle up fairly without manual math.

## Tech stack

- **Backend:** .NET 8, ASP.NET Core Web API, EF Core 8, PostgreSQL 16
- **Frontend:** Angular (coming in step 6)
- **Tests:** xUnit, FluentAssertions, NSubstitute, Testcontainers

## Solution structure

```
src/
  Racha.Domain          Entities and business rules. No dependencies.
  Racha.Application     Use cases, interfaces, DTOs, validation.
  Racha.Infrastructure  EF Core, PostgreSQL, repositories, password hashing, JWT.
  Racha.Api             Expenses API (groups, expenses, balances).
  Racha.Auth.Api        Auth API (register, login, current user).
tests/
  One test project per source project.
```

## Getting started

Prerequisites: .NET 8 SDK, Docker.

```bash
docker compose up -d     # start PostgreSQL
dotnet build
dotnet test
```

_Run instructions, seeded credentials and API docs will be added as the project grows._
