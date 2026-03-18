using CustomerEngagement.Application.Services.Automations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace CustomerEngagement.Application.Tests.Services;

public class CampaignServiceTests
{
    private readonly Mock<IRepository<Campaign>> _campaignRepoMock;
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<CampaignService>> _loggerMock;
    private readonly CampaignService _sut;

    public CampaignServiceTests()
    {
        _campaignRepoMock = new Mock<IRepository<Campaign>>();
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<CampaignService>>();
        _sut = new CampaignService(
            _campaignRepoMock.Object,
            _contactRepoMock.Object,
            _unitOfWorkMock.Object,
            _mediatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCampaignExists_ReturnsDto()
    {
        var campaign = CreateTestCampaign(1);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var result = await _sut.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Title.Should().Be("Test Campaign");
    }

    [Fact]
    public async Task GetByIdAsync_WhenCampaignNotFound_ReturnsNull()
    {
        _campaignRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Campaign?)null);

        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_CreatesCampaignAndSaves()
    {
        var request = new CreateCampaignRequest
        {
            Title = "New Campaign",
            Message = "Hello!",
            CampaignType = 0
        };

        _campaignRepoMock.Setup(r => r.AddAsync(It.IsAny<Campaign>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Campaign c, CancellationToken _) => c);

        var result = await _sut.CreateAsync(10, request);

        result.Should().NotBeNull();
        result.Title.Should().Be("New Campaign");
        result.IsEnabled.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivateAsync_EnablesCampaign()
    {
        var campaign = CreateTestCampaign(1);
        campaign.Enabled = false;
        _campaignRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        await _sut.ActivateAsync(1);

        campaign.Enabled.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCampaignDisabled_DoesNotSendMessages()
    {
        var campaign = CreateTestCampaign(1);
        campaign.Enabled = false;
        _campaignRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        await _sut.ExecuteAsync(1);

        _mediatorMock.Verify(
            m => m.Publish(It.IsAny<CampaignMessageEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCampaignEnabled_SendsMessageToEachContact()
    {
        var campaign = CreateTestCampaign(1);
        campaign.Enabled = true;
        _campaignRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var contacts = new List<Contact>
        {
            new() { Id = 1, AccountId = 10, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, AccountId = 10, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        _contactRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contacts);

        await _sut.ExecuteAsync(1);

        _mediatorMock.Verify(
            m => m.Publish(It.IsAny<CampaignMessageEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    private static Campaign CreateTestCampaign(int id) => new()
    {
        Id = id,
        AccountId = 10,
        Title = "Test Campaign",
        Message = "Hello!",
        CampaignType = CustomerEngagement.Core.Enums.CampaignType.OneOff,
        Enabled = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
