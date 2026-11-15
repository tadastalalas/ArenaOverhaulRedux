using ArenaOverhaul.Core;
using ArenaOverhaul.Managers;

using Bannerlord.ButterLib.Common.Helpers;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace ArenaOverhaul.Data;

//Public: constructed by the save system and MapNotificationVM via Activator.
public class GrandTournamentMapNotification : InformationData
{
    //Base InformationData uses low ids - stay clear of them.
    [SaveableField(100)]
    private readonly Town _town;

    [SaveableField(101)]
    private readonly Kingdom? _kingdom;

    [SaveableField(102)]
    private readonly Hero? _winner;

    [SaveableField(103)]
    private readonly CampaignTime _startTime;

    [SaveableField(104)]
    private readonly CampaignTime _endTime;

    [SaveableField(105)]
    private readonly string _prizeName;

    public Town Town => _town;

    public bool IsKingdomTournament => _kingdom != null;

    public bool IsResolved => _winner != null;

    public GrandTournamentMapNotification(GrandTournamentGame game, Hero? winner, TextObject description) : base(description)
    {
        _town = game.Town;
        _kingdom = game.HostKingdom;
        _winner = winner;
        _startTime = game.StartTime;
        _endTime = game.EndTime;
        _prizeName = TournamentRewardManager.GetPrizeItemName(game);
    }

    public override TextObject TitleText
    {
        get
        {
            TextObject title = new(IsResolved
                ? (IsKingdomTournament ? Texts.Ids.NotificationTitleKingdomConcluded : Texts.Ids.NotificationTitleGrandConcluded)
                : (IsKingdomTournament ? Texts.Ids.NotificationTitleKingdom : Texts.Ids.NotificationTitleGrand));
            title.SetTextVariable("KINGDOM", _kingdom?.Name ?? TextObject.GetEmpty());
            return title;
        }
    }

    public override string SoundEventPath => "event:/ui/notification/kingdom_decision";

    internal TextObject GetDetailsText()
    {
        TextObject text = new(SelectDetailsTemplate());
        text.SetTextVariable("TOWN", _town.Name);
        text.SetTextVariable("KINGDOM", _kingdom?.Name ?? TextObject.GetEmpty());
        text.SetTextVariable("PRIZE", _prizeName);
        text.SetTextVariable("WINNER", _winner?.Name ?? TextObject.GetEmpty());
        text.SetTextVariable("CLAN", _winner?.Clan?.Name ?? TextObject.GetEmpty());
        LocalizationHelper.SetNumericVariable(text, "DAYS", Math.Max(1, (int) Math.Ceiling((_startTime.IsFuture ? _startTime : _endTime).RemainingDaysFromNow)));
        LocalizationHelper.SetNumericVariable(text, "DURATION", Math.Max(1, (int) Math.Ceiling((_endTime - _startTime).ToDays)));
        float scale = GrandTournamentPrizeManager.GetRewardScale(IsKingdomTournament);
        text.SetTextVariable("GOLD", (int) (ModSettings.GrandTournamentGoldPrize * scale));
        text.SetTextVariable("RENOWN", (int) (ModSettings.GrandTournamentRenownReward * scale));
        text.SetTextVariable("INFLUENCE", _winner?.Clan?.Kingdom != null ? (int) (ModSettings.GrandTournamentInfluenceReward * scale) : 0);
        return text;
    }

    private string SelectDetailsTemplate()
    {
        if (IsResolved)
        {
            return IsKingdomTournament
                ? Texts.Ids.NotificationKingdomWinner
                : Texts.Ids.NotificationGrandWinner;
        }
        if (_startTime.IsFuture)
        {
            return IsKingdomTournament
                ? Texts.Ids.NotificationKingdomAnnounced
                : Texts.Ids.NotificationGrandAnnounced;
        }
        if (_endTime.IsFuture)
        {
            return IsKingdomTournament
                ? Texts.Ids.NotificationKingdomUnderway
                : Texts.Ids.NotificationGrandUnderway;
        }
        return Texts.Ids.NotificationConcluded;
    }
}