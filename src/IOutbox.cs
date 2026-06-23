namespace IndustrialLogic;

public interface IOutbox
{
    void Append(ReceiptRequested @event);
    IReadOnlyList<ReceiptRequested> GetPending();
    void MarkProcessed(string eventId);
}
