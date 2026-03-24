using System.Text.Json;
using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Integrations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CustomerEngagement.Application.Tests.Services;

public class WebhookServiceTests
{
    private readonly Mock<IRepository<Webhook>> _webhookRepoMock;
    private readonly Mock<IRepository<WebhookDelivery>> _deliveryRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<WebhookService>> _loggerMock;
    private readonly WebhookService _sut;

    public WebhookServiceTests()
    {
        _webhookRepoMock = new Mock<IRepository<Webhook>>();
        _deliveryRepoMock = new Mock<IRepository<WebhookDelivery>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<WebhookService>>();
        _sut = new WebhookService(
            _webhookRepoMock.Object,
            _deliveryRepoMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_CreatesWebhookAndSaves()
    {
        var request = new RegisterWebhookRequest
        {
            Url = "https://example.com/webhook",
            EventTypes = new List<string> { "conversation_created", "message_created" },
            Secret = "mysecret"
        };

        _webhookRepoMock.Setup(r => r.AddAsync(It.IsAny<Webhook>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Webhook w, CancellationToken _) => w);

        var result = await _sut.RegisterAsync(10, request);

        result.Should().NotBeNull();
        result.Url.Should().Be("https://example.com/webhook");
        result.IsActive.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenWebhookNotFound_Throws()
    {
        _webhookRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Webhook?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task DeleteAsync_WhenWebhookExists_DeletesAndSaves()
    {
        var webhook = CreateTestWebhook(1);
        _webhookRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(webhook);

        await _sut.DeleteAsync(1);

        _webhookRepoMock.Verify(r => r.DeleteAsync(webhook, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FireWebhookAsync_WhenWebhookMatchesEvent_QueuesDelivery()
    {
        var webhook = CreateTestWebhook(1);
        _webhookRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Webhook> { webhook });

        _deliveryRepoMock.Setup(r => r.AddAsync(It.IsAny<WebhookDelivery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WebhookDelivery d, CancellationToken _) => d);

        await _sut.FireWebhookAsync(10, "conversation_created", new { Id = 1 });

        _deliveryRepoMock.Verify(
            r => r.AddAsync(
                It.Is<WebhookDelivery>(d =>
                    d.WebhookId == 1 &&
                    d.EventType == "conversation_created" &&
                    d.Status == "queued"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FireWebhookAsync_WhenWebhookDoesNotMatchEvent_SkipsDelivery()
    {
        var webhook = CreateTestWebhook(1);
        _webhookRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Webhook> { webhook });

        await _sut.FireWebhookAsync(10, "contact_deleted", new { Id = 1 });

        _deliveryRepoMock.Verify(
            r => r.AddAsync(It.IsAny<WebhookDelivery>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Webhook CreateTestWebhook(int id) => new()
    {
        Id = id,
        AccountId = 10,
        Url = "https://example.com/webhook",
        EventTypes = JsonSerializer.Serialize(new List<string> { "conversation_created", "message_created" }),
        Secret = "testsecret",
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
