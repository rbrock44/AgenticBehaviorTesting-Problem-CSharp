namespace IndustrialLogic;

public record ReceiptRequested(string EventId, string Email, string OrderId, decimal Total);
