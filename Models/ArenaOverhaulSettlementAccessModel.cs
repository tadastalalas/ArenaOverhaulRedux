using ArenaOverhaul.Data;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace ArenaOverhaul.Models;

public class ArenaOverhaulSettlementAccessModel(SettlementAccessModel previouslyAssignedModel) : SettlementAccessModel
{
    private readonly SettlementAccessModel _previouslyAssignedModel = previouslyAssignedModel;

    public override bool CanMainHeroDoSettlementAction(Settlement settlement, SettlementAction settlementAction, out bool disableOption, out TextObject disabledText)
    {
        if (settlementAction is SettlementAction.JoinTournament or SettlementAction.WatchTournament
            && settlement?.Town is Town town
            && Campaign.Current.TournamentManager.GetTournamentGame(town) is GrandTournamentGame game
            && !game.CanPlayerAttend(settlementAction == SettlementAction.JoinTournament, out TextObject explanation))
        {
            disableOption = true;
            disabledText = explanation;
            return false;
        }
        return _previouslyAssignedModel.CanMainHeroDoSettlementAction(settlement, settlementAction, out disableOption, out disabledText);
    }

    public override void CanMainHeroEnterSettlement(Settlement settlement, out AccessDetails accessDetails) =>
        _previouslyAssignedModel.CanMainHeroEnterSettlement(settlement, out accessDetails);

    public override void CanMainHeroEnterLordsHall(Settlement settlement, out AccessDetails accessDetails) =>
        _previouslyAssignedModel.CanMainHeroEnterLordsHall(settlement, out accessDetails);

    public override void CanMainHeroEnterDungeon(Settlement settlement, out AccessDetails accessDetails) =>
        _previouslyAssignedModel.CanMainHeroEnterDungeon(settlement, out accessDetails);

    public override bool CanMainHeroAccessLocation(Settlement settlement, string locationId, out bool disableOption, out TextObject disabledText) =>
        _previouslyAssignedModel.CanMainHeroAccessLocation(settlement, locationId, out disableOption, out disabledText);

    public override bool IsRequestMeetingOptionAvailable(Settlement settlement, out bool disableOption, out TextObject disabledText) =>
        _previouslyAssignedModel.IsRequestMeetingOptionAvailable(settlement, out disableOption, out disabledText);
}