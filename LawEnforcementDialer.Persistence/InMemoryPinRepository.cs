using Microsoft.Extensions.Configuration;

namespace LawEnforcementDialer.Persistence;

public class InMemoryPinRepository : IPinRepository
{
    // key = phoneNumber, value = pin
    private readonly Dictionary<string, string> _pins = new();

    public InMemoryPinRepository(IConfiguration configuration)
    {
        _pins = configuration.GetSection("Pins").Get<Dictionary<string, string>>();
    }
    public Task<string?> GetPinAsync(string number)
    {
        _pins.TryGetValue(number, out var pin);
        return Task.FromResult(pin);
    }

    public Task<bool> NumberExistsAsync(string number) => Task.FromResult(_pins.ContainsKey(number));
}