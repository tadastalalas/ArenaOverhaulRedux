using ArenaOverhaul.Core;
using ArenaOverhaul.Managers;

using HarmonyLib;

using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;

namespace ArenaOverhaul.Patches;

[HarmonyPatch(typeof(DefaultCombatXpModel))]
public static class DefaultCombatXpModelPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("GetXpfMultiplierForMissionType")]
    public static void GetXpfMultiplierForMissionTypePostfix(ref float __result, CombatXpModel.MissionTypeEnum missionType)
    {
        if (missionType == CombatXpModel.MissionTypeEnum.Tournament)
            __result = ModSettings.TournamentExperienceRate;
        else if (missionType == CombatXpModel.MissionTypeEnum.PracticeFight && AOArenaBehaviorManager.Instance is { } manager)
            __result = manager.GetPracticeExperienceRate();
    }
}