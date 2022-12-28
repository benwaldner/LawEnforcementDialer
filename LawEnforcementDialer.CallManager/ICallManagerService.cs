namespace LawEnforcementDialer.CallManager;

public interface ICallManagerService
{
    Task SaveActiveCallAsync(ActiveCall activeCall);

    Task<ActiveCall> GetActiveCallAsync(string id);

    Task RemoveActiveCallAsync(string correlationId);

    Task SetCallTargetAsync(ActiveCall activeCall, string target);

    Task<string?> GetCallTargetAsync(ActiveCall activeCall);
}