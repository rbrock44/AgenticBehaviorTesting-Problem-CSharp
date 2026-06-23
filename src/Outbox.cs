namespace IndustrialLogic;

public class Outbox : IOutbox
{
    private readonly List<ReceiptRequested> _pending = new();

    public void Append(ReceiptRequested @event) => _pending.Add(@event);

    public IReadOnlyList<ReceiptRequested> GetPending() => _pending.AsReadOnly();

    public void MarkProcessed(string eventId) =>
        _pending.RemoveAll(e => e.EventId == eventId);
}
