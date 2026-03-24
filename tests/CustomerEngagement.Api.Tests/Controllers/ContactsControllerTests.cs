using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Contacts.Commands;
using CustomerEngagement.Application.Contacts.Queries;
using CustomerEngagement.Api.Controllers.V1;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using Xunit;

namespace CustomerEngagement.Api.Tests.Controllers;

public class ContactsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ContactsController _controller;

    public ContactsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ContactsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        var expectedResult = new PaginatedResultDto<ContactDto>
        {
            Items = new List<ContactDto>().AsReadOnly(),
            TotalCount = 0,
            Page = 1,
            PageSize = 25
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetContactsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _controller.GetAll(1);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        var contact = new ContactDto(
            1, 1, "John Doe", "john@example.com",
            "+1234567890", null, "Visitor",
            "Acme", "New York", null,
            DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow,
            null, 0);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetContactByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);

        var result = await _controller.GetById(1, 1);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(contact);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtActionResult()
    {
        var command = new CreateContactCommand(0, "John Doe", "john@example.com", "+1234567890", "Visitor", "Acme", null);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateContactCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42L);

        var result = await _controller.Create(1, command);

        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)result;
        createdResult.ActionName.Should().Be(nameof(ContactsController.GetById));
    }

    [Fact]
    public async Task Update_ReturnsNoContent()
    {
        var command = new UpdateContactCommand(0, 0, "Jane Doe", "jane@example.com", "+9876543210");

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateContactCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Unit>(Unit.Value));

        var result = await _controller.Update(1, 1, command);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteContactCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Unit>(Unit.Value));

        var result = await _controller.Delete(1, 1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Search_ReturnsOkResult()
    {
        var expectedResult = new Application.Services.Contacts.PaginatedContactSearchResult
        {
            Items = [],
            TotalCount = 0,
            Page = 1,
            PageSize = 25
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SearchContactsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _controller.Search(1, "test");

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetAll_WithPagination_PassesCorrectParameters()
    {
        var expectedResult = new PaginatedResultDto<ContactDto>
        {
            Items = new List<ContactDto>().AsReadOnly(),
            TotalCount = 50,
            Page = 2,
            PageSize = 10
        };

        _mediatorMock.Setup(m => m.Send(
                It.Is<GetContactsQuery>(q => q.AccountId == 5 && q.Page == 2 && q.PageSize == 10),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _controller.GetAll(5, 2, 10);

        result.Should().BeOfType<OkObjectResult>();
    }
}
