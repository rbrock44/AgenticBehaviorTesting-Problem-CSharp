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

using System.Text.Json;
using Xunit;

namespace IndustrialLogic.Tests;

public class OrdersWriterTests
{
    private static Product CreateProduct(
        string id = "P001",
        string name = "Widget",
        Color color = Color.Red,
        decimal price = 10.0m,
        ProductSize size = ProductSize.NotApplicable,
        decimal discount = 0)
    {
        return new Product(id, name, color, price, size, discount);
    }

    private static Orders CreateOrderWithProducts(string orderId, params Product[] products)
    {
        var orders = new Orders();
        var order = new Order(orderId);
        foreach (var p in products) order.AddProduct(p);
        orders.AddOrder(order);
        return orders;
    }

    [Fact]
    public void GetContents_ReturnsValidJson()
    {
        var orders = new Orders();
        var writer = new OrdersWriter(orders);
        Assert.NotNull(JsonDocument.Parse(writer.GetContents()));
    }

    [Fact]
    public void GetContents_EmptyOrders_ProducesEmptyArray()
    {
        var orders = new Orders();
        var writer = new OrdersWriter(orders);
        var result = JsonDocument.Parse(writer.GetContents());
        Assert.Equal(0, result.RootElement.GetProperty("orders").GetArrayLength());
    }

    [Fact]
    public void GetContents_EmptyOrders_ExactString()
    {
        var orders = new Orders();
        var writer = new OrdersWriter(orders);
        Assert.Equal("{\"orders\": []}", writer.GetContents());
    }

    [Fact]
    public void GetContents_IncludesOrderId()
    {
        var orders = CreateOrderWithProducts("order-42", CreateProduct());
        var writer = new OrdersWriter(orders);
        var result = JsonDocument.Parse(writer.GetContents());
        Assert.Equal("order-42", result.RootElement.GetProperty("orders")[0].GetProperty("id").GetString());
    }

    [Fact]
    public void GetContents_IncludesProductDetails()
    {
        var product = CreateProduct(id: "prod-1", name: "Fire Truck", color: Color.Red, price: 8.95m, size: ProductSize.Medium);
        var orders = CreateOrderWithProducts("o1", product);
        var writer = new OrdersWriter(orders);
        var result = JsonDocument.Parse(writer.GetContents());
        var p = result.RootElement.GetProperty("orders")[0].GetProperty("products")[0];

        Assert.Equal("prod-1", p.GetProperty("id").GetString());
        Assert.Equal("Fire Truck", p.GetProperty("name").GetString());
        Assert.Equal("red", p.GetProperty("color").GetString());
        Assert.Equal("medium", p.GetProperty("size").GetString());
        Assert.Equal(8.95m, p.GetProperty("price").GetProperty("amount").GetDecimal());
        Assert.Equal("USD", p.GetProperty("price").GetProperty("currency").GetString());
    }

    [Fact]
    public void GetContents_MapsAllColors()
    {
        var orders = new Orders();
        var order = new Order("o1");
        order.AddProduct(CreateProduct(id: "p1", color: Color.Red));
        order.AddProduct(CreateProduct(id: "p2", color: Color.Pink));
        order.AddProduct(CreateProduct(id: "p3", color: Color.White));
        order.AddProduct(CreateProduct(id: "p4", color: Color.Yellow));
        orders.AddOrder(order);

        var writer = new OrdersWriter(orders);
        var result = JsonDocument.Parse(writer.GetContents());
        var products = result.RootElement.GetProperty("orders")[0].GetProperty("products");

        Assert.Equal("red", products[0].GetProperty("color").GetString());
        Assert.Equal("pink", products[1].GetProperty("color").GetString());
        Assert.Equal("white", products[2].GetProperty("color").GetString());
        Assert.Equal("yellow", products[3].GetProperty("color").GetString());
    }

    [Fact]
    public void GetContents_OmitsSizeWhenNotApplicable()
    {
        var product = CreateProduct(size: ProductSize.NotApplicable);
        var orders = CreateOrderWithProducts("o1", product);
        var writer = new OrdersWriter(orders);
        var result = JsonDocument.Parse(writer.GetContents());
        var p = result.RootElement.GetProperty("orders")[0].GetProperty("products")[0];

        Assert.False(p.TryGetProperty("size", out _));
    }

    [Fact]
    public void GetContents_IncludesSizeWhenApplicable()
    {
        var product = CreateProduct(size: ProductSize.Large);
        var orders = CreateOrderWithProducts("o1", product);
        var writer = new OrdersWriter(orders);
        var result = JsonDocument.Parse(writer.GetContents());

        Assert.Equal("large", result.RootElement.GetProperty("orders")[0].GetProperty("products")[0].GetProperty("size").GetString());
    }

    [Fact]
    public void GetContents_AppliesDiscountToPrice()
    {
        var product = CreateProduct(price: 100.0m, discount: 0.2m);
        var orders = CreateOrderWithProducts("o1", product);
        var writer = new OrdersWriter(orders);
        var result = JsonDocument.Parse(writer.GetContents());

        Assert.Equal(80.0m, result.RootElement.GetProperty("orders")[0].GetProperty("products")[0].GetProperty("price").GetProperty("amount").GetDecimal());
    }

    [Fact]
    public void GetContents_IncludesDiscountLabel()
    {
        var product = CreateProduct(price: 100.0m, discount: 0.25m);
        var orders = CreateOrderWithProducts("o1", product);
        var writer = new OrdersWriter(orders);
        var result = JsonDocument.Parse(writer.GetContents());

        Assert.Equal("25%", result.RootElement.GetProperty("orders")[0].GetProperty("products")[0].GetProperty("discount").GetString());
    }

    [Fact]
    public void GetContents_NoDiscountField_WhenFullPrice()
    {
        var product = CreateProduct(price: 50.0m);
        var orders = CreateOrderWithProducts("o1", product);
        var writer = new OrdersWriter(orders);
        var result = JsonDocument.Parse(writer.GetContents());
        var p = result.RootElement.GetProperty("orders")[0].GetProperty("products")[0];

        Assert.False(p.TryGetProperty("discount", out _));
    }

    [Fact]
    public void GetContents_MultipleOrders()
    {
        var orders = new Orders();
        var o1 = new Order("o1");
        o1.AddProduct(CreateProduct());
        var o2 = new Order("o2");
        o2.AddProduct(CreateProduct());
        orders.AddOrder(o1);
        orders.AddOrder(o2);

        var writer = new OrdersWriter(orders);
        var result = JsonDocument.Parse(writer.GetContents());

        Assert.Equal(2, result.RootElement.GetProperty("orders").GetArrayLength());
        Assert.Equal("o1", result.RootElement.GetProperty("orders")[0].GetProperty("id").GetString());
        Assert.Equal("o2", result.RootElement.GetProperty("orders")[1].GetProperty("id").GetString());
    }

    [Fact]
    public void GetContents_MultipleProducts()
    {
        var p1 = CreateProduct(id: "p1");
        var p2 = CreateProduct(id: "p2");
        var orders = CreateOrderWithProducts("o1", p1, p2);
        var writer = new OrdersWriter(orders);
        var result = JsonDocument.Parse(writer.GetContents());
        var products = result.RootElement.GetProperty("orders")[0].GetProperty("products");

        Assert.Equal(2, products.GetArrayLength());
        Assert.Equal("p1", products[0].GetProperty("id").GetString());
        Assert.Equal("p2", products[1].GetProperty("id").GetString());
    }

    [Fact]
    public void GetContents_UsesCurrencyUsd()
    {
        var product = CreateProduct();
        var orders = CreateOrderWithProducts("o1", product);
        var writer = new OrdersWriter(orders);
        var result = JsonDocument.Parse(writer.GetContents());

        Assert.Equal("USD", result.RootElement.GetProperty("orders")[0].GetProperty("products")[0].GetProperty("price").GetProperty("currency").GetString());
    }

    [Fact]
    public void GetContents_StartsWithOrdersKey()
    {
        var orders = new Orders();
        var writer = new OrdersWriter(orders);
        Assert.StartsWith("{\"orders\": [", writer.GetContents());
    }

    [Fact]
    public void GetContents_EndsWithClosingBrackets()
    {
        var orders = new Orders();
        var writer = new OrdersWriter(orders);
        Assert.EndsWith("]}", writer.GetContents());
    }

    [Fact]
    public void GetContents_ProductFieldsAppearInCorrectOrder()
    {
        var product = CreateProduct(id: "p1", name: "Toy", color: Color.Red, price: 5.0m, size: ProductSize.Small);
        var orders = CreateOrderWithProducts("o1", product);
        var writer = new OrdersWriter(orders);
        var contents = writer.GetContents();

        var idIndex = contents.IndexOf("\"id\": \"p1\"");
        var colorIndex = contents.IndexOf("\"color\": \"red\"");
        var sizeIndex = contents.IndexOf("\"size\": \"small\"");
        var priceIndex = contents.IndexOf("\"price\": {");
        var nameIndex = contents.IndexOf("\"name\": \"Toy\"");

        Assert.True(idIndex < colorIndex, "id should appear before color");
        Assert.True(colorIndex < sizeIndex, "color should appear before size");
        Assert.True(sizeIndex < priceIndex, "size should appear before price");
        Assert.True(priceIndex < nameIndex, "price should appear before name");
    }

    [Fact]
    public void GetContents_MultipleOrders_SeparatedByCommaSpace()
    {
        var orders = new Orders();
        var o1 = new Order("o1");
        o1.AddProduct(CreateProduct());
        var o2 = new Order("o2");
        o2.AddProduct(CreateProduct());
        orders.AddOrder(o1);
        orders.AddOrder(o2);

        var writer = new OrdersWriter(orders);
        Assert.Contains("]}, {\"id\":", writer.GetContents());
    }

    [Fact]
    public void GetContents_DiscountedProduct_DiscountAppearsAfterPrice()
    {
        var product = CreateProduct(price: 100.0m, discount: 0.2m);
        var orders = CreateOrderWithProducts("o1", product);
        var writer = new OrdersWriter(orders);
        var contents = writer.GetContents();

        var priceIndex = contents.IndexOf("\"amount\":");
        var discountIndex = contents.IndexOf("\"discount\":");
        Assert.True(priceIndex < discountIndex, "price should appear before discount");
    }

    [Fact]
    public void GetContents_UsesSpaceAfterColonInOrders()
    {
        var orders = new Orders();
        var writer = new OrdersWriter(orders);
        Assert.Contains("\"orders\": [", writer.GetContents());
    }

    [Fact]
    public void GetContents_ProductContainsExactCurrencyFormat()
    {
        var product = CreateProduct(price: 10.0m);
        var orders = CreateOrderWithProducts("o1", product);
        var writer = new OrdersWriter(orders);
        Assert.Contains("\"currency\": \"USD\"", writer.GetContents());
    }
}
