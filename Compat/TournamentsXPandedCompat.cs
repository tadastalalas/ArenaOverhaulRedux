using ArenaOverhaul.Data;
using ArenaOverhaul.Diagnostics;

using HarmonyLib;

using System;
using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

using MathF = TaleWorlds.Library.MathF;

namespace ArenaOverhaul.Compat;

// AOR-FEATURE: Tournaments XPanded compatibility: its prize re-roll and prize selection are hidden for Grand and kingdom tournaments, its win bonuses are shown in the merged tournament results | gate: always (only when Tournaments XPanded is installed) | Postfix: reads results and arguments of its methods
internal static class TournamentsXPandedCompat
{
    private const string BehaviorTypeName = "TournamentsXPanded.Behaviors.TournamentsXPandedBehavior";
    private const string RerollConditionMethod = "RerollCondition";
    private const string PrizeSelectConditionMethod = "PrizeSelectCondition";
    private const string InitializeMethod = "Initialize";
    private const string GetTournamentGoldPrizeMethod = "GetTournamentGoldPrize";
    private const string GiveBonusRenownMethod = "GiveBonusRenown";
    private const string ShowAchievementQuickInformationMethod = "ShowAchievementQuickInformation";
    private const string AchievementGoldMember = "Gold";

    // Called once from SubModule, after PatchAll. Does nothing when Tournaments XPanded is not installed.
    internal static void TryApply(Harmony harmony)
    {
        if (harmony is null)
            return;

        Type? behaviorType = AccessTools.TypeByName(BehaviorTypeName);
        if (behaviorType != null)
        {
            PatchPostfix(harmony, behaviorType, RerollConditionMethod, nameof(HideForGrandTournamentPostfix));
            PatchPostfix(harmony, behaviorType, PrizeSelectConditionMethod, nameof(HideForGrandTournamentPostfix));
        }

        Type? managerType = TournamentsXPandedRewards.ManagerType;
        if (managerType != null)
        {
            PatchPostfix(harmony, managerType, InitializeMethod, nameof(InitializePostfix));
            PatchPostfix(harmony, managerType, GetTournamentGoldPrizeMethod, nameof(GoldPrizePostfix));
            PatchPostfix(harmony, managerType, GiveBonusRenownMethod, nameof(BonusRenownPostfix));
        }

        Type? missionControllerType = TournamentsXPandedRewards.MissionControllerType;
        if (missionControllerType != null)
        {
            PatchPostfix(harmony, missionControllerType, ShowAchievementQuickInformationMethod, nameof(AchievementGoldPostfix));
        }
    }

    private static void PatchPostfix(Harmony harmony, Type type, string methodName, string postfixName)
    {
        MethodInfo? target = AccessTools.Method(type, methodName);
        if (target is null)
        {
            AorLog.Warn("Tournaments XPanded compatibility: " + type.Name + "." + methodName + " was not found, that part is skipped.");
            return;
        }

        harmony.Patch(target, postfix: new HarmonyMethod(typeof(TournamentsXPandedCompat), postfixName));
        AorLog.Debug("Tournaments XPanded compatibility: patched " + type.Name + "." + methodName + ".");
    }

    // Grand and kingdom tournament prizes are fixed, so the re-roll and prize selection menu options are hidden for them.
    private static void HideForGrandTournamentPostfix(ref bool __result)
    {
        if (__result && Settlement.CurrentSettlement?.Town is Town town
                     && Campaign.Current?.TournamentManager?.GetTournamentGame(town) is GrandTournamentGame)
        {
            __result = false;
        }
    }

    private static void InitializePostfix() => TournamentsXPandedRewards.Reset();

    private static void GoldPrizePostfix(int __result) => TournamentsXPandedRewards.GoldPrize = __result;

    private static void BonusRenownPostfix(float renown) => TournamentsXPandedRewards.BonusRenown += MathF.Round(renown);

    private static void AchievementGoldPostfix(object achievements)
    {
        if (achievements is null)
            return;

        Traverse gold = Traverse.Create(achievements).Property(AchievementGoldMember);
        TournamentsXPandedRewards.AchievementGold += gold.PropertyExists() ? gold.GetValue<int>() : Traverse.Create(achievements).Field(AchievementGoldMember).GetValue<int>();
    }
}