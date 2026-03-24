using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Labels.Commands;
using CustomerEngagement.Application.Labels.Queries;
using CustomerEngagement.Api.Controllers.V1;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using Xunit;

namespace CustomerEngagement.Tests.Controllers;

public class LabelsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly LabelsController _controller;

    public LabelsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new LabelsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithLabels()
    {
        var expectedResult = new List<LabelDto>
        {
            new(1, "Bug", "Bug reports", "#ff0000", true),
            new(2, "Feature", "Feature requests", "#00ff00", true)
        }.AsReadOnly();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetLabelsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _controller.GetAll(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetById_ReturnsOkWithLabel()
    {
        var label = new LabelDto(1, "Bug", "Bug reports", "#ff0000", true);

        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<LabelDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(label);

        var result = await _controller.GetById(1, 1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(label);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var label = new LabelDto(42, "New Label", "Description", "#0000ff", true);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateLabelCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(label);

        var command = new CreateLabelCommand(0, "New Label", "Description", "#0000ff", true);

        var result = await _controller.Create(1, command);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(LabelsController.GetById));
    }

    [Fact]
    public async Task Update_ReturnsNoContent()
    {
        var label = new LabelDto(1, "Updated Label", "Updated desc", "#ff00ff", false);

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateLabelCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(label);

        var command = new UpdateLabelCommand(0, 0, "Updated Label", "Updated desc", "#ff00ff", false);

        var result = await _controller.Update(1, 1, command);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteLabelCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Unit>(Unit.Value));

        var result = await _controller.Delete(1, 1);

        result.Should().BeOfType<NoContentResult>();
    }
}
