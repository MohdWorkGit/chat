using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Channels;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Entities.Channels;
using CustomerEngagement.Core.Interfaces;
using Moq;
using FluentAssertions;
using Xunit;

namespace CustomerEngagement.Application.Tests.Services;

public class WebWidgetServiceTests
{
    private readonly Mock<IRepository<ChannelWebWidget>> _widgetRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly WebWidgetService _sut;

    public WebWidgetServiceTests()
    {
        _widgetRepoMock = new Mock<IRepository<ChannelWebWidget>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new WebWidgetService(_widgetRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateWidgetAsync_CreatesWidgetWithDefaults()
    {
        var request = new CreateWebWidgetRequest
        {
            InboxId = 1,
            WebsiteUrl = "https://example.com"
        };

        _widgetRepoMock.Setup(r => r.AddAsync(It.IsAny<ChannelWebWidget>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChannelWebWidget w, CancellationToken _) => w);

        var result = await _sut.CreateWidgetAsync(10, request);

        result.Should().NotBeNull();
        result.AccountId.Should().Be(10);
        result.WebsiteUrl.Should().Be("https://example.com");
        result.WelcomeTitle.Should().Be("Welcome!");
        result.WelcomeTagline.Should().Be("We are here to help. Ask us anything.");
        result.WidgetColor.Should().Be("#1F93FF");
        result.IsEnabled.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateWidgetAsync_UsesCustomValues()
    {
        var request = new CreateWebWidgetRequest
        {
            InboxId = 1,
            WebsiteUrl = "https://custom.com",
            WelcomeTitle = "Custom Title",
            WelcomeTagline = "Custom Tagline",
            WidgetColor = "#FF0000"
        };

        _widgetRepoMock.Setup(r => r.AddAsync(It.IsAny<ChannelWebWidget>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChannelWebWidget w, CancellationToken _) => w);

        var result = await _sut.CreateWidgetAsync(10, request);

        result.WelcomeTitle.Should().Be("Custom Title");
        result.WelcomeTagline.Should().Be("Custom Tagline");
        result.WidgetColor.Should().Be("#FF0000");
    }

    [Fact]
    public async Task GetConfigAsync_WhenTokenValid_ReturnsConfig()
    {
        var widget = CreateTestWidget();
        _widgetRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChannelWebWidget> { widget });

        var result = await _sut.GetConfigAsync("valid-token");

        result.Should().NotBeNull();
        result!.Token.Should().Be("test-token-abc");
    }

    [Fact]
    public async Task GetConfigAsync_WhenTokenInvalid_ReturnsNull()
    {
        _widgetRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChannelWebWidget>());

        var result = await _sut.GetConfigAsync("invalid-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateTokenAsync_WhenEnabledWidget_ReturnsTrue()
    {
        var widget = CreateTestWidget();
        _widgetRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChannelWebWidget> { widget });

        var result = await _sut.ValidateTokenAsync("valid-token");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateTokenAsync_WhenDisabledWidget_ReturnsFalse()
    {
        var widget = CreateTestWidget();
        widget.IsEnabled = false;
        _widgetRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChannelWebWidget> { widget });

        var result = await _sut.ValidateTokenAsync("disabled-token");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateTokenAsync_WhenNoWidget_ReturnsFalse()
    {
        _widgetRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChannelWebWidget>());

        var result = await _sut.ValidateTokenAsync("nonexistent-token");

        result.Should().BeFalse();
    }

    private static ChannelWebWidget CreateTestWidget() => new()
    {
        Id = 1,
        AccountId = 10,
        InboxId = 1,
        WebsiteUrl = "https://example.com",
        WelcomeTitle = "Welcome!",
        WelcomeTagline = "Ask us anything.",
        WidgetColor = "#1F93FF",
        WebsiteToken = "test-token-abc",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
