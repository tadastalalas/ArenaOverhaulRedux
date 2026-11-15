using ArenaOverhaul.Core;
using ArenaOverhaul.Managers;

using HarmonyLib;

using SandBox.View.Missions;

using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace ArenaOverhaul.Patches;

[HarmonyPatch(typeof(SandBoxMissionViews), "OpenArenaStartMission")]
internal static class ArenaPracticeFightMissionViewsPatch
{
    public static void Postfix(ref MissionView[] __result, Mission mission)
    {
        if (mission is null || __result is null || (AOArenaBehaviorManager.Instance?.PracticeMode ?? ArenaPracticeMode.Standard) != ArenaPracticeMode.Team)
            return;

        __result = [.. __result, ViewCreator.CreateMissionAgentLabelUIHandler(mission)];
    }
}