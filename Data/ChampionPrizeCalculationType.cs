using ArenaOverhaul.Core;

using System.ComponentModel;

namespace ArenaOverhaul.Data;

public enum ChampionPrizeCalculationType : byte
{
    [Description(Texts.Ids.ChampionPrizeCalculationTypeStandard)] Standard = 0,
    [Description(Texts.Ids.ChampionPrizeCalculationTypeAdditive)] Additive = 1,
    [Description(Texts.Ids.ChampionPrizeCalculationTypeMultiplicative)] Multiplicative = 2
}