using Azure.Communication.CallAutomation;
using CallAutomation.Extensions;
using LawEnforcementDialer.Api.Interfaces;
using LawEnforcementDialer.CallManager;
using LawEnforcementDialer.PinManager;
using Microsoft.Extensions.Options;

namespace LawEnforcementDialer.Api.Handlers;

public sealed class PinHandler : CallAutomationHandler
{
    private readonly IPinManager _pinManager;
    private readonly IOptionsMonitor<PinManagerConfiguration> _pinManagerConfiguration;
    private readonly ICallManagerService _callManagerService;
    private readonly IDialogController _dialogController;
    private readonly ILogger<PinHandler> _logger;

    public PinHandler(
        IPinManager pinManager,
        IOptionsMonitor<PinManagerConfiguration> pinManagerConfiguration,
        ICallManagerService callManagerService,
        IDialogController dialogController,
        ILogger<PinHandler> logger)
    {
        _pinManager = pinManager;
        _pinManagerConfiguration = pinManagerConfiguration;
        _callManagerService = callManagerService;
        _dialogController = dialogController;
        _logger = logger;
    }

    public override async ValueTask OnCallConnected(CallConnected @event, CallConnection callConnection, CallMedia callMedia, CallRecording callRecording)
    {
        _logger.LogInformation("CorrelationId: {correlationId}", @event.CorrelationId);
        var activeCall = await _callManagerService.GetActiveCallAsync(@event.CallConnectionId);
        await _dialogController.InvokePinEntryRecognition(callMedia, activeCall);
    }

    public override async ValueTask OnStopToneDetected(RecognizeCompleted @event, CallConnection callConnection, CallMedia callMedia, CallRecording callRecording, IReadOnlyList<DtmfTone> tones)
    {
        var activeCall = await _callManagerService.GetActiveCallAsync(@event.CallConnectionId);

        try
        {
            var pin = ToneConverter.ToString(tones);
            var phoneNumber = activeCall.Source.Replace("4:", "");
            var pinIsValid = await _pinManager.ValidatePin(phoneNumber, pin);
            if (pinIsValid)
            {
                await _dialogController.InvokeDialerRecognition(callMedia, activeCall);
            }
        }
        catch (InvalidPinException e)
        {
            await callMedia
                .Play(x => x.FileUrl = _pinManagerConfiguration.CurrentValue.Prompts.InvalidPin)
                .OnPlayCompleted(async () => await _dialogController.InvokePinEntryRecognition(callMedia, activeCall))
                .ExecuteAsync();
        }
        catch (InvalidConfigurationException e)
        {
            await callMedia
                .Play(x => x.FileUrl = _pinManagerConfiguration.CurrentValue.Prompts.InvalidConfiguration)
                .OnPlayCompleted(async () => await callConnection.HangUpAsync(true))
                .ExecuteAsync();
        }
        catch (MaximumPinAttemptsReachedException e)
        {
            await callMedia
                .Play(x => x.FileUrl = _pinManagerConfiguration.CurrentValue.Prompts.MaximumPinAttemptsReached)
                .OnPlayCompleted(async () => await callConnection.HangUpAsync(true))
                .ExecuteAsync();
        }
    }
}