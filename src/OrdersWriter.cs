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

using System.Text;

namespace IndustrialLogic;

public class OrdersWriter
{
    private readonly Orders _orders;

    public OrdersWriter(Orders orders)
    {
        _orders = orders;
    }

    public string GetContents()
    {
        var contents = new StringBuilder();
        contents.Append("{\"orders\": [");
        var orderList = _orders.GetOrders();
        for (int i = 0; i < orderList.Count; i++)
        {
            var order = orderList[i];
            if (i > 0) contents.Append(", ");
            contents.Append("{\"id\": \"" + order.OrderId + "\", ");
            contents.Append("\"products\": [");
            var products = order.GetProducts();
            for (int j = 0; j < products.Count; j++)
            {
                var product = products[j];
                if (j > 0) contents.Append(", ");
                contents.Append("{\"id\": \"" + product.Id + "\"");
                contents.Append(", \"color\": \"" + GetColorFor(product) + "\"");
                if (product.Size != ProductSize.NotApplicable)
                {
                    contents.Append(", \"size\": \"" + GetSizeFor(product) + "\"");
                }
                contents.Append(", \"price\": ");
                var finalPrice = product.Discount > 0
                    ? DiscountCalculator.ApplyDiscount(product)
                    : product.Price;
                contents.Append("{\"currency\": \"" + GetCurrencyFor() + "\", ");
                contents.Append("\"amount\": " + finalPrice + "}");
                if (product.Discount > 0)
                {
                    contents.Append(", \"discount\": \"" + DiscountCalculator.FormatDiscount(product.Discount) + "\"");
                }
                contents.Append(", \"name\": \"" + product.Name + "\"");
                contents.Append("}");
            }
            contents.Append("]}");
        }
        contents.Append("]}");
        return contents.ToString();
    }

    private string GetCurrencyFor()
    {
        return "USD";
    }

    private string GetColorFor(Product product)
    {
        if (product.Color == Color.Red) return "red";
        if (product.Color == Color.Pink) return "pink";
        if (product.Color == Color.White) return "white";
        if (product.Color == Color.Yellow) return "yellow";
        return "";
    }

    private string GetSizeFor(Product product)
    {
        if (product.Size == ProductSize.Small) return "small";
        if (product.Size == ProductSize.Medium) return "medium";
        if (product.Size == ProductSize.Large) return "large";
        return "";
    }
}
