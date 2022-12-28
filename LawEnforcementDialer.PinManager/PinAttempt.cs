namespace LawEnforcementDialer.PinManager;

public class PinAttempt
{
    public string PhoneNumber { get; }

    public int BadAttempts { get; private set; }

    public DateTimeOffset DateTimeOfLastTry { get; private set; }

    public PinAttempt(string phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }

    public void AddBadAttempt()
    {
        BadAttempts++;
        DateTimeOfLastTry = DateTimeOffset.UtcNow;
    }

    public void Reset()
    {
        BadAttempts = 0;
    }
}