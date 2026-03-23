using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Contacts;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Moq;
using FluentAssertions;

namespace CustomerEngagement.Tests.Services;

public class ContactMergeServiceTests
{
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IRepository<Conversation>> _conversationRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ContactMergeService _sut;

    public ContactMergeServiceTests()
    {
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _conversationRepoMock = new Mock<IRepository<Conversation>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mediatorMock = new Mock<IMediator>();
        _sut = new ContactMergeService(
            _contactRepoMock.Object,
            _conversationRepoMock.Object,
            _unitOfWorkMock.Object,
            _mediatorMock.Object);
    }

    [Fact]
    public async Task MergeContacts_CombinesConversations()
    {
        var baseContact = CreateTestContact(1, "Base Contact", "base@example.com");
        var mergeContact = CreateTestContact(2, "Merge Contact", "merge@example.com");

        var conversations = new List<Conversation>
        {
            new() { Id = 10, ContactId = 2, AccountId = 1, InboxId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 11, ContactId = 2, AccountId = 1, InboxId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        }.AsReadOnly();

        _contactRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(baseContact);
        _contactRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(mergeContact);
        _conversationRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversations);

        await _sut.MergeContactsAsync(1, 2);

        // Conversations should be reassigned to base contact
        conversations[0].ContactId.Should().Be(1);
        conversations[1].ContactId.Should().Be(1);
        _conversationRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _contactRepoMock.Verify(r => r.DeleteAsync(mergeContact, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MergeContacts_CombinesAttributes()
    {
        var baseContact = CreateTestContact(1, "Base Contact", null);
        baseContact.Phone = null;
        baseContact.CompanyName = null;
        baseContact.Location = null;

        var mergeContact = CreateTestContact(2, "Merge Contact", "merge@example.com");
        mergeContact.Phone = "+1234567890";
        mergeContact.CompanyName = "Acme Corp";
        mergeContact.Location = "New York";

        _contactRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(baseContact);
        _contactRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(mergeContact);
        _conversationRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Conversation>().AsReadOnly());

        var result = await _sut.MergeContactsAsync(1, 2);

        // Missing fields on base should be filled from merge contact
        result.Email.Should().Be("merge@example.com");
        result.Phone.Should().Be("+1234567890");
        baseContact.CompanyName.Should().Be("Acme Corp");
        baseContact.Location.Should().Be("New York");
    }

    [Fact]
    public async Task MergeContacts_WithInvalidBaseId_Throws()
    {
        _contactRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var act = () => _sut.MergeContactsAsync(999, 2);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task MergeContacts_WithInvalidMergeId_Throws()
    {
        var baseContact = CreateTestContact(1, "Base Contact", "base@example.com");
        _contactRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(baseContact);
        _contactRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var act = () => _sut.MergeContactsAsync(1, 999);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task MergeContacts_KeepsPrimaryContactData()
    {
        var baseContact = CreateTestContact(1, "Base Name", "base@example.com");
        baseContact.Phone = "+1111111111";
        baseContact.CompanyName = "Base Corp";

        var mergeContact = CreateTestContact(2, "Merge Name", "merge@example.com");
        mergeContact.Phone = "+2222222222";
        mergeContact.CompanyName = "Merge Corp";

        _contactRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(baseContact);
        _contactRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(mergeContact);
        _conversationRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Conversation>().AsReadOnly());

        var result = await _sut.MergeContactsAsync(1, 2);

        // Primary contact data should be preserved (not overwritten by merge contact)
        result.Name.Should().Be("Base Name");
        result.Email.Should().Be("base@example.com");
        result.Phone.Should().Be("+1111111111");
        baseContact.CompanyName.Should().Be("Base Corp");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<ContactMergedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Contact CreateTestContact(int id, string? name, string? email) => new()
    {
        Id = id,
        AccountId = 1,
        Name = name,
        Email = email,
        ContactType = ContactType.Visitor,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
