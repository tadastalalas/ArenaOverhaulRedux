using ArenaOverhaul.Core;
using ArenaOverhaul.Data;

using Bannerlord.ButterLib.Common.Helpers;

using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ArenaOverhaul.Managers;

internal static class GrandTournamentScheduler
{
    private const int RequiredLords = 16;
    private static AOArenaBehaviorManager Manager => AOArenaBehaviorManager.Instance!;
    private static ITournamentManager Tournaments => Campaign.Current.TournamentManager;

    internal static void OnDailyTick()
    {
        ResolveFinished();

        if (!ModSettings.GrandTournamentEnabled && !ModSettings.KingdomTournamentsEnabled)
            return;

        //Current season and the next one, so an announcement window can spill over a season boundary.
        for (int seasonOffset = 0; seasonOffset <= 1; seasonOffset++)
        {
            int roundIndex = GetRoundIndex(seasonOffset);
            bool isGrandSeason = roundIndex % CampaignTime.SeasonsInYear == (int) ModSettings.GrandTournamentSeason;
            if (isGrandSeason)
            {
                if (ModSettings.GrandTournamentEnabled)
                    TickRound(GetEventStart(seasonOffset, ModSettings.GrandTournamentDayOfSeason), roundIndex, isGrand: true);
            }
            else if (ModSettings.KingdomTournamentsEnabled && roundIndex % ModSettings.KingdomTournamentIntervalSeasons == 0)
            {
                TickRound(GetEventStart(seasonOffset, ModSettings.KingdomTournamentDayOfSeason), roundIndex, isGrand: false);
            }
        }
    }

    private static void TickRound(CampaignTime start, int roundIndex, bool isGrand)
    {
        CampaignTime announceAt = start - CampaignTime.Days(ModSettings.GrandTournamentAnnounceDays);
        int lastStartedRound = isGrand ? Manager.LastGrandTournamentRound : Manager.LastKingdomTournamentRound;

        if (lastStartedRound >= roundIndex || announceAt.IsFuture)
            return;

        //Before start: keep the announced tournaments alive (re-host if a siege cancelled one).
        //At start: mark the round as started so nothing gets (re)created once fighting has begun.
        if (!start.IsFuture)
        {
            if (isGrand)
                Manager.LastGrandTournamentRound = roundIndex;
            else
                Manager.LastKingdomTournamentRound = roundIndex;
        }

        //Round already over (mod installed/enabled late): only mark it.
        if (!(start + CampaignTime.Days(ModSettings.GrandTournamentDurationDays)).IsFuture)
            return;
        
        if (isGrand)
            EnsureGrandTournament(start);
        else
            EnsureKingdomTournaments(start);
    }

    private static void ResolveFinished()
    {
        MBReadOnlyList<Town> towns = Town.AllTowns;
        for (int i = 0; i < towns.Count; i++)
        {
            if (Tournaments.GetTournamentGame(towns[i]) is not GrandTournamentGame game)
                continue;

            if (HasLostVenue(game))
                Cancel(game);
            else if (!game.EndTime.IsFuture)
                Tournaments.ResolveTournament(game, towns[i]);
        }
    }
    
    //Kingdom event whose venue was lost (rebellion, defection, elimination, barter) - vanilla only handles siege.
    internal static bool HasLostVenue(GrandTournamentGame game) =>
        game.HostKingdom is Kingdom kingdom && (game.Town.InRebelliousState || game.Town.Settlement.MapFaction != kingdom);

    //Vanilla's siege-cancel path without the IsUnderSiege precondition: no simulation, no leaderboard entry, no prize, no popup.
    internal static void Cancel(GrandTournamentGame game)
    {
        CampaignEventDispatcher.Instance.OnTournamentCancelled(game.Town);
        (Tournaments as TournamentManager)?.RemoveTournament(game);
    }

    private static void EnsureGrandTournament(CampaignTime start)
    {
        MBReadOnlyList<Town> towns = Town.AllTowns;
        for (int i = 0; i < towns.Count; i++)
        {
            if (Tournaments.GetTournamentGame(towns[i]) is GrandTournamentGame { IsKingdomTournament: false })
                return;
        }

        Kingdom? kingdom = FindGrandHostKingdom();
        Town? town = kingdom is null ? null : SelectHostTown(kingdom);
        if (town is null || GrandTournamentRoster.CountEligibleLords(null, town.Settlement.MapFaction) < RequiredLords)
            return;

        Create(town, null, start);
    }

    private static void EnsureKingdomTournaments(CampaignTime start)
    {
        MBReadOnlyList<Kingdom> kingdoms = Kingdom.All;
        for (int i = 0; i < kingdoms.Count; i++)
        {
            Kingdom kingdom = kingdoms[i];
            if (kingdom.IsEliminated || HasKingdomTournament(kingdom))
                continue;

            Town? town = SelectHostTown(kingdom);
            if (town is null || GrandTournamentRoster.CountEligibleLords(kingdom, kingdom) < RequiredLords)
                continue;

            Create(town, kingdom, start);
        }
    }

    //Scan every town: a game whose venue left the host kingdom is no longer in kingdom.Fiefs and must not trigger a re-host.
    private static bool HasKingdomTournament(Kingdom kingdom)
    {
        MBReadOnlyList<Town> towns = Town.AllTowns;
        for (int i = 0; i < towns.Count; i++)
        {
            if (Tournaments.GetTournamentGame(towns[i]) is GrandTournamentGame game && game.HostKingdom == kingdom)
                return true;
        }
        return false;
    }

    //Strongest leaderboard lord (player included) that belongs to a living, non-mercenary kingdom.
    private static Kingdom? FindGrandHostKingdom()
    {
        List<KeyValuePair<Hero, int>> leaderboard = Tournaments.GetLeaderboard();
        leaderboard.Sort(GrandTournamentRoster.CompareLeaderboardEntries);
        for (int i = 0; i < leaderboard.Count; i++)
        {
            Hero hero = leaderboard[i].Key;
            if (hero.HeroState == Hero.CharacterStates.Active
                && (hero.IsLord || hero == Hero.MainHero)
                && hero.Clan is { IsUnderMercenaryService: false, Kingdom: { IsEliminated: false } kingdom })
            {
                return kingdom;
            }
        }
        return null;
    }

    //"Capital": the ruling clan's most prosperous town, else the kingdom's.
    private static Town? SelectHostTown(Kingdom kingdom) =>
        PickMostProsperous(kingdom.RulingClan?.Fiefs) ?? PickMostProsperous(kingdom.Fiefs);

    private static Town? PickMostProsperous(MBReadOnlyList<Town>? fiefs)
    {
        if (fiefs is null)
            return null;

        Town? best = null;
        for (int i = 0; i < fiefs.Count; i++)
        {
            Town town = fiefs[i];
            if (town.IsTown && !town.IsUnderSiege && !town.InRebelliousState && Tournaments.GetTournamentGame(town) is null
                && (best is null || town.Prosperity > best.Prosperity))
            {
                best = town;
            }
        }
        return best;
    }

    private static void Create(Town town, Kingdom? kingdom, CampaignTime start)
    {
        GrandTournamentGame game = new(town, kingdom, start);
        Tournaments.AddTournament(game);
        GrandTournamentNotifier.NotifyAnnounced(game);

        TextObject message = new(kingdom is null
            ? Texts.Ids.GrandTournamentAnnounced
            : Texts.Ids.KingdomTournamentAnnounced);
        message.SetTextVariable("TOWN", town.Name);
        if (kingdom != null)
            message.SetTextVariable("KINGDOM", kingdom.Name);
        LocalizationHelper.SetNumericVariable(message, "DAYS", Math.Max(0, (int) Math.Ceiling(start.RemainingDaysFromNow)));

        if (kingdom is null && ModSettings.GrandTournamentAnnouncePopup)
            MessageHelper.QuickInformationMessage(message);
        MessageHelper.SimpleMessage(message);
    }

    private static int GetRoundIndex(int seasonOffset) =>
        CampaignTime.Now.GetYear * CampaignTime.SeasonsInYear + (int) CampaignTime.Now.GetSeasonOfYear + seasonOffset;

    //Start of the current season is Now minus elapsed days and hours of the season; dayOfSeason is 1-based (MCM slider).
    private static CampaignTime GetEventStart(int seasonOffset, int dayOfSeason)
    {
        CampaignTime now = CampaignTime.Now;
        CampaignTime seasonStart = now - CampaignTime.Days(now.GetDayOfSeason) - CampaignTime.Hours(now.CurrentHourInDay);
        return seasonStart + CampaignTime.Days(seasonOffset * CampaignTime.DaysInSeason + dayOfSeason - 1);
    }
}