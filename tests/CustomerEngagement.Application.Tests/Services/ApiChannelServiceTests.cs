using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Channels;
using CustomerEngagement.Application.Services.Conversations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace CustomerEngagement.Application.Tests.Services;

public class ApiChannelServiceTests
{
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IConversationService> _conversationServiceMock;
    private readonly Mock<IMessageService> _messageServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ApiChannelService>> _loggerMock;
    private readonly ApiChannelService _sut;

    public ApiChannelServiceTests()
    {
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _conversationServiceMock = new Mock<IConversationService>();
        _messageServiceMock = new Mock<IMessageService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ApiChannelService>>();
        _sut = new ApiChannelService(
            _contactRepoMock.Object,
            _conversationServiceMock.Object,
            _messageServiceMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessInboundMessageAsync_WithExistingContact_CreatesConversation()
    {
        var existingContact = new Contact { Id = 5, AccountId = 10, Email = "test@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _contactRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contact> { existingContact });

        var conversationDto = CreateTestConversationDto(100, 1, 5);
        _conversationServiceMock.Setup(s => s.CreateAsync(10, It.IsAny<CreateConversationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationDto);

        var messageDto = CreateTestMessageDto(200, 100);
        _messageServiceMock.Setup(s => s.CreateAsync(100, It.IsAny<CreateMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(messageDto);

        var request = new ApiInboundMessageRequest
        {
            AccountId = 10,
            InboxId = 1,
            Content = "Hello via API",
            Contact = new ContactIdentifier { Email = "test@example.com" }
        };

        var result = await _sut.ProcessInboundMessageAsync(request);

        result.Success.Should().BeTrue();
        result.ConversationId.Should().Be(100);
        result.MessageId.Should().Be(200);
    }

    [Fact]
    public async Task ProcessInboundMessageAsync_WithNewContact_CreatesContactAndConversation()
    {
        _contactRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contact>());
        _contactRepoMock.Setup(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact c, CancellationToken _) => { c.Id = 99; return c; });

        var conversationDto = CreateTestConversationDto(100, 1, 99);
        _conversationServiceMock.Setup(s => s.CreateAsync(10, It.IsAny<CreateConversationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationDto);

        var messageDto = CreateTestMessageDto(200, 100);
        _messageServiceMock.Setup(s => s.CreateAsync(100, It.IsAny<CreateMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(messageDto);

        var request = new ApiInboundMessageRequest
        {
            AccountId = 10,
            InboxId = 1,
            Content = "New contact message",
            Contact = new ContactIdentifier { Email = "new@example.com", Name = "New User" }
        };

        var result = await _sut.ProcessInboundMessageAsync(request);

        result.Success.Should().BeTrue();
        _contactRepoMock.Verify(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessInboundMessageAsync_WhenServiceThrows_ReturnsFailure()
    {
        _contactRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var request = new ApiInboundMessageRequest
        {
            AccountId = 10,
            InboxId = 1,
            Content = "Will fail"
        };

        var result = await _sut.ProcessInboundMessageAsync(request);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Database error");
    }

    [Fact]
    public async Task SendOutboundMessageAsync_CreatesOutgoingMessage()
    {
        var messageDto = CreateTestMessageDto(300, 50);
        _messageServiceMock.Setup(s => s.CreateAsync(50, It.IsAny<CreateMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(messageDto);

        var request = new ApiOutboundMessageRequest
        {
            ConversationId = 50,
            Content = "Reply via API",
            SenderId = 3
        };

        var result = await _sut.SendOutboundMessageAsync(request);

        result.Success.Should().BeTrue();
        result.ConversationId.Should().Be(50);
        result.MessageId.Should().Be(300);
    }

    [Fact]
    public async Task SendOutboundMessageAsync_WhenServiceThrows_ReturnsFailure()
    {
        _messageServiceMock.Setup(s => s.CreateAsync(It.IsAny<long>(), It.IsAny<CreateMessageRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Conversation not found"));

        var request = new ApiOutboundMessageRequest
        {
            ConversationId = 999,
            Content = "Will fail"
        };

        var result = await _sut.SendOutboundMessageAsync(request);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Conversation not found");
    }

    private static ConversationDto CreateTestConversationDto(int id, int inboxId, int contactId) =>
        new(id, 10, inboxId, contactId, null, null, id, "open", "none", null, null, false, null,
            DateTime.UtcNow, DateTime.UtcNow, null, null, null, 0, Array.Empty<string>());

    private static MessageDto CreateTestMessageDto(int id, int conversationId) =>
        new(id, conversationId, 10, null, null, "Test", "text", "incoming", false, "sent",
            DateTime.UtcNow, DateTime.UtcNow, Array.Empty<AttachmentDto>());
}
