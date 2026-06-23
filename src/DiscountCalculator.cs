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

public static class DiscountCalculator
{
    public static decimal ApplyDiscount(Product product)
    {
        var price = product.Price;
        var discount = product.Discount;
        if (discount <= 0) return price;
        if (discount >= 1.0m) return 0;
        return price * (1 - discount);
    }

    public static string FormatDiscount(decimal discount)
    {
        return $"{discount * 100:0}%";
    }
}
