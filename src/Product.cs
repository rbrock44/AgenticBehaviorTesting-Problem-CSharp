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

public class Product
{
    private readonly string _id;
    private readonly string _name;
    private readonly Color _color;
    private readonly decimal _price;
    private readonly ProductSize _size;
    private decimal _discount;

    public Product(string id, string name, Color color, decimal price, ProductSize size, decimal discount = 0)
    {
        _id = id;
        _name = name;
        _color = color;
        _price = price;
        _size = size;
        _discount = discount;
    }

    public string Id => _id;
    public string Name => _name;
    public Color Color => _color;
    public decimal Price => _price;
    public ProductSize Size => _size;
    public decimal Discount
    {
        get => _discount;
        set => _discount = value;
    }
}
