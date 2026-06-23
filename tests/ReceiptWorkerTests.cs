using Xunit;
using IndustrialLogic;

namespace IndustrialLogic.Tests;

public class ReceiptWorkerTests
{
    // ── In-memory fake ────────────────────────────────────────────────────────

    private record DeliveredReceipt(string Email, string OrderId, decimal Total);

    private class InMemoryReceiptSender : IReceiptSender
    {
        private readonly List<DeliveredReceipt> _inbox = new();

        public void Send(string email, string orderId, decimal total) =>
            _inbox.Add(new DeliveredReceipt(email, orderId, total));

        public IReadOnlyList<DeliveredReceipt> InboxFor(string email) =>
            _inbox.Where(r => r.Email == email).ToList();

        public IReadOnlyList<DeliveredReceipt> AllDelivered => _inbox.AsReadOnly();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ReceiptRequested AnEvent(
        string email = "bob@example.com",
        string orderId = "order-1",
        decimal total = 99.00m) =>
        new(Guid.NewGuid().ToString(), email, orderId, total);

    // ── ProcessPending ────────────────────────────────────────────────────────

    [Fact]
    public void ProcessPending_EmptyOutbox_NoReceiptsDelivered()
    {
        var outbox = new Outbox();
        var sender = new InMemoryReceiptSender();

        new ReceiptWorker(outbox, sender).ProcessPending();

        Assert.Empty(sender.AllDelivered);
    }

    [Fact]
    public void ProcessPending_SingleEvent_ReceiptDeliveredToCorrectEmail()
    {
        var outbox = new Outbox();
        outbox.Append(AnEvent(email: "carol@example.com"));
        var sender = new InMemoryReceiptSender();

        new ReceiptWorker(outbox, sender).ProcessPending();

        Assert.Single(sender.InboxFor("carol@example.com"));
    }

    [Fact]
    public void ProcessPending_SingleEvent_DeliveredReceiptHasCorrectTotal()
    {
        var outbox = new Outbox();
        outbox.Append(AnEvent(total: 123.45m));
        var sender = new InMemoryReceiptSender();

        new ReceiptWorker(outbox, sender).ProcessPending();

        Assert.Equal(123.45m, sender.AllDelivered[0].Total);
    }

    [Fact]
    public void ProcessPending_SingleEvent_DeliveredReceiptHasCorrectOrderId()
    {
        var outbox = new Outbox();
        outbox.Append(AnEvent(orderId: "order-xyz"));
        var sender = new InMemoryReceiptSender();

        new ReceiptWorker(outbox, sender).ProcessPending();

        Assert.Equal("order-xyz", sender.AllDelivered[0].OrderId);
    }

    [Fact]
    public void ProcessPending_MultipleEvents_AllReceiptsDelivered()
    {
        var outbox = new Outbox();
        outbox.Append(AnEvent(email: "a@example.com", orderId: "order-1"));
        outbox.Append(AnEvent(email: "b@example.com", orderId: "order-2"));
        var sender = new InMemoryReceiptSender();

        new ReceiptWorker(outbox, sender).ProcessPending();

        Assert.Equal(2, sender.AllDelivered.Count);
    }

    [Fact]
    public void ProcessPending_AfterProcessing_OutboxIsEmpty()
    {
        var outbox = new Outbox();
        outbox.Append(AnEvent());
        var sender = new InMemoryReceiptSender();

        new ReceiptWorker(outbox, sender).ProcessPending();

        Assert.Empty(outbox.GetPending());
    }

    [Fact]
    public void ProcessPending_CalledTwice_DoesNotDeliverAlreadyProcessedEvents()
    {
        var outbox = new Outbox();
        outbox.Append(AnEvent());
        var sender = new InMemoryReceiptSender();
        var worker = new ReceiptWorker(outbox, sender);

        worker.ProcessPending();
        worker.ProcessPending();

        Assert.Single(sender.AllDelivered);
    }
}
