using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Teams.Commands;
using CustomerEngagement.Application.Teams.Queries;
using CustomerEngagement.Api.Controllers.V1;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;

namespace CustomerEngagement.Tests.Controllers;

public class TeamsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly TeamsController _controller;

    public TeamsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new TeamsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithTeams()
    {
        var expectedResult = new List<TeamDto>
        {
            new(1, "Support", "Support team", true, 3),
            new(2, "Sales", "Sales team", false, 5)
        }.AsReadOnly();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetTeamsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _controller.GetAll(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetById_ReturnsOkWithTeam()
    {
        var team = new TeamDto(1, "Support", "Support team", true, 3);

        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<TeamDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var result = await _controller.GetById(1, 1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(team);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var team = new TeamDto(42, "New Team", "Description", true, 0);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateTeamCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var command = new CreateTeamCommand(0, "New Team", "Description", true);

        var result = await _controller.Create(1, command);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(TeamsController.GetById));
    }

    [Fact]
    public async Task Update_ReturnsNoContent()
    {
        var team = new TeamDto(1, "Updated Team", "Updated desc", false, 2);

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateTeamCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var command = new UpdateTeamCommand(0, 0, "Updated Team", "Updated desc", false);

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
            new(1, "Agent One", "Agent One", "agent1@example.com", "Online", null, "Agent", DateTime.UtcNow),
            new(2, "Agent Two", "Agent Two", "agent2@example.com", "Online", null, "Agent", DateTime.UtcNow)
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

        var command = new AddTeamMemberCommand(0, 0, 5);

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
