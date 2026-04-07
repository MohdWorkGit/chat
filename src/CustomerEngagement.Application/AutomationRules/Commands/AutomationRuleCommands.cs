using System.Text.Json;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.AutomationRules.Commands;

public record CreateAutomationRuleCommand(
    long AccountId = 0,
    string Name = "",
    string? Description = null,
    string? EventName = null,
    List<AutomationRule.AutomationCondition>? Conditions = null,
    List<AutomationRule.AutomationAction>? Actions = null,
    bool Active = true) : IRequest<object>;

public record UpdateAutomationRuleCommand(
    long AccountId = 0,
    long Id = 0,
    string? Name = null,
    string? Description = null,
    string? EventName = null,
    List<AutomationRule.AutomationCondition>? Conditions = null,
    List<AutomationRule.AutomationAction>? Actions = null,
    bool? Active = null) : IRequest<object>;

public record DeleteAutomationRuleCommand(long AccountId, long Id) : IRequest<object>;

public class CreateAutomationRuleCommandHandler : IRequestHandler<CreateAutomationRuleCommand, object>
{
    private readonly IRepository<AutomationRule> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAutomationRuleCommandHandler(IRepository<AutomationRule> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(CreateAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.", nameof(request.Name));
        if (string.IsNullOrWhiteSpace(request.EventName))
            throw new ArgumentException("EventName is required.", nameof(request.EventName));

        var rule = new AutomationRule
        {
            AccountId = (int)request.AccountId,
            Name = request.Name,
            Description = request.Description,
            EventName = request.EventName,
            Conditions = request.Conditions is null ? null : JsonSerializer.Serialize(request.Conditions),
            Actions = request.Actions is null ? null : JsonSerializer.Serialize(request.Actions),
            Active = request.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (long)rule.Id;
    }
}

public class UpdateAutomationRuleCommandHandler : IRequestHandler<UpdateAutomationRuleCommand, object>
{
    private readonly IRepository<AutomationRule> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAutomationRuleCommandHandler(IRepository<AutomationRule> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(UpdateAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (rule is null || rule.AccountId != (int)request.AccountId)
            throw new InvalidOperationException($"Automation rule {request.Id} not found.");

        if (request.Name is not null) rule.Name = request.Name;
        if (request.Description is not null) rule.Description = request.Description;
        if (request.EventName is not null) rule.EventName = request.EventName;
        if (request.Conditions is not null) rule.Conditions = JsonSerializer.Serialize(request.Conditions);
        if (request.Actions is not null) rule.Actions = JsonSerializer.Serialize(request.Actions);
        if (request.Active.HasValue) rule.Active = request.Active.Value;
        rule.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (long)rule.Id;
    }
}

public class DeleteAutomationRuleCommandHandler : IRequestHandler<DeleteAutomationRuleCommand, object>
{
    private readonly IRepository<AutomationRule> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAutomationRuleCommandHandler(IRepository<AutomationRule> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(DeleteAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (rule is null || rule.AccountId != (int)request.AccountId)
            throw new InvalidOperationException($"Automation rule {request.Id} not found.");

        await _repository.DeleteAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (long)request.Id;
    }
}
