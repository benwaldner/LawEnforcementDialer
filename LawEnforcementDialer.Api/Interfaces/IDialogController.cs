using Azure.Communication;
using Azure.Communication.CallAutomation;
using LawEnforcementDialer.CallManager;

namespace LawEnforcementDialer.Api.Interfaces;

public interface IDialogController
{
    Task InvokeDialerRecognition(CallMedia callMedia, ActiveCall activeCall);

    Task InvokePinEntryRecognition(CallMedia callMedia, ActiveCall activeCall);

    Task InvokeAddPstnTarget(CallConnection callConnection, CallMedia callMedia, ActiveCall activeCall, PhoneNumberIdentifier target);
}