using ArenaOverhaul.Data;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Library;

namespace ArenaOverhaul.Managers;

internal static class GrandTournamentRoster
{
    //Evaluated at creation, at resolve and at mission start, so political changes between announcement
    //and the event drop the lord and the next in rank fills the slot automatically.
    internal static MBList<CharacterObject> Build(GrandTournamentGame game, bool includePlayer)
    {
        int max = game.MaximumParticipantCount;
        MBList<CharacterObject> result = new(max);
        if (includePlayer)
        {
            result.Add(CharacterObject.PlayerCharacter);
        }

        IFaction hostFaction = game.Town.Settlement.MapFaction;
        Kingdom? hostKingdom = game.HostKingdom;

        List<KeyValuePair<Hero, int>> leaderboard = Campaign.Current.TournamentManager.GetLeaderboard();
        leaderboard.Sort(CompareLeaderboardEntries);
        for (int i = 0; i < leaderboard.Count && result.Count < max; i++)
        {
            Hero hero = leaderboard[i].Key;
            if (IsEligible(hero, hostKingdom, hostFaction))
            {
                result.Add(hero.CharacterObject);
            }
        }

        if (result.Count < max)
        {
            FillWithStrongestLords(result, max, hostKingdom, hostFaction);
        }
        //Fallbacks for a captured host town / destroyed host kingdom: vanilla ResolveTournament dereferences the
        //simulated winner and CreateTournamentTree expects a full bracket, so the roster must never be short.
        if (result.Count < max && hostKingdom != null)
        {
            FillWithStrongestLords(result, max, null, hostFaction);
        }
        if (result.Count < max)
        {
            FillWithStrongestLords(result, max, null, null);
        }
        return result;
    }

    internal static bool IsEligible(Hero? hero, Kingdom? hostKingdom, IFaction hostFaction) =>
        hero != null
        && hero != Hero.MainHero
        && hero.IsLord
        && hero.HeroState == Hero.CharacterStates.Active
        && !hero.IsWounded
        && !hero.IsNoncombatant
        && hero.Age >= Campaign.Current.Models.AgeModel.HeroComesOfAge
        && hero.Clan is Clan clan
        && (hostKingdom is null || (clan.Kingdom == hostKingdom && !clan.IsUnderMercenaryService))
        && hero.PartyBelongedTo?.MapEvent is null
        && hero.PartyBelongedTo?.SiegeEvent is null
        && hero.MapFaction is IFaction heroFaction
        && (hostFaction is null || !FactionManager.IsAtWarAgainstFaction(heroFaction, hostFaction));

    //Rare fallback (the leaderboard is seeded with 500 wins at game start): best remaining lords by simulation score.
    private static void FillWithStrongestLords(MBList<CharacterObject> result, int max, Kingdom? hostKingdom, IFaction hostFaction)
    {
        TournamentModel model = Campaign.Current.Models.TournamentModel;
        MBReadOnlyList<Hero> heroes = Hero.AllAliveHeroes;
        List<(Hero Hero, float Score)> candidates = new();
        for (int i = 0; i < heroes.Count; i++)
        {
            Hero hero = heroes[i];
            if (IsEligible(hero, hostKingdom, hostFaction) && !result.Contains(hero.CharacterObject))
            {
                candidates.Add((hero, model.GetTournamentSimulationScore(hero.CharacterObject)));
            }
        }
        candidates.Sort((x, y) => y.Score.CompareTo(x.Score));
        for (int i = 0; i < candidates.Count && result.Count < max; i++)
        {
            result.Add(candidates[i].Hero.CharacterObject);
        }
    }

    internal static int CountEligibleLords(Kingdom? hostKingdom, IFaction hostFaction)
    {
        MBReadOnlyList<Hero> heroes = Hero.AllAliveHeroes;
        int count = 0;
        for (int i = 0; i < heroes.Count; i++)
        {
            if (IsEligible(heroes[i], hostKingdom, hostFaction))
            {
                count++;
            }
        }
        return count;
    }

    internal static int CompareLeaderboardEntries(KeyValuePair<Hero, int> x, KeyValuePair<Hero, int> y)
    {
        int byWins = y.Value.CompareTo(x.Value);
        if (byWins != 0)
        {
            return byWins;
        }
        int byRenown = (y.Key.Clan?.Renown ?? 0f).CompareTo(x.Key.Clan?.Renown ?? 0f);
        return byRenown != 0 ? byRenown : string.CompareOrdinal(x.Key.StringId, y.Key.StringId);
    }
}