using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.HelpCenter;
using CustomerEngagement.Api.Controllers.V1;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;

namespace CustomerEngagement.Api.Tests.Controllers;

public class ArticlesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ArticlesController _controller;

    public ArticlesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ArticlesController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithPaginatedResult()
    {
        var expectedResult = new PaginatedResultDto<ArticleDto>
        {
            Items = [],
            TotalCount = 0,
            Page = 1,
            PageSize = 25
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<PaginatedResultDto<ArticleDto>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _controller.GetAll(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetById_ReturnsOkWithArticle()
    {
        var article = new ArticleDto
        {
            Id = 1,
            PortalId = 1,
            AccountId = 10,
            Title = "Test Article",
            Slug = "test-article",
            Content = "Content",
            Status = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<ArticleDto?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(article);

        var result = await _controller.GetById(1, 1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(article);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42L);

        var command = new Application.Articles.Commands.CreateArticleCommand(
            PortalId: 1, Title: "New Article", Slug: null, Content: "Content",
            Description: "Desc", Status: 0, CategoryId: null, AuthorId: 1);

        var result = await _controller.Create(1, command);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var result = await _controller.Delete(1, 1);

        result.Should().BeOfType<NoContentResult>();
    }
}
