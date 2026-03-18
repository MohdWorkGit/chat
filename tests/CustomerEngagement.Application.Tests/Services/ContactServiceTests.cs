using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Contacts;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Moq;
using FluentAssertions;

namespace CustomerEngagement.Application.Tests.Services;

public class ContactServiceTests
{
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IContactMergeService> _contactMergeServiceMock;
    private readonly ContactService _sut;

    public ContactServiceTests()
    {
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mediatorMock = new Mock<IMediator>();
        _contactMergeServiceMock = new Mock<IContactMergeService>();
        _sut = new ContactService(
            _contactRepoMock.Object,
            _unitOfWorkMock.Object,
            _mediatorMock.Object,
            _contactMergeServiceMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenContactExists_ReturnsDto()
    {
        var contact = CreateTestContact(1);
        _contactRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);

        var result = await _sut.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.AccountId.Should().Be(contact.AccountId);
        result.Name.Should().Be(contact.Name);
        result.Email.Should().Be(contact.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WhenContactNotFound_ReturnsNull()
    {
        _contactRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_CreatesContactAndPublishesEvent()
    {
        var request = new CreateContactRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Phone = "+1234567890",
            Company = "Acme Inc",
            Location = "New York"
        };

        _contactRepoMock.Setup(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact c, CancellationToken _) => c);

        var result = await _sut.CreateAsync(10, request);

        result.Should().NotBeNull();
        result.Name.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
        result.Phone.Should().Be("+1234567890");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<ContactCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenContactExists_UpdatesAndReturnsDto()
    {
        var contact = CreateTestContact(1);
        _contactRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);

        var request = new UpdateContactRequest
        {
            Name = "Jane Doe",
            Email = "jane@example.com"
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Jane Doe");
        result.Email.Should().Be("jane@example.com");
        _contactRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenContactNotFound_Throws()
    {
        _contactRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var request = new UpdateContactRequest { Name = "Test" };

        var act = () => _sut.UpdateAsync(999, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task DeleteAsync_WhenContactExists_DeletesContact()
    {
        var contact = CreateTestContact(1);
        _contactRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);

        await _sut.DeleteAsync(1);

        _contactRepoMock.Verify(r => r.DeleteAsync(contact, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenContactNotFound_Throws()
    {
        _contactRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task SearchAsync_ReturnsMatchingContacts()
    {
        var contacts = new List<Contact>
        {
            CreateTestContact(1),
            CreateTestContact(2),
            CreateTestContact(3)
        };

        _contactRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contacts.AsReadOnly());

        var result = await _sut.SearchAsync(10, "test", 1, 25);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(25);
    }

    [Fact]
    public async Task SearchAsync_ReturnsPaginatedResults()
    {
        var contacts = new List<Contact>
        {
            CreateTestContact(1),
            CreateTestContact(2),
            CreateTestContact(3)
        };

        _contactRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contacts.AsReadOnly());

        var result = await _sut.SearchAsync(10, "test", 1, 2);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
    }

    private static Contact CreateTestContact(int id) => new()
    {
        Id = id,
        AccountId = 10,
        Name = $"Contact {id}",
        Email = $"contact{id}@example.com",
        Phone = $"+100000000{id}",
        ContactType = ContactType.Visitor,
        CompanyName = "Test Company",
        Location = "Test City",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
