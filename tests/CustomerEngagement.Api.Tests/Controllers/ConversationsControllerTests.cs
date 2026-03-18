using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Api.Controllers.V1;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;

namespace CustomerEngagement.Api.Tests.Controllers;

public class ConversationsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ConversationsController _controller;

    public ConversationsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ConversationsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        var expectedResult = new ConversationListDto(
            new List<ConversationDto>().AsReadOnly(),
            new MetaDto(0, 1, 25, 0));

        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<ConversationListDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _controller.GetAll(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        var conversation = new ConversationDto(
            1, 1, 1, 1, null, null, 100,
            "Open", "None", null, null, false,
            DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow,
            null, null, null, 0, []);

        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<ConversationDto?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var result = await _controller.GetById(1, 1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Resolve_ReturnsOk()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Unit>(Unit.Value));

        var result = await _controller.Resolve(1, 1);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Reopen_ReturnsOk()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Unit>(Unit.Value));

        var result = await _controller.Reopen(1, 1);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Mute_ReturnsOk()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Unit>(Unit.Value));

        var result = await _controller.Mute(1, 1);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task TogglePriority_ReturnsOk()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Unit>(Unit.Value));

        var result = await _controller.TogglePriority(1, 1);

        result.Should().BeOfType<OkResult>();
    }
}
