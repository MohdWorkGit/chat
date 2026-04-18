using CustomerEngagement.Application.Portals.Commands;
using FluentValidation;

namespace CustomerEngagement.Application.Portals.Validators;

public class CreatePortalCommandValidator : AbstractValidator<CreatePortalCommand>
{
    public CreatePortalCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Portal name is required.")
            .MaximumLength(255);

        RuleFor(x => x.Slug)
            .MaximumLength(255)
            .Matches("^[a-z0-9-]*$")
            .WithMessage("Slug may contain only lowercase letters, digits, and hyphens.")
            .When(x => !string.IsNullOrEmpty(x.Slug));

        RuleFor(x => x.CustomDomain).MaximumLength(255);
        RuleFor(x => x.Color).MaximumLength(50);
        RuleFor(x => x.HeaderText).MaximumLength(500);
        RuleFor(x => x.PageTitle).MaximumLength(255);
        RuleFor(x => x.HomepageLink).MaximumLength(2048);
    }
}

public class UpdatePortalCommandValidator : AbstractValidator<UpdatePortalCommand>
{
    public UpdatePortalCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Name)
            .MaximumLength(255)
            .When(x => x.Name is not null);

        RuleFor(x => x.Slug)
            .MaximumLength(255)
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Slug may contain only lowercase letters, digits, and hyphens.")
            .When(x => !string.IsNullOrEmpty(x.Slug));

        RuleFor(x => x.CustomDomain).MaximumLength(255);
        RuleFor(x => x.Color).MaximumLength(50);
        RuleFor(x => x.HeaderText).MaximumLength(500);
        RuleFor(x => x.PageTitle).MaximumLength(255);
        RuleFor(x => x.HomepageLink).MaximumLength(2048);
    }
}
