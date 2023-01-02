using Azure.Communication;
using Azure.Communication.CallAutomation;
using CallAutomation.Extensions;
using LawEnforcementDialer.Api.Interfaces;
using LawEnforcementDialer.CallManager;

namespace LawEnforcementDialer.Api.Handlers;

public sealed class DialerHandler : CallAutomationHandler
{
    private readonly ICallManagerService _callManagerService;
    private readonly IDialogController _dialogController;
    private readonly ILogger<DialerHandler> _logger;

    public DialerHandler(
        ICallManagerService callManagerService,
        IDialogController dialogController,
        ILogger<DialerHandler> logger)
    {
        _callManagerService = callManagerService;
        _dialogController = dialogController;
        _logger = logger;
    }

    public override async ValueTask OnStopToneDetected(RecognizeCompleted @event, CallConnection callConnection, CallMedia callMedia, CallRecording callRecording, IReadOnlyList<DtmfTone> tones)
    {
        var activeCall = await _callManagerService.GetActiveCallAsync(@event.CallConnectionId);
        if (tones.Count < 10)
        {
            // retry input
            _logger.LogInformation($"DTMF input was {tones.Count}.");

            await _dialogController.InvokeDialerRecognition(callMedia, activeCall);
            return;
        }

        // add participant to call
        var input = ToneConverter.ToString(tones);

        var target = tones.Count == 11
            ? "+" + input
            : "+1" + input;

        await _dialogController.InvokeAddPstnTarget(callConnection, callMedia, activeCall, new PhoneNumberIdentifier(target));
    }
}