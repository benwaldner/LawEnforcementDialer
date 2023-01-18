using Azure.Communication;
using Azure.Communication.CallAutomation;
using CallAutomation.Extensions;
using CallAutomation.Extensions.Models;
using LawEnforcementDialer.Api.Handlers;
using LawEnforcementDialer.Api.Interfaces;
using LawEnforcementDialer.CallManager;

namespace LawEnforcementDialer.Api;

public sealed class DialogController : IDialogController
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DialogController> _logger;

    public DialogController(IConfiguration configuration, ILogger<DialogController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeDialerRecognition(CallMedia callMedia, ActiveCall activeCall)
    {
        await callMedia
            .ReceiveDtmfTone()
            .FromParticipant(activeCall.Source)
            .WithPrompt(_configuration["DialerPromptUri"])
            .WithOptions(x =>
            {
                x.MaxToneCount = 11;
                x.AllowInterruptExistingMediaOperation = true;
                x.AllowInterruptPrompt = true;
                x.WaitBetweenTonesInSeconds = 3;
                x.WaitForResponseInSeconds = 5;
                x.StopTones.Add(DtmfTone.Pound);
            })
            .OnStopToneDetected<DialerHandler>()
            .ExecuteAsync();
    }

    public async Task InvokePinEntryRecognition(CallMedia callMedia, ActiveCall activeCall)
    {
        await callMedia
            .ReceiveDtmfTone()
            .FromParticipant(activeCall.Source)
            .WithPrompt(_configuration["WelcomePromptUri"])
            .WithOptions(x =>
            {
                x.MaxToneCount = 6;
                x.AllowInterruptExistingMediaOperation = true;
                x.AllowInterruptPrompt = true;
                x.WaitBetweenTonesInSeconds = 3;
                x.WaitForResponseInSeconds = 8;
                x.StopTones.Add(DtmfTone.Pound);
            })
            .OnStopToneDetected<PinHandler>()
            .ExecuteAsync();
    }

    public async Task InvokeAddPstnTarget(CallConnection callConnection, CallMedia callMedia, ActiveCall activeCall, PhoneNumberIdentifier target)
    {
        var enablePromptToRecord = _configuration.GetValue<bool>("EnablePromptToRecord");
        if (enablePromptToRecord)
        {
            await callMedia.ReceiveDtmfTone()
                .FromParticipant(activeCall.Source)
                .WithPrompt(_configuration["OptionToRecordPromptUri"])
                .WithOptions(x =>
                {
                    x.MaxToneCount = 1;
                    x.AllowInterruptExistingMediaOperation = true;
                    x.AllowInterruptPrompt = true;
                    x.WaitForResponseInSeconds = 2;
                })
                .OnPress<One>(async (recognizeCompleted, _, _, callRecording, _) =>
                {
                    await callRecording.StartRecordingAsync(new StartRecordingOptions(new ServerCallLocator(recognizeCompleted.ServerCallId))
                    {
                        RecordingStorageType = RecordingStorageType.Acs,
                        RecordingContent = RecordingContent.Audio,
                        RecordingFormat = RecordingFormat.Mp3,
                        RecordingStateCallbackEndpoint = new Uri(_configuration["VS_TUNNEL_URL"] + "api/callRecording")
                    });

                    await AddParticipant(callConnection, callMedia, activeCall, target);
                })
                .OnFail<SilenceTimeout>(async () => await AddParticipant(callConnection, callMedia, activeCall, target))
                .ExecuteAsync();
        }
        else
        {
            await AddParticipant(callConnection, callMedia, activeCall, target);
        }
    }

    private async Task AddParticipant(CallConnection callConnection, CallMedia callMedia, ActiveCall activeCall, PhoneNumberIdentifier target)
    {
        await callMedia
            .Play(x => x.FileUrl = _configuration["WaitWhileWeConnectCallUri"])
            .OnPlayCompleted(async () =>
            {
                await callMedia
                    .Play(x => x.FileUrl = _configuration["HoldMusic"])
                    .ToParticipant(activeCall.Source)
                    .ExecuteAsync();
            })
            .ExecuteAsync();

        await callConnection
            .AddParticipant(target.RawId, x =>
            {
                x.SourceCallerIdNumber = _configuration["CallerIdNumber"];
            })
            .OnAddParticipantsFailed(async (addParticipantFailed, _, _, _) =>
            {
                var message = addParticipantFailed.ResultInformation.Message;
                _logger.LogError(message);

                await callMedia
                    .Play(x => x.FileUrl = _configuration["AddParticipantFailedPromptUri"])
                    .ExecuteAsync();
            })
            .OnAddParticipantsSucceeded(async () =>
            {
                _logger.LogInformation($"Participant added: {target}");

                await callMedia.CancelAllMediaOperationsAsync();

                await callMedia
                    .Play(x => x.FileUrl = _configuration["NotifyAboutRecordingUri"])
                    .ExecuteAsync();
            })
            .ExecuteAsync();
    }
}