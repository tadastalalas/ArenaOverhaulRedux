using ArenaOverhaul.Core;
using ArenaOverhaul.Data;

using HarmonyLib;

using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapNotificationTypes;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ArenaOverhaul.UI;

//Public: instantiated by MapNotificationVM via Activator.
public class GrandTournamentMapNotificationVM : MapNotificationItemBaseVM
{
    //MapNotificationItemBaseVM.GoToMapPosition is internal.
    private static readonly MethodInfo? GoToMapPositionMethod = AccessTools.Method(typeof(MapNotificationItemBaseVM), "GoToMapPosition");

    public GrandTournamentMapNotificationVM(GrandTournamentMapNotification data) : base(data)
    {
        NotificationIdentifier = data.IsKingdomTournament
            ? "custom_arena_overhaul_kingdom_tournament_map_notification"
            : "custom_arena_overhaul_grand_tournament_map_notification";
        _onInspect = () => ShowDetails(data);
    }

    private void ShowDetails(GrandTournamentMapNotification data)
    {
        string title = data.TitleText.ToString();
        string details = data.GetDetailsText().ToString();
        string close = new TextObject(Texts.Ids.NotificationClose).ToString();
        InquiryData inquiry = data.IsResolved
            ? new(title, details, true, false, close, string.Empty, null, null)
            : new(title, details, true, true, new TextObject(Texts.Ids.NotificationShowOnMap).ToString(), close,
                () => GoToMapPositionMethod?.Invoke(this, [data.Town.Settlement.Position]), null);
        InformationManager.ShowInquiry(inquiry, true);
    }
}