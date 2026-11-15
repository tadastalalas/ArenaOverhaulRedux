using System;

namespace ArenaOverhaul.Core;

// The arena practice kinds. Flags, so dialog lines can be offered for several modes at once.
[Flags]
public enum ArenaPracticeMode
{
    Standard = 1,
    Expansive = 2,
    Parry = 4,
    Team = 8,

    ExceptParry = Standard | Expansive | Team,
    All = Standard | Expansive | Parry | Team,
}
