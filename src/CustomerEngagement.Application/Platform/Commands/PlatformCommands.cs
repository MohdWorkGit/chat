using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using MediatR;

namespace CustomerEngagement.Application.Platform.Commands;

// -------- Platform Accounts --------

public record CreatePlatformAccountCommand(
    string Name = "",
    string? Locale = null,
    string? Domain = null,
    string? SupportEmail = null) : IRequest<object>;

public record UpdatePlatformAccountCommand(
    long Id = 0,
    string? Name = null,
    string? Locale = null,
    string? Domain = null,
    string? SupportEmail = null) : IRequest<object>;

public record DeletePlatformAccountCommand(long Id) : IRequest<object>;

public class CreatePlatformAccountCommandHandler : IRequestHandler<CreatePlatformAccountCommand, object>
{
    private readonly IRepository<Account> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePlatformAccountCommandHandler(IRepository<Account> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> Handle(CreatePlatformAccountCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Account name is required.", nameof(request));

        var account = new Account
        {
            Name = request.Name,
            Locale = request.Locale,
            Domain = request.Domain,
            SupportEmail = request.SupportEmail,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return created.Id;
    }
}

public class UpdatePlatformAccountCommandHandler : IRequestHandler<UpdatePlatformAccountCommand, object>
{
    private readonly IRepository<Account> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePlatformAccountCommandHandler(IRepository<Account> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> Handle(UpdatePlatformAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync((int)request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Account {request.Id} not found.");

        if (!string.IsNullOrWhiteSpace(request.Name)) account.Name = request.Name;
        if (request.Locale is not null) account.Locale = request.Locale;
        if (request.Domain is not null) account.Domain = request.Domain;
        if (request.SupportEmail is not null) account.SupportEmail = request.SupportEmail;
        account.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return account.Id;
    }
}

public class DeletePlatformAccountCommandHandler : IRequestHandler<DeletePlatformAccountCommand, object>
{
    private readonly IRepository<Account> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePlatformAccountCommandHandler(IRepository<Account> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> Handle(DeletePlatformAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (account is null) return false;

        await _repository.DeleteAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// -------- Agent Bots --------

public record CreateAgentBotCommand(
    string Name = "",
    string? Description = null,
    string? OutgoingUrl = null,
    string? BotType = null,
    int? AccountId = null) : IRequest<object>;

public record UpdateAgentBotCommand(
    long Id = 0,
    string? Name = null,
    string? Description = null,
    string? OutgoingUrl = null,
    string? BotType = null,
    int? AccountId = null) : IRequest<object>;

public record DeleteAgentBotCommand(long Id) : IRequest<object>;

public class CreateAgentBotCommandHandler : IRequestHandler<CreateAgentBotCommand, object>
{
    private readonly IRepository<AgentBot> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAgentBotCommandHandler(IRepository<AgentBot> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> Handle(CreateAgentBotCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Agent bot name is required.", nameof(request));

        var bot = new AgentBot
        {
            Name = request.Name,
            Description = request.Description,
            OutgoingUrl = request.OutgoingUrl,
            BotType = request.BotType,
            AccountId = request.AccountId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(bot, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return created.Id;
    }
}

public class UpdateAgentBotCommandHandler : IRequestHandler<UpdateAgentBotCommand, object>
{
    private readonly IRepository<AgentBot> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAgentBotCommandHandler(IRepository<AgentBot> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> Handle(UpdateAgentBotCommand request, CancellationToken cancellationToken)
    {
        var bot = await _repository.GetByIdAsync((int)request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Agent bot {request.Id} not found.");

        if (!string.IsNullOrWhiteSpace(request.Name)) bot.Name = request.Name;
        if (request.Description is not null) bot.Description = request.Description;
        if (request.OutgoingUrl is not null) bot.OutgoingUrl = request.OutgoingUrl;
        if (request.BotType is not null) bot.BotType = request.BotType;
        if (request.AccountId is not null) bot.AccountId = request.AccountId;
        bot.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(bot, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return bot.Id;
    }
}

public class DeleteAgentBotCommandHandler : IRequestHandler<DeleteAgentBotCommand, object>
{
    private readonly IRepository<AgentBot> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAgentBotCommandHandler(IRepository<AgentBot> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> Handle(DeleteAgentBotCommand request, CancellationToken cancellationToken)
    {
        var bot = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (bot is null) return false;

        await _repository.DeleteAsync(bot, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// -------- Platform Users --------

public record CreatePlatformUserCommand(
    string Name = "",
    string? Email = null,
    string? DisplayName = null) : IRequest<object>;

public record UpdatePlatformUserCommand(
    long Id = 0,
    string? Name = null,
    string? Email = null,
    string? DisplayName = null) : IRequest<object>;

public record DeletePlatformUserCommand(long Id) : IRequest<object>;

public class CreatePlatformUserCommandHandler : IRequestHandler<CreatePlatformUserCommand, object>
{
    private readonly IRepository<User> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePlatformUserCommandHandler(IRepository<User> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> Handle(CreatePlatformUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("User name is required.", nameof(request));

        var user = new User
        {
            Name = request.Name,
            DisplayName = request.DisplayName,
            Email = request.Email,
            UserName = request.Email ?? request.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return created.Id;
    }
}

public class UpdatePlatformUserCommandHandler : IRequestHandler<UpdatePlatformUserCommand, object>
{
    private readonly IRepository<User> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePlatformUserCommandHandler(IRepository<User> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> Handle(UpdatePlatformUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync((int)request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.Id} not found.");

        if (!string.IsNullOrWhiteSpace(request.Name)) user.Name = request.Name;
        if (request.DisplayName is not null) user.DisplayName = request.DisplayName;
        if (request.Email is not null)
        {
            user.Email = request.Email;
            user.UserName ??= request.Email;
        }
        user.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}

public class DeletePlatformUserCommandHandler : IRequestHandler<DeletePlatformUserCommand, object>
{
    private readonly IRepository<User> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePlatformUserCommandHandler(IRepository<User> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<object> Handle(DeletePlatformUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (user is null) return false;

        await _repository.DeleteAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
