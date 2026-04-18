using CustomerEngagement.Application.Articles.Commands;
using FluentValidation;

namespace CustomerEngagement.Application.Articles.Validators;

public class CreateArticleCommandValidator : AbstractValidator<CreateArticleCommand>
{
    public CreateArticleCommandValidator()
    {
        RuleFor(x => x.PortalId).GreaterThan(0);

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Article title is required.")
            .MaximumLength(500);

        RuleFor(x => x.Slug)
            .MaximumLength(500)
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Slug may contain only lowercase letters, digits, and hyphens.")
            .When(x => !string.IsNullOrEmpty(x.Slug));

        RuleFor(x => x.Description).MaximumLength(1000);

        RuleFor(x => x.Status)
            .InclusiveBetween(0, 2)
            .WithMessage("Status must be 0 (Draft), 1 (Published), or 2 (Archived).");
    }
}

public class UpdateArticleCommandValidator : AbstractValidator<UpdateArticleCommand>
{
    public UpdateArticleCommandValidator()
    {
        RuleFor(x => x.PortalId).GreaterThan(0);
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Title)
            .MaximumLength(500)
            .When(x => x.Title is not null);

        RuleFor(x => x.Description).MaximumLength(1000);

        RuleFor(x => x.Status)
            .InclusiveBetween(0, 2)
            .When(x => x.Status.HasValue);
    }
}
