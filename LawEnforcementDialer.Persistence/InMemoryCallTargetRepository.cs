namespace LawEnforcementDialer.Persistence;

public class InMemoryCallTargetRepository : ICallTargetRepository
{
    private readonly Dictionary<string, string> _callTargets = new();

    public Task SetCallTargetAsync(string callId, string target)
    {
        _callTargets.TryAdd(callId, target);
        return Task.CompletedTask;
    }

    public Task<string?> GetCallTargetAsync(string callId)
    {
        _callTargets.TryGetValue(callId, out var target);
        if (target is not null)
        {
            _callTargets.Remove(callId);
        }
        return Task.FromResult(target);
    }
}