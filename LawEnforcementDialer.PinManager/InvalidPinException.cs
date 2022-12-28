namespace LawEnforcementDialer.PinManager;

public class InvalidPinException : ApplicationException
{
    public InvalidPinException(string message)
        : base(message)
    {

    }
}