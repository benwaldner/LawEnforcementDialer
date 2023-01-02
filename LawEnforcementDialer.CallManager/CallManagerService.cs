using LawEnforcementDialer.Persistence;

namespace LawEnforcementDialer.CallManager
{
    public class CallManagerService : ICallManagerService
    {
        private readonly IActiveCallRepository _activeCallRepository;

        public CallManagerService(
            IActiveCallRepository activeCallRepository)
        {
            _activeCallRepository = activeCallRepository;
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

        public async Task RemoveActiveCallAsync(string correlationId) => await _activeCallRepository.RemoveByCorrelationId(correlationId);
    }
}