using ArenaOverhaul.Data;
using ArenaOverhaul.UI;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.CampaignSystem.ViewModelCollection.Map;

namespace ArenaOverhaul.Patches;

[HarmonyPatch(typeof(MapNotificationVM))]
public static class MapNotificationVMPatch
{
    private static readonly FieldInfo? ItemConstructorsField = AccessTools.Field(typeof(MapNotificationVM), "_itemConstructors");

    [HarmonyPostfix]
    [HarmonyPatch("PopulateTypeDictionary")]
    public static void PopulateTypeDictionaryPostfix(MapNotificationVM __instance)
    {
        if (__instance is null || ItemConstructorsField?.GetValue(__instance) is not Dictionary<Type, Type> constructors)
            return;

        constructors[typeof(GrandTournamentMapNotification)] = typeof(GrandTournamentMapNotificationVM);
    }
}