using ArenaOverhaul.Core;

using System.ComponentModel;

namespace ArenaOverhaul.Data;

public enum WeaponPreference : byte
{
    [Description(Texts.Ids.WeaponPreferenceNone)]
    None = 0,
    [Description(Texts.Ids.WeaponPreferenceOneHanded)]
    OneHanded = 1,
    [Description(Texts.Ids.WeaponPreferenceTwoHanded)]
    TwoHanded = 2,
    [Description(Texts.Ids.WeaponPreferencePolearm)]
    Polearm = 3,
    [Description(Texts.Ids.WeaponPreferenceBow)]
    Bow = 4,
    [Description(Texts.Ids.WeaponPreferenceCrossbow)]
    Crossbow = 5,
    [Description(Texts.Ids.WeaponPreferenceThrowing)]
    Throwing = 6,
}