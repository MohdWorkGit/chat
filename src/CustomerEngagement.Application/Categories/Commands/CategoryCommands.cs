using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Categories.Commands;

public record CreateCategoryCommand(
    long PortalId,
    string Name,
    string? Description,
    string? Locale,
    int? ParentCategoryId,
    int Position = 0) : IRequest<long>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, long>
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Portal> _portalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        IRepository<Category> categoryRepository,
        IRepository<Portal> portalRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _portalRepository = portalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<long> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var portal = await _portalRepository.GetByIdAsync((int)request.PortalId, cancellationToken)
            ?? throw new KeyNotFoundException($"Portal {request.PortalId} not found.");

        var slug = request.Name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');

        var portalIdInt = (int)request.PortalId;
        var locale = request.Locale;
        if (await _categoryRepository.AnyAsync(
                c => c.PortalId == portalIdInt && c.Slug == slug && c.Locale == locale,
                cancellationToken))
        {
            throw new InvalidOperationException(
                $"A category with slug '{slug}' already exists in this portal for locale '{locale ?? "default"}'.");
        }

        var category = new Category
        {
            AccountId = portal.AccountId,
            PortalId = (int)request.PortalId,
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            Locale = request.Locale,
            ParentCategoryId = request.ParentCategoryId,
            Position = request.Position,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}

public record UpdateCategoryCommand(
    long PortalId,
    long Id,
    string? Name,
    string? Description,
    string? Locale,
    int? ParentCategoryId,
    int? Position) : IRequest;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(IRepository<Category> categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.FindOneAsync(
            c => c.Id == (int)request.Id && c.PortalId == (int)request.PortalId,
            cancellationToken)
            ?? throw new KeyNotFoundException($"Category {request.Id} not found.");

        if (request.Name is not null)
        {
            category.Name = request.Name;
            var newSlug = request.Name.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("--", "-")
                .Trim('-');
            if (newSlug != category.Slug)
            {
                var portalIdInt = category.PortalId;
                var locale = category.Locale;
                var categoryId = category.Id;
                if (await _categoryRepository.AnyAsync(
                        c => c.PortalId == portalIdInt
                             && c.Slug == newSlug
                             && c.Locale == locale
                             && c.Id != categoryId,
                        cancellationToken))
                {
                    throw new InvalidOperationException(
                        $"A category with slug '{newSlug}' already exists in this portal for locale '{locale ?? "default"}'.");
                }
                category.Slug = newSlug;
            }
        }
        if (request.Description is not null)
            category.Description = request.Description;
        if (request.Locale is not null)
            category.Locale = request.Locale;
        if (request.ParentCategoryId.HasValue)
            category.ParentCategoryId = request.ParentCategoryId;
        if (request.Position.HasValue)
            category.Position = request.Position.Value;

        category.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public record DeleteCategoryCommand(long PortalId, long Id) : IRequest;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(IRepository<Category> categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.FindOneAsync(
            c => c.Id == (int)request.Id && c.PortalId == (int)request.PortalId,
            cancellationToken)
            ?? throw new KeyNotFoundException($"Category {request.Id} not found.");

        _categoryRepository.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
