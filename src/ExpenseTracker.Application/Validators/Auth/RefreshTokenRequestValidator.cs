using ExpenseTracker.Application.Contracts.Auth;
using FluentValidation;

namespace ExpenseTracker.Application.Validators.Auth;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}

