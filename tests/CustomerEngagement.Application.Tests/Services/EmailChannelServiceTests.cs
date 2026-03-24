using CustomerEngagement.Application.Services.Channels;
using CustomerEngagement.Application.Services.Conversations;
using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CustomerEngagement.Application.Tests.Services;

public class EmailChannelServiceTests
{
    private readonly Mock<IRepository<Inbox>> _inboxRepoMock;
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IConversationService> _conversationServiceMock;
    private readonly Mock<IMessageService> _messageServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<EmailChannelService>> _loggerMock;
    private readonly EmailChannelService _sut;

    public EmailChannelServiceTests()
    {
        _inboxRepoMock = new Mock<IRepository<Inbox>>();
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _conversationServiceMock = new Mock<IConversationService>();
        _messageServiceMock = new Mock<IMessageService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<EmailChannelService>>();
        _sut = new EmailChannelService(
            _inboxRepoMock.Object,
            _contactRepoMock.Object,
            _conversationServiceMock.Object,
            _messageServiceMock.Object,
            _unitOfWorkMock.Object,
            _mediatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessInboundEmailAsync_WhenNoInboxFound_ReturnsWithoutProcessing()
    {
        _inboxRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Inbox>());

        var request = new InboundEmailRequest
        {
            From = "sender@example.com",
            To = "unknown@example.com",
            Subject = "Test",
            Body = "Hello"
        };

        await _sut.ProcessInboundEmailAsync(request);

        _conversationServiceMock.Verify(
            c => c.CreateAsync(It.IsAny<int>(), It.IsAny<CreateConversationRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessInboundEmailAsync_WhenInboxExists_CreatesConversationAndMessage()
    {
        var inbox = new Inbox { Id = 1, AccountId = 10, Name = "Email Inbox", ChannelType = "email", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _inboxRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Inbox> { inbox });

        var contact = new Contact { Id = 5, AccountId = 10, Email = "sender@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _contactRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contact> { contact });

        var conversationDto = new ConversationDto(
            100, 10, 1, 5, null, null, 1,
            "Open", "None", null, null, false,
            DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow,
            null, null, null, 0, []);
        _conversationServiceMock.Setup(c => c.CreateAsync(10, It.IsAny<CreateConversationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationDto);

        var messageDto = new MessageDto(1, 100, 10, null, null, "Test body", "text", "incoming", false, "sent", DateTime.UtcNow, DateTime.UtcNow, []);
        _messageServiceMock.Setup(m => m.CreateAsync(It.IsAny<long>(), It.IsAny<CreateMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(messageDto);

        var request = new InboundEmailRequest
        {
            From = "sender@example.com",
            To = "inbox@company.com",
            Subject = "Test Subject",
            Body = "Test body"
        };

        await _sut.ProcessInboundEmailAsync(request);

        _conversationServiceMock.Verify(
            c => c.CreateAsync(10, It.IsAny<CreateConversationRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _messageServiceMock.Verify(
            m => m.CreateAsync(100, It.IsAny<CreateMessageRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessInboundEmailAsync_WhenContactNotFound_CreatesNewContact()
    {
        var inbox = new Inbox { Id = 1, AccountId = 10, Name = "Email Inbox", ChannelType = "email", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _inboxRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Inbox> { inbox });

        _contactRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contact>());
        _contactRepoMock.Setup(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact c, CancellationToken _) => c);

        var conversationDto = new ConversationDto(
            100, 10, 1, 0, null, null, 1,
            "Open", "None", null, null, false,
            DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow,
            null, null, null, 0, []);
        _conversationServiceMock.Setup(c => c.CreateAsync(10, It.IsAny<CreateConversationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationDto);

        var messageDto = new MessageDto(1, 100, 10, null, null, "World", "text", "incoming", false, "sent", DateTime.UtcNow, DateTime.UtcNow, []);
        _messageServiceMock.Setup(m => m.CreateAsync(It.IsAny<long>(), It.IsAny<CreateMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(messageDto);

        var request = new InboundEmailRequest
        {
            From = "new@example.com",
            To = "inbox@company.com",
            Subject = "Hello",
            Body = "World"
        };

        await _sut.ProcessInboundEmailAsync(request);

        _contactRepoMock.Verify(
            r => r.AddAsync(It.Is<Contact>(c => c.Email == "new@example.com"), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendOutboundEmailAsync_PublishesOutboundEmailEvent()
    {
        var request = new OutboundEmailRequest
        {
            ConversationId = 100,
            MessageId = 200,
            To = "recipient@example.com",
            Subject = "Re: Test",
            Body = "Reply body"
        };

        await _sut.SendOutboundEmailAsync(request);

        _mediatorMock.Verify(
            m => m.Publish(
                It.Is<OutboundEmailEvent>(e =>
                    e.ConversationId == 100 &&
                    e.To == "recipient@example.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
