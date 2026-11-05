#!/usr/bin/env bash
# Racha - solution scaffold (Step 1).
# Run once from the repository root:  bash setup.sh
# Works on macOS/Linux and on Windows via Git Bash. Requires .NET 8 SDK.
set -euo pipefail

if [ -d src ] || [ -d tests ]; then
  echo "src/ or tests/ already exists - aborting to avoid overwriting." >&2
  exit 1
fi

# Pinned to .NET 8-compatible versions on purpose: without --version,
# `dotnet add package` picks the newest major, which may target a newer runtime.
EF_VERSION=8.0.11
ASPNET_VERSION=8.0.11

echo "==> Solution-level files"
# Each step is skipped if its file already exists, so the script can be re-run.
[ -f global.json ]   || dotnet new globaljson --sdk-version 8.0.100 --roll-forward latestFeature
[ -f Racha.sln ]     || dotnet new sln -n Racha
[ -f .gitignore ]    || dotnet new gitignore
[ -f .editorconfig ] || dotnet new editorconfig

echo "==> SDK in use: $(dotnet --version)"

echo "==> Source projects"
dotnet new classlib -n Racha.Domain         -o src/Racha.Domain         -f net8.0
dotnet new classlib -n Racha.Application    -o src/Racha.Application    -f net8.0
dotnet new classlib -n Racha.Infrastructure -o src/Racha.Infrastructure -f net8.0
dotnet new webapi   -n Racha.Api            -o src/Racha.Api            -f net8.0 --use-controllers
dotnet new webapi   -n Racha.Auth.Api       -o src/Racha.Auth.Api       -f net8.0 --use-controllers

echo "==> Test projects"
for p in Domain Application Infrastructure Api Auth.Api; do
  dotnet new xunit -n "Racha.$p.Tests" -o "tests/Racha.$p.Tests" -f net8.0
done

echo "==> Remove template boilerplate"
rm -f src/Racha.Domain/Class1.cs src/Racha.Application/Class1.cs src/Racha.Infrastructure/Class1.cs
rm -f tests/*/UnitTest1.cs
for api in Racha.Api Racha.Auth.Api; do
  rm -f "src/$api/WeatherForecast.cs" "src/$api/Controllers/WeatherForecastController.cs" "src/$api/$api.http"
done

echo "==> Add projects to the solution"
dotnet sln add src/*/*.csproj --solution-folder src
dotnet sln add tests/*/*.csproj --solution-folder tests

echo "==> Project references (dependencies point inward)"
dotnet add src/Racha.Application    reference src/Racha.Domain
dotnet add src/Racha.Infrastructure reference src/Racha.Application
dotnet add src/Racha.Api            reference src/Racha.Application src/Racha.Infrastructure
dotnet add src/Racha.Auth.Api       reference src/Racha.Application src/Racha.Infrastructure

dotnet add tests/Racha.Domain.Tests         reference src/Racha.Domain
dotnet add tests/Racha.Application.Tests    reference src/Racha.Application
dotnet add tests/Racha.Infrastructure.Tests reference src/Racha.Infrastructure
dotnet add tests/Racha.Api.Tests            reference src/Racha.Api
dotnet add tests/Racha.Auth.Api.Tests       reference src/Racha.Auth.Api

echo "==> NuGet packages"
dotnet add src/Racha.Application package FluentValidation --version 11.11.0
dotnet add src/Racha.Application package FluentValidation.DependencyInjectionExtensions --version 11.11.0

dotnet add src/Racha.Infrastructure package Microsoft.EntityFrameworkCore --version $EF_VERSION
dotnet add src/Racha.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL --version $EF_VERSION
dotnet add src/Racha.Infrastructure package Microsoft.EntityFrameworkCore.Design --version $EF_VERSION
dotnet add src/Racha.Infrastructure package Microsoft.AspNetCore.Cryptography.KeyDerivation --version $ASPNET_VERSION
dotnet add src/Racha.Infrastructure package System.IdentityModel.Tokens.Jwt --version 8.2.1

for api in Racha.Api Racha.Auth.Api; do
  dotnet add "src/$api" package Microsoft.AspNetCore.Authentication.JwtBearer --version $ASPNET_VERSION
  dotnet add "src/$api" package Microsoft.EntityFrameworkCore.Design --version $EF_VERSION
done

for t in tests/*/; do
  # FluentAssertions pinned to 6.x: v8+ requires a commercial license.
  dotnet add "$t" package FluentAssertions --version 6.12.2
  dotnet add "$t" package NSubstitute --version 5.3.0
done

for t in Racha.Infrastructure.Tests Racha.Api.Tests Racha.Auth.Api.Tests; do
  dotnet add "tests/$t" package Testcontainers.PostgreSql --version 4.0.0
done
dotnet add tests/Racha.Api.Tests      package Microsoft.AspNetCore.Mvc.Testing --version $ASPNET_VERSION
dotnet add tests/Racha.Auth.Api.Tests package Microsoft.AspNetCore.Mvc.Testing --version $ASPNET_VERSION

echo "==> Build and test"
dotnet build
dotnet test --no-build

echo
echo "Done. Next: docker compose up -d   (starts PostgreSQL)"
