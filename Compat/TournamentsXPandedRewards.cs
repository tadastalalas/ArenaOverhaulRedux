using HarmonyLib;

using System;
using System.Linq;
using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;

namespace ArenaOverhaul.Compat;

// AOR-FEATURE: Tournaments XPanded compatibility: keeps its win bonuses (gold prize, bonus renown, bonus influence, achievement gold) so the tournament results screen can show them merged with this mod's rewards | gate: always (all zero when Tournaments XPanded is not installed)
internal static class TournamentsXPandedRewards
{
    private const string ManagerTypeName = "TournamentsXPanded.TXPManager";
    private const string SettingsTypeName = "TournamentsXPanded.TXPSettings";
    private const string SettingsTypeShortName = "TXPSettings";
    private const string MissionControllerTypeName = "TournamentsXPanded.Behaviors.TXPTournamentMissionController";
    private const string SettingsInstanceProperty = "Instance";
    private const string BonusInfluenceProperty = "BonusInfluence";
    private const string TryFinalizeMatchMethod = "TryFinalizeMatch";

    internal static readonly Type? ManagerType = AccessTools.TypeByName(ManagerTypeName);
    internal static readonly Type? MissionControllerType = AccessTools.TypeByName(MissionControllerTypeName);

    private static readonly Type? SettingsType = AccessTools.TypeByName(SettingsTypeName)
        ?? ManagerType?.Assembly.GetTypes().FirstOrDefault(t => t.Name == SettingsTypeShortName);
    private static readonly PropertyInfo? SettingsInstanceInfo = SettingsType is null ? null : AccessTools.Property(SettingsType, SettingsInstanceProperty);
    private static readonly PropertyInfo? BonusInfluenceInfo = SettingsType is null ? null : AccessTools.Property(SettingsType, BonusInfluenceProperty);
    private static readonly MethodInfo? TryFinalizeMatchInfo = MissionControllerType is null ? null : AccessTools.Method(MissionControllerType, TryFinalizeMatchMethod);

    internal static int GoldPrize { get; set; }
    internal static int BonusRenown { get; set; }
    internal static int AchievementGold { get; set; }

    // Mirrors the condition of TXPManager.GainInfluence.
    internal static int BonusInfluence =>
        Hero.MainHero?.Clan?.Kingdom != null && SettingsInstanceInfo?.GetValue(null) is object settings && BonusInfluenceInfo?.GetValue(settings) is int value ? value : 0;

    internal static void Reset() => (GoldPrize, BonusRenown, AchievementGold) = (0, 0, 0);

    // Tournaments XPanded settles the last match on a later tick / OnEndMission, after TournamentVM.OnTournamentEnd.
    // It is forced here so the final match counts; its own "applied" guard prevents double grants.
    internal static void FinalizePendingMatch()
    {
        if (TryFinalizeMatchInfo is null || Mission.Current is not Mission mission)
            return;

        foreach (MissionBehavior behavior in mission.MissionBehaviors)
        {
            if (behavior.GetType() == MissionControllerType)
            {
                TryFinalizeMatchInfo.Invoke(behavior, [true]);
                return;
            }
        }
    }
}