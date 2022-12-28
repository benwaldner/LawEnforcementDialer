namespace LawEnforcementDialer.CallManager;

public class ActiveCall
{
    public string Id { get; set; }

    public string Source { get; set; }

    public string Target { get; set; }

    public List<string> Participants { get; set; } = new();

    public string CorrelationId { get; set; }
}