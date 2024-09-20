﻿using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Stalker.Shop.Prototypes;
/// <summary>
/// Stalker shop preset prototype
/// </summary>
[Prototype("shopPreset")]
public sealed class ShopPresetPrototype : IPrototype
{
    [ViewVariables] [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    /// List of all categories of the shop
    /// </summary>
    [DataField, ViewVariables]
    public List<CategoryInfo> Categories = new();

    [DataField]
    public Dictionary<int, List<CategoryInfo>> SponsorCategories = new();

    [DataField]
    public List<CategoryInfo> ContributorCategories = new();

    /// <summary>
    /// Special field, where you can specify custom price for item when selling
    /// </summary>
    [DataField("itemsForSale")]
    public Dictionary<string, int> SellingItems = new();
}
[DataDefinition, Serializable, NetSerializable]
public sealed partial class CategoryInfo
{
    // TODO: Add sponsor category check
    // TODO: Add sponsors...

    /// <summary>
    /// Name of category, this will be displayed in shop window
    /// </summary>
    [DataField]
    public string Name = string.Empty;

    /// <summary>
    /// Items for each category
    /// </summary>
    [DataField]
    public Dictionary<string, int> Items = new();

    /// <summary>
    /// Probably shouldn't touch this, this used in code to get ListingData
    /// </summary>
    public List<ListingData> ListingItems = new();

    /// <summary>
    /// Priority to figure out a position for category
    /// </summary>
    [DataField]
    public int Priority;
}

