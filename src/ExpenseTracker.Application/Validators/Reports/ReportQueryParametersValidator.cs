using ExpenseTracker.Application.Contracts.Reports;
using FluentValidation;

namespace ExpenseTracker.Application.Validators.Reports;

public class ReportQueryParametersValidator : AbstractValidator<ReportQueryParameters>
{
    private static readonly string[] AllowedGroupByValues = ["category", "month", "year"];

    public ReportQueryParametersValidator()
    {
        RuleFor(x => x.StartDate)
            .Must((request, startDate) => !startDate.HasValue || !request.EndDate.HasValue || startDate <= request.EndDate)
            .WithMessage("Start date must be earlier than or equal to end date.");

        RuleFor(x => x.GroupBy)
            .NotEmpty()
            .Must(value => AllowedGroupByValues.Contains(value.ToLowerInvariant()))
            .WithMessage($"GroupBy must be one of: {string.Join(", ", AllowedGroupByValues)}.");
    }
}

