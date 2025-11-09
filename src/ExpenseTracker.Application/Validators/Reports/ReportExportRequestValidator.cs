using ExpenseTracker.Application.Contracts.Reports;
using FluentValidation;

namespace ExpenseTracker.Application.Validators.Reports;

public class ReportExportRequestValidator : AbstractValidator<ReportExportRequest>
{
    public ReportExportRequestValidator()
    {
        Include(new ReportQueryParametersValidator());

        RuleFor(x => x.Format)
            .IsInEnum();
    }
}

