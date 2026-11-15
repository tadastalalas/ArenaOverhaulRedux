using ArenaOverhaul.Core;
using ArenaOverhaul.Data;
using ArenaOverhaul.Diagnostics;
using ArenaOverhaul.Managers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace ArenaOverhaul.Patches;

[HarmonyPatch(typeof(FightTournamentGame))]
public static class FightTournamentGamePatch
{
    private static readonly MethodInfo? miTournamentPrizeGetter = AccessTools2.PropertyGetter(typeof(TournamentGame), "Prize");
    private static readonly MethodInfo? miItemObjectNameGetter = AccessTools2.PropertyGetter(typeof(ItemObject), "Name");
    
    private static readonly MethodInfo? miShouldPrizeBeRerolled = AccessTools.Method(typeof(FightTournamentGamePatch), "ShouldPrizeBeRerolled");

    private static readonly MethodInfo? miGetPrizeItemName = AccessTools.Method(typeof(TournamentRewardManager), "GetPrizeItemName");

    private static readonly FightTournamentApplicantManager _applicantManager = new FightTournamentApplicantManager();

    private delegate void SortTournamentParticipantsDelegate(FightTournamentGame instance, MBList<CharacterObject> participantCharacters);
    private static readonly SortTournamentParticipantsDelegate? deSortTournamentParticipants =
        AccessTools2.GetDelegate<SortTournamentParticipantsDelegate>(typeof(FightTournamentGame), "SortTournamentParticipants");

    [HarmonyPrefix]
    [HarmonyPatch("GetParticipantCharacters")]
    public static bool GetParticipantCharactersPrefix(FightTournamentGame __instance, ref MBList<CharacterObject> __result, Settlement settlement, bool includePlayer = true)
    {
        if (__instance is null || settlement is null)
            return true;

        __result = _applicantManager.GetParticipantCharacters(__instance, settlement, includePlayer);
        deSortTournamentParticipants?.Invoke(__instance, __result);
        return false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch("GetTournamentPrize")]
    public static IEnumerable<CodeInstruction> GetTournamentPrizeTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
    {
        List<CodeInstruction> codes = new(instructions);
        int numberOfEdits = 0;
        int ldarg2Index = 0;
        int continueIndex = 0;
        for (int i = 2; i < codes.Count; ++i)
        {
            if (numberOfEdits == 0 && codes[i].opcode == OpCodes.Bne_Un_S && codes[i - 1].opcode == OpCodes.Ldloc_0 && codes[i - 2].opcode == OpCodes.Ldarg_2)
            {
                ldarg2Index = i - 2;
                continueIndex = i;
                ++numberOfEdits;
                break;
            }
        }

        //Logging
        const int RequiredNumberOfEdits = 1;
        if (ldarg2Index == 0 || continueIndex == 0 || numberOfEdits < RequiredNumberOfEdits || miShouldPrizeBeRerolled is null)
        {
            TranspilerReport.HooksNotFound(
                codes, numberOfEdits, RequiredNumberOfEdits, __originalMethod,
                [
                    (nameof(ldarg2Index), ldarg2Index),
                    (nameof(continueIndex), continueIndex),
                ],
                [
                    (nameof(miShouldPrizeBeRerolled), miShouldPrizeBeRerolled)
                ]);
        }
        if (ldarg2Index > 0 && continueIndex > 0)
        {
            codes[continueIndex].opcode = OpCodes.Brtrue;
            codes.InsertRange(continueIndex, [new CodeInstruction(opcode: OpCodes.Call, operand: miShouldPrizeBeRerolled)]);
        }
        else
        {
            AorLog.Error("Harmony transpiler for FightTournamentGame. GetTournamentPrize could not find code hooks for applying reroll settings!");
        }

        return codes.AsEnumerable();
    }

    [HarmonyTranspiler]
    [HarmonyPatch("GetMenuText")]
    public static IEnumerable<CodeInstruction> GetMenuTextTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
    {
        List<CodeInstruction> codes = new(instructions);
        int numberOfEdits = 0;
        int prizeItemNameStartIndex1 = 0;
        int prizeItemNameEndIndex1 = 0;
        int prizeItemNameStartIndex2 = 0;
        int prizeItemNameEndIndex2 = 0;
        if (miTournamentPrizeGetter != null && miItemObjectNameGetter != null)
        {
            for (int i = 2; i < codes.Count; ++i)
            {
                if (numberOfEdits == 0 && codes[i].LoadsConstant("TOURNAMENT_PRIZE") && codes[i + 1].opcode == OpCodes.Ldarg_0 && codes[i + 2].Calls(miTournamentPrizeGetter) && codes[i + 3].Calls(miItemObjectNameGetter))
                {
                    prizeItemNameStartIndex1 = i + 2;
                    prizeItemNameEndIndex1 = i + 3;
                    ++numberOfEdits;
                }
                else if (numberOfEdits == 1 && codes[i].LoadsConstant("TOURNAMENT_PRIZE") && codes[i + 1].opcode == OpCodes.Ldarg_0 && codes[i + 2].Calls(miTournamentPrizeGetter) && codes[i + 3].Calls(miItemObjectNameGetter))
                {
                    prizeItemNameStartIndex2 = i + 2;
                    prizeItemNameEndIndex2 = i + 3;
                    ++numberOfEdits;
                    break;
                }
            }
        }

        //Logging
        const int RequiredNumberOfEdits = 2;
        if (prizeItemNameStartIndex1 == 0 || prizeItemNameEndIndex1 == 0 || prizeItemNameStartIndex2 == 0 || prizeItemNameEndIndex2 == 0 || numberOfEdits < RequiredNumberOfEdits || miGetPrizeItemName is null)
        {
            TranspilerReport.HooksNotFound(
                codes, numberOfEdits, RequiredNumberOfEdits, __originalMethod,
                [
                    (nameof(prizeItemNameStartIndex1), prizeItemNameStartIndex1),
                    (nameof(prizeItemNameEndIndex1), prizeItemNameEndIndex1),
                    (nameof(prizeItemNameStartIndex2), prizeItemNameStartIndex2),
                    (nameof(prizeItemNameEndIndex2), prizeItemNameEndIndex2)
                ],
                [
                    (nameof(miTournamentPrizeGetter), miTournamentPrizeGetter),
                    (nameof(miItemObjectNameGetter), miItemObjectNameGetter),
                    (nameof(miGetPrizeItemName), miGetPrizeItemName)
                ]);
        }
        if (prizeItemNameStartIndex1 > 0 && prizeItemNameEndIndex1 > 0 && prizeItemNameStartIndex2 > 0 && prizeItemNameEndIndex2 > 0)
        {
            SetPrizeItemName(codes, prizeItemNameStartIndex2, prizeItemNameEndIndex2);
            SetPrizeItemName(codes, prizeItemNameStartIndex1, prizeItemNameEndIndex1);
        }
        else
        {
            AorLog.Error("Harmony transpiler for FightTournamentGame. GetMenuText could not find code hooks for stating correct prize item name!");
        }

        return codes.AsEnumerable();

        //local methods
        static void SetPrizeItemName(List<CodeInstruction> codes, int prizeItemNameStartIndex, int prizeItemNameEndIndex)
        {
            codes.RemoveRange(prizeItemNameStartIndex, prizeItemNameEndIndex - prizeItemNameStartIndex + 1);
            codes.InsertRange(prizeItemNameStartIndex, [new CodeInstruction(opcode: OpCodes.Call, operand: miGetPrizeItemName)]);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("GetTournamentPrize")]
    public static void GetTournamentPrizePostfix(FightTournamentGame __instance, ref ItemObject? __result)
    {
        if (!ModSettings.TournamentEnableHighQualityPrizes || TournamentRewardManager.CurrentPrizeHasRegisteredModifier(__instance.Town, __result))
        {
            return;
        }
        ItemModifier? itemModifier = TournamentRewardManager.GetRandomItemModifier(__result);
        TournamentRewardManager.RegisterPrizeModifier(__instance.Town, __result, itemModifier);
    }

    internal static bool ShouldPrizeBeRerolled(int lastRecordedNobleCountForTournamentPrize, int participantingNoblesCount) =>
        ModSettings.TournamentPrizeRerollCondition switch
        {
            PrizeRerollCondition.Never => false,
            PrizeRerollCondition.OnPrizeTierImprovement => lastRecordedNobleCountForTournamentPrize < 4 && participantingNoblesCount >= 4,
            PrizeRerollCondition.OnImprovement => lastRecordedNobleCountForTournamentPrize < participantingNoblesCount,
            PrizeRerollCondition.OnChange => lastRecordedNobleCountForTournamentPrize != participantingNoblesCount,
            _ => true,
        };
    
    [HarmonyPrefix]
    [HarmonyPriority(900)] // Prefix required: TournamentsXPanded prefixes this at 800 and skips the original, bypassing our transpiled reroll guard
    [HarmonyPatch("GetTournamentPrize")]
    public static bool GetTournamentPrizePrefix(FightTournamentGame __instance, ref ItemObject? __result, bool includePlayer, int lastRecordedLordCountForTournamentPrize)
    {
        if (__instance?.Town?.Settlement is null || __instance.Prize is null)
            return true;

        MBList<CharacterObject> participants = __instance.GetParticipantCharacters(__instance.Town.Settlement, includePlayer);
        int lordCount = 0;
        for (int i = 0; i < participants.Count; i++)
        {
            if (participants[i].IsHero && participants[i].HeroObject.IsLord)
                ++lordCount;
        }

        if (ShouldPrizeBeRerolled(lastRecordedLordCountForTournamentPrize, lordCount))
            return true;

        __result = __instance.Prize;
        return false;
    }
}