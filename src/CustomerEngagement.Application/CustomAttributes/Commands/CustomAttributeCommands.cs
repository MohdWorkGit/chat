using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.CustomAttributes.Commands;

public record CreateCustomAttributeCommand(
    long AccountId = 0,
    string DisplayName = "",
    string? Key = null,
    string? Description = null,
    string AttributeType = "text",
    string AppliedTo = "contact",
    List<string>? ListValues = null) : IRequest<object>;

public record UpdateCustomAttributeCommand(
    long AccountId = 0,
    long Id = 0,
    string? DisplayName = null,
    string? Key = null,
    string? Description = null,
    string? AttributeType = null,
    string? AppliedTo = null,
    List<string>? ListValues = null) : IRequest<object>;

public record DeleteCustomAttributeCommand(long AccountId, long Id) : IRequest<object>;

public class CreateCustomAttributeCommandHandler : IRequestHandler<CreateCustomAttributeCommand, object>
{
    private readonly IRepository<CustomAttributeDefinition> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomAttributeCommandHandler(IRepository<CustomAttributeDefinition> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(CreateCustomAttributeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            throw new ArgumentException("Display name is required.", nameof(request.DisplayName));
        if (string.IsNullOrWhiteSpace(request.Key))
            throw new ArgumentException("Key is required.", nameof(request.Key));

        var type = CustomAttributeMapping.ValidateType(request.AttributeType);
        var appliedTo = CustomAttributeMapping.ValidateAppliedTo(request.AppliedTo);

        var existing = await _repository.FindOneAsync(
            ca => ca.AccountId == (int)request.AccountId
                  && ca.AttributeKey == request.Key
                  && ca.AttributeModel == appliedTo,
            cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException(
                $"A {appliedTo} attribute with key '{request.Key}' already exists.");

        var now = DateTime.UtcNow;
        var entity = new CustomAttributeDefinition
        {
            AccountId = (int)request.AccountId,
            AttributeDisplayName = request.DisplayName,
            AttributeKey = request.Key!,
            AttributeDisplayType = type,
            AttributeModel = appliedTo,
            AttributeDescription = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description,
            CreatedAt = now,
            UpdatedAt = now
        };

        if (CustomAttributeMapping.IsListType(type))
            entity.SetListValues(request.ListValues);

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CustomAttributeMapping.ToDto(entity);
    }
}

public class UpdateCustomAttributeCommandHandler : IRequestHandler<UpdateCustomAttributeCommand, object>
{
    private readonly IRepository<CustomAttributeDefinition> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomAttributeCommandHandler(IRepository<CustomAttributeDefinition> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(UpdateCustomAttributeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.FindOneAsync(
            ca => ca.Id == (int)request.Id && ca.AccountId == (int)request.AccountId,
            cancellationToken)
            ?? throw new KeyNotFoundException($"Custom attribute {request.Id} not found.");

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
            entity.AttributeDisplayName = request.DisplayName;

        if (!string.IsNullOrWhiteSpace(request.Key))
            entity.AttributeKey = request.Key;

        if (request.Description is not null)
            entity.AttributeDescription = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description;

        if (request.AttributeType is not null)
            entity.AttributeDisplayType = CustomAttributeMapping.ValidateType(request.AttributeType);

        if (request.AppliedTo is not null)
            entity.AttributeModel = CustomAttributeMapping.ValidateAppliedTo(request.AppliedTo);

        if (CustomAttributeMapping.IsListType(entity.AttributeDisplayType))
        {
            if (request.ListValues is not null)
                entity.SetListValues(request.ListValues);
        }
        else
        {
            entity.SetListValues(null);
        }

        // Guard against renaming into a conflicting (key, applied-to) pair.
        var conflict = await _repository.FindOneAsync(
            ca => ca.AccountId == entity.AccountId
                  && ca.Id != entity.Id
                  && ca.AttributeKey == entity.AttributeKey
                  && ca.AttributeModel == entity.AttributeModel,
            cancellationToken);
        if (conflict is not null)
            throw new InvalidOperationException(
                $"A {entity.AttributeModel} attribute with key '{entity.AttributeKey}' already exists.");

        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CustomAttributeMapping.ToDto(entity);
    }
}

public class DeleteCustomAttributeCommandHandler : IRequestHandler<DeleteCustomAttributeCommand, object>
{
    private readonly IRepository<CustomAttributeDefinition> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCustomAttributeCommandHandler(IRepository<CustomAttributeDefinition> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<object> Handle(DeleteCustomAttributeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.FindOneAsync(
            ca => ca.Id == (int)request.Id && ca.AccountId == (int)request.AccountId,
            cancellationToken)
            ?? throw new KeyNotFoundException($"Custom attribute {request.Id} not found.");

        _repository.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new { entity.Id };
    }
}
