namespace LawEnforcementDialer.PinManager;

public class InvalidConfigurationException : ApplicationException
{
    public InvalidConfigurationException(string message)
        : base(message)
    {
    }
}