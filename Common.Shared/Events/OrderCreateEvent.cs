namespace Common.Shared.Events;

public record OrderCreateEvent
{
    public string OrderCode { get; set; }
}