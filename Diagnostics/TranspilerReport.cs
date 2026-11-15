using HarmonyLib;

using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ArenaOverhaul.Diagnostics;

// Writes what a transpiler could not find in the vanilla method to the log file.
// The found/missing members are always written; the full IL and the other transpilers only with "Logging for debugging".
internal static class TranspilerReport
{
    internal static void HooksNotFound(List<CodeInstruction> codes, int numberOfEdits, int requiredNumberOfEdits, MethodBase originalMethod, (string IndexName, int IndexValue)[] indexArgs, (string MemberInfoName, MemberInfo? MemberInfo)[] memberInfoArgs)
    {
        StringBuilder issueInfo = new();
        issueInfo.Append($"Transpiler for {originalMethod.DeclaringType?.Name}.{originalMethod.Name}");
        if (numberOfEdits < requiredNumberOfEdits)
        {
            issueInfo.Append(" was not able to make all required changes!");
        }
        if (indexArgs.Length > 0)
        {
            issueInfo.Append("\nIndexes:");
            foreach ((string IndexName, int IndexValue) indexInfo in indexArgs)
            {
                issueInfo.Append($"\n\t{indexInfo.IndexName}={indexInfo.IndexValue}");
            }
        }
        issueInfo.Append($"\nNumberOfEdits: {numberOfEdits} out of {requiredNumberOfEdits}");
        if (memberInfoArgs.Length > 0)
        {
            issueInfo.Append("\nMemberInfos:");
            foreach ((string MemberInfoName, MemberInfo? MemberInfo) memberInfo in memberInfoArgs)
            {
                issueInfo.Append($"\n\t{memberInfo.MemberInfoName}={(memberInfo.MemberInfo != null ? memberInfo.MemberInfo.ToString() : "not found")}");
            }
        }
        if (AorLog.IsDebugEnabled)
        {
            AppendILAndPatches(codes, issueInfo, originalMethod);
        }
        AorLog.Error(issueInfo.ToString());
    }

    internal static void HookNotApplied(MethodBase originalMethod, string what) =>
        AorLog.Error($"Transpiler for {originalMethod.DeclaringType?.Name}.{originalMethod.Name} could not find code hooks for {what}!");

    private static void AppendILAndPatches(List<CodeInstruction> codes, StringBuilder issueInfo, MethodBase originalMethod)
    {
        issueInfo.Append("\nIL:");
        for (int i = 0; i < codes.Count; ++i)
        {
            issueInfo.Append($"\n\t{i:D4}:\t{codes[i]}");
        }
        // info about the other transpilers on the original method
        HarmonyLib.Patches patches = Harmony.GetPatchInfo(originalMethod);
        if (patches != null && patches.Transpilers.Count > 0)
        {
            issueInfo.Append("\nOther transpilers:");
            foreach (Patch patch in patches.Transpilers)
            {
                issueInfo.Append($"\t\tPatching method: {patch.PatchMethod}\n\t\tOwner: {patch.PatchMethod.DeclaringType!.Assembly.GetName().Name} (HarmonyID: \"{patch.owner}\")\n\t\tPriority: {patch.priority}\n\t\tBefore: {patch.before}\n\t\tAfter: {patch.after}");
            }
        }
    }
}