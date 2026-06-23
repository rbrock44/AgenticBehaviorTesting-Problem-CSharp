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

public class Cart
{
    private readonly List<CartItem> _items = new();

    public void AddItem(string productId, string name, int quantity, decimal unitPrice)
    {
        _items.Add(new CartItem(productId, name, quantity, unitPrice));
    }

    public IReadOnlyList<CartItem> GetItems() => _items.AsReadOnly();

    public decimal GetTotal() => _items.Sum(item => item.Quantity * item.UnitPrice);

    public bool IsEmpty => _items.Count == 0;
}
