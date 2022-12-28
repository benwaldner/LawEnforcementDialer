namespace LawEnforcementDialer.Persistence;

public interface ICallTargetRepository
{
    Task SetCallTargetAsync(string callId, string target);

    Task<string?> GetCallTargetAsync(string callId);
}