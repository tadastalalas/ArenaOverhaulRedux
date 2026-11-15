using System.Collections.Generic;
using System.Linq;

using TaleWorlds.Core;

namespace ArenaOverhaul.Data;

public class TeamTournamentRound
{
    public IEnumerable<TeamTournamentMatch> Matches { get => _matches; }
    public int MatchCount { get => _matches.Count; }
    public int CurrentMatchIndex { get; private set; }
    public TeamTournamentMatch? CurrentMatch => (CurrentMatchIndex >= _matches.Count) ? null : _matches[CurrentMatchIndex];
    public IEnumerable<TeamTournamentTeam>? Teams => Matches?.SelectMany(x => x.Teams);

    public TeamTournamentRound(int teamsInRound, int numberOfMatches, int numerOfWinnerTeams)
    {
        CurrentMatchIndex = 0;
        _matches = new List<TeamTournamentMatch>(numberOfMatches);
        for (int i = 0; i < numberOfMatches; i++)
            _matches.Add(new TeamTournamentMatch(teamsInRound / numberOfMatches, numerOfWinnerTeams));
    }

    public int AddTeam(TeamTournamentTeam team)
    {
        int matchNum = -1;
        for (int tryNum = 0; tryNum < 64 && matchNum < 0; tryNum++)
        {
            int candidate = MBRandom.RandomInt(_matches.Count);
            if (!_matches[candidate].IsFullMatch)
                matchNum = candidate;
        }
        if (matchNum < 0)
            matchNum = _matches.FindIndex(x => !x.IsFullMatch);
        if (matchNum < 0)
            matchNum = _matches.IndexOf(_matches.OrderBy(x => x.Teams.Count()).First()); // bracket overflow – least populated match

        _matches[matchNum].AddTeam(team);
        return matchNum;
    }

    public void EndMatch()
    {
        CurrentMatch?.End();
        CurrentMatchIndex++;
    }

    private List<TeamTournamentMatch> _matches;
}