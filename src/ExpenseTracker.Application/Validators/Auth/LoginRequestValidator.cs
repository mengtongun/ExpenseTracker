using ExpenseTracker.Application.Contracts.Auth;
using FluentValidation;

namespace ExpenseTracker.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}

