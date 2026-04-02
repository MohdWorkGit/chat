using MediatR;

namespace CustomerEngagement.Application.EmailTemplates.Commands;

public record EmailTemplateDto(int Id, int AccountId, string Name, string? Body, string? TemplateType, string? Locale, DateTime UpdatedAt);

public record CreateEmailTemplateCommand(
    int AccountId,
    string Name,
    string? Body,
    string? TemplateType,
    string? Locale) : IRequest<EmailTemplateDto>;

public class CreateEmailTemplateCommandHandler : IRequestHandler<CreateEmailTemplateCommand, EmailTemplateDto>
{
    private readonly Core.Interfaces.IRepository<Core.Entities.EmailTemplate> _repository;
    private readonly Core.Interfaces.IUnitOfWork _unitOfWork;

    public CreateEmailTemplateCommandHandler(
        Core.Interfaces.IRepository<Core.Entities.EmailTemplate> repository,
        Core.Interfaces.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmailTemplateDto> Handle(CreateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = new Core.Entities.EmailTemplate
        {
            AccountId = request.AccountId,
            Name = request.Name,
            Body = request.Body,
            TemplateType = request.TemplateType,
            Locale = request.Locale
        };

        await _repository.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EmailTemplateDto(template.Id, template.AccountId, template.Name,
            template.Body, template.TemplateType, template.Locale, template.UpdatedAt);
    }
}

public record UpdateEmailTemplateCommand(
    int Id,
    int AccountId,
    string Name,
    string? Body,
    string? TemplateType,
    string? Locale) : IRequest<EmailTemplateDto>;

public class UpdateEmailTemplateCommandHandler : IRequestHandler<UpdateEmailTemplateCommand, EmailTemplateDto>
{
    private readonly Core.Interfaces.IRepository<Core.Entities.EmailTemplate> _repository;
    private readonly Core.Interfaces.IUnitOfWork _unitOfWork;

    public UpdateEmailTemplateCommandHandler(
        Core.Interfaces.IRepository<Core.Entities.EmailTemplate> repository,
        Core.Interfaces.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmailTemplateDto> Handle(UpdateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Email template with ID {request.Id} not found.");

        template.Name = request.Name;
        template.Body = request.Body;
        template.TemplateType = request.TemplateType;
        template.Locale = request.Locale;
        template.UpdatedAt = DateTime.UtcNow;

        _repository.Update(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EmailTemplateDto(template.Id, template.AccountId, template.Name,
            template.Body, template.TemplateType, template.Locale, template.UpdatedAt);
    }
}

public record DeleteEmailTemplateCommand(int Id, int AccountId) : IRequest;

public class DeleteEmailTemplateCommandHandler : IRequestHandler<DeleteEmailTemplateCommand>
{
    private readonly Core.Interfaces.IRepository<Core.Entities.EmailTemplate> _repository;
    private readonly Core.Interfaces.IUnitOfWork _unitOfWork;

    public DeleteEmailTemplateCommandHandler(
        Core.Interfaces.IRepository<Core.Entities.EmailTemplate> repository,
        Core.Interfaces.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Email template with ID {request.Id} not found.");

        _repository.Remove(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
