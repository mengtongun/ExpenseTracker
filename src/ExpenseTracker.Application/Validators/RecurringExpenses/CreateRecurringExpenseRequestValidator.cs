using ExpenseTracker.Application.Contracts.RecurringExpenses;
using FluentValidation;

namespace ExpenseTracker.Application.Validators.RecurringExpenses;

public class CreateRecurringExpenseRequestValidator : AbstractValidator<CreateRecurringExpenseRequest>
{
    public CreateRecurringExpenseRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(x => x.Description)
            .MaximumLength(512)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.StartDate)
            .Must(d => d != default)
            .WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .Must((request, endDate) => !endDate.HasValue || endDate.Value >= request.StartDate)
            .WithMessage("End date must be greater than or equal to start date.");

        RuleFor(x => x.Frequency)
            .IsInEnum();
    }
}

