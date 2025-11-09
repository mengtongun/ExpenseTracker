using AutoMapper;
using ExpenseTracker.Application.Common.Interfaces.Services;
using ExpenseTracker.Application.Services.Auth;
using ExpenseTracker.Application.Services.Categories;
using ExpenseTracker.Application.Services.Expenses;
using ExpenseTracker.Application.Services.RecurringExpenses;
using ExpenseTracker.Application.Services.Reports;
using ExpenseTracker.Application.Services.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(AssemblyReference.Assembly);
        services.AddValidatorsFromAssembly(AssemblyReference.Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IRecurringExpenseService, RecurringExpenseService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}

