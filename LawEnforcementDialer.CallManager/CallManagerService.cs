using LawEnforcementDialer.Persistence;

namespace LawEnforcementDialer.CallManager
{
    public class CallManagerService : ICallManagerService
    {
        private readonly IActiveCallRepository _activeCallRepository;
        private readonly ICallTargetRepository _callTargetRepository;

        public CallManagerService(
            IActiveCallRepository activeCallRepository,
            ICallTargetRepository callTargetRepository)
        {
            _activeCallRepository = activeCallRepository;
            _callTargetRepository = callTargetRepository;
        }

        public async Task SaveActiveCallAsync(ActiveCall activeCall)
        {
            await _activeCallRepository.SetAsync(new PersistedActiveCall()
            {
                Id = activeCall.Id,
                Participants = activeCall.Participants,
                Source = activeCall.Source,
                Target = activeCall.Target
            });
        }

        public async Task<ActiveCall> GetActiveCallAsync(string id)
        {
            var persistedActiveCall = await _activeCallRepository.FindAsync(id);
            return new ActiveCall()
            {
                Id = persistedActiveCall.Id,
                Participants = persistedActiveCall.Participants,
                Source = persistedActiveCall.Source,
                Target = persistedActiveCall.Target
            };
        }

        public async Task SetCallTargetAsync(ActiveCall activeCall, string target) => await _callTargetRepository.SetCallTargetAsync(activeCall.Id, target);

        public async Task<string?> GetCallTargetAsync(ActiveCall activeCall) => await _callTargetRepository.GetCallTargetAsync(activeCall.Id);

        public async Task RemoveActiveCallAsync(string correlationId) => await _activeCallRepository.RemoveByCorrelationId(correlationId);
    }
}