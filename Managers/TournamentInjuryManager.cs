using ArenaOverhaul.Core;
using ArenaOverhaul.Data;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Localization;

using MathF = TaleWorlds.Library.MathF;

namespace ArenaOverhaul.Managers;

internal static class TournamentInjuryManager
{
    internal static void ApplyMatchInjuries(TournamentMatch? match)
    {
        if (match is null || !ModSettings.TournamentEnableInjuries)
            return;
        foreach (TournamentParticipant participant in match.Participants.Where(p => !match.Winners.Contains(p)))
            InjureHero(participant.Character);
    }

    internal static void ApplyMatchInjuries(TeamTournamentMatch? match)
    {
        if (match is null || !ModSettings.TournamentEnableInjuries)
            return;
        foreach (TeamTournamentMember member in match.Teams.Where(t => !match.Winners.Contains(t)).SelectMany(t => t.Members))
            InjureHero(member.Character);
    }

    private static void InjureHero(CharacterObject? character)
    {
        if (character is null || !character.IsHero || character.HeroObject is not Hero hero || !hero.IsAlive)
            return;
        int loss = MathF.Ceiling(hero.MaxHitPoints * ModSettings.TournamentInjuryHitPointsLoss / 100f);
        hero.HitPoints = MathF.Max(1, hero.HitPoints - loss);
        if (hero == Hero.MainHero)
            MessageHelper.QuickInformationMessage(new TextObject(Texts.Ids.KnockedOutLostHitPoints, new() { ["HP_LOSS"] = loss }), 0, null, "");
    }
}