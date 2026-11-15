using ArenaOverhaul.Core;

using SandBox.View.Menu;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace ArenaOverhaul.Managers;

public class TeamTournamentInfo
{
    private const int MinimumTeamMembers = 2;

    public static TeamTournamentInfo? Current { get; private set; }
    public TroopRoster? SelectedRoster { get; private set; }
    public TournamentTeam? PlayerTeam { get; set; }
    public int TeamsCount { get; }
    public int FirstRoundMatches { get; }
    public int Rounds { get; }
    public int TeamSize { get; }
    public bool IsFinished { get; set; }
    public Town Town { get; }
    public bool IsStarted => SelectedRoster != null;

    public TeamTournamentInfo()
    {
        // TODO: if a need arises to show tournament information beforehand, extract creation into upper chain
        //       e.g. when tournaments are generated in the world, also replace "Current" instance
        Town = Settlement.CurrentSettlement.Town;

        int teamSizeMax = Math.Max(ModSettings.TeamTournamentTeamSizeMax, MinimumTeamMembers);
        int teamSizeMin = MBMath.ClampInt(ModSettings.TeamTournamentTeamSizeMin, MinimumTeamMembers, teamSizeMax);
        TeamSize = MBRandom.RandomInt(teamSizeMin, teamSizeMax + 1);

        // 8, 16, 32 teams possible -> also 4 but needs more testing and fixing 
        TeamsCount = (int) Math.Pow(2, MBRandom.RandomInt(3, (int) Math.Log(ModSettings.TeamTournamentTeamsCountMax, 2) + 1));

        // if full (32) => 8 rounds, else can be 4 or 8
        FirstRoundMatches = TeamsCount == 32 ? 8 : (TeamsCount / (MBRandom.RandomInt(2) * 2 + 2));

        // simple log2 round progression (members/2 in every round)
        Rounds = (int) Math.Min(Math.Log(TeamsCount, 2), 4);
        Current = this;
    }

    public void OpenSelectionMenu(MenuViewContext menuViewContext)
    {
        TroopRoster mainHeroRoster = TroopRoster.CreateDummyTroopRoster();
        mainHeroRoster.AddToCounts(Hero.MainHero.CharacterObject, 1);
        menuViewContext.MenuContext.OpenTroopSelection(
            GetAvailableSelection(),
            mainHeroRoster,
            delegate (CharacterObject character) { return !character.IsPlayerCharacter; },
            delegate (TroopRoster selection)
            {
                SelectedRoster = selection;
                TournamentGame tournamentGame = Campaign.Current.TournamentManager.GetTournamentGame(Settlement.CurrentSettlement.Town);
                GameMenu.SwitchToMenu("town");
                tournamentGame.PrepareForTournamentGame(true);
                Campaign.Current.TournamentManager.OnPlayerJoinTournament(tournamentGame.GetType(), Settlement.CurrentSettlement);
            },
            TeamSize,
            2
        );
    }

    public bool IsTeamTournament => SelectedRoster != null;

    public static TroopRoster GetAvailableSelection()
    {
        TroopRoster availableRoster = TroopRoster.CreateDummyTroopRoster();

        // we add everyone is settlement that is relevant to the selection list
        IEnumerable<CharacterObject> selectableChars = Settlement.CurrentSettlement
          .GetCombatantHeroesInSettlement()
          .Where(x => CanBeSelected(x));

        FlattenedTroopRoster flattenTroopRoster = new FlattenedTroopRoster(0);

        // add the main hero at the top
        flattenTroopRoster.Add(Hero.MainHero.CharacterObject, 1, 0);

        // add every other hero afterwards
        foreach (CharacterObject? character in selectableChars)
            flattenTroopRoster.Add(character, 1, 0);

        // add the flattened heroes selection into the roster
        foreach (FlattenedTroopRosterElement element in flattenTroopRoster)
            availableRoster.AddToCounts(element.Troop, 1, false, element.IsWounded ? 1 : 0, element.Xp, true, -1);

        // now also add own troops in party roster
        IEnumerable<FlattenedTroopRosterElement> partyTroops = MobileParty.MainParty.MemberRoster.ToFlattenedRoster()
            .Where(x => (!x.Troop.IsHero || !availableRoster.Contains(x.Troop)) && !x.IsWounded && x.Troop.CanBeAParticipant(false, false));
        foreach (FlattenedTroopRosterElement element in partyTroops)
            availableRoster.AddToCounts(element.Troop, 1, false, 0, element.Xp, true, -1);
        return availableRoster;
    }

    public static bool CanBeSelected(CharacterObject character)
    {
        bool isSameSettlement = character.HeroObject.CurrentSettlement == Hero.MainHero.CurrentSettlement;
        if (!isSameSettlement) return false;

        bool isSameClan = Hero.MainHero.Clan == character.HeroObject.Clan;
        if (isSameClan) return true;

        bool isSameAlliance = Hero.MainHero.MapFaction == character.HeroObject.MapFaction && (Hero.MainHero.IsFriend(character.HeroObject) || Hero.MainHero.IsFactionLeader);
        if (isSameAlliance) return true;

        return Hero.MainHero.AllLivingRelatedHeroes().Contains(character.HeroObject);
    }

    internal void Finish()
    {
        SelectedRoster = null;
        Current = null;
        IsFinished = true;
    }
}