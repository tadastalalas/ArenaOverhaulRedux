using ArenaOverhaul.Core;
using ArenaOverhaul.Data;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace ArenaOverhaul.Managers;

internal static class GrandTournamentNotifier
{
    internal static void NotifyAnnounced(GrandTournamentGame game)
    {
        if (!ModSettings.GrandTournamentAnnouncePopup || !IsRelevantToPlayer(game.HostKingdom))
            return;

        TextObject description = new(Texts.Ids.NotificationAnnouncedIn);
        description.SetTextVariable("TOWN", game.Town.Name);
        Push(new GrandTournamentMapNotification(game, null, description));
    }

    internal static void NotifyResolved(GrandTournamentGame game, Hero winner)
    {
        if (!ModSettings.GrandTournamentResultPopup || !IsRelevantToPlayer(game.HostKingdom))
            return;

        TextObject description = new(Texts.Ids.NotificationTriumphedIn);
        description.SetTextVariable("WINNER", winner.Name);
        description.SetTextVariable("TOWN", game.Town.Name);
        Push(new GrandTournamentMapNotification(game, winner, description));
    }

    //Grand: always. Kingdom: only the one the player may compete in (mirrors GrandTournamentGame.CanPlayerAttend).
    private static bool IsRelevantToPlayer(Kingdom? kingdom) =>
        kingdom is null || (Clan.PlayerClan.Kingdom == kingdom && !Clan.PlayerClan.IsUnderMercenaryService);

    private static void Push(GrandTournamentMapNotification notification) =>
        Campaign.Current?.CampaignInformationManager?.NewMapNoticeAdded(notification);
}