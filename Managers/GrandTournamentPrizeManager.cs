using ArenaOverhaul.Core;
using ArenaOverhaul.Data;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ArenaOverhaul.Managers;

internal static class GrandTournamentPrizeManager
{
    //Culture-strict first, culture-neutral fallback. Never a mount or horse harness; always an item that can carry a modifier.
    internal static ItemObject? PickPrizeItem(CultureObject? culture)
    {
        MBReadOnlyList<ItemObject> all = Items.All;
        List<ItemObject> candidates = new();
        for (int i = 0; i < all.Count; i++)
        {
            if (IsCandidate(all[i], culture))
            {
                candidates.Add(all[i]);
            }
        }
        if (candidates.Count == 0)
        {
            return culture is null ? null : PickPrizeItem(null);
        }
        return candidates[MBRandom.RandomInt(candidates.Count)];
    }

    //Best modifier the item's group offers - "Legendary" for weapons, the top armor prefix for armor.
    internal static ItemModifier? GetBestModifier(ItemObject item)
    {
        ItemModifierGroup? group = item.ItemComponent?.ItemModifierGroup;
        if (group is null)
        {
            return null;
        }
        for (ItemQuality quality = ItemQuality.Legendary; quality > ItemQuality.Common; --quality)
        {
            List<ItemModifier> modifiers = group.GetModifiersBasedOnQuality(quality);
            if (modifiers.Count > 0)
            {
                return modifiers[0];
            }
        }
        return null;
    }

    private static bool IsCandidate(ItemObject item, CultureObject? culture) =>
        (culture is null ? item.Culture is null || !item.Culture.CanHaveSettlement : item.Culture == culture)
        && item.Tier is ItemObject.ItemTiers.Tier5 or ItemObject.ItemTiers.Tier6
        && !item.IsMountable
        && item.ItemType != ItemObject.ItemTypeEnum.HorseHarness
        && (item.IsCraftedWeapon || item.ArmorComponent != null)
        && !ItemHelper.IsRuntimeCrafted(item)
        && item.ItemComponent?.ItemModifierGroup != null
        && !item.StringId.EndsWith("_blunt")
        && !item.StringId.StartsWith("practice_")
        && !item.StringId.StartsWith("dummy_")
        && !item.StringId.StartsWith("peasant_");
    
            //Bonus on top of the regular TournamentRewardManager rewards. Called from the GivePrizeToWinner prefix,
    //which both the simulated and the played path hit right before TournamentFinished.
    internal static void GiveWinnerRewards(GrandTournamentGame game, Hero winner)
    {
        float scale = GetRewardScale(game.IsKingdomTournament);

        int gold = (int) (ModSettings.GrandTournamentGoldPrize * scale);
        if (gold > 0)
            GiveGoldAction.ApplyBetweenCharacters(null, winner, gold);

        int renown = (int) (ModSettings.GrandTournamentRenownReward * scale);
        if (renown > 0 && winner.Clan != null)
            GainRenownAction.Apply(winner, renown);

        int influence = (int) (ModSettings.GrandTournamentInfluenceReward * scale);
        if (influence > 0 && winner.Clan?.Kingdom != null)
            GainKingdomInfluenceAction.ApplyForDefault(winner, influence);

        //Vanilla adds one entry itself in both paths; weight N means N entries in total.
        int weight = game.IsKingdomTournament ? ModSettings.KingdomTournamentLeaderboardWeight : ModSettings.GrandTournamentLeaderboardWeight;
        for (int i = 1; i < weight; i++)
            Campaign.Current.TournamentManager.AddLeaderboardEntry(winner);

        AnnounceWinner(game, winner);
    }

    private static void AnnounceWinner(GrandTournamentGame game, Hero winner)
    {
        TextObject message = new(game.IsKingdomTournament
            ? Texts.Ids.KingdomTournamentWon
            : Texts.Ids.GrandTournamentWon);
        message.SetTextVariable("WINNER", winner.Name);
        message.SetTextVariable("TOWN", game.Town.Name);
        if (game.HostKingdom != null)
        {
            message.SetTextVariable("KINGDOM", game.HostKingdom.Name);
        }
        if (ModSettings.GrandTournamentResultPopup && (winner == Hero.MainHero || !game.IsKingdomTournament))
            MessageHelper.QuickInformationMessage(message);
        MessageHelper.SimpleMessage(message);
    }
    
    internal static float GetRewardScale(bool isKingdomTournament) =>
        isKingdomTournament ? ModSettings.KingdomTournamentRewardPercent / 100f : 1f;
}