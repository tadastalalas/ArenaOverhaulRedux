using ArenaOverhaul.Core;

using System.ComponentModel;

namespace ArenaOverhaul.Data;

public enum CultureRestrictedPrizesMode : byte
{
    [Description(Texts.Ids.OptionNever)] Never = 0,
    [Description(Texts.Ids.CultureRestrictedPrizesModeStandardItemsOnly)] StandardItemsOnly = 1,
    [Description(Texts.Ids.CultureRestrictedPrizesModeAlwaysExceptForMounts)] AlwaysExceptForMounts = 2,
    [Description(Texts.Ids.CultureRestrictedPrizesModeAlways)] Always = 3
}