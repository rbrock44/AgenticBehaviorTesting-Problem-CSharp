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

public class DiscountCalculatorTests
{
    private static Product CreateProduct(decimal price, decimal discount = 0)
    {
        return new Product("p1", "Test", Color.Red, price, ProductSize.Medium, discount);
    }

    [Fact]
    public void ApplyDiscount_TenPercentDiscount()
    {
        var product = CreateProduct(100.0m, 0.1m);
        Assert.Equal(90.0m, DiscountCalculator.ApplyDiscount(product));
    }

    [Fact]
    public void ApplyDiscount_TwentyFivePercentDiscount()
    {
        var product = CreateProduct(200.0m, 0.25m);
        Assert.Equal(150.0m, DiscountCalculator.ApplyDiscount(product));
    }

    [Fact]
    public void ApplyDiscount_FiftyPercentDiscount()
    {
        var product = CreateProduct(80.0m, 0.5m);
        Assert.Equal(40.0m, DiscountCalculator.ApplyDiscount(product));
    }

    [Fact]
    public void ApplyDiscount_ZeroDiscount_ReturnsFullPrice()
    {
        var product = CreateProduct(45.0m, 0m);
        Assert.Equal(45.0m, DiscountCalculator.ApplyDiscount(product));
    }

    [Fact]
    public void ApplyDiscount_NegativeDiscount_ReturnsFullPrice()
    {
        var product = CreateProduct(45.0m, -0.1m);
        Assert.Equal(45.0m, DiscountCalculator.ApplyDiscount(product));
    }

    [Fact]
    public void ApplyDiscount_HundredPercent_ReturnsZero()
    {
        var product = CreateProduct(99.99m, 1.0m);
        Assert.Equal(0m, DiscountCalculator.ApplyDiscount(product));
    }

    [Fact]
    public void ApplyDiscount_OverHundredPercent_ReturnsZero()
    {
        var product = CreateProduct(99.99m, 1.5m);
        Assert.Equal(0m, DiscountCalculator.ApplyDiscount(product));
    }

    [Fact]
    public void ApplyDiscount_SmallPrice()
    {
        var product = CreateProduct(0.99m, 0.1m);
        Assert.Equal(0.891m, DiscountCalculator.ApplyDiscount(product));
    }

    [Fact]
    public void ApplyDiscount_LargePrice()
    {
        var product = CreateProduct(9999.99m, 0.2m);
        Assert.Equal(7999.992m, DiscountCalculator.ApplyDiscount(product));
    }

    [Fact]
    public void FormatDiscount_TenPercent()
    {
        Assert.Equal("10%", DiscountCalculator.FormatDiscount(0.1m));
    }

    [Fact]
    public void FormatDiscount_TwentyFivePercent()
    {
        Assert.Equal("25%", DiscountCalculator.FormatDiscount(0.25m));
    }

    [Fact]
    public void FormatDiscount_FiftyPercent()
    {
        Assert.Equal("50%", DiscountCalculator.FormatDiscount(0.5m));
    }

    [Fact]
    public void FormatDiscount_HundredPercent()
    {
        Assert.Equal("100%", DiscountCalculator.FormatDiscount(1.0m));
    }

    [Fact]
    public void FormatDiscount_FivePercent()
    {
        Assert.Equal("5%", DiscountCalculator.FormatDiscount(0.05m));
    }

    [Fact]
    public void FormatDiscount_ThirtyThreePercent()
    {
        Assert.Equal("33%", DiscountCalculator.FormatDiscount(0.333m));
    }
}
