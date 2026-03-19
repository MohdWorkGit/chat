using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Categories.Queries;

public record CategoryDto(
    int Id,
    string Name,
    string Slug,
    string? Description,
    string? Locale,
    int Position,
    int? ParentCategoryId);

public record GetCategoriesQuery(long PortalId) : IRequest<IReadOnlyList<CategoryDto>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly IRepository<Category> _categoryRepository;

    public GetCategoriesQueryHandler(IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.FindAsync(
            c => c.PortalId == (int)request.PortalId,
            cancellationToken);

        return categories
            .OrderBy(c => c.Position)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Slug, c.Description, c.Locale, c.Position, c.ParentCategoryId))
            .ToList()
            .AsReadOnly();
    }
}

public record GetCategoryByIdQuery(long PortalId, long CategoryId) : IRequest<CategoryDto?>;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly IRepository<Category> _categoryRepository;

    public GetCategoryByIdQueryHandler(IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.FindOneAsync(
            c => c.Id == (int)request.CategoryId && c.PortalId == (int)request.PortalId,
            cancellationToken);

        if (category is null) return null;

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.Locale,
            category.Position,
            category.ParentCategoryId);
    }
}
