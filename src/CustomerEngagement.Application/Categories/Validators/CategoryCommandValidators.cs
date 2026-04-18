using CustomerEngagement.Application.Categories.Commands;
using FluentValidation;

namespace CustomerEngagement.Application.Categories.Validators;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.PortalId).GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(255);

        RuleFor(x => x.Description).MaximumLength(2000);

        RuleFor(x => x.Locale)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.Locale));

        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
    }
}

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.PortalId).GreaterThan(0);
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255)
            .When(x => x.Name is not null);

        RuleFor(x => x.Description).MaximumLength(2000);

        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Position.HasValue);
    }
}
