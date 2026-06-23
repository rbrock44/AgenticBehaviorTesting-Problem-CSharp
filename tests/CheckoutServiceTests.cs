using Xunit;
using IndustrialLogic;

namespace IndustrialLogic.Tests;

public class CheckoutServiceTests
{
    // ── In-memory fakes ───────────────────────────────────────────────────────

    private class InMemoryInventory : IInventoryService
    {
        private readonly Exception? _toThrow;

        public InMemoryInventory(Exception? toThrow = null) => _toThrow = toThrow;

        public void Reserve(IReadOnlyList<CartItem> items)
        {
            if (_toThrow is not null) throw _toThrow;
        }
    }

    private class InMemoryPaymentGateway : IPaymentGateway
    {
        private readonly string _transactionId;
        private readonly Exception? _toThrow;

        public InMemoryPaymentGateway(string transactionId = "txn-ok", Exception? toThrow = null)
        {
            _transactionId = transactionId;
            _toThrow = toThrow;
        }

        public string Charge(Customer customer, decimal amount)
        {
            if (_toThrow is not null) throw _toThrow;
            return _transactionId;
        }
    }

    private class InMemoryOrderRepository : IOrderRepository
    {
        private readonly Dictionary<string, Order> _store = new();

        public void Save(Order order) => _store[order.OrderId] = order;
        public Order? FindById(string orderId) => _store.GetValueOrDefault(orderId);
        public bool IsEmpty => _store.Count == 0;
    }

    private record DeliveredReceipt(string Email, string OrderId, decimal Total);

    private class InMemoryReceiptSender : IReceiptSender
    {
        private readonly List<DeliveredReceipt> _inbox = new();

        public void Send(string email, string orderId, decimal total) =>
            _inbox.Add(new DeliveredReceipt(email, orderId, total));

        public IReadOnlyList<DeliveredReceipt> InboxFor(string email) =>
            _inbox.Where(r => r.Email == email).ToList();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Cart CartWith(string productId = "prod-1", int qty = 2, decimal price = 10.00m)
    {
        var cart = new Cart();
        cart.AddItem(productId, "Widget", qty, price);
        return cart;
    }

    private static Customer Alice() => new("Alice", "alice@example.com");

    private static (
        CheckoutService service,
        InMemoryOrderRepository repository,
        InMemoryReceiptSender receipts
    ) BuildWith(
        IInventoryService? inventory = null,
        IPaymentGateway? payment = null)
    {
        var repository = new InMemoryOrderRepository();
        var receipts = new InMemoryReceiptSender();
        var service = new CheckoutService(
            inventory ?? new InMemoryInventory(),
            payment   ?? new InMemoryPaymentGateway(),
            repository,
            receipts);
        return (service, repository, receipts);
    }

    // ── Successful checkout ───────────────────────────────────────────────────

    [Fact]
    public void PlaceOrder_ConfirmationTotalMatchesCartTotal()
    {
        var (service, _, _) = BuildWith();

        var confirmation = service.PlaceOrder(CartWith(qty: 3, price: 5.00m), Alice());

        Assert.Equal(15.00m, confirmation.Total);
    }

    [Fact]
    public void PlaceOrder_ConfirmationHasNonEmptyOrderId()
    {
        var (service, _, _) = BuildWith();

        var confirmation = service.PlaceOrder(CartWith(), Alice());

        Assert.False(string.IsNullOrWhiteSpace(confirmation.OrderId));
    }

    [Fact]
    public void PlaceOrder_ConfirmationTransactionIdMatchesGatewayResponse()
    {
        var (service, _, _) = BuildWith(payment: new InMemoryPaymentGateway("txn-abc"));

        var confirmation = service.PlaceOrder(CartWith(), Alice());

        Assert.Equal("txn-abc", confirmation.TransactionId);
    }

    [Fact]
    public void PlaceOrder_OrderCanBeRetrievedFromRepository()
    {
        var (service, repository, _) = BuildWith();

        var confirmation = service.PlaceOrder(CartWith(), Alice());

        Assert.NotNull(repository.FindById(confirmation.OrderId));
    }

    [Fact]
    public void PlaceOrder_PersistedOrderTotalMatchesCartTotal()
    {
        var (service, repository, _) = BuildWith();

        var confirmation = service.PlaceOrder(CartWith(qty: 2, price: 10.00m), Alice());
        var order = repository.FindById(confirmation.OrderId);

        Assert.Equal(20.00m, order!.Total);
    }

    [Fact]
    public void PlaceOrder_PersistedOrderCustomerEmailMatchesCustomer()
    {
        var (service, repository, _) = BuildWith();

        var confirmation = service.PlaceOrder(CartWith(), Alice());
        var order = repository.FindById(confirmation.OrderId);

        Assert.Equal("alice@example.com", order!.CustomerEmail);
    }

    [Fact]
    public void PlaceOrder_PersistedOrderItemsMatchCartItems()
    {
        var (service, repository, _) = BuildWith();
        var cart = CartWith(productId: "sku-99", qty: 4);

        var confirmation = service.PlaceOrder(cart, Alice());
        var order = repository.FindById(confirmation.OrderId);

        Assert.Single(order!.Items);
        Assert.Equal("sku-99", order.Items[0].ProductId);
        Assert.Equal(4, order.Items[0].Quantity);
    }

    [Fact]
    public void PlaceOrder_ReceiptIsDeliveredToCustomerEmail()
    {
        var (service, _, receipts) = BuildWith();

        service.PlaceOrder(CartWith(qty: 1, price: 50.00m), Alice());

        Assert.Single(receipts.InboxFor("alice@example.com"));
    }

    [Fact]
    public void PlaceOrder_DeliveredReceiptTotalMatchesCartTotal()
    {
        var (service, _, receipts) = BuildWith();

        service.PlaceOrder(CartWith(qty: 1, price: 50.00m), Alice());

        Assert.Equal(50.00m, receipts.InboxFor("alice@example.com")[0].Total);
    }

    [Fact]
    public void PlaceOrder_DeliveredReceiptOrderIdMatchesConfirmation()
    {
        var (service, _, receipts) = BuildWith();

        var confirmation = service.PlaceOrder(CartWith(), Alice());

        Assert.Equal(confirmation.OrderId, receipts.InboxFor("alice@example.com")[0].OrderId);
    }

    // ── Empty cart ────────────────────────────────────────────────────────────

    [Fact]
    public void PlaceOrder_EmptyCart_ThrowsInvalidOperationException()
    {
        var (service, _, _) = BuildWith();

        Assert.Throws<InvalidOperationException>(
            () => service.PlaceOrder(new Cart(), Alice()));
    }

    [Fact]
    public void PlaceOrder_EmptyCart_ExceptionMessageMentionsCart()
    {
        var (service, _, _) = BuildWith();

        var ex = Assert.Throws<InvalidOperationException>(
            () => service.PlaceOrder(new Cart(), Alice()));

        Assert.Contains("cart", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PlaceOrder_EmptyCart_NoOrderPersistedToRepository()
    {
        var (service, repository, _) = BuildWith();

        try { service.PlaceOrder(new Cart(), Alice()); } catch { }

        Assert.True(repository.IsEmpty);
    }

    // ── Payment failure ───────────────────────────────────────────────────────

    [Fact]
    public void PlaceOrder_PaymentFails_ExceptionPropagates()
    {
        var (service, _, _) = BuildWith(payment: new InMemoryPaymentGateway(toThrow: new Exception("Declined")));

        Assert.Throws<Exception>(() => service.PlaceOrder(CartWith(), Alice()));
    }

    [Fact]
    public void PlaceOrder_PaymentFails_NoOrderPersistedToRepository()
    {
        var (service, repository, _) = BuildWith(payment: new InMemoryPaymentGateway(toThrow: new Exception("Declined")));

        try { service.PlaceOrder(CartWith(), Alice()); } catch { }

        Assert.True(repository.IsEmpty);
    }

    [Fact]
    public void PlaceOrder_PaymentFails_NoReceiptDelivered()
    {
        var (service, _, receipts) = BuildWith(payment: new InMemoryPaymentGateway(toThrow: new Exception("Declined")));

        try { service.PlaceOrder(CartWith(), Alice()); } catch { }

        Assert.Empty(receipts.InboxFor("alice@example.com"));
    }

    // ── Inventory failure ─────────────────────────────────────────────────────

    [Fact]
    public void PlaceOrder_InventoryFails_ExceptionPropagates()
    {
        var (service, _, _) = BuildWith(inventory: new InMemoryInventory(toThrow: new Exception("Out of stock")));

        Assert.Throws<Exception>(() => service.PlaceOrder(CartWith(), Alice()));
    }

    [Fact]
    public void PlaceOrder_InventoryFails_NoOrderPersistedToRepository()
    {
        var (service, repository, _) = BuildWith(inventory: new InMemoryInventory(toThrow: new Exception("Out of stock")));

        try { service.PlaceOrder(CartWith(), Alice()); } catch { }

        Assert.True(repository.IsEmpty);
    }

    [Fact]
    public void PlaceOrder_InventoryFails_NoReceiptDelivered()
    {
        var (service, _, receipts) = BuildWith(inventory: new InMemoryInventory(toThrow: new Exception("Out of stock")));

        try { service.PlaceOrder(CartWith(), Alice()); } catch { }

        Assert.Empty(receipts.InboxFor("alice@example.com"));
    }
}
