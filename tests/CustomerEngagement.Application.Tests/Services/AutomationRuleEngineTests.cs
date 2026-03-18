using CustomerEngagement.Application.Services.Automations;
using CustomerEngagement.Application.Services.Conversations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CustomerEngagement.Application.Tests.Services;

public class AutomationRuleEngineTests
{
    private readonly Mock<IRepository<AutomationRule>> _ruleRepoMock;
    private readonly Mock<IConversationService> _conversationServiceMock;
    private readonly Mock<IAssignmentService> _assignmentServiceMock;
    private readonly Mock<ILogger<AutomationRuleEngine>> _loggerMock;
    private readonly AutomationRuleEngine _sut;

    public AutomationRuleEngineTests()
    {
        _ruleRepoMock = new Mock<IRepository<AutomationRule>>();
        _conversationServiceMock = new Mock<IConversationService>();
        _assignmentServiceMock = new Mock<IAssignmentService>();
        _loggerMock = new Mock<ILogger<AutomationRuleEngine>>();
        _sut = new AutomationRuleEngine(
            _ruleRepoMock.Object,
            _conversationServiceMock.Object,
            _assignmentServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task EvaluateAsync_WhenNoRulesExist_DoesNotExecuteActions()
    {
        _ruleRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AutomationRule>());

        var context = new AutomationContext { AccountId = 1, ConversationId = 100 };

        await _sut.EvaluateAsync("conversation_created", context);

        _assignmentServiceMock.Verify(
            a => a.AssignToAgentAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteActionsAsync_AssignAgentAction_CallsAssignmentService()
    {
        var rule = new AutomationRuleDto
        {
            Id = 1,
            AccountId = 10,
            Actions = new List<AutomationActionDto>
            {
                new()
                {
                    ActionType = "assign_agent",
                    Parameters = new Dictionary<string, object> { { "agent_id", 5 } }
                }
            }
        };

        var context = new AutomationContext { AccountId = 10, ConversationId = 100 };

        await _sut.ExecuteActionsAsync(rule, context);

        _assignmentServiceMock.Verify(
            a => a.AssignToAgentAsync(100, 5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteActionsAsync_AssignTeamAction_CallsAssignmentService()
    {
        var rule = new AutomationRuleDto
        {
            Id = 1,
            AccountId = 10,
            Actions = new List<AutomationActionDto>
            {
                new()
                {
                    ActionType = "assign_team",
                    Parameters = new Dictionary<string, object> { { "team_id", 3 } }
                }
            }
        };

        var context = new AutomationContext { AccountId = 10, ConversationId = 100 };

        await _sut.ExecuteActionsAsync(rule, context);

        _assignmentServiceMock.Verify(
            a => a.AssignToTeamAsync(100, 3, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteActionsAsync_ResolveAction_CallsConversationService()
    {
        var rule = new AutomationRuleDto
        {
            Id = 1,
            AccountId = 10,
            Actions = new List<AutomationActionDto>
            {
                new() { ActionType = "resolve" }
            }
        };

        var context = new AutomationContext { AccountId = 10, ConversationId = 100 };

        await _sut.ExecuteActionsAsync(rule, context);

        _conversationServiceMock.Verify(
            c => c.ResolveAsync(100, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteActionsAsync_WhenActionThrows_ContinuesWithNextAction()
    {
        _assignmentServiceMock.Setup(a => a.AssignToAgentAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Agent not found"));

        var rule = new AutomationRuleDto
        {
            Id = 1,
            AccountId = 10,
            Actions = new List<AutomationActionDto>
            {
                new()
                {
                    ActionType = "assign_agent",
                    Parameters = new Dictionary<string, object> { { "agent_id", 999 } }
                },
                new() { ActionType = "mute" }
            }
        };

        var context = new AutomationContext { AccountId = 10, ConversationId = 100 };

        await _sut.ExecuteActionsAsync(rule, context);

        _conversationServiceMock.Verify(
            c => c.MuteAsync(100, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
