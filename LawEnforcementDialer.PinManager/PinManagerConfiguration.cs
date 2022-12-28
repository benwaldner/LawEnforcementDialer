namespace LawEnforcementDialer.PinManager;

public sealed class PinManagerConfiguration
{
    public const string Name = nameof(PinManagerConfiguration);

    public int PinMaxAttempts { get; set; }

    public double PinLockoutDurationInMinutes { get; set; } = 1;

    public double PinMonitorSleepInMinutes { get; set; } = 1;

    public PinPrompts Prompts { get; set; }
}