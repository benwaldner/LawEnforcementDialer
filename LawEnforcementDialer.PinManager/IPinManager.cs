namespace LawEnforcementDialer.PinManager;

public interface IPinManager
{
    /// <summary>
    /// Attempts to validate pin based on phone number.
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="digits"></param>
    /// <returns></returns>
    Task<bool> ValidatePin(string phoneNumber, string digits);

    /// <summary>
    /// Resets bad attempts based on configuration.
    /// </summary>
    void ResetBadAttempts(DateTimeOffset currentDateTimeOffset);
}