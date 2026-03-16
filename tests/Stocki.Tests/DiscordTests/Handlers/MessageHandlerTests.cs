using Microsoft.Extensions.Logging;
using Moq;
using Stocki.Application.Interfaces;
using Stocki.Discord.Handlers;
using Stocki.Discord.Interfaces;

namespace Stocki.Tests.DiscordTests.Handlers;

public class MessageHandlerTests
{
    private readonly Mock<IGeminiClient> _clientMock;
    private readonly Mock<ILogger<MessageHandler>> _loggerMock;
    private readonly Mock<IDiscordClientWrapper> _discordClientMock;
    private readonly MessageHandler _handler;

    public MessageHandlerTests()
    {
        _clientMock = new Mock<IGeminiClient>();
        _loggerMock = new Mock<ILogger<MessageHandler>>();
        _discordClientMock = new Mock<IDiscordClientWrapper>();
        _handler = new MessageHandler(_clientMock.Object, _loggerMock.Object, _discordClientMock.Object);
    }

    [Fact]
    public async Task HandleMessageAsync_BotMessage_ReturnsEarly()
    {
        var userMock = new Mock<IDiscordUser>();
        userMock.Setup(u => u.IsBot).Returns(true);

        var messageMock = new Mock<IDiscordMessage>();
        messageMock.Setup(m => m.Author).Returns(userMock.Object);

        await _handler.HandleMessageAsync(messageMock.Object);

        _clientMock.Verify(
            c => c.GetResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleMessageAsync_MessageNotMentioningBot_ReturnsEarly()
    {
        var userMock = new Mock<IDiscordUser>();
        userMock.Setup(u => u.IsBot).Returns(false);

        _discordClientMock.Setup(c => c.CurrentUserId).Returns(12345);

        var messageMock = new Mock<IDiscordMessage>();
        messageMock.Setup(m => m.Author).Returns(userMock.Object);
        messageMock.Setup(m => m.MentionedUsers).Returns(new List<IDiscordUser>());
        messageMock.Setup(m => m.Content).Returns("Hello world");

        await _handler.HandleMessageAsync(messageMock.Object);

        _clientMock.Verify(
            c => c.GetResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleMessageAsync_EmptyPromptAfterMention_SendsErrorMessage()
    {
        var botUserMock = new Mock<IDiscordUser>();
        botUserMock.Setup(u => u.Id).Returns(12345);
        botUserMock.Setup(u => u.Mention).Returns("<@12345>");
        botUserMock.Setup(u => u.IsBot).Returns(false);

        var userMock = new Mock<IDiscordUser>();
        userMock.Setup(u => u.IsBot).Returns(false);

        var channelMock = new Mock<IDiscordMessageChannel>();

        _discordClientMock.Setup(c => c.CurrentUserId).Returns(12345);

        var messageMock = new Mock<IDiscordMessage>();
        messageMock.Setup(m => m.Author).Returns(userMock.Object);
        messageMock.Setup(m => m.MentionedUsers).Returns(new List<IDiscordUser> { botUserMock.Object });
        messageMock.Setup(m => m.Content).Returns("<@12345>");
        messageMock.Setup(m => m.Channel).Returns(channelMock.Object);

        await _handler.HandleMessageAsync(messageMock.Object);

        await Task.Delay(100);

        channelMock.Verify(
            c => c.SendMessageAsync("Please enter a prompt.", false, null, null),
            Times.Once);
    }

    [Fact]
    public async Task HandleMessageAsync_ValidPrompt_CallsGeminiClient()
    {
        var botUserMock = new Mock<IDiscordUser>();
        botUserMock.Setup(u => u.Id).Returns(12345);
        botUserMock.Setup(u => u.Mention).Returns("<@12345>");
        botUserMock.Setup(u => u.IsBot).Returns(false);

        var userMock = new Mock<IDiscordUser>();
        userMock.Setup(u => u.IsBot).Returns(false);

        var channelMock = new Mock<IDiscordMessageChannel>();
        channelMock.Setup(c => c.EnterTypingState(null)).Returns(Mock.Of<IDisposable>());

        _discordClientMock.Setup(c => c.CurrentUserId).Returns(12345);
        _clientMock.Setup(c => c.GetResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Test response");

        var messageMock = new Mock<IDiscordMessage>();
        messageMock.Setup(m => m.Author).Returns(userMock.Object);
        messageMock.Setup(m => m.MentionedUsers).Returns(new List<IDiscordUser> { botUserMock.Object });
        messageMock.Setup(m => m.Content).Returns("<@12345> What is a stock?");
        messageMock.Setup(m => m.Channel).Returns(channelMock.Object);

        await _handler.HandleMessageAsync(messageMock.Object);

        await Task.Delay(100);

        _clientMock.Verify(
            c => c.GetResponseAsync("What is a stock?", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleMessageAsync_ResponseOver2000Chars_TruncatesMessage()
    {
        var botUserMock = new Mock<IDiscordUser>();
        botUserMock.Setup(u => u.Id).Returns(12345);
        botUserMock.Setup(u => u.Mention).Returns("<@12345>");
        botUserMock.Setup(u => u.IsBot).Returns(false);

        var userMock = new Mock<IDiscordUser>();
        userMock.Setup(u => u.IsBot).Returns(false);

        var channelMock = new Mock<IDiscordMessageChannel>();
        channelMock.Setup(c => c.EnterTypingState(null)).Returns(Mock.Of<IDisposable>());

        _discordClientMock.Setup(c => c.CurrentUserId).Returns(12345);

        var longResponse = new string('a', 2001);
        _clientMock.Setup(c => c.GetResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(longResponse);

        var messageMock = new Mock<IDiscordMessage>();
        messageMock.Setup(m => m.Author).Returns(userMock.Object);
        messageMock.Setup(m => m.MentionedUsers).Returns(new List<IDiscordUser> { botUserMock.Object });
        messageMock.Setup(m => m.Content).Returns("<@12345> Tell me something");
        messageMock.Setup(m => m.Channel).Returns(channelMock.Object);

        await _handler.HandleMessageAsync(messageMock.Object);

        await Task.Delay(100);

        channelMock.Verify(
            c => c.SendMessageAsync(It.Is<string>(s => s.Length == 2000 && s.EndsWith("...")), false, null, null),
            Times.Once);
    }
}
