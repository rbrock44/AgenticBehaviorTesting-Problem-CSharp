namespace IndustrialLogic;

public class ReceiptWorker
{
    private readonly IOutbox _outbox;
    private readonly IReceiptSender _receipts;

    public ReceiptWorker(IOutbox outbox, IReceiptSender receipts)
    {
        _outbox = outbox;
        _receipts = receipts;
    }

    public void ProcessPending()
    {
        foreach (var @event in _outbox.GetPending().ToList())
        {
            _receipts.Send(@event.Email, @event.OrderId, @event.Total);
            _outbox.MarkProcessed(@event.EventId);
        }
    }
}
