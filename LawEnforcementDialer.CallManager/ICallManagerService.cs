namespace LawEnforcementDialer.CallManager;

public interface ICallManagerService
{
    Task SaveActiveCallAsync(ActiveCall activeCall);

    Task<ActiveCall> GetActiveCallAsync(string id);

    Task RemoveActiveCallAsync(string correlationId);
}