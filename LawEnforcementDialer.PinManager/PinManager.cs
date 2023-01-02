using LawEnforcementDialer.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LawEnforcementDialer.PinManager;

public class PinManager : IPinManager
{
    private readonly IOptionsMonitor<PinManagerConfiguration> _pinManagerConfiguration;
    private readonly IPinRepository _pinRepository;
    private readonly ILogger<PinManager> _logger;
    private readonly Dictionary<string, PinAttempt> _pinAttempts = new();

    public PinManager(
        IOptionsMonitor<PinManagerConfiguration> pinManagerConfiguration,
        IPinRepository pinRepository,
        ILogger<PinManager> logger)
    {
        _pinManagerConfiguration = pinManagerConfiguration;
        _pinRepository = pinRepository;
        _logger = logger;
    }

    public async Task<bool> ValidatePin(string phoneNumber, string digits)
    {
        if (!await _pinRepository.NumberExistsAsync(phoneNumber))
        {
            // configuration not found for this number
            throw new InvalidConfigurationException(_pinManagerConfiguration.CurrentValue.Prompts.InvalidConfiguration);
        }

        if (!_pinAttempts.ContainsKey(phoneNumber))
        {
            // first attempt for this phone number
            _pinAttempts.Add(phoneNumber, new PinAttempt(phoneNumber));
        }

        var pin = await _pinRepository.GetPinAsync(phoneNumber);
        if (digits == pin) return true;

        _pinAttempts[phoneNumber].AddBadAttempt();

        // check if PIN is locked out
        _pinAttempts.TryGetValue(phoneNumber, out var badAttempt);
        if (badAttempt?.BadAttempts >= _pinManagerConfiguration.CurrentValue.PinMaxAttempts)
        {
            throw new MaximumPinAttemptsReachedException(_pinManagerConfiguration.CurrentValue.Prompts.MaximumPinAttemptsReached);
        }

        throw new InvalidPinException(_pinManagerConfiguration.CurrentValue.Prompts.InvalidPin);
    }

    public void ResetBadAttempts(DateTimeOffset currentDateTimeOffset)
    {
        var lockoutDurationInMinutes = TimeSpan.FromMinutes(_pinManagerConfiguration.CurrentValue.PinLockoutDurationInMinutes);

        foreach (var pinAttempt in _pinAttempts)
        {
            if (currentDateTimeOffset >= pinAttempt.Value.DateTimeOfLastTry.AddMinutes(lockoutDurationInMinutes.Minutes) && pinAttempt.Value.BadAttempts > 0)
            {
                _logger.LogDebug($"Resetting {pinAttempt.Key} which had {pinAttempt.Value.BadAttempts} bad attempts, last tried on {pinAttempt.Value.DateTimeOfLastTry}");
                pinAttempt.Value.Reset();
            }
        }
    }
}