using CustomerEngagement.Application.Services.Automations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CustomerEngagement.Application.Tests.Services;

public class MacroExecutionServiceTests
{
    private readonly Mock<IRepository<Macro>> _macroRepoMock;
    private readonly Mock<IAutomationRuleEngine> _automationEngineMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<MacroExecutionService>> _loggerMock;
    private readonly MacroExecutionService _sut;

    public MacroExecutionServiceTests()
    {
        _macroRepoMock = new Mock<IRepository<Macro>>();
        _automationEngineMock = new Mock<IAutomationRuleEngine>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<MacroExecutionService>>();
        _sut = new MacroExecutionService(
            _macroRepoMock.Object,
            _automationEngineMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMacroExists_ReturnsDto()
    {
        var macro = CreateTestMacro(1);
        _macroRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(macro);

        var result = await _sut.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Macro");
    }

    [Fact]
    public async Task GetByIdAsync_WhenMacroNotFound_ReturnsNull()
    {
        _macroRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Macro?)null);

        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_CreatesMacroAndSaves()
    {
        var request = new CreateMacroRequest
        {
            Name = "New Macro",
            Visibility = 1,
            CreatedById = 5,
            Actions = new List<AutomationActionDto>
            {
                new() { ActionType = "assign_agent", Parameters = new Dictionary<string, object> { { "agent_id", 3 } } }
            }
        };

        _macroRepoMock.Setup(r => r.AddAsync(It.IsAny<Macro>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Macro m, CancellationToken _) => m);

        var result = await _sut.CreateAsync(10, request);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Macro");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenMacroNotFound_Throws()
    {
        _macroRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Macro?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task ExecuteAsync_DelegatesToAutomationRuleEngine()
    {
        var macro = CreateTestMacro(1);
        _macroRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(macro);

        await _sut.ExecuteAsync(1, 200);

        _automationEngineMock.Verify(
            e => e.ExecuteActionsAsync(
                It.Is<AutomationRuleDto>(r => r.Id == 1 && r.AccountId == 10),
                It.Is<AutomationContext>(c => c.ConversationId == 200 && c.AccountId == 10),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Macro CreateTestMacro(int id) => new()
    {
        Id = id,
        AccountId = 10,
        Name = "Test Macro",
        Visibility = "1",
        CreatedById = 5,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
