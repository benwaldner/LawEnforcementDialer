namespace LawEnforcementDialer.PinManager;

public class MaximumPinAttemptsReachedException : ApplicationException
{
    public MaximumPinAttemptsReachedException(string message)
        : base(message)
    {

    }
}