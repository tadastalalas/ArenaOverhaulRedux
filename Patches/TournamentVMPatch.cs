using ArenaOverhaul.Compat;
using ArenaOverhaul.Core;
using ArenaOverhaul.Data;
using ArenaOverhaul.Diagnostics;
using ArenaOverhaul.Managers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using SandBox.ViewModelCollection;
using SandBox.ViewModelCollection.Tournament;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ArenaOverhaul.Patches;

[HarmonyPatch(typeof(TournamentVM))]
public static class TournamentVMPatch
{
    private static readonly MethodInfo? miTournamentGetter = AccessTools2.PropertyGetter(typeof(TournamentVM), "Tournament");
    private static readonly MethodInfo? miTournamentPrizeGetter = AccessTools2.PropertyGetter(typeof(TournamentGame), "Prize");
    private static readonly MethodInfo? miTextObjectToString = AccessTools2.Method(typeof(object), "ToString");

    private static readonly MethodInfo? miGetFullPrizeName = AccessTools2.DeclaredMethod(typeof(TournamentVMPatch), "GetFullPrizeName");

    [HarmonyTranspiler]
    [HarmonyPatch("OnTournamentEnd")]
    public static IEnumerable<CodeInstruction> OnTournamentEndTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
    {
        List<CodeInstruction> codes = new(instructions);
        int numberOfEdits = 0;
        int getPrizeIndex = 0;
        int toStringIndex = 0;
        if (miTournamentGetter != null && miTournamentPrizeGetter != null && miTextObjectToString != null)
        {
            for (int i = 2; i < codes.Count; ++i)
            {
                if (numberOfEdits == 0 && codes[i].Calls(miTournamentGetter) && codes[i + 2].Calls(miTournamentPrizeGetter) && codes[i + 4].Calls(miTextObjectToString) && codes[i + 6].opcode == OpCodes.Ldstr && codes[i + 6].operand.ToString() == "REWARD")
                {
                    getPrizeIndex = i + 2;
                    toStringIndex = i + 5;
                    ++numberOfEdits;
                    break;
                }
            }
        }

        //Logging
        const int RequiredNumberOfEdits = 1;
        if (getPrizeIndex == 0 || toStringIndex == 0 || numberOfEdits < RequiredNumberOfEdits || miGetFullPrizeName is null)
        {
            TranspilerReport.HooksNotFound(
                codes, numberOfEdits, RequiredNumberOfEdits, __originalMethod,
                [
                    (nameof(getPrizeIndex), getPrizeIndex),
                    (nameof(toStringIndex), toStringIndex),
                ],
                [
                    (nameof(miTournamentGetter), miTournamentGetter),
                    (nameof(miTournamentPrizeGetter), miTournamentPrizeGetter),
                    (nameof(miTextObjectToString), miTextObjectToString),
                    (nameof(miGetFullPrizeName), miGetFullPrizeName)
                ]);
        }

        if (getPrizeIndex > 0 && toStringIndex > 0)
        {
            codes.RemoveRange(getPrizeIndex, toStringIndex - getPrizeIndex);
            codes.InsertRange(getPrizeIndex, [new CodeInstruction(opcode: OpCodes.Call, operand: miGetFullPrizeName)]);
        }
        else
        {
            AorLog.Error("Harmony transpiler for TournamentVM. OnTournamentEnd could not find code hooks for applying prize quality name!");
        }

        return codes.AsEnumerable();
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)] //run after other mods' postfixes so their lines can be folded in
    [HarmonyPatch("OnTournamentEnd")]
    public static void OnTournamentEndPostfix(TournamentVM __instance)
    {
        if (__instance?.Tournament?.Winner is null || __instance.Tournament.TournamentGame?.Town is not Town tournamentTown || __instance.BattleRewards is null)
            return;

        TournamentGame game = __instance.Tournament.TournamentGame;
        bool winnerIsPlayer = __instance.Tournament.Winner.IsPlayer;
        if (winnerIsPlayer && game is GrandTournamentGame grandGame)
        {
            //TOURNAMENT_FINAL_OPPONENT is still set from vanilla's str_tournament_result_won.
            TextObject intro = new(grandGame.IsKingdomTournament
                ? Texts.Ids.PlayerWonKingdomTournament
                : Texts.Ids.PlayerWonGrandTournament);
            intro.SetTextVariable("KINGDOM", grandGame.HostKingdom?.Name ?? TextObject.GetEmpty());
            __instance.WinnerIntro = intro.ToString();
        }
        int takedownRenown = TournamentRewardManager.GetTakedownRenownReward(Hero.MainHero, tournamentTown);

        int roundWinnings = 0;
        List<(Hero Participant, int Winnings)> roundPrizes = TournamentRewardManager.RoundPrizeWinners[tournamentTown];
        for (int i = 0; i < roundPrizes.Count; i++)
            if (roundPrizes[i].Participant?.IsHumanPlayerCharacter == true) { roundWinnings = roundPrizes[i].Winnings; break; }

        int renown = winnerIsPlayer ? (int) game.TournamentWinRenown : takedownRenown; //TournamentWinRenown already includes takedown renown
        int influence = winnerIsPlayer ? (int) game.TournamentWinInfluence + TournamentRewardManager.GetInfluenceReward(Hero.MainHero) : 0;
        int gold = (winnerIsPlayer ? TournamentRewardManager.GetTournamentGoldPrize(tournamentTown) : 0) + roundWinnings;

        if (winnerIsPlayer && game is GrandTournamentGame grand)
        {
            float scale = GrandTournamentPrizeManager.GetRewardScale(grand.IsKingdomTournament);
            gold += (int) (ModSettings.GrandTournamentGoldPrize * scale);
            renown += (int) (ModSettings.GrandTournamentRenownReward * scale);
            influence += Hero.MainHero.Clan?.Kingdom != null ? (int) (ModSettings.GrandTournamentInfluenceReward * scale) : 0;
        }
        
        TournamentsXPandedRewards.FinalizePendingMatch();
        if (winnerIsPlayer)
        {
            gold += TournamentsXPandedRewards.GoldPrize;
            influence += TournamentsXPandedRewards.BonusInfluence;
        }
        gold += TournamentsXPandedRewards.AchievementGold;   //earned per match, also when the player lost
        renown += TournamentsXPandedRewards.BonusRenown;
        TournamentsXPandedRewards.Reset();

        //Drop vanilla's separate renown/influence lines; they are re-added below as merged totals.
        RemoveVanillaLine(__instance.BattleRewards, "str_tournament_renown", "RENOWN", game.TournamentWinRenown.ToString("F1"));
        RemoveVanillaLine(__instance.BattleRewards, "str_tournament_influence", "INFLUENCE", game.TournamentWinInfluence.ToString("F1"));

        if (renown > 0)
        {
            GameTexts.SetVariable("RENOWN", renown.ToString());
            __instance.BattleRewards.Insert(0, new TournamentRewardVM(GameTexts.FindText("str_tournament_renown").ToString()));
        }
        if (influence > 0)
        {
            TextObject text = GameTexts.FindText("str_tournament_influence");
            text.SetTextVariable("INFLUENCE", influence.ToString());
            text.SetTextVariable("INFLUENCE_ICON", Texts.Ids.InfluenceIcon);
            __instance.BattleRewards.Insert(renown > 0 ? 1 : 0, new TournamentRewardVM(text.ToString()));
        }
        if (gold > 0)
        {
            GameTexts.SetVariable("TOTAL_GOLD_REWARD", gold.ToString());
            __instance.BattleRewards.Add(new TournamentRewardVM(GameTexts.FindText("str_tournament_gold_reward", "3").ToString()));
        }
    }

    private static void RemoveVanillaLine(MBBindingList<TournamentRewardVM> rewards, string textId, string varName, string varValue)
    {
        GameTexts.SetVariable(varName, varValue);
        string expected = GameTexts.FindText(textId).ToString();
        for (int i = rewards.Count - 1; i >= 0; i--)
            if (rewards[i].Text == expected) { rewards.RemoveAt(i); return; }
    }

    [HarmonyPrefix]
    [HarmonyPatch("ExecuteShowPrizeItemTooltip")]
    public static bool ExecuteShowPrizeItemTooltipPrefix(TournamentVM __instance) //Bool prefixes compete with each other and skip others, as well as original, if return false
    {
        if (!__instance.HasPrizeItem)
        {
            return false;
        }
        TournamentRewardManager.ShowPrizeItemHint(__instance.Tournament.TournamentGame);
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("RefreshValues")]
    public static void RefreshValuesPostfix(TournamentVM __instance)
    {
        if (__instance?.Tournament?.TournamentGame is GrandTournamentGame grand)
            __instance.TournamentTitle = GetGrandTitle(grand).ToString();
        
        if (!__instance.HasPrizeItem)
            return;

        if (TournamentRewardManager.TryGetPrizeItemModifier(__instance.Tournament.TournamentGame, out PrizeItemInfo? prizeItemInfo))
        {
            EquipmentElement equipmentElement = new EquipmentElement(prizeItemInfo!.ItemObject, prizeItemInfo.ItemModifier);
            __instance.PrizeItemName = equipmentElement.GetModifiedItemName().ToString();
        }
    }

    internal static string GetFullPrizeName(TournamentGame tournamentGame)
    {
        if (!TournamentRewardManager.TournamentPrizeAwards.TryGetValue(tournamentGame.Town, out PrizeItemInfo? prizeItemInfo) || prizeItemInfo is null || tournamentGame.Prize.StringId != prizeItemInfo.ItemObject.StringId || prizeItemInfo.ItemModifier is null)
        {
            return tournamentGame.Prize.Name.ToString();
        }
        return new EquipmentElement(prizeItemInfo.ItemObject, prizeItemInfo.ItemModifier).GetModifiedItemName().ToString();
    }
    
    private static TextObject GetGrandTitle(GrandTournamentGame grand)
    {
        TextObject title = new(grand.IsKingdomTournament
            ? Texts.Ids.KingdomTournamentTitle
            : Texts.Ids.GrandTournamentTitle);
        title.SetTextVariable("KINGDOM", grand.HostKingdom?.Name ?? TextObject.GetEmpty());
        return title;
    }
}