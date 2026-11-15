using ArenaOverhaul.Data;
using ArenaOverhaul.Managers;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;

namespace ArenaOverhaul.Patches;

[HarmonyPatch(typeof(TournamentManager))]
public static class TournamentManagerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("GivePrizeToWinner")]
    public static bool ExecuteShowPrizeItemTooltipPrefix(TournamentManager __instance, TournamentGame tournament, Hero winner, bool isPlayerParticipated) //Bool prefixes compete with each other and skip others, as well as original, if return false
    {
        if (tournament is null || winner is null)
        {
            return true;
        }
        if (tournament is GrandTournamentGame grandTournament)
        {
            GrandTournamentPrizeManager.GiveWinnerRewards(grandTournament, winner);
            GrandTournamentNotifier.NotifyResolved(grandTournament, winner);
        }
        
        if (!isPlayerParticipated)
        {
            tournament.UpdateTournamentPrize(isPlayerParticipated);
        }

        EquipmentElement prizeEquipmentElement = TournamentRewardManager.TryGetPrizeItemModifier(tournament, out PrizeItemInfo? prizeItemInfo)
            ? new(prizeItemInfo!.ItemObject, prizeItemInfo.ItemModifier)
            : new(tournament.Prize);

        if (winner.PartyBelongedTo == MobileParty.MainParty)
        {
            winner.PartyBelongedTo.ItemRoster.AddToCounts(prizeEquipmentElement, 1);
        }
        else
        {
            if (winner.Clan == null)
            {
                return false;
            }
            GiveGoldAction.ApplyBetweenCharacters(null, winner.Clan.Leader, tournament.Town.MarketData.GetPrice(prizeEquipmentElement));
        }

        return false;
    }
    
    //Prefix: vanilla simulates, adds the leaderboard entry and pays the prize before any postfix could intervene.
    [HarmonyPrefix]
    [HarmonyPatch("ResolveTournament")]
    public static bool ResolveTournamentPrefix(TournamentGame tournament, Town town)
    {
        if (tournament is not GrandTournamentGame game || town is null || (game.IsOpen && !GrandTournamentScheduler.HasLostVenue(game)))
            return true;

        GrandTournamentScheduler.Cancel(game);
        return false;
    }
}