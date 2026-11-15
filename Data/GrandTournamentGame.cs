using ArenaOverhaul.Core;
using ArenaOverhaul.Managers;

using Bannerlord.ButterLib.Common.Helpers;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace ArenaOverhaul.Data;

public class GrandTournamentGame : FightTournamentGame
{
    //Base TournamentGame uses 10/20/30/40/60 - stay clear of them.
    [SaveableField(100)]
    private Kingdom? _hostKingdom;

    [SaveableField(101)]
    private ItemObject? _hostPrize;

    [SaveableField(102)]
    private ItemObject? _playerPrize;

    //Null = yearly grand tournament, set = seasonal kingdom tournament.
    public Kingdom? HostKingdom => _hostKingdom;

    public bool IsKingdomTournament => _hostKingdom != null;

    //1v1 duels: with MaxTeamSize 1 and MaxTeamNumberPerMatch 2 vanilla TournamentBehavior.CreateTournamentTree
    //resolves to 8/4/2/1 matches of 2 single-member teams, so no mission logic patching is needed.
    public override int MaxTeamSize => 1;

    public override int MaxTeamNumberPerMatch => 2;

    public override int MaximumParticipantCount => 16;

    [SaveableField(103)]
    private CampaignTime _startTime;

    public CampaignTime StartTime => _startTime;

    public CampaignTime EndTime => _startTime + CampaignTime.Days(ModSettings.GrandTournamentDurationDays);

    public bool IsOpen => !_startTime.IsFuture;

    //Vanilla's own resolve path can never fire earlier than ours: creation + announce + duration >= start + duration.
    public override int RemoveTournamentAfterDays => ModSettings.GrandTournamentAnnounceDays + ModSettings.GrandTournamentDurationDays;

    public GrandTournamentGame(Town town, Kingdom? hostKingdom, CampaignTime startTime) : base(town)
    {
        _hostKingdom = hostKingdom;
        _startTime = startTime;
    }

    public override MBList<CharacterObject> GetParticipantCharacters(Settlement settlement, bool includePlayer = true) =>
        GrandTournamentRoster.Build(this, includePlayer);

    //Registering through the existing pipeline makes GivePrizeToWinner, GetPrizeItemName, ShowPrizeItemHint and
    //TournamentVMPatch deliver/display the modified item without any grand-specific branches.
    protected override ItemObject GetTournamentPrize(bool includePlayer, int lastRecordedLordCountForTournamentPrize)
    {
        if (ItemHelper.IsRuntimeCrafted(_playerPrize))
            _playerPrize = null;
        if (ItemHelper.IsRuntimeCrafted(_hostPrize))
            _hostPrize = null;
        
        ItemObject prize = includePlayer
            ? _playerPrize ??= GrandTournamentPrizeManager.PickPrizeItem(Hero.MainHero.Culture) ?? base.GetTournamentPrize(includePlayer, lastRecordedLordCountForTournamentPrize)
            : _hostPrize ??= GrandTournamentPrizeManager.PickPrizeItem(Town.Culture) ?? base.GetTournamentPrize(includePlayer, lastRecordedLordCountForTournamentPrize);

        TournamentRewardManager.RegisterPrizeModifier(Town, prize, ModSettings.GrandTournamentLegendaryPrize ? GrandTournamentPrizeManager.GetBestModifier(prize) : null);
        return prize;
    }

    public override TextObject GetMenuText()
    {
        TextObject text = _hostKingdom is null
            ? new(Texts.Ids.GrandTournamentMenuText)
            : new(Texts.Ids.KingdomTournamentMenuText);
        text.SetTextVariable("TOWN", Town.Name);
        text.SetTextVariable("TOURNAMENT_PRIZE", TournamentRewardManager.GetPrizeItemName(this));
        if (_hostKingdom != null)
        {
            text.SetTextVariable("KINGDOM", _hostKingdom.Name);
        }
        return text;
    }
    
    //Join/Watch gate used by the access model. Watching before start would simulate the whole event early, so both are locked.
    internal bool CanPlayerAttend(bool asParticipant, out TextObject explanation)
    {
        if (!IsOpen)
        {
            explanation = new(_hostKingdom is null
                ? Texts.Ids.GrandTournamentBeginsIn
                : Texts.Ids.KingdomTournamentBeginsIn);
            LocalizationHelper.SetNumericVariable(explanation, "DAYS", Math.Max(1, (int) Math.Ceiling(_startTime.RemainingDaysFromNow)));
            return false;
        }
        if (asParticipant && _hostKingdom != null && (Clan.PlayerClan.Kingdom != _hostKingdom || Clan.PlayerClan.IsUnderMercenaryService))
        {
            explanation = new(Texts.Ids.OnlySwornLordsMayCompete);
            explanation.SetTextVariable("KINGDOM", _hostKingdom.Name);
            return false;
        }
        explanation = TextObject.GetEmpty();
        return true;
    }
    
    //Arena master sign-up path: the access model only gates the town menu buttons.
    internal static bool IsJoinLocked(Settlement? settlement, out TextObject explanation)
    {
        if (settlement?.Town is Town town && Campaign.Current?.TournamentManager?.GetTournamentGame(town) is GrandTournamentGame game)
            return !game.CanPlayerAttend(true, out explanation);
        explanation = TextObject.GetEmpty();
        return false;
    }
}