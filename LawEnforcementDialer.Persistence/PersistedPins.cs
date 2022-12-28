namespace LawEnforcementDialer.Persistence;

public class PersistedPins
{
    public const string Name = "Pins";

    public Dictionary<string, string> Values { get; set; } = new();
}