using ArenaOverhaul.Core;

using System.ComponentModel;

namespace ArenaOverhaul.Data;

//Values intentionally match CampaignTime.Seasons.
public enum GrandTournamentSeason : byte
{
    [Description(Texts.Ids.GrandTournamentSeasonSpring)] Spring = 0,
    [Description(Texts.Ids.GrandTournamentSeasonSummer)] Summer = 1,
    [Description(Texts.Ids.GrandTournamentSeasonAutumn)] Autumn = 2,
    [Description(Texts.Ids.GrandTournamentSeasonWinter)] Winter = 3
}