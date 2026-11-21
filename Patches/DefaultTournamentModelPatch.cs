using ArenaOverhaul.Core;
using ArenaOverhaul.Diagnostics;

using HarmonyLib;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using TaleWorlds.CampaignSystem.GameComponents;

namespace ArenaOverhaul.Patches;

[HarmonyPatch(typeof(DefaultTournamentModel))]
public static class DefaultTournamentModelPatch
{
    private static readonly MethodInfo miGetRenownReward = AccessTools.Method(typeof(DefaultTournamentModelPatch), "GetRenownReward");

    [HarmonyTranspiler]
    [HarmonyPatch("GetRenownReward")]
    public static IEnumerable<CodeInstruction> GetRenownRewardTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
    {
        List<CodeInstruction> codes = new(instructions);
        int numberOfEdits = 0;
        if (miGetRenownReward != null)
        {
            for (int i = 0; i < codes.Count - 1; ++i)
            {
                if (codes[i].LoadsConstant(3f) && codes[i + 1].opcode == OpCodes.Stloc_0)
                {
                    codes[i] = new CodeInstruction(opcode: OpCodes.Call, operand: miGetRenownReward);
                    ++numberOfEdits;
                    break;
                }
            }
        }

        //Logging
        const int RequiredNumberOfEdits = 1;
        if (numberOfEdits < RequiredNumberOfEdits)
        {
            TranspilerReport.HooksNotFound(
                codes, numberOfEdits, RequiredNumberOfEdits, __originalMethod, [],
                [
                    (nameof(miGetRenownReward), miGetRenownReward),
                ]);
        }

        return codes.AsEnumerable();
    }

    /* service methods */
    internal static float GetRenownReward()
    {
        return ModSettings.TournamentBaseRenownReward;
    }
}