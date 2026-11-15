using ArenaOverhaul.Core;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.Core;

using static TaleWorlds.CampaignSystem.TournamentGames.TournamentMatch;

namespace ArenaOverhaul.Data;

public class TeamTournamentMatch
{
    private enum TeamIndex
    {
        First = 0,
        Second = 1,
        Third = 2,
        Fourth = 3,
    }

    public IEnumerable<TeamTournamentTeam> Teams { get => _teams; }
    public MatchState State { get; private set; }
    public bool IsReady => State == MatchState.Ready;
    public bool IsFinished => State == MatchState.Finished;

    public IEnumerable<TeamTournamentTeam> Winners => GetWinners();
    
    private readonly int _teamCount;

    public TeamTournamentMatch(int teamCount, int winnerTeamsPerMatch)
    {
        _teamCount = teamCount;
        _winnerTeamsPerMatch = winnerTeamsPerMatch;
        _teams = new List<TeamTournamentTeam>(teamCount);
        State = MatchState.Ready;
    }

    public void AddTeam(TeamTournamentTeam team)
    {
        if (!team.IsPlayerTeam)
        {
            int colorId = GetColorIndex((TeamIndex) (_teams.Count % 4));
            team.TeamColor = BannerManager.GetColor(colorId);
            team.TeamBanner = Banner.CreateOneColoredEmptyBanner(colorId);
        }

        _teams.Add(team);
    }

    private int GetColorIndex(TeamIndex teamIndex)
    {
        int colorIndex;
        switch (teamIndex)
        {
            case TeamIndex.First:
                colorIndex = ModSettings.TeamTournamentTeamOneColor;
                break;
            case TeamIndex.Second:
                colorIndex = ModSettings.TeamTournamentTeamTwoColor;
                break;
            case TeamIndex.Third:
                colorIndex = ModSettings.TeamTournamentTeamThreeColor;
                break;
            case TeamIndex.Fourth:
                colorIndex = ModSettings.TeamTournamentTeamFourColor;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return colorIndex;
    }

    public void End()
    {
        State = MatchState.Finished;
    }

    public void Start()
    {
        if (State != MatchState.Started)
        {
            State = MatchState.Started;
            _teams.ForEach(t => t.ResetScore());
        }
    }

    public IEnumerable<TeamTournamentMember> MatchMembers => Teams.SelectMany(x => x.Members);
    public bool IsPlayerParticipating => Teams.Any(x => x.IsPlayerTeam);
    public bool IsPlayerTeamWinner => GetWinners().Any(x => x.IsPlayerTeam);
    internal bool IsPlayerTeamQualified => Teams.OrderByDescending(x => x.Score).Take(_winnerTeamsPerMatch).Any(x => x.IsPlayerTeam);

    public bool IsFullMatch => _teams.Count >= _teamCount;

    private List<TeamTournamentTeam> GetWinners() =>
        State switch
        {
            MatchState.Finished => Teams.OrderByDescending(x => x.Score).Take(_winnerTeamsPerMatch).ToList(),
            _ => new()
        };

    private readonly int _winnerTeamsPerMatch;
    private List<TeamTournamentTeam> _teams;
}