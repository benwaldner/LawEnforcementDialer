namespace LawEnforcementDialer.Persistence;

public interface IPinRepository
{
    /// <summary>
    /// Get the pin based on the phone number
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    Task<string?> GetPinAsync(string number);

    /// <summary>
    /// Check if the phone number exists.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    Task<bool> NumberExistsAsync(string number);
}