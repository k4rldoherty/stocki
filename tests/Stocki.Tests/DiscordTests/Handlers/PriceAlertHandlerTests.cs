using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Stocki.Discord.Handlers;
using Stocki.Discord.Interfaces;
using Stocki.Domain.Interfaces;
using Stocki.Shared.Notifications;
using DiscordEmbed = Discord.Embed;

namespace Stocki.Tests.DiscordTests.Handlers;

public class PriceAlertHandlerTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IDiscordClientWrapper> _discordClientMock;
    private readonly Mock<ILogger<PriceAlertHandler>> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IStockPriceSubscriptionRepository> _repositoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly PriceAlertHandler _handler;

    public PriceAlertHandlerTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _discordClientMock = new Mock<IDiscordClientWrapper>();
        _loggerMock = new Mock<ILogger<PriceAlertHandler>>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _repositoryMock = new Mock<IStockPriceSubscriptionRepository>();
        _scopeMock = new Mock<IServiceScope>();

        _scopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
        _scopeFactoryMock.Setup(s => s.CreateScope()).Returns(_scopeMock.Object);
        _serviceProviderMock.Setup(s => s.GetService(typeof(IStockPriceSubscriptionRepository)))
            .Returns(_repositoryMock.Object);

        _handler = new PriceAlertHandler(
            _scopeFactoryMock.Object,
            _discordClientMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_NoSubscribedUsers_DoesNothing()
    {
        var notification = new PriceAlertNotification("AAPL", 150.00m, 2.5m);
        _repositoryMock.Setup(r => r.GetAllUsersSubscribedToAStock("AAPL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ulong>());

        await _handler.Handle(notification, CancellationToken.None);

        _discordClientMock.Verify(
            c => c.GetUserAsync(It.IsAny<ulong>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFound_LogsWarning()
    {
        var notification = new PriceAlertNotification("AAPL", 150.00m, 2.5m);
        var userId = 12345ul;

        _repositoryMock.Setup(r => r.GetAllUsersSubscribedToAStock("AAPL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ulong> { userId });

        _discordClientMock.Setup(c => c.GetUserAsync(userId))
            .ReturnsAsync((IDiscordUser?)null);

        await _handler.Handle(notification, CancellationToken.None);

        _discordClientMock.Verify(
            c => c.GetUserAsync(userId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidUser_SendsDmWithEmbed()
    {
        var notification = new PriceAlertNotification("AAPL", 150.00m, 2.5m);
        var userId = 12345ul;

        var userMock = new Mock<IDiscordUser>();
        userMock.Setup(u => u.Id).Returns(userId);

        var dmChannelMock = new Mock<IDiscordDmChannel>();

        userMock.Setup(u => u.CreateDMChannelAsync())
            .ReturnsAsync(dmChannelMock.Object);

        _repositoryMock.Setup(r => r.GetAllUsersSubscribedToAStock("AAPL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ulong> { userId });

        _discordClientMock.Setup(c => c.GetUserAsync(userId))
            .ReturnsAsync(userMock.Object);

        await _handler.Handle(notification, CancellationToken.None);

        dmChannelMock.Verify(
            c => c.SendMessageAsync(
                null,
                false,
                It.IsAny<DiscordEmbed>(),
                null),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DmFails_LogsError()
    {
        var notification = new PriceAlertNotification("AAPL", 150.00m, 2.5m);
        var userId = 12345ul;

        var userMock = new Mock<IDiscordUser>();
        userMock.Setup(u => u.Id).Returns(userId);

        var dmChannelMock = new Mock<IDiscordDmChannel>();
        dmChannelMock.Setup(c => c.SendMessageAsync(
                null, false, It.IsAny<DiscordEmbed>(), null))
            .ThrowsAsync(new Exception("Discord API error"));

        userMock.Setup(u => u.CreateDMChannelAsync())
            .ReturnsAsync(dmChannelMock.Object);

        _repositoryMock.Setup(r => r.GetAllUsersSubscribedToAStock("AAPL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ulong> { userId });

        _discordClientMock.Setup(c => c.GetUserAsync(userId))
            .ReturnsAsync(userMock.Object);

        await _handler.Handle(notification, CancellationToken.None);

        dmChannelMock.Verify(
            c => c.SendMessageAsync(
                null,
                false,
                It.IsAny<DiscordEmbed>(),
                null),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleUsers_SendsToAll()
    {
        var notification = new PriceAlertNotification("AAPL", 150.00m, 2.5m);
        var userId1 = 12345ul;
        var userId2 = 67890ul;

        var userMock1 = new Mock<IDiscordUser>();
        userMock1.Setup(u => u.Id).Returns(userId1);
        var dmChannelMock1 = new Mock<IDiscordDmChannel>();
        userMock1.Setup(u => u.CreateDMChannelAsync()).ReturnsAsync(dmChannelMock1.Object);

        var userMock2 = new Mock<IDiscordUser>();
        userMock2.Setup(u => u.Id).Returns(userId2);
        var dmChannelMock2 = new Mock<IDiscordDmChannel>();
        userMock2.Setup(u => u.CreateDMChannelAsync()).ReturnsAsync(dmChannelMock2.Object);

        _repositoryMock.Setup(r => r.GetAllUsersSubscribedToAStock("AAPL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ulong> { userId1, userId2 });

        _discordClientMock.Setup(c => c.GetUserAsync(userId1))
            .ReturnsAsync(userMock1.Object);
        _discordClientMock.Setup(c => c.GetUserAsync(userId2))
            .ReturnsAsync(userMock2.Object);

        await _handler.Handle(notification, CancellationToken.None);

        dmChannelMock1.Verify(
            c => c.SendMessageAsync(null, false, It.IsAny<DiscordEmbed>(), null),
            Times.Once);
        dmChannelMock2.Verify(
            c => c.SendMessageAsync(null, false, It.IsAny<DiscordEmbed>(), null),
            Times.Once);
    }
}
