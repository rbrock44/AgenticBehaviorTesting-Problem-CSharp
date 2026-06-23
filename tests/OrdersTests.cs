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

public class OrdersTests
{
    [Fact]
    public void NewOrders_HasZeroCount()
    {
        var orders = new Orders();
        Assert.Equal(0, orders.OrderCount);
    }

    [Fact]
    public void NewOrders_GetOrdersReturnsEmptyList()
    {
        var orders = new Orders();
        Assert.Empty(orders.GetOrders());
    }

    [Fact]
    public void AddOrder_IncreasesOrderCount()
    {
        var orders = new Orders();
        orders.AddOrder(new Order("o1"));
        Assert.Equal(1, orders.OrderCount);
    }

    [Fact]
    public void AddOrder_OrderIsRetrievableViaGetOrders()
    {
        var orders = new Orders();
        var order = new Order("o1");
        orders.AddOrder(order);
        Assert.Contains(order, orders.GetOrders());
    }

    [Fact]
    public void GetOrder_RetrievesByIndex()
    {
        var orders = new Orders();
        var o1 = new Order("o1");
        var o2 = new Order("o2");
        orders.AddOrder(o1);
        orders.AddOrder(o2);

        Assert.Same(o1, orders.GetOrder(0));
        Assert.Same(o2, orders.GetOrder(1));
    }

    [Fact]
    public void MultipleOrders_PreserveInsertionOrder()
    {
        var orders = new Orders();
        orders.AddOrder(new Order("first"));
        orders.AddOrder(new Order("second"));
        orders.AddOrder(new Order("third"));

        var allOrders = orders.GetOrders();
        Assert.Equal("first", allOrders[0].OrderId);
        Assert.Equal("second", allOrders[1].OrderId);
        Assert.Equal("third", allOrders[2].OrderId);
    }

    [Fact]
    public void OrderCount_ReflectsAdditions()
    {
        var orders = new Orders();
        Assert.Equal(0, orders.OrderCount);
        orders.AddOrder(new Order("a"));
        Assert.Equal(1, orders.OrderCount);
        orders.AddOrder(new Order("b"));
        Assert.Equal(2, orders.OrderCount);
    }

    [Fact]
    public void GetOrders_ReturnsSameListReference()
    {
        var orders = new Orders();
        var ref1 = orders.GetOrders();
        var ref2 = orders.GetOrders();
        Assert.Same(ref1, ref2);
    }
}
