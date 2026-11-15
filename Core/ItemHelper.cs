using System;

using TaleWorlds.Core;

namespace ArenaOverhaul.Core;

public static class ItemHelper
{
    //Ids from vanilla CraftingCampaignBehavior.GetNextCraftedItemId (smithing) and GetNextTownOrderId (smithing orders).
    //Order items never get IsCraftedByPlayer, and crafted items can outlive their save, so check the id too.
    private const string CraftedItemIdPrefix = "crafted_item_";
    private const string TownOrderIdPrefix = "town_order_";

    public static bool IsRuntimeCrafted(ItemObject? item) =>
        item?.StringId is string id
        && (item.IsCraftedByPlayer
            || id.StartsWith(CraftedItemIdPrefix, StringComparison.Ordinal)
            || id.StartsWith(TownOrderIdPrefix, StringComparison.Ordinal));
}