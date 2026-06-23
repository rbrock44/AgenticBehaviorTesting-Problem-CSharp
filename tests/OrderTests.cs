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

using Xunit;

namespace IndustrialLogic.Tests;

public class OrderTests
{
    private static Product CreateProduct(string id = "p1", string name = "Widget")
    {
        return new Product(id, name, Color.Red, 10.0m, ProductSize.Medium);
    }

    [Fact]
    public void Constructor_SetsOrderId()
    {
        var order = new Order("order-123");
        Assert.Equal("order-123", order.OrderId);
    }

    [Fact]
    public void NewOrder_HasZeroProducts()
    {
        var order = new Order("o1");
        Assert.Equal(0, order.ProductCount);
    }

    [Fact]
    public void NewOrder_GetProductsReturnsEmptyList()
    {
        var order = new Order("o1");
        Assert.Empty(order.GetProducts());
    }

    [Fact]
    public void AddProduct_IncreasesProductCount()
    {
        var order = new Order("o1");
        order.AddProduct(CreateProduct());
        Assert.Equal(1, order.ProductCount);
    }

    [Fact]
    public void AddProduct_ProductIsRetrievableViaGetProducts()
    {
        var order = new Order("o1");
        var product = CreateProduct("p1", "Fire Truck");
        order.AddProduct(product);

        var products = order.GetProducts();
        Assert.Single(products);
        Assert.Same(product, products[0]);
    }

    [Fact]
    public void GetProduct_RetrievesByIndex()
    {
        var order = new Order("o1");
        var p1 = CreateProduct("p1");
        var p2 = CreateProduct("p2");
        order.AddProduct(p1);
        order.AddProduct(p2);

        Assert.Same(p1, order.GetProduct(0));
        Assert.Same(p2, order.GetProduct(1));
    }

    [Fact]
    public void MultipleProducts_MaintainInsertionOrder()
    {
        var order = new Order("o1");
        var p1 = CreateProduct("p1", "First");
        var p2 = CreateProduct("p2", "Second");
        var p3 = CreateProduct("p3", "Third");

        order.AddProduct(p1);
        order.AddProduct(p2);
        order.AddProduct(p3);

        Assert.Equal(3, order.ProductCount);
        Assert.Equal("p1", order.GetProducts()[0].Id);
        Assert.Equal("p2", order.GetProducts()[1].Id);
        Assert.Equal("p3", order.GetProducts()[2].Id);
    }

    [Fact]
    public void GetProducts_ReturnsSameListReference()
    {
        var order = new Order("o1");
        var ref1 = order.GetProducts();
        var ref2 = order.GetProducts();
        Assert.Same(ref1, ref2);
    }
}
