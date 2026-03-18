using CustomerEngagement.Application.Services.HelpCenter;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using Moq;
using FluentAssertions;
using System.Linq.Expressions;

namespace CustomerEngagement.Application.Tests.Services;

public class ArticleServiceTests
{
    private readonly Mock<IRepository<Article>> _articleRepoMock;
    private readonly Mock<IRepository<Portal>> _portalRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ArticleService _sut;

    public ArticleServiceTests()
    {
        _articleRepoMock = new Mock<IRepository<Article>>();
        _portalRepoMock = new Mock<IRepository<Portal>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new ArticleService(
            _articleRepoMock.Object,
            _portalRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenArticleExists_ReturnsDto()
    {
        var article = CreateTestArticle(1);
        _articleRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(article);

        var result = await _sut.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Title.Should().Be("Test Article");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _articleRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Article?)null);

        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_FindsArticleByPortalAndSlug()
    {
        var article = CreateTestArticle(1);
        _articleRepoMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Article, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(article);

        var result = await _sut.GetBySlugAsync(1, "test-article");

        result.Should().NotBeNull();
        result!.Slug.Should().Be("test-article");
    }

    [Fact]
    public async Task CreateAsync_CreatesArticleWithGeneratedSlug()
    {
        var portal = new Portal
        {
            Id = 1,
            AccountId = 10,
            Name = "Test Portal",
            Slug = "test-portal",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _portalRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portal);
        _articleRepoMock.Setup(r => r.AddAsync(It.IsAny<Article>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Article a, CancellationToken _) => a);

        var request = new CreateArticleRequest
        {
            Title = "My New Article",
            Content = "<p>Content here</p>",
            Status = (int)ArticleStatus.Draft,
            AuthorId = 5
        };

        var result = await _sut.CreateAsync(1, request);

        result.Should().NotBeNull();
        result.Title.Should().Be("My New Article");
        result.Slug.Should().Be("my-new-article");
        result.AccountId.Should().Be(10);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenPortalNotFound_Throws()
    {
        _portalRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Portal?)null);

        var request = new CreateArticleRequest { Title = "Test", AuthorId = 1 };

        var act = () => _sut.CreateAsync(999, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesArticleFields()
    {
        var article = CreateTestArticle(1);
        _articleRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(article);

        var request = new UpdateArticleRequest
        {
            Title = "Updated Title",
            Status = (int)ArticleStatus.Published
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Title.Should().Be("Updated Title");
        result.Status.Should().Be((int)ArticleStatus.Published);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_DeletesArticle()
    {
        var article = CreateTestArticle(1);
        _articleRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(article);

        await _sut.DeleteAsync(1);

        _articleRepoMock.Verify(r => r.DeleteAsync(article, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_Throws()
    {
        _articleRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Article?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static Article CreateTestArticle(int id) => new()
    {
        Id = id,
        PortalId = 1,
        AccountId = 10,
        Title = "Test Article",
        Slug = "test-article",
        Content = "<p>Test content</p>",
        Description = "Test description",
        Status = ArticleStatus.Draft,
        AuthorId = 5,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
