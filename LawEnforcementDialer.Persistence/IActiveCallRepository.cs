namespace LawEnforcementDialer.Persistence;

public interface IActiveCallRepository
{
    Task SetAsync(PersistedActiveCall persistedActiveCall);

    Task<PersistedActiveCall?> FindAsync(string id);

    Task RemoveAsync(string id);

    Task RemoveByCorrelationId(string correlationId);
}