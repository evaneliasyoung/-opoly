using System;
using System.Collections.Generic;
using UnityEngine;

public class Bank : MonoBehaviour
{
    public Byte Houses { get; set; } = 32;
    public Byte Hotels { get; set; } = 12;
    public Dictionary<Byte, Street> Streets { get; } = new Dictionary<byte, Street>();
    public Dictionary<Byte, Property> Properties { get; } = new Dictionary<byte, Property>();

    private void Start()
    {
        // Streets
        Streets.Add(1, new Street(1, 60));
        Streets.Add(3, new Street(3, 60));
        Streets.Add(6, new Street(6, 100));
        Streets.Add(8, new Street(8, 100));
        Streets.Add(9, new Street(9, 120));
        Streets.Add(11, new Street(11, 140));
        Streets.Add(13, new Street(13, 140));
        Streets.Add(14, new Street(14, 160));
        Streets.Add(16, new Street(16, 180));
        Streets.Add(18, new Street(18, 180));
        Streets.Add(19, new Street(19, 200));
        Streets.Add(21, new Street(21, 220));
        Streets.Add(23, new Street(23, 220));
        Streets.Add(24, new Street(24, 240));
        Streets.Add(26, new Street(26, 260));
        Streets.Add(27, new Street(27, 260));
        Streets.Add(29, new Street(29, 280));
        Streets.Add(31, new Street(31, 300));
        Streets.Add(32, new Street(32, 300));
        Streets.Add(34, new Street(34, 320));
        Streets.Add(37, new Street(37, 350));
        Streets.Add(39, new Street(39, 400));

        // Railroads
        Properties.Add(5, new Property(5, 200));
        Properties.Add(15, new Property(15, 200));
        Properties.Add(25, new Property(25, 200));
        Properties.Add(35, new Property(35, 200));

        // Utilities
        Properties.Add(12, new Property(12, 150));
        Properties.Add(28, new Property(28, 150));
    }

    public Street GetStreetByIndex(Byte Index)
    {
        return Streets[Index];
    }

    public Property GetRailOrUtilityByIndex(Byte Index)
    {
        return Properties[Index];
    }

    public Property GetPropertyByIndex(Byte Index)
    {
        if (Streets.ContainsKey(Index))
        {
            return (Property)(GetStreetByIndex(Index));
        }
        else if (Properties.ContainsKey(Index))
        {
            return GetRailOrUtilityByIndex(Index);
        }
        else
        {
            throw new IndexOutOfRangeException("Index must be in [0, 39]");
        }
    }

    public Byte? GetPropertyOwnerByIndex(Byte Index)
    {
        return GetPropertyByIndex(Index).Owner;
    }

    public UInt16 GetPropertyCostByIndex(Byte Index)
    {
        return GetPropertyByIndex(Index).Price;
    }

    public bool CanOwnerPurchaseProperty(IPropertyOwner Owner, IProperty Property)
    {
        return Property.IsForSale && Owner.LiquidAssets >= Property.Price;
    }

    public bool CanOwnerPurchaseProperty(IPropertyOwner Owner, Byte Index)
    {
        return CanOwnerPurchaseProperty(Owner, GetPropertyByIndex(Index));
    }

    /// <summary>
    /// Processes an asset holder pruchasing a property.
    /// </summary>
    /// <param name="Owner">The asset holder purchasing a property.</param>
    /// <param name="Property">The property being purchased.</param>
    /// <returns>True if the purchase went through, false otherwise.</returns>
    bool PurchaseProperty(ref IPropertyOwner Owner, ref Property Property)
    {
        if (CanOwnerPurchaseProperty(Owner, Property))
        {
            Owner.LiquidAssets -= Convert.ToInt16(Property.Price);
            Property.Owner = Owner.Index;
            return true;
        }
        return false;
    }

    public bool PurchaseProperty(ref IPropertyOwner Owner, Byte Index)
    {
        var Property = Streets.ContainsKey(Index) ? Streets[Index] : Properties[Index];
        return PurchaseProperty(ref Owner, ref Property);
    }

    /// <summary>
    /// Processes an asset holder mortgaging a property.
    /// </summary>
    /// <param name="Owner">The asset holder mortgaging a property.</param>
    /// <param name="Property">The property being mortgaged.</param>
    /// <returns>True if the mortgage went through, false otherwise.</returns>
    bool MortgageProperty(ref IPropertyOwner Owner, ref IProperty Property)
    {
        if (Property.Owner == Owner.Index && !Property.IsMortgaged && (Property.PropertyType != PropertyTileType.Street || (Property.PropertyType == PropertyTileType.Street && ((IStreet)Property).Housing == 0)))
        {
            Owner.LiquidAssets += Property.MortgageValue;
            Property.IsMortgaged = true;
            return true;
        }
        return false;
    }

    public bool MortgageProperty(ref IPropertyOwner Owner, Byte Index)
    {
        var Property = Streets.ContainsKey(Index) ? Streets[Index] : Properties[Index];
        return MortgageProperty(ref Owner, ref Property);
    }

    /// <summary>
    /// Processes an asset holder mortgaging a property.
    /// </summary>
    /// <param name="Owner">The asset holder mortgaging a property.</param>
    /// <param name="Property">The property being mortgaged.</param>
    /// <returns>True if the mortgage went through, false otherwise.</returns>
    bool UnmortgageProperty(ref IPropertyOwner Owner, ref IProperty Property)
    {
        if (Property.Owner == Owner.Index && Property.IsMortgaged && Owner.LiquidAssets >= Property.UnmortgageCost)
        {
            Owner.LiquidAssets -= Property.UnmortgageCost;
            Property.IsMortgaged = false;
            return true;
        }
        return false;
    }

    public bool UnmortgageProperty(ref IPropertyOwner Owner, Byte Index)
    {
        var Property = Streets.ContainsKey(Index) ? Streets[Index] : Properties[Index];
        return UnmortgageProperty(ref Owner, ref Property);
    }

    /// <summary>
    /// Processes an asset holder building on a property.
    /// </summary>
    /// <param name="Owner">The asset holder building on a property.</param>
    /// <param name="Street">The property being built upon.</param>
    /// <returns>True if the building went through, false otherwise.</returns>
    bool BuildResidence(ref IPropertyOwner Owner, ref IStreet Street)
    {
        if (Street.Owner == Owner.Index && !Street.IsMortgaged && Street.Housing < 5 && Owner.LiquidAssets >= Street.BuildCost)
        {
            if (Street.Houses == 4 && Hotels > 0)
            {
                Hotels--;
            }
            else if (Street.Houses < 4 && Houses > 0)
            {
                Houses--;
            }
            else
            {
                return false;
            }
            Owner.LiquidAssets -= Street.BuildCost;
            ++Street.Housing;
            return true;
        }
        return false;
    }

    public bool BuildResidence(ref IPropertyOwner Owner, Byte Index)
    {
        var Property = Streets.ContainsKey(Index) ? Streets[Index] : Properties[Index];
        return BuildResidence(ref Owner, ref Property);
    }

    /// <summary>
    /// Processes an asset holder demolishing a residence.
    /// </summary>
    /// <param name="Owner">The asset holder demolishing a residence.</param>
    /// <param name="Street">The property being with the residence to be demolished.</param>
    /// <returns>True if the demolishing a residence went through, false otherwise.</returns>
    bool DemolishResidence(ref IPropertyOwner Owner, ref IStreet Street)
    {
        if (Street.Owner == Owner.Index && !Street.IsMortgaged && Street.Housing >= 1)
        {
            Owner.LiquidAssets += Convert.ToInt16(Street.BuildCost / 2);
            if (Street.Hotels == 1)
            {
                ++Hotels;
            }
            else
            {
                ++Houses;
            }
            --Street.Housing;
            return true;
        }
        return false;
    }

    public bool DemolishResidence(ref IPropertyOwner Owner, Byte Index)
    {
        var Property = Streets.ContainsKey(Index) ? Streets[Index] : Properties[Index];
        return DemolishResidence(ref Owner, ref Property);
    }
}
