using ArenaOverhaul.Core;

using Bannerlord.ButterLib.Common.Helpers;

using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ArenaOverhaul.Managers;

internal static class TournamentHostingManager
{
    private static Dictionary<Town, CampaignTime> HostedTournaments => AOArenaBehaviorManager.Instance!.PlayerHostedTournaments;

    internal static int GetGoldCost(Town town)
    {
        float prosperityFactor = MBMath.ClampFloat(town.Prosperity / 3000f, 0.5f, 2f);
        float clanFactor = 1f + Clan.PlayerClan.Tier * 0.25f;
        float renownFactor = 1f + MathHelper.GetSoftCappedValue(Clan.PlayerClan.Renown) / 2000f;
        return (int) (Math.Round(ModSettings.TournamentHostingBaseGold * prosperityFactor * clanFactor * renownFactor / 50.0) * 50);
    }

    internal static int GetInfluenceCost() =>
        (int) Math.Round(ModSettings.TournamentHostingBaseInfluence * (1f + Clan.PlayerClan.Tier * 0.5f));

    internal static bool IsOnCooldown(Town town, out float daysLeft)
    {
        daysLeft = HostedTournaments.TryGetValue(town, out CampaignTime last) ? ModSettings.TournamentHostingCooldownDays - last.ElapsedDaysUntilNow : 0f;
        return daysLeft > 0f;
    }

    // No alliance concept in vanilla: own town, same map faction, or decent standing with the owner clan's leader.
    internal static bool IsAffiliated(Town town) =>
        town.OwnerClan == Clan.PlayerClan
        || town.Settlement.MapFaction == Clan.PlayerClan.MapFaction
        || (town.OwnerClan?.Leader is Hero owner && Hero.MainHero.GetRelation(owner) >= 10);

    internal static bool CanHost(Town town, out TextObject explanation)
    {
        int gold = GetGoldCost(town);
        int influence = GetInfluenceCost();

        if (town.Settlement.IsUnderSiege || FactionManager.IsAtWarAgainstFaction(town.Settlement.MapFaction, Clan.PlayerClan.MapFaction))
        {
            explanation = new(Texts.Ids.HostingNoFestivities);
            return false;
        }
        if (ModSettings.TournamentHostingRequireAffiliation && !IsAffiliated(town))
        {
            explanation = new(Texts.Ids.HostingNotAffiliated);
            return false;
        }
        if (IsOnCooldown(town, out float daysLeft))
        {
            explanation = new(Texts.Ids.HostingCooldown);
            LocalizationHelper.SetNumericVariable(explanation, "DAYS", (int) Math.Ceiling(daysLeft));
            return false;
        }

        explanation = new(Hero.MainHero.Gold < gold || Clan.PlayerClan.Influence < influence
            ? Texts.Ids.HostingCannotAfford
            : Texts.Ids.HostingCost);
        explanation.SetTextVariable("GOLD", gold);
        explanation.SetTextVariable("INFLUENCE", influence);
        explanation.SetTextVariable("HAS_INFLUENCE", influence > 0 ? 1 : 0);
        return Hero.MainHero.Gold >= gold && Clan.PlayerClan.Influence >= influence;
    }

    internal static void Host(Town town)
    {
        int gold = GetGoldCost(town);
        int influence = GetInfluenceCost();

        GiveGoldAction.ApplyForCharacterToSettlement(Hero.MainHero, town.Settlement, gold);
        if (influence > 0)
            ChangeClanInfluenceAction.Apply(Clan.PlayerClan, -influence);

        Campaign.Current.TournamentManager.AddTournament(Campaign.Current.Models.TournamentModel.CreateTournament(town));
        HostedTournaments[town] = CampaignTime.Now;

        for (int i = 0; i < town.Settlement.Notables.Count; i++)
            ChangeRelationAction.ApplyPlayerRelation(town.Settlement.Notables[i], 1, true, false);

        TextObject message = new(Texts.Ids.HostingDone);
        message.SetTextVariable("GOLD", gold);
        message.SetTextVariable("INFLUENCE", influence);
        message.SetTextVariable("HAS_INFLUENCE", influence > 0 ? 1 : 0);
        message.SetTextVariable("TOWN", town.Name);
        MessageHelper.QuickInformationMessage(message);
    }
}