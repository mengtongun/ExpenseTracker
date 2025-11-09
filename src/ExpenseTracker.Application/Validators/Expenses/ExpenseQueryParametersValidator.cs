using ExpenseTracker.Application.Contracts.Expenses;
using FluentValidation;

namespace ExpenseTracker.Application.Validators.Expenses;

public class ExpenseQueryParametersValidator : AbstractValidator<ExpenseQueryParameters>
{
    public ExpenseQueryParametersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200);

        RuleFor(x => x)
            .Must(p => !p.StartDate.HasValue || !p.EndDate.HasValue || p.StartDate <= p.EndDate)
            .WithMessage("Start date must be earlier than or equal to end date.");
    }
}

