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

public class Order
{
    private readonly string _id;
    private readonly List<Product> _products = new();

    public Order(string orderId)
    {
        _id = orderId;
    }

    public void AddProduct(Product product)
    {
        _products.Add(product);
    }

    public string OrderId => _id;
    public int ProductCount => _products.Count;

    public Product GetProduct(int index) => _products[index];

    public List<Product> GetProducts() => _products;
}
