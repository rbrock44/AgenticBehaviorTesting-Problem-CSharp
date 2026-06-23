// ***************************************************************************
// Copyright (c) 2026, Industrial Logic, Inc., All Rights Reserved.
//
// This code is the exclusive property of Industrial Logic, Inc. It may ONLY be
// used by students during Industrial Logic's workshops or by individuals
// who are being coached by Industrial Logic on a project.
//
// This code may NOT be copied or used for any other purpose without the prior
// written consent of Industrial Logic, Inc.
// ****************************************************************************

namespace IndustrialLogic;

public class CheckoutService
{
    private readonly IInventoryService _inventory;
    private readonly IPaymentGateway _payment;
    private readonly IOrderRepository _repository;
    private readonly IReceiptSender _receipts;

    public CheckoutService(
        IInventoryService inventory,
        IPaymentGateway payment,
        IOrderRepository repository,
        IReceiptSender receipts)
    {
        _inventory = inventory;
        _payment = payment;
        _repository = repository;
        _receipts = receipts;
    }

    public OrderConfirmation PlaceOrder(Cart cart, Customer customer)
    {
        if (cart.IsEmpty)
            throw new InvalidOperationException("Cart is empty");

        var items = cart.GetItems();
        var total = cart.GetTotal();

        _inventory.Reserve(items);
        var transactionId = _payment.Charge(customer, total);

        var order = new Order(Guid.NewGuid().ToString(), customer.Email, items, total);
        _repository.Save(order);

        _receipts.Send(customer.Email, order.OrderId, order.Total);

        return new OrderConfirmation(order.OrderId, order.Total, transactionId);
    }
}
