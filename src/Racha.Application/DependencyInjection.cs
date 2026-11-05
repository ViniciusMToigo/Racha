using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Racha.Application.Auth;
using Racha.Application.Balances;
using Racha.Application.Expenses;
using Racha.Application.Groups;

namespace Racha.Application;

public static class DependencyInjection
{
    /// <summary>Registers use-case services and every validator in this assembly.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IBalanceService, BalanceService>();

        return services;
    }
}
