using ArenaOverhaul.Core;

using System;

namespace ArenaOverhaul.Data;

// Arena practice settings of one clan hero.
// With MCM they are edited in the per-save "AO Hero Practice Settings" menu; without MCM every hero keeps these defaults.
internal sealed class HeroPracticeSettings
{
    public bool EnableLoadoutChoice { get; set; } = ModSettings.HeroDefaultEnableLoadoutChoice;
    public bool OnlyPriorityLoadouts { get; set; } = ModSettings.HeroDefaultOnlyPriorityLoadouts;
    public bool PrioritizeExpensiveEquipment { get; set; } = ModSettings.HeroDefaultPrioritizeExpensiveEquipment;

    // Set by the MCM menu, which keeps the chosen value in its own dropdown; read live. Not set: no priority.
    internal Func<WeaponPreference>? FirstPrioritySource { get; set; }
    internal Func<WeaponPreference>? SecondPrioritySource { get; set; }
    internal Func<WeaponPreference>? ThirdPrioritySource { get; set; }

    public WeaponPreference FirstPriorityWeapons => FirstPrioritySource?.Invoke() ?? WeaponPreference.None;
    public WeaponPreference SecondPriorityWeapons => SecondPrioritySource?.Invoke() ?? WeaponPreference.None;
    public WeaponPreference ThirdPriorityWeapons => ThirdPrioritySource?.Invoke() ?? WeaponPreference.None;
}