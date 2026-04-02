using CustomerEngagement.Application.EmailTemplates.Commands;
using MediatR;

namespace CustomerEngagement.Application.EmailTemplates.Queries;

public record GetEmailTemplatesQuery(int AccountId) : IRequest<IEnumerable<EmailTemplateDto>>;

public class GetEmailTemplatesQueryHandler : IRequestHandler<GetEmailTemplatesQuery, IEnumerable<EmailTemplateDto>>
{
    private readonly Core.Interfaces.IRepository<Core.Entities.EmailTemplate> _repository;

    public GetEmailTemplatesQueryHandler(Core.Interfaces.IRepository<Core.Entities.EmailTemplate> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<EmailTemplateDto>> Handle(GetEmailTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = await _repository.GetAllAsync(cancellationToken);
        return templates
            .Where(t => t.AccountId == request.AccountId)
            .Select(t => new EmailTemplateDto(t.Id, t.AccountId, t.Name, t.Body, t.TemplateType, t.Locale, t.UpdatedAt))
            .ToList();
    }
}

public record GetEmailTemplateByIdQuery(int Id, int AccountId) : IRequest<EmailTemplateDto?>;

public class GetEmailTemplateByIdQueryHandler : IRequestHandler<GetEmailTemplateByIdQuery, EmailTemplateDto?>
{
    private readonly Core.Interfaces.IRepository<Core.Entities.EmailTemplate> _repository;

    public GetEmailTemplateByIdQueryHandler(Core.Interfaces.IRepository<Core.Entities.EmailTemplate> repository)
    {
        _repository = repository;
    }

    public async Task<EmailTemplateDto?> Handle(GetEmailTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (template is null || template.AccountId != request.AccountId)
            return null;

        return new EmailTemplateDto(template.Id, template.AccountId, template.Name,
            template.Body, template.TemplateType, template.Locale, template.UpdatedAt);
    }
}

public record RenderEmailTemplateQuery(int Id, int AccountId, Dictionary<string, object> Variables) : IRequest<string>;

public class RenderEmailTemplateQueryHandler : IRequestHandler<RenderEmailTemplateQuery, string>
{
    private readonly Core.Interfaces.IRepository<Core.Entities.EmailTemplate> _repository;

    public RenderEmailTemplateQueryHandler(Core.Interfaces.IRepository<Core.Entities.EmailTemplate> repository)
    {
        _repository = repository;
    }

    public async Task<string> Handle(RenderEmailTemplateQuery request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Email template with ID {request.Id} not found.");

        if (string.IsNullOrEmpty(template.Body))
            return string.Empty;

        // Simple variable substitution using Fluid/Liquid-style {{ variable }}
        var rendered = template.Body;
        foreach (var (key, value) in request.Variables)
        {
            rendered = rendered.Replace($"{{{{{key}}}}}", value?.ToString() ?? "");
            rendered = rendered.Replace($"{{{{ {key} }}}}", value?.ToString() ?? "");
        }

        return rendered;
    }
}
