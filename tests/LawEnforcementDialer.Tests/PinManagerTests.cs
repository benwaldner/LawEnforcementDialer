using FluentAssertions;
using LawEnforcementDialer.Persistence;
using LawEnforcementDialer.PinManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LawEnforcementDialer.Tests;

public sealed class PinManagerTests
{
    private readonly IPinRepository _pinRepository;
    private readonly IOptionsMonitor<PinManagerConfiguration> _pinManagerConfiguration;

    public PinManagerTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("testconfig.json")
            .Build();

        var services = new ServiceCollection();
        services.Configure<PinManagerConfiguration>(s => configuration.GetSection(PinManagerConfiguration.Name).Bind(s));
        services.AddSingleton<IPinRepository>(new InMemoryPinRepository(configuration));

        var serviceProvider = services.BuildServiceProvider();

        _pinRepository = serviceProvider.GetService<IPinRepository>();
        _pinManagerConfiguration = serviceProvider.GetService<IOptionsMonitor<PinManagerConfiguration>>();
    }

    [Fact]
    public async Task PinManager_ValidAttempt_ShouldReturnTrue()
    {
        // arrange
        var mockLogger = new Mock<ILogger<PinManager.PinManager>>();
        var subject = new PinManager.PinManager(_pinManagerConfiguration, _pinRepository, mockLogger.Object);

        // act
        var result = await subject.ValidatePin("+14255551212", "0000");

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task PinManager_MultipleValidAttempts_ShouldReturnTrue()
    {
        // arrange
        var mockLogger = new Mock<ILogger<PinManager.PinManager>>();
        var subject = new PinManager.PinManager(_pinManagerConfiguration, _pinRepository, mockLogger.Object);

        // act
        var result1 = await subject.ValidatePin("+14255551212", "0000");
        var result2 = await subject.ValidatePin("+14255551212", "0000");
        var result3 = await subject.ValidatePin("+14255551212", "0000");

        // assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
    }

    [Fact]
    public async Task PinManager_BadAttempt_ShouldThrowInvalidPinException()
    {
        // arrange
        var mockLogger = new Mock<ILogger<PinManager.PinManager>>();
        var subject = new PinManager.PinManager(_pinManagerConfiguration, _pinRepository, mockLogger.Object);

        // act & assert
        await subject.Invoking(async x => await x.ValidatePin("+14255551212", "1234")).Should().ThrowAsync<InvalidPinException>();
    }

    [Fact]
    public async Task PinManager_InvalidPhoneNumber_ShouldThrowInvalidConfigurationException()
    {
        // arrange
        var mockLogger = new Mock<ILogger<PinManager.PinManager>>();
        var subject = new PinManager.PinManager(_pinManagerConfiguration, _pinRepository, mockLogger.Object);

        // act & assert
        await subject.Invoking(async x => await x.ValidatePin("+14255550000", "1234")).Should().ThrowAsync<InvalidConfigurationException>();
    }

    [Fact]
    public async Task PinManager_MultipleBadAttempts_ShouldThrowMaximumAttemptsReachedException()
    {
        // arrange
        var mockLogger = new Mock<ILogger<PinManager.PinManager>>();
        var subject = new PinManager.PinManager(_pinManagerConfiguration, _pinRepository, mockLogger.Object);

        // act & assert
        await subject.Invoking(async x => await x.ValidatePin("+14255551212", "1234")).Should().ThrowAsync<InvalidPinException>();
        await subject.Invoking(async x => await x.ValidatePin("+14255551212", "1234")).Should().ThrowAsync<MaximumPinAttemptsReachedException>();
    }

    [Fact]
    public async Task PinManager_ResetBadAttempts_ShouldReset()
    {
        // arrange
        var mockLogger = new Mock<ILogger<PinManager.PinManager>>();
        var subject = new PinManager.PinManager(_pinManagerConfiguration, _pinRepository, mockLogger.Object);

        // act
        await subject.Invoking(async x => await x.ValidatePin("+14255551212", "1234")).Should().ThrowAsync<InvalidPinException>();
        await subject.Invoking(async x => await x.ValidatePin("+14255551212", "1234")).Should().ThrowAsync<MaximumPinAttemptsReachedException>();

        subject.ResetBadAttempts(DateTimeOffset.UtcNow.AddHours(1));

        await subject.Invoking(async x => await x.ValidatePin("+14255551212", "1234")).Should().ThrowAsync<InvalidPinException>();

        var result = await subject.ValidatePin("+14255551212", "0000");

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task PinManager_ResetBadAttempts_ShouldNotReset()
    {
        // arrange
        var mockLogger = new Mock<ILogger<PinManager.PinManager>>();
        var subject = new PinManager.PinManager(_pinManagerConfiguration, _pinRepository, mockLogger.Object);

        // act
        await subject.Invoking(async x => await x.ValidatePin("+14255551212", "1234")).Should().ThrowAsync<InvalidPinException>();
        await subject.Invoking(async x => await x.ValidatePin("+14255551212", "1234")).Should().ThrowAsync<MaximumPinAttemptsReachedException>();

        subject.ResetBadAttempts(DateTimeOffset.UtcNow);

        // assert
        await subject.Invoking(async x => await x.ValidatePin("+14255551212", "1234")).Should().ThrowAsync<MaximumPinAttemptsReachedException>();
    }
}