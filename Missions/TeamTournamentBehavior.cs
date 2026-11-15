using ArenaOverhaul.Core;
using ArenaOverhaul.Data;
using ArenaOverhaul.Managers;

using SandBox.Tournaments;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

using MathF = TaleWorlds.Library.MathF;

namespace ArenaOverhaul.Missions;

public class TeamTournamentBehavior : MissionLogic, ICameraModeLogic
{
    public TournamentGame TournamentGame { get; private set; }
    public int PlayerTeamLostAtRound { get => _playerLostAtRound; }
    public bool IsPlayerEliminated { get; private set; }
    public int CurrentRoundIndex { get; private set; } = -1;
    public TeamTournamentMatch? LastMatch { get; private set; }
    public TeamTournamentRound[] Rounds { get; private set; } = new TeamTournamentRound[TeamTournamentInfo.Current!.Rounds];
    public TeamTournamentMember? Winner { get; private set; }
    public bool IsPlayerParticipating { get; }
    public Settlement Settlement { get; private set; }
    public float BetOdd { get; private set; }
    public int BettedDenars { get; private set; }
    public int OverallExpectedDenars { get; private set; }
    private TeamTournamentInfo CurrentInfo { get; set; } = TeamTournamentInfo.Current!;
    private TeamTournamentMissionController MissionBehavior { get; set; }

    public SpectatorCameraTypes GetMissionCameraLockMode(bool lockedToMainPlayer)
      => !IsPlayerParticipating ? SpectatorCameraTypes.LockToAnyAgent : SpectatorCameraTypes.Invalid;
    public TeamTournamentRound CurrentRound => Rounds[CurrentRoundIndex];
    public TeamTournamentRound? NextRound => CurrentRoundIndex < Rounds.Length - 1 ? Rounds[CurrentRoundIndex + 1] : null;
    public TeamTournamentMatch? CurrentMatch => CurrentRound?.CurrentMatch;
    public int PlayerDenars => Hero.MainHero.Gold;
    public int MaximumBetInstance => Math.Min(150, PlayerDenars);

    public TeamTournamentBehavior(TournamentGame tournamentGame, Settlement settlement, ITournamentGameBehavior gameBehavior, bool isPlayerParticipating)
    {
        Settlement = settlement;
        TournamentGame = tournamentGame;
        MissionBehavior = (gameBehavior as TeamTournamentMissionController)!;
        CreateTeams();
        LastMatch = null;
        Winner = null;
        IsPlayerParticipating = isPlayerParticipating;
    }

    private void CreateTeams()
    {
        // first we take in our selected team
        List<TeamTournamentMember> mainParticipants = new List<TeamTournamentMember>();
        foreach (FlattenedTroopRosterElement flattenedTroopRosterElement in CurrentInfo.SelectedRoster!.ToFlattenedRoster())
            mainParticipants.Add(new TeamTournamentMember(flattenedTroopRosterElement.Troop));
        _teams = new List<TeamTournamentTeam>() {
            new TeamTournamentTeam(mainParticipants, 0, Hero.MainHero.ClanBanner, Hero.MainHero.ClanBanner.GetPrimaryColor())
        };

        // create rest
        CreateTournamentTeams();
    }

    private bool IsAlreadySelected(CharacterObject c) => _teams!.Any(x => x.Members.Any(y => y.Character == c));

    private void AddTournamentTeam(IEnumerable<TeamTournamentMember> members, Banner? teamBanner = null, uint teamColor = 0, TeamTournamentMember? leader = null)
    {
        TeamTournamentMember? localLeader = leader;
        if (localLeader is null)
        {
            localLeader =
              members.Where(x => x.Character.IsHero).OrderByDescending(x => x.Character.GetBattlePower()).FirstOrDefault()
              ?? members.OrderByDescending(x => x.Character.GetBattlePower()).First();
        }

        int teamIndex = 0;
        if (!localLeader.Character.IsHero)
        {
            List<TeamTournamentTeam> similarTeams = _teams!.Where(x => x.GetTeamLeader().Character.Name == localLeader.Character.Name).ToList();
            teamIndex = similarTeams.Count > 0 ? similarTeams.Max(x => x.TeamIndex) + 1 : 0;
        }

        _teams!.Add(new TeamTournamentTeam(members, teamIndex, teamBanner, teamColor, leader));
    }

    private void CreateTournamentTeams()
    {
        int totalTroopsNeeded = (CurrentInfo.TeamsCount - 1) * CurrentInfo.TeamSize;

        List<CharacterObject> combatantHeroes = Settlement.GetCombatantHeroesInSettlement()
            .Where(x => !IsAlreadySelected(x) && !CurrentInfo.SelectedRoster!.Contains(x))
            .OrderByDescending(x => x.HeroObject.IsPartyLeader)
            .ThenByDescending(x => x.HeroObject.IsLord)
            .ThenByDescending(x => x.GetBattlePower())
            .Take(totalTroopsNeeded)
            .ToList();

        List<CharacterObject> garrisonTroops = Settlement.Town.GarrisonParty?.MemberRoster.GetTroopRoster()
            .Where(x => !x.Character.IsHero && x.WoundedNumber < x.Number && x.Character.CanBeAParticipant(true))
            .Select(x => x.Character).ToList() ?? [];

        foreach (CharacterObject hero in Settlement.GetHeroesInSettlement().Where(x => x.HeroObject.IsPartyLeader && !x.HeroObject.IsPrisoner))
        {
            garrisonTroops.AddRange(hero.HeroObject.PartyBelongedTo.MemberRoster.GetTroopRoster()
                .Where(y => !y.Character.IsHero && y.WoundedNumber < y.Number && y.Character.CanBeAParticipant(true))
                .Select(y => y.Character));
        }
        List<CharacterObject> distinctTroops = [.. garrisonTroops.Distinct().OrderByDescending(x => x.GetBattlePower())];

        if (distinctTroops.Count == 0)
            distinctTroops = [.. GetSimpletons(Settlement.Culture).Distinct().OrderByDescending(x => x.GetBattlePower())];
        if (distinctTroops.Count == 0)
            distinctTroops = [.. GetSimpletons().Distinct()];

        List<CharacterObject> charsToUse = [.. combatantHeroes];
        if (distinctTroops.Count > 0)
        {
            for (int i = 0; charsToUse.Count < totalTroopsNeeded; i++)
                charsToUse.Add(distinctTroops[i % distinctTroops.Count]);
        }

        //Now create the Teams
        List<List<TeamTournamentMember>> teams = new();
        for (int n = 0; n < CurrentInfo.TeamsCount - 1; n++) teams.Add(new List<TeamTournamentMember>());

        //Place troops in teams in a zig-zag pattern to try to make the team strength as even as possible
        int teamNum = 0;
        int increment = 1;
        foreach (CharacterObject troop in charsToUse)
        {
            teams[teamNum].Add(new TeamTournamentMember(troop));
            teamNum += increment;

            if (teamNum == -1)
            {
                increment = 1;
                teamNum = 0;
            }
            else if (teamNum == teams.Count)
            {
                increment = -1;
                teamNum = teams.Count - 1;
            }
        }

        teams.ForEach(curTeam => AddTournamentTeam(curTeam, curTeam.First().Character?.HeroObject?.ClanBanner, 0, curTeam.First()));
    }

    private static List<CharacterObject> GetSimpletons(CultureObject? culture = null, CharacterObject? baseChar = null)
    {
        baseChar ??= CharacterObject.FindFirst(x => x.IsBasicTroop && x.UpgradeTargets != null && (culture == null || x.Culture == culture));
        if (baseChar is null)
            return [];

        List<CharacterObject> simpletons = [baseChar];
        if (baseChar.UpgradeTargets != null)
        {
            for (int i = 0; i < baseChar.UpgradeTargets.Length; i++)
                simpletons.AddRange(GetSimpletons(culture, baseChar.UpgradeTargets[i]));
        }
        return simpletons;
    }

    public override void AfterStart()
    {
        CurrentRoundIndex = 0;
        CreateTorunamentTree();
        CalculateBet();
        TournamentRewardManager.InitiateTournament(TournamentGame.Town);
    }

#if !LOWER_THAN_1_5
    // 1.5.7 only: vanilla TournamentBehavior (replaced by this class in team tournaments) reports the initial spawn here.
    public override void OnAfterMissionLoadingFinished() => Mission.OnInitialSpawnCompleted();
#endif

    public override void OnMissionTick(float dt)
    {
        if (CurrentMatch != null && CurrentMatch.State == TournamentMatch.MatchState.Started && MissionBehavior.IsMatchEnded())
            EndCurrentMatch(false);
    }

    public void StartMatch()
    {
        if (CurrentMatch!.IsPlayerParticipating)
        {
            Campaign.Current.TournamentManager.OnPlayerJoinMatch(TournamentGame.GetType());
        }
        CurrentMatch.Start();
        base.Mission.SetMissionMode(MissionMode.Tournament, true);
        MissionBehavior.StartMatch(CurrentMatch, NextRound == null);
        CampaignEventDispatcher.Instance.OnPlayerStartedTournamentMatch(Settlement.Town);
    }

    public void SkipMatch(bool isLeave = false)
    {
        if (CurrentMatch!.IsReady)
            CurrentMatch.Start();

        MissionBehavior.SkipMatch(CurrentMatch);
        EndCurrentMatch(isLeave);
    }

    private void EndCurrentMatch(bool isLeave)
    {
        LastMatch = CurrentMatch;
        CurrentRound.EndMatch();
        MissionBehavior.OnMatchEnded();

        // Update round rewards
        TournamentRewardManager.UpdateRoundWinnings(this);
        TournamentInjuryManager.ApplyMatchInjuries(LastMatch);

        // add winners to next round
        if (NextRound != null)
        {
            // fill in round
            LastMatch!.Winners.ToList().ForEach(x => NextRound.AddTeam(x));
            MatchEnd?.Invoke(LastMatch);
        }

        // fire off events if player was disqualified 
        if (LastMatch!.IsPlayerParticipating)
        {
            if (!LastMatch.IsPlayerTeamWinner)
                OnPlayerTeamEliminated();
            else
                OnPlayerTeamWinMatch();
        }

        if (CurrentRound.CurrentMatch == null) // done with this round
        {
            // check if done with Tournament or not
            if (CurrentRoundIndex < Rounds.Length - 1)
            {
                // not done yet, go to next round
                CurrentRoundIndex++;
                CalculateBet();
            }
            else
            {
                // done with Tournament
                CalculateBet();
                MessageHelper.QuickInformationMessage(new TextObject(Texts.Ids.TournamentIsOver, null), 0, null, "");
                TeamTournamentTeam? winningTeam = LastMatch.Winners?.FirstOrDefault();
                Winner = winningTeam?.GetTeamLeader();
                if (Winner?.Character.IsHero ?? false)
                {
                    if (Winner.Character == CharacterObject.PlayerCharacter)
                        OnPlayerWinTournament();

                    Campaign.Current.TournamentManager.GivePrizeToWinner(TournamentGame, Winner.Character.HeroObject, true);
                    Campaign.Current.TournamentManager.AddLeaderboardEntry(Winner.Character.HeroObject);
                }

                if (ModSettings.TeamTournamentScoresForWinningTeam && winningTeam != null)
                {
                    winningTeam.Members.
                        Where(x => x.Character.IsHero && x != winningTeam.GetTeamLeader()).ToList().
                        ForEach(x => Campaign.Current.TournamentManager.AddLeaderboardEntry(x.Character.HeroObject));
                }

                List<CharacterObject> list = new List<CharacterObject>(_teams!.SelectMany(x => x.Members).Select(y => y.Character));
                CampaignEventDispatcher.Instance.OnTournamentFinished(Winner?.Character, new(list), Settlement.Town, TournamentGame.Prize);

                CurrentInfo.Finish();

                if (!isLeave)
                    TournamentEnd?.Invoke();
            }
        }
    }

    public void EndTournamentViaLeave()
    {
        while (CurrentMatch != null)
        {
            SkipMatch(true);
        }
    }

    private void OnPlayerTeamEliminated()
    {
        _playerLostAtRound = CurrentRoundIndex + 1;
        IsPlayerEliminated = true;
        BetOdd = 0f;
        if (BettedDenars > 0)
        {
            GiveGoldAction.ApplyForCharacterToSettlement(null, Settlement.CurrentSettlement, BettedDenars, false);
        }
        OverallExpectedDenars = 0;
        CampaignEventDispatcher.Instance.OnPlayerEliminatedFromTournament(CurrentRoundIndex, Settlement.Town);
    }

    private void OnPlayerTeamWinMatch() => Campaign.Current.TournamentManager.OnPlayerWinMatch(TournamentGame.GetType());

    private void OnPlayerWinTournament()
    {
        if (Campaign.Current.GameMode != CampaignGameMode.Campaign)
            return;

        //Renown, influence and gold prizes are awarded by the TournamentRewardManager, so we only account bet winnings here
        if (OverallExpectedDenars > 0)
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, OverallExpectedDenars, false);

        Campaign.Current.TournamentManager.OnPlayerWinTournament(TournamentGame.GetType());
    }

    private void CreateTorunamentTree()
    {
        int T = CurrentInfo.TeamsCount;          // Teams count
        int R = CurrentInfo.Rounds;              // Rounds count 
        int M = CurrentInfo.FirstRoundMatches;   // Matches count
        int TPM = T / M;                         // Teams per match
        int W = TPM / 2;                         // Winners per match

        if (Math.Log(T, 2) > 4) // current interface allows 4 rounds max
            W = 1;

        for (int r = 0; r < R; r++)
        {
            Rounds[r] = new TeamTournamentRound(T, M, W);

            if (r < R)
            {
                T = W * M;
                TPM = Math.Max(Math.Min(MBRandom.RandomInt(0, 2) * 2 + 2, T / 2), 2);
                M = T / TPM;
                W = TPM / 2;
            }
        }

        // fill in first round
        _teams!.ForEach(x => Rounds[0].AddTeam(x));
    }

    public override InquiryData? OnEndMissionRequest(out bool canPlayerLeave)
    {
        canPlayerLeave = false;
        return null;
    }

    public void PlaceABet(int bet)
    {
        BettedDenars += bet;
        OverallExpectedDenars += GetExpectedDenarsForBet(bet);
        GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, bet, true);
    }

    public int GetExpectedDenarsForBet(int bet) => (int) (BetOdd * bet);

    public int GetMaximumBet()
    {
        int defaultMax = ModSettings.TournamentMaximumBet;
        if (Hero.MainHero.GetPerkValue(DefaultPerks.Roguery.DeepPockets))
        {
            return defaultMax * (int) DefaultPerks.Roguery.DeepPockets.PrimaryBonus;
        }
        return defaultMax;
    }

    //Vanilla bet calculation actually maps nicely to the team tournaments
    private void CalculateBet()
    {
        if (!IsPlayerParticipating)
        {
            return;
        }

        if (CurrentRound.CurrentMatch == null)
        {
            BetOdd = 0.0f;
        }
        else if (IsPlayerEliminated)
        {
            OverallExpectedDenars = 0;
            BetOdd = 0.0f;
        }
        else
        {
            List<KeyValuePair<Hero, int>> leaderboard = Campaign.Current.TournamentManager.GetLeaderboard();
            int playerTournamentWins = 0;
            int maxLeaderbordWins = 0;
            for (int index = 0; index < leaderboard.Count; ++index)
            {
                if (leaderboard[index].Key == Hero.MainHero)
                    playerTournamentWins = leaderboard[index].Value;
                if (leaderboard[index].Value > maxLeaderbordWins)
                    maxLeaderbordWins = leaderboard[index].Value;
            }
            float playerRating = 30f + Hero.MainHero.Level + Math.Max(0, playerTournamentWins * 12 - maxLeaderbordWins * 2);
            float totalParticipantsRating = 0.0f;
            float playerTeamRating = 0.0f;
            float otherTeamsRating = 0.0f;
            foreach (TeamTournamentMatch match in CurrentRound.Matches)
            {
                foreach (TeamTournamentTeam tournamentTeam in match.Teams)
                {
                    float teamRating = 0.0f;
                    foreach (TeamTournamentMember participant in tournamentTeam.Members)
                    {
                        if (participant.Character != CharacterObject.PlayerCharacter)
                        {
                            int participantTournamentWins = 0;
                            if (participant.Character.IsHero)
                            {
                                for (int index = 0; index < leaderboard.Count; ++index)
                                {
                                    if (leaderboard[index].Key == participant.Character.HeroObject)
                                        participantTournamentWins = leaderboard[index].Value;
                                }
                            }
                            teamRating += participant.Character.Level + Math.Max(0, participantTournamentWins * 8 - maxLeaderbordWins * 2);
                        }
                    }
                    if (tournamentTeam.IsPlayerTeam)
                    {
                        playerTeamRating = teamRating;
                        foreach (TeamTournamentTeam otherTournamentTeam in match.Teams)
                        {
                            if (tournamentTeam != otherTournamentTeam)
                            {
                                foreach (TeamTournamentMember participant in otherTournamentTeam.Members)
                                {
                                    int participantTournamentWins = 0;
                                    if (participant.Character.IsHero)
                                    {
                                        for (int index = 0; index < leaderboard.Count; ++index)
                                        {
                                            if (leaderboard[index].Key == participant.Character.HeroObject)
                                                participantTournamentWins = leaderboard[index].Value;
                                        }
                                    }
                                    otherTeamsRating += participant.Character.Level + Math.Max(0, participantTournamentWins * 8 - maxLeaderbordWins * 2);
                                }
                            }
                        }
                    }
                    totalParticipantsRating += teamRating;
                }
            }
            float randomFactor = ModSettings.TournamentEnableRandomizedBettingOdds ? MBRandom.RandomFloatRanged(0.75f, 1.25f) : 1f;
            BetOdd = (int) (MathF.Clamp(MathF.Pow(1f / ((playerTeamRating + playerRating) / totalParticipantsRating * (playerRating / (playerTeamRating + playerRating + 0.5f * (totalParticipantsRating - (playerTeamRating + otherTeamsRating))))), 0.75f) * randomFactor, 1.1f, MaximumOdd) * 10.0) / 10f;
        }
    }

    internal List<CharacterObject> GetAllPossibleParticipants() => _teams!.SelectMany(t => t.Members.Select(m => m.Character)).ToList();

    public event Action? TournamentEnd;
    public event Action<TeamTournamentMatch>? MatchEnd;
    public const float EndMatchTimerDuration = 6f;
    private List<TeamTournamentTeam>? _teams;
    private int _playerLostAtRound;
    public const float MaximumOdd = 4f;
}