using ExpenseTracker.Application.Contracts.Expenses;
using FluentValidation;

namespace ExpenseTracker.Application.Validators.Expenses;

public class CreateExpenseRequestValidator : AbstractValidator<CreateExpenseRequest>
{
    public CreateExpenseRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(x => x.Description)
            .MaximumLength(512)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.ExpenseDate)
            .Must(d => d != default)
            .WithMessage("Expense date is required.");
    }
}

