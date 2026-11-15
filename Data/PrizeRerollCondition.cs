using ArenaOverhaul.Core;

using System.ComponentModel;

namespace ArenaOverhaul.Data;

public enum PrizeRerollCondition : byte
{
    [Description(Texts.Ids.OptionNever)] Never = 0,
    [Description(Texts.Ids.PrizeRerollConditionOnPrizeTierImprovement)] OnPrizeTierImprovement = 1,
    [Description(Texts.Ids.PrizeRerollConditionOnImprovement)] OnImprovement = 2,
    [Description(Texts.Ids.PrizeRerollConditionOnChange)] OnChange = 3
}