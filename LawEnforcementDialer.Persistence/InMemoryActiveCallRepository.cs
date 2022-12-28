namespace LawEnforcementDialer.Persistence;

public class InMemoryActiveCallRepository : IActiveCallRepository
{
    private readonly Dictionary<string, PersistedActiveCall> _data = new();

    public Task SetAsync(PersistedActiveCall persistedActiveCall)
    {
        _data.TryGetValue(persistedActiveCall.Id, out var existingData);
        if (existingData is not null)
        {
            _data[persistedActiveCall.Id] = persistedActiveCall;
        }
        else
        {
            _data.TryAdd(persistedActiveCall.Id, persistedActiveCall);
        }

        return Task.CompletedTask;
    }

    public Task<PersistedActiveCall?> FindAsync(string id)
    {
        _data.TryGetValue(id, out var existingData);
        return Task.FromResult(existingData);
    }

    public Task RemoveAsync(string id) => throw new NotImplementedException();

    public Task RemoveByCorrelationId(string correlationId)
    {
        var persistedActiveCall = _data.FirstOrDefault(x => x.Value.CorrelationId == correlationId);
        _data.Remove(persistedActiveCall.Key);
        return Task.CompletedTask;
    }
}