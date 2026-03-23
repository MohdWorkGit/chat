using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Api.Controllers.V1;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;

namespace CustomerEngagement.Tests.Controllers;

public class InboxesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly InboxesController _controller;

    public InboxesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new InboxesController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithInboxes()
    {
        var expectedResult = new List<InboxDto>().AsReadOnly();

        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<IReadOnlyList<InboxDto>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _controller.GetAll(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetById_ReturnsOkWithInbox()
    {
        var inbox = new InboxDto(1, 1, "Email Inbox", "email", true, null, null, DateTime.UtcNow);

        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<InboxDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inbox);

        var result = await _controller.GetById(1, 1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(inbox);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42L);

        var command = new Application.Inboxes.Commands.CreateInboxCommand(0, "New Inbox", "web_widget");

        var result = await _controller.Create(1, command);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(InboxesController.GetById));
    }

    [Fact]
    public async Task Update_ReturnsNoContent()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Unit>(Unit.Value));

        var command = new Application.Inboxes.Commands.UpdateInboxCommand(0, 0, "Updated Inbox", true);

        var result = await _controller.Update(1, 1, command);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Unit>(Unit.Value));

        var result = await _controller.Delete(1, 1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetMembers_ReturnsOkWithMembers()
    {
        var members = new List<UserDto>
        {
            new(1, "Agent One", "agent1@example.com", "Agent"),
            new(2, "Agent Two", "agent2@example.com", "Agent")
        }.AsReadOnly();

        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<IReadOnlyList<UserDto>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(members);

        var result = await _controller.GetMembers(1, 1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(members);
    }

    [Fact]
    public async Task AddMember_ReturnsOk()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Unit>(Unit.Value));

        var command = new Application.Inboxes.Commands.AddInboxMemberCommand(0, 0, 5);

        var result = await _controller.AddMember(1, 1, command);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task RemoveMember_ReturnsNoContent()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Unit>(Unit.Value));

        var result = await _controller.RemoveMember(1, 1, 5);

        result.Should().BeOfType<NoContentResult>();
    }
}
