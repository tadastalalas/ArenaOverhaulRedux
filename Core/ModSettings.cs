using ArenaOverhaul.Data;
using ArenaOverhaul.Diagnostics;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using TaleWorlds.CampaignSystem;

namespace ArenaOverhaul.Core;

// The only place mod code reads settings from. MCM loaded -> MCM. MCM not loaded -> ModConfiguration.xml.
internal static class ModSettings
{
    private const string McmAssemblyName = "MCMv5";

    private static ModConfiguration _xml = new();
    private static XmlSerializer? _serializer;
    private static bool _useMcm;
    private static bool _heroPracticeSettingsInMcm;

    #region Default Arena Practice settings for heroes
    internal static bool HeroDefaultEnableLoadoutChoice => _useMcm ? McmHeroDefaultEnableLoadoutChoice() : _xml.HeroPracticeDefaults.HeroDefaultEnableLoadoutChoice;
    internal static bool HeroDefaultOnlyPriorityLoadouts => _useMcm ? McmHeroDefaultOnlyPriorityLoadouts() : _xml.HeroPracticeDefaults.HeroDefaultOnlyPriorityLoadouts;
    internal static bool HeroDefaultPrioritizeExpensiveEquipment => _useMcm ? McmHeroDefaultPrioritizeExpensiveEquipment() : _xml.HeroPracticeDefaults.HeroDefaultPrioritizeExpensiveEquipment;
    #endregion

    #region Arena Practice settings
    internal static bool PracticeEnableAgentSwitching => _useMcm ? McmPracticeEnableAgentSwitching() : _xml.ArenaPractice.PracticeEnableAgentSwitching;
    internal static int PracticeTotalParticipants => _useMcm ? McmPracticeTotalParticipants() : _xml.ArenaPractice.PracticeTotalParticipants;
    internal static int PracticeActiveParticipants => _useMcm ? McmPracticeActiveParticipants() : _xml.ArenaPractice.PracticeActiveParticipants;
    internal static int PracticeActiveParticipantsMinimum => _useMcm ? McmPracticeActiveParticipantsMinimum() : _xml.ArenaPractice.PracticeActiveParticipantsMinimum;
    internal static int PracticeInitialParticipants => _useMcm ? McmPracticeInitialParticipants() : _xml.ArenaPractice.PracticeInitialParticipants;
    internal static int PracticeAITeamsCount => _useMcm ? McmPracticeAITeamsCount() : _xml.ArenaPractice.PracticeAITeamsCount;
    internal static bool PracticeEnableLoadoutChoice => _useMcm ? McmPracticeEnableLoadoutChoice() : _xml.ArenaPractice.PracticeEnableLoadoutChoice;
    internal static int PracticeLoadoutChoiceCost => _useMcm ? McmPracticeLoadoutChoiceCost() : _xml.ArenaPractice.PracticeLoadoutChoiceCost;
    internal static PracticeEquipmentType PracticeEquipment => _useMcm ? McmPracticeEquipment() : _xml.ArenaPractice.PracticeEquipment;
    internal static int PracticeValorReward1 => _useMcm ? McmPracticeValorReward1() : _xml.ArenaPractice.PracticeValorReward1;
    internal static int PracticeValorReward2 => _useMcm ? McmPracticeValorReward2() : _xml.ArenaPractice.PracticeValorReward2;
    internal static int PracticeValorReward3 => _useMcm ? McmPracticeValorReward3() : _xml.ArenaPractice.PracticeValorReward3;
    internal static int PracticeValorReward4 => _useMcm ? McmPracticeValorReward4() : _xml.ArenaPractice.PracticeValorReward4;
    internal static int PracticeValorReward5 => _useMcm ? McmPracticeValorReward5() : _xml.ArenaPractice.PracticeValorReward5;
    internal static ChampionPrizeCalculationType PracticeChampionPrizeCalculation => _useMcm ? McmPracticeChampionPrizeCalculation() : _xml.ArenaPractice.PracticeChampionPrizeCalculation;
    internal static int PracticeChampionReward => _useMcm ? McmPracticeChampionReward() : _xml.ArenaPractice.PracticeChampionReward;
    internal static float PracticeExperienceRate => _useMcm ? McmPracticeExperienceRate() : _xml.ArenaPractice.PracticeExperienceRate;
    #endregion

    #region Expansive Arena Practice settings
    internal static bool ExpansivePracticeEnableAgentSwitching => _useMcm ? McmExpansivePracticeEnableAgentSwitching() : _xml.ExpansiveArenaPractice.ExpansivePracticeEnableAgentSwitching;
    internal static int ExpansivePracticeTotalParticipants => _useMcm ? McmExpansivePracticeTotalParticipants() : _xml.ExpansiveArenaPractice.ExpansivePracticeTotalParticipants;
    internal static int ExpansivePracticeActiveParticipants => _useMcm ? McmExpansivePracticeActiveParticipants() : _xml.ExpansiveArenaPractice.ExpansivePracticeActiveParticipants;
    internal static int ExpansivePracticeActiveParticipantsMinimum => _useMcm ? McmExpansivePracticeActiveParticipantsMinimum() : _xml.ExpansiveArenaPractice.ExpansivePracticeActiveParticipantsMinimum;
    internal static int ExpansivePracticeInitialParticipants => _useMcm ? McmExpansivePracticeInitialParticipants() : _xml.ExpansiveArenaPractice.ExpansivePracticeInitialParticipants;
    internal static int ExpansivePracticeAITeamsCount => _useMcm ? McmExpansivePracticeAITeamsCount() : _xml.ExpansiveArenaPractice.ExpansivePracticeAITeamsCount;
    internal static bool ExpansivePracticeEnableLoadoutChoice => _useMcm ? McmExpansivePracticeEnableLoadoutChoice() : _xml.ExpansiveArenaPractice.ExpansivePracticeEnableLoadoutChoice;
    internal static int ExpansivePracticeLoadoutChoiceCost => _useMcm ? McmExpansivePracticeLoadoutChoiceCost() : _xml.ExpansiveArenaPractice.ExpansivePracticeLoadoutChoiceCost;
    internal static PracticeEquipmentType ExpansivePracticeEquipment => _useMcm ? McmExpansivePracticeEquipment() : _xml.ExpansiveArenaPractice.ExpansivePracticeEquipment;
    internal static int ExpansivePracticeValorReward1 => _useMcm ? McmExpansivePracticeValorReward1() : _xml.ExpansiveArenaPractice.ExpansivePracticeValorReward1;
    internal static int ExpansivePracticeValorReward2 => _useMcm ? McmExpansivePracticeValorReward2() : _xml.ExpansiveArenaPractice.ExpansivePracticeValorReward2;
    internal static int ExpansivePracticeValorReward3 => _useMcm ? McmExpansivePracticeValorReward3() : _xml.ExpansiveArenaPractice.ExpansivePracticeValorReward3;
    internal static int ExpansivePracticeValorReward4 => _useMcm ? McmExpansivePracticeValorReward4() : _xml.ExpansiveArenaPractice.ExpansivePracticeValorReward4;
    internal static int ExpansivePracticeValorReward5 => _useMcm ? McmExpansivePracticeValorReward5() : _xml.ExpansiveArenaPractice.ExpansivePracticeValorReward5;
    internal static ChampionPrizeCalculationType ExpansivePracticeChampionPrizeCalculation => _useMcm ? McmExpansivePracticeChampionPrizeCalculation() : _xml.ExpansiveArenaPractice.ExpansivePracticeChampionPrizeCalculation;
    internal static int ExpansivePracticeChampionReward => _useMcm ? McmExpansivePracticeChampionReward() : _xml.ExpansiveArenaPractice.ExpansivePracticeChampionReward;
    internal static bool ExpansivePracticeEnableViewerExperienceGain => _useMcm ? McmExpansivePracticeEnableViewerExperienceGain() : _xml.ExpansiveArenaPractice.ExpansivePracticeEnableViewerExperienceGain;
    internal static float ExpansivePracticeExperienceRate => _useMcm ? McmExpansivePracticeExperienceRate() : _xml.ExpansiveArenaPractice.ExpansivePracticeExperienceRate;
    #endregion

    #region Parry Practice settings
    internal static bool ParryPracticeEnabled => _useMcm ? McmParryPracticeEnabled() : _xml.ParryPractice.ParryPracticeEnabled;
    internal static int ParryPracticeSetupCost => _useMcm ? McmParryPracticeSetupCost() : _xml.ParryPractice.ParryPracticeSetupCost;
    internal static bool ParryPracticeEnableAgentSwitching => _useMcm ? McmParryPracticeEnableAgentSwitching() : _xml.ParryPractice.ParryPracticeEnableAgentSwitching;
    internal static int ParryPracticeTotalParticipants => _useMcm ? McmParryPracticeTotalParticipants() : _xml.ParryPractice.ParryPracticeTotalParticipants;
    internal static int ParryPracticeActiveParticipants => _useMcm ? McmParryPracticeActiveParticipants() : _xml.ParryPractice.ParryPracticeActiveParticipants;
    internal static int ParryPracticeActiveParticipantsMinimum => _useMcm ? McmParryPracticeActiveParticipantsMinimum() : _xml.ParryPractice.ParryPracticeActiveParticipantsMinimum;
    internal static int ParryPracticeInitialParticipants => _useMcm ? McmParryPracticeInitialParticipants() : _xml.ParryPractice.ParryPracticeInitialParticipants;
    internal static int ParryPracticeAITeamsCount => _useMcm ? McmParryPracticeAITeamsCount() : _xml.ParryPractice.ParryPracticeAITeamsCount;
    internal static bool ParryPracticeEnableLoadoutChoice => _useMcm ? McmParryPracticeEnableLoadoutChoice() : _xml.ParryPractice.ParryPracticeEnableLoadoutChoice;
    internal static int ParryPracticeLoadoutChoiceCost => _useMcm ? McmParryPracticeLoadoutChoiceCost() : _xml.ParryPractice.ParryPracticeLoadoutChoiceCost;
    internal static PracticeEquipmentType ParryPracticeEquipment => _useMcm ? McmParryPracticeEquipment() : _xml.ParryPractice.ParryPracticeEquipment;
    internal static float ParryPracticeExperienceRate => _useMcm ? McmParryPracticeExperienceRate() : _xml.ParryPractice.ParryPracticeExperienceRate;
    #endregion

    #region Team Arena Practice settings
    internal static bool TeamPracticeEnabled => _useMcm ? McmTeamPracticeEnabled() : _xml.TeamArenaPractice.TeamPracticeEnabled;
    internal static int TeamPracticeSetupCost => _useMcm ? McmTeamPracticeSetupCost() : _xml.TeamArenaPractice.TeamPracticeSetupCost;
    internal static bool TeamPracticeEnableAgentSwitching => _useMcm ? McmTeamPracticeEnableAgentSwitching() : _xml.TeamArenaPractice.TeamPracticeEnableAgentSwitching;
    internal static int TeamPracticeTotalParticipants => _useMcm ? McmTeamPracticeTotalParticipants() : _xml.TeamArenaPractice.TeamPracticeTotalParticipants;
    internal static int TeamPracticeActiveParticipants => _useMcm ? McmTeamPracticeActiveParticipants() : _xml.TeamArenaPractice.TeamPracticeActiveParticipants;
    internal static int TeamPracticeActiveParticipantsMinimum => _useMcm ? McmTeamPracticeActiveParticipantsMinimum() : _xml.TeamArenaPractice.TeamPracticeActiveParticipantsMinimum;
    internal static int TeamPracticeInitialParticipants => _useMcm ? McmTeamPracticeInitialParticipants() : _xml.TeamArenaPractice.TeamPracticeInitialParticipants;
    internal static int TeamPracticeAITeamsCount => _useMcm ? McmTeamPracticeAITeamsCount() : _xml.TeamArenaPractice.TeamPracticeAITeamsCount;
    internal static bool TeamPracticeEnableLoadoutChoice => _useMcm ? McmTeamPracticeEnableLoadoutChoice() : _xml.TeamArenaPractice.TeamPracticeEnableLoadoutChoice;
    internal static int TeamPracticeLoadoutChoiceCost => _useMcm ? McmTeamPracticeLoadoutChoiceCost() : _xml.TeamArenaPractice.TeamPracticeLoadoutChoiceCost;
    internal static PracticeEquipmentType TeamPracticeEquipment => _useMcm ? McmTeamPracticeEquipment() : _xml.TeamArenaPractice.TeamPracticeEquipment;
    internal static int TeamPracticeValorReward1 => _useMcm ? McmTeamPracticeValorReward1() : _xml.TeamArenaPractice.TeamPracticeValorReward1;
    internal static int TeamPracticeValorReward2 => _useMcm ? McmTeamPracticeValorReward2() : _xml.TeamArenaPractice.TeamPracticeValorReward2;
    internal static int TeamPracticeValorReward3 => _useMcm ? McmTeamPracticeValorReward3() : _xml.TeamArenaPractice.TeamPracticeValorReward3;
    internal static int TeamPracticeValorReward4 => _useMcm ? McmTeamPracticeValorReward4() : _xml.TeamArenaPractice.TeamPracticeValorReward4;
    internal static int TeamPracticeValorReward5 => _useMcm ? McmTeamPracticeValorReward5() : _xml.TeamArenaPractice.TeamPracticeValorReward5;
    internal static ChampionPrizeCalculationType TeamPracticeChampionPrizeCalculation => _useMcm ? McmTeamPracticeChampionPrizeCalculation() : _xml.TeamArenaPractice.TeamPracticeChampionPrizeCalculation;
    internal static int TeamPracticeChampionReward => _useMcm ? McmTeamPracticeChampionReward() : _xml.TeamArenaPractice.TeamPracticeChampionReward;
    internal static bool TeamPracticeEnableViewerExperienceGain => _useMcm ? McmTeamPracticeEnableViewerExperienceGain() : _xml.TeamArenaPractice.TeamPracticeEnableViewerExperienceGain;
    internal static float TeamPracticeExperienceRate => _useMcm ? McmTeamPracticeExperienceRate() : _xml.TeamArenaPractice.TeamPracticeExperienceRate;
    internal static int TeamPracticeTeamOneColor => _useMcm ? McmTeamPracticeTeamOneColor() : _xml.TeamArenaPractice.TeamPracticeTeamOneColor;
    internal static int TeamPracticeTeamTwoColor => _useMcm ? McmTeamPracticeTeamTwoColor() : _xml.TeamArenaPractice.TeamPracticeTeamTwoColor;
    internal static int TeamPracticeTeamThreeColor => _useMcm ? McmTeamPracticeTeamThreeColor() : _xml.TeamArenaPractice.TeamPracticeTeamThreeColor;
    internal static int TeamPracticeTeamFourColor => _useMcm ? McmTeamPracticeTeamFourColor() : _xml.TeamArenaPractice.TeamPracticeTeamFourColor;
    internal static int TeamPracticeTeamFiveColor => _useMcm ? McmTeamPracticeTeamFiveColor() : _xml.TeamArenaPractice.TeamPracticeTeamFiveColor;
    internal static int TeamPracticeTeamSixColor => _useMcm ? McmTeamPracticeTeamSixColor() : _xml.TeamArenaPractice.TeamPracticeTeamSixColor;
    #endregion

    #region Tournament settings
    internal static int TournamentMaximumBet => _useMcm ? McmTournamentMaximumBet() : _xml.Tournaments.TournamentMaximumBet;
    internal static bool TournamentEnableRandomizedBettingOdds => _useMcm ? McmTournamentEnableRandomizedBettingOdds() : _xml.Tournaments.TournamentEnableRandomizedBettingOdds;
    internal static bool TournamentAllowNotablesParticipation => _useMcm ? McmTournamentAllowNotablesParticipation() : _xml.Tournaments.TournamentAllowNotablesParticipation;
    internal static PrizeRerollCondition TournamentPrizeRerollCondition => _useMcm ? McmTournamentPrizeRerollCondition() : _xml.Tournaments.TournamentPrizeRerollCondition;
    internal static bool TournamentEnableGoldPrizes => _useMcm ? McmTournamentEnableGoldPrizes() : _xml.Tournaments.TournamentEnableGoldPrizes;
    internal static bool TournamentEnablePrizeScaling => _useMcm ? McmTournamentEnablePrizeScaling() : _xml.Tournaments.TournamentEnablePrizeScaling;
    internal static bool TournamentEnableHighQualityPrizes => _useMcm ? McmTournamentEnableHighQualityPrizes() : _xml.Tournaments.TournamentEnableHighQualityPrizes;
    internal static CultureRestrictedPrizesMode TournamentCultureRestrictedPrizes => _useMcm ? McmTournamentCultureRestrictedPrizes() : _xml.Tournaments.TournamentCultureRestrictedPrizes;
    internal static int TournamentRoundWonReward => _useMcm ? McmTournamentRoundWonReward() : _xml.Tournaments.TournamentRoundWonReward;
    internal static int TournamentBaseRenownReward => _useMcm ? McmTournamentBaseRenownReward() : _xml.Tournaments.TournamentBaseRenownReward;
    internal static int TournamentTakedownRenownReward => _useMcm ? McmTournamentTakedownRenownReward() : _xml.Tournaments.TournamentTakedownRenownReward;
    internal static int TournamentInfluenceReward => _useMcm ? McmTournamentInfluenceReward() : _xml.Tournaments.TournamentInfluenceReward;
    internal static float TournamentExperienceRate => _useMcm ? McmTournamentExperienceRate() : _xml.Tournaments.TournamentExperienceRate;
    internal static bool TournamentEnableInjuries => _useMcm ? McmTournamentEnableInjuries() : _xml.Tournaments.TournamentEnableInjuries;
    internal static int TournamentInjuryHitPointsLoss => _useMcm ? McmTournamentInjuryHitPointsLoss() : _xml.Tournaments.TournamentInjuryHitPointsLoss;
    internal static bool TournamentHostingEnabled => _useMcm ? McmTournamentHostingEnabled() : _xml.Tournaments.TournamentHostingEnabled;
    internal static int TournamentHostingBaseGold => _useMcm ? McmTournamentHostingBaseGold() : _xml.Tournaments.TournamentHostingBaseGold;
    internal static int TournamentHostingBaseInfluence => _useMcm ? McmTournamentHostingBaseInfluence() : _xml.Tournaments.TournamentHostingBaseInfluence;
    internal static int TournamentHostingCooldownDays => _useMcm ? McmTournamentHostingCooldownDays() : _xml.Tournaments.TournamentHostingCooldownDays;
    internal static bool TournamentHostingRequireAffiliation => _useMcm ? McmTournamentHostingRequireAffiliation() : _xml.Tournaments.TournamentHostingRequireAffiliation;
    internal static bool GrandTournamentEnabled => _useMcm ? McmGrandTournamentEnabled() : _xml.Tournaments.GrandTournamentEnabled;
    internal static bool KingdomTournamentsEnabled => _useMcm ? McmKingdomTournamentsEnabled() : _xml.Tournaments.KingdomTournamentsEnabled;
    internal static GrandTournamentSeason GrandTournamentSeason => _useMcm ? McmGrandTournamentSeason() : _xml.Tournaments.GrandTournamentSeason;
    internal static int GrandTournamentDayOfSeason => _useMcm ? McmGrandTournamentDayOfSeason() : _xml.Tournaments.GrandTournamentDayOfSeason;
    internal static int KingdomTournamentDayOfSeason => _useMcm ? McmKingdomTournamentDayOfSeason() : _xml.Tournaments.KingdomTournamentDayOfSeason;
    internal static int GrandTournamentAnnounceDays => _useMcm ? McmGrandTournamentAnnounceDays() : _xml.Tournaments.GrandTournamentAnnounceDays;
    internal static int GrandTournamentDurationDays => _useMcm ? McmGrandTournamentDurationDays() : _xml.Tournaments.GrandTournamentDurationDays;
    internal static bool GrandTournamentLegendaryPrize => _useMcm ? McmGrandTournamentLegendaryPrize() : _xml.Tournaments.GrandTournamentLegendaryPrize;
    internal static int GrandTournamentGoldPrize => _useMcm ? McmGrandTournamentGoldPrize() : _xml.Tournaments.GrandTournamentGoldPrize;
    internal static int GrandTournamentRenownReward => _useMcm ? McmGrandTournamentRenownReward() : _xml.Tournaments.GrandTournamentRenownReward;
    internal static int GrandTournamentInfluenceReward => _useMcm ? McmGrandTournamentInfluenceReward() : _xml.Tournaments.GrandTournamentInfluenceReward;
    internal static int GrandTournamentLeaderboardWeight => _useMcm ? McmGrandTournamentLeaderboardWeight() : _xml.Tournaments.GrandTournamentLeaderboardWeight;
    internal static int KingdomTournamentLeaderboardWeight => _useMcm ? McmKingdomTournamentLeaderboardWeight() : _xml.Tournaments.KingdomTournamentLeaderboardWeight;
    internal static int KingdomTournamentRewardPercent => _useMcm ? McmKingdomTournamentRewardPercent() : _xml.Tournaments.KingdomTournamentRewardPercent;
    internal static bool GrandTournamentAnnouncePopup => _useMcm ? McmGrandTournamentAnnouncePopup() : _xml.Tournaments.GrandTournamentAnnouncePopup;
    internal static bool GrandTournamentResultPopup => _useMcm ? McmGrandTournamentResultPopup() : _xml.Tournaments.GrandTournamentResultPopup;
    internal static int KingdomTournamentIntervalSeasons => _useMcm ? McmKingdomTournamentIntervalSeasons() : _xml.Tournaments.KingdomTournamentIntervalSeasons;
    internal static bool TeamTournamentEnabled => _useMcm ? McmTeamTournamentEnabled() : _xml.Tournaments.TeamTournamentEnabled;
    internal static int TeamTournamentTeamSizeMin => _useMcm ? McmTeamTournamentTeamSizeMin() : _xml.Tournaments.TeamTournamentTeamSizeMin;
    internal static int TeamTournamentTeamSizeMax => _useMcm ? McmTeamTournamentTeamSizeMax() : _xml.Tournaments.TeamTournamentTeamSizeMax;
    internal static int TeamTournamentTeamsCountMax => _useMcm ? McmTeamTournamentTeamsCountMax() : _xml.Tournaments.TeamTournamentTeamsCountMax;
    internal static bool TeamTournamentScoresForWinningTeam => _useMcm ? McmTeamTournamentScoresForWinningTeam() : _xml.Tournaments.TeamTournamentScoresForWinningTeam;
    internal static int TeamTournamentTeamOneColor => _useMcm ? McmTeamTournamentTeamOneColor() : _xml.Tournaments.TeamTournamentTeamOneColor;
    internal static int TeamTournamentTeamTwoColor => _useMcm ? McmTeamTournamentTeamTwoColor() : _xml.Tournaments.TeamTournamentTeamTwoColor;
    internal static int TeamTournamentTeamThreeColor => _useMcm ? McmTeamTournamentTeamThreeColor() : _xml.Tournaments.TeamTournamentTeamThreeColor;
    internal static int TeamTournamentTeamFourColor => _useMcm ? McmTeamTournamentTeamFourColor() : _xml.Tournaments.TeamTournamentTeamFourColor;
    #endregion

    #region Compatibility settings
    internal static int PracticeLoadoutStages => _useMcm ? McmPracticeLoadoutStages() : _xml.Compatibility.PracticeLoadoutStages;
    #endregion

    #region Technical settings
    internal static bool LoggingEnabled => _useMcm ? McmLoggingEnabled() : _xml.TechnicalSettings.LoggingEnabled;
    #endregion

    private static XmlSerializer Serializer => _serializer ??= new XmlSerializer(typeof(ModConfiguration));

    // Called on every game start, and once at the main menu before the Harmony patches are applied.
    internal static void Reload()
    {
        _useMcm = false;
        _xml = new ModConfiguration();

        if (IsMcmLoaded())
        {
            if (McmHasSettings())
            {
                _useMcm = true;
                return;
            }

            AorLog.Warn("MCM is loaded but returned no settings for this mod. Using " + ModInfo.ConfigurationFileName + " instead.");
        }

        _xml = LoadFromFile();
    }

    #region Per-hero practice settings
    // MCM: the per-save "AO Hero Practice Settings" menu fills and edits the entries.
    // No MCM: every clan hero gets the "Default Arena Practice settings for heroes" values and no weapon priorities.
    internal static void RegisterHeroPracticeSettings(Dictionary<Hero, HeroPracticeSettings> heroPracticeSettings)
    {
        if (_useMcm)
        {
            McmRegisterHeroPracticeSettings(heroPracticeSettings);
            _heroPracticeSettingsInMcm = true;
            return;
        }

        AddDefaultHeroPracticeSettings(heroPracticeSettings, Hero.MainHero);
        foreach (Hero hero in Hero.FindAll(x => x.IsAlive && !x.IsChild && x.Clan == Clan.PlayerClan && x != Hero.MainHero))
        {
            AddDefaultHeroPracticeSettings(heroPracticeSettings, hero);
        }
    }

    internal static void UnregisterHeroPracticeSettings()
    {
        if (!_heroPracticeSettingsInMcm)
            return;

        _heroPracticeSettingsInMcm = false;
        McmUnregisterHeroPracticeSettings();
    }

    private static void AddDefaultHeroPracticeSettings(Dictionary<Hero, HeroPracticeSettings> heroPracticeSettings, Hero? hero)
    {
        if (hero != null && !heroPracticeSettings.ContainsKey(hero))
            heroPracticeSettings[hero] = new HeroPracticeSettings();
    }
    #endregion

    private static bool IsMcmLoaded()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            if (assemblies[i].GetName().Name == McmAssemblyName) return true;
        }

        return false;
    }

    #region MCM access
    // Every method that touches MCMSettings must be NoInlining and may only be called when MCM is loaded.
    // Otherwise the game tries to load MCM's classes and crashes when MCM is not installed.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmHasSettings() => MCMSettings.Instance != null;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void McmRegisterHeroPracticeSettings(Dictionary<Hero, HeroPracticeSettings> heroPracticeSettings) => MCMSettings.RegisterHeroPracticeSettings(heroPracticeSettings);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void McmUnregisterHeroPracticeSettings() => MCMSettings.UnregisterHeroPracticeSettings();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmHeroDefaultEnableLoadoutChoice() => MCMSettings.Instance?.HeroDefaultEnableLoadoutChoice ?? ModConfiguration.HeroDefaultEnableLoadoutChoiceDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmHeroDefaultOnlyPriorityLoadouts() => MCMSettings.Instance?.HeroDefaultOnlyPriorityLoadouts ?? ModConfiguration.HeroDefaultOnlyPriorityLoadoutsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmHeroDefaultPrioritizeExpensiveEquipment() => MCMSettings.Instance?.HeroDefaultPrioritizeExpensiveEquipment ?? ModConfiguration.HeroDefaultPrioritizeExpensiveEquipmentDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmPracticeEnableAgentSwitching() => MCMSettings.Instance?.PracticeEnableAgentSwitching ?? ModConfiguration.PracticeEnableAgentSwitchingDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmPracticeTotalParticipants() => MCMSettings.Instance?.PracticeTotalParticipants ?? ModConfiguration.PracticeTotalParticipantsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmPracticeActiveParticipants() => MCMSettings.Instance?.PracticeActiveParticipants ?? ModConfiguration.PracticeActiveParticipantsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmPracticeActiveParticipantsMinimum() => MCMSettings.Instance?.PracticeActiveParticipantsMinimum ?? ModConfiguration.PracticeActiveParticipantsMinimumDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmPracticeInitialParticipants() => MCMSettings.Instance?.PracticeInitialParticipants ?? ModConfiguration.PracticeInitialParticipantsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmPracticeAITeamsCount() => MCMSettings.Instance?.PracticeAITeamsCount ?? ModConfiguration.PracticeAITeamsCountDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmPracticeEnableLoadoutChoice() => MCMSettings.Instance?.PracticeEnableLoadoutChoice ?? ModConfiguration.PracticeEnableLoadoutChoiceDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmPracticeLoadoutChoiceCost() => MCMSettings.Instance?.PracticeLoadoutChoiceCost ?? ModConfiguration.PracticeLoadoutChoiceCostDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static PracticeEquipmentType McmPracticeEquipment() => MCMSettings.Instance?.PracticeEquipment.SelectedValue.EnumValue ?? ModConfiguration.PracticeEquipmentDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmPracticeValorReward1() => MCMSettings.Instance?.PracticeValorReward1 ?? ModConfiguration.PracticeValorReward1Default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmPracticeValorReward2() => MCMSettings.Instance?.PracticeValorReward2 ?? ModConfiguration.PracticeValorReward2Default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmPracticeValorReward3() => MCMSettings.Instance?.PracticeValorReward3 ?? ModConfiguration.PracticeValorReward3Default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmPracticeValorReward4() => MCMSettings.Instance?.PracticeValorReward4 ?? ModConfiguration.PracticeValorReward4Default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmPracticeValorReward5() => MCMSettings.Instance?.PracticeValorReward5 ?? ModConfiguration.PracticeValorReward5Default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ChampionPrizeCalculationType McmPracticeChampionPrizeCalculation() => MCMSettings.Instance?.PracticeChampionPrizeCalculation.SelectedValue.EnumValue ?? ModConfiguration.PracticeChampionPrizeCalculationDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmPracticeChampionReward() => MCMSettings.Instance?.PracticeChampionReward ?? ModConfiguration.PracticeChampionRewardDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static float McmPracticeExperienceRate() => MCMSettings.Instance?.PracticeExperienceRate ?? ModConfiguration.PracticeExperienceRateDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmExpansivePracticeEnableAgentSwitching() => MCMSettings.Instance?.ExpansivePracticeEnableAgentSwitching ?? ModConfiguration.ExpansivePracticeEnableAgentSwitchingDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmExpansivePracticeTotalParticipants() => MCMSettings.Instance?.ExpansivePracticeTotalParticipants ?? ModConfiguration.ExpansivePracticeTotalParticipantsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmExpansivePracticeActiveParticipants() => MCMSettings.Instance?.ExpansivePracticeActiveParticipants ?? ModConfiguration.ExpansivePracticeActiveParticipantsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmExpansivePracticeActiveParticipantsMinimum() => MCMSettings.Instance?.ExpansivePracticeActiveParticipantsMinimum ?? ModConfiguration.ExpansivePracticeActiveParticipantsMinimumDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmExpansivePracticeInitialParticipants() => MCMSettings.Instance?.ExpansivePracticeInitialParticipants ?? ModConfiguration.ExpansivePracticeInitialParticipantsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmExpansivePracticeAITeamsCount() => MCMSettings.Instance?.ExpansivePracticeAITeamsCount ?? ModConfiguration.ExpansivePracticeAITeamsCountDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmExpansivePracticeEnableLoadoutChoice() => MCMSettings.Instance?.ExpansivePracticeEnableLoadoutChoice ?? ModConfiguration.ExpansivePracticeEnableLoadoutChoiceDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmExpansivePracticeLoadoutChoiceCost() => MCMSettings.Instance?.ExpansivePracticeLoadoutChoiceCost ?? ModConfiguration.ExpansivePracticeLoadoutChoiceCostDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static PracticeEquipmentType McmExpansivePracticeEquipment() => MCMSettings.Instance?.ExpansivePracticeEquipment.SelectedValue.EnumValue ?? ModConfiguration.ExpansivePracticeEquipmentDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmExpansivePracticeValorReward1() => MCMSettings.Instance?.ExpansivePracticeValorReward1 ?? ModConfiguration.ExpansivePracticeValorReward1Default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmExpansivePracticeValorReward2() => MCMSettings.Instance?.ExpansivePracticeValorReward2 ?? ModConfiguration.ExpansivePracticeValorReward2Default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmExpansivePracticeValorReward3() => MCMSettings.Instance?.ExpansivePracticeValorReward3 ?? ModConfiguration.ExpansivePracticeValorReward3Default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmExpansivePracticeValorReward4() => MCMSettings.Instance?.ExpansivePracticeValorReward4 ?? ModConfiguration.ExpansivePracticeValorReward4Default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmExpansivePracticeValorReward5() => MCMSettings.Instance?.ExpansivePracticeValorReward5 ?? ModConfiguration.ExpansivePracticeValorReward5Default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ChampionPrizeCalculationType McmExpansivePracticeChampionPrizeCalculation() => MCMSettings.Instance?.ExpansivePracticeChampionPrizeCalculation.SelectedValue.EnumValue ?? ModConfiguration.ExpansivePracticeChampionPrizeCalculationDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmExpansivePracticeChampionReward() => MCMSettings.Instance?.ExpansivePracticeChampionReward ?? ModConfiguration.ExpansivePracticeChampionRewardDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmExpansivePracticeEnableViewerExperienceGain() => MCMSettings.Instance?.ExpansivePracticeEnableViewerExperienceGain ?? ModConfiguration.ExpansivePracticeEnableViewerExperienceGainDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static float McmExpansivePracticeExperienceRate() => MCMSettings.Instance?.ExpansivePracticeExperienceRate ?? ModConfiguration.ExpansivePracticeExperienceRateDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmParryPracticeEnabled() => MCMSettings.Instance?.ParryPracticeEnabled ?? ModConfiguration.ParryPracticeEnabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmParryPracticeSetupCost() => MCMSettings.Instance?.ParryPracticeSetupCost ?? ModConfiguration.ParryPracticeSetupCostDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmParryPracticeEnableAgentSwitching() => MCMSettings.Instance?.ParryPracticeEnableAgentSwitching ?? ModConfiguration.ParryPracticeEnableAgentSwitchingDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmParryPracticeTotalParticipants() => MCMSettings.Instance?.ParryPracticeTotalParticipants ?? ModConfiguration.ParryPracticeTotalParticipantsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmParryPracticeActiveParticipants() => MCMSettings.Instance?.ParryPracticeActiveParticipants ?? ModConfiguration.ParryPracticeActiveParticipantsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmParryPracticeActiveParticipantsMinimum() => MCMSettings.Instance?.ParryPracticeActiveParticipantsMinimum ?? ModConfiguration.ParryPracticeActiveParticipantsMinimumDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmParryPracticeInitialParticipants() => MCMSettings.Instance?.ParryPracticeInitialParticipants ?? ModConfiguration.ParryPracticeInitialParticipantsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmParryPracticeAITeamsCount() => MCMSettings.Instance?.ParryPracticeAITeamsCount ?? ModConfiguration.ParryPracticeAITeamsCountDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmParryPracticeEnableLoadoutChoice() => MCMSettings.Instance?.ParryPracticeEnableLoadoutChoice ?? ModConfiguration.ParryPracticeEnableLoadoutChoiceDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmParryPracticeLoadoutChoiceCost() => MCMSettings.Instance?.ParryPracticeLoadoutChoiceCost ?? ModConfiguration.ParryPracticeLoadoutChoiceCostDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static PracticeEquipmentType McmParryPracticeEquipment() => MCMSettings.Instance?.ParryPracticeEquipment.SelectedValue.EnumValue ?? ModConfiguration.ParryPracticeEquipmentDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static float McmParryPracticeExperienceRate() => MCMSettings.Instance?.ParryPracticeExperienceRate ?? ModConfiguration.ParryPracticeExperienceRateDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmTeamPracticeEnabled() => MCMSettings.Instance?.TeamPracticeEnabled ?? ModConfiguration.TeamPracticeEnabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeSetupCost() => MCMSettings.Instance?.TeamPracticeSetupCost ?? ModConfiguration.TeamPracticeSetupCostDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmTeamPracticeEnableAgentSwitching() => MCMSettings.Instance?.TeamPracticeEnableAgentSwitching ?? ModConfiguration.TeamPracticeEnableAgentSwitchingDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeTotalParticipants() => MCMSettings.Instance?.TeamPracticeTotalParticipants ?? ModConfiguration.TeamPracticeTotalParticipantsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeActiveParticipants() => MCMSettings.Instance?.TeamPracticeActiveParticipants ?? ModConfiguration.TeamPracticeActiveParticipantsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeActiveParticipantsMinimum() => MCMSettings.Instance?.TeamPracticeActiveParticipantsMinimum ?? ModConfiguration.TeamPracticeActiveParticipantsMinimumDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeInitialParticipants() => MCMSettings.Instance?.TeamPracticeInitialParticipants ?? ModConfiguration.TeamPracticeInitialParticipantsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeAITeamsCount() => MCMSettings.Instance?.TeamPracticeAITeamsCount ?? ModConfiguration.TeamPracticeAITeamsCountDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmTeamPracticeEnableLoadoutChoice() => MCMSettings.Instance?.TeamPracticeEnableLoadoutChoice ?? ModConfiguration.TeamPracticeEnableLoadoutChoiceDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeLoadoutChoiceCost() => MCMSettings.Instance?.TeamPracticeLoadoutChoiceCost ?? ModConfiguration.TeamPracticeLoadoutChoiceCostDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static PracticeEquipmentType McmTeamPracticeEquipment() => MCMSettings.Instance?.TeamPracticeEquipment.SelectedValue.EnumValue ?? ModConfiguration.TeamPracticeEquipmentDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeValorReward1() => MCMSettings.Instance?.TeamPracticeValorReward1 ?? ModConfiguration.TeamPracticeValorReward1Default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeValorReward2() => MCMSettings.Instance?.TeamPracticeValorReward2 ?? ModConfiguration.TeamPracticeValorReward2Default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeValorReward3() => MCMSettings.Instance?.TeamPracticeValorReward3 ?? ModConfiguration.TeamPracticeValorReward3Default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeValorReward4() => MCMSettings.Instance?.TeamPracticeValorReward4 ?? ModConfiguration.TeamPracticeValorReward4Default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeValorReward5() => MCMSettings.Instance?.TeamPracticeValorReward5 ?? ModConfiguration.TeamPracticeValorReward5Default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ChampionPrizeCalculationType McmTeamPracticeChampionPrizeCalculation() => MCMSettings.Instance?.TeamPracticeChampionPrizeCalculation.SelectedValue.EnumValue ?? ModConfiguration.TeamPracticeChampionPrizeCalculationDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeChampionReward() => MCMSettings.Instance?.TeamPracticeChampionReward ?? ModConfiguration.TeamPracticeChampionRewardDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmTeamPracticeEnableViewerExperienceGain() => MCMSettings.Instance?.TeamPracticeEnableViewerExperienceGain ?? ModConfiguration.TeamPracticeEnableViewerExperienceGainDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static float McmTeamPracticeExperienceRate() => MCMSettings.Instance?.TeamPracticeExperienceRate ?? ModConfiguration.TeamPracticeExperienceRateDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeTeamOneColor() => MCMSettings.Instance?.TeamPracticeTeamOneColor ?? ModConfiguration.TeamPracticeTeamOneColorDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeTeamTwoColor() => MCMSettings.Instance?.TeamPracticeTeamTwoColor ?? ModConfiguration.TeamPracticeTeamTwoColorDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeTeamThreeColor() => MCMSettings.Instance?.TeamPracticeTeamThreeColor ?? ModConfiguration.TeamPracticeTeamThreeColorDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeTeamFourColor() => MCMSettings.Instance?.TeamPracticeTeamFourColor ?? ModConfiguration.TeamPracticeTeamFourColorDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeTeamFiveColor() => MCMSettings.Instance?.TeamPracticeTeamFiveColor ?? ModConfiguration.TeamPracticeTeamFiveColorDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamPracticeTeamSixColor() => MCMSettings.Instance?.TeamPracticeTeamSixColor ?? ModConfiguration.TeamPracticeTeamSixColorDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTournamentMaximumBet() => MCMSettings.Instance?.TournamentMaximumBet ?? ModConfiguration.TournamentMaximumBetDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmTournamentEnableRandomizedBettingOdds() => MCMSettings.Instance?.TournamentEnableRandomizedBettingOdds ?? ModConfiguration.TournamentEnableRandomizedBettingOddsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmTournamentAllowNotablesParticipation() => MCMSettings.Instance?.TournamentAllowNotablesParticipation ?? ModConfiguration.TournamentAllowNotablesParticipationDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static PrizeRerollCondition McmTournamentPrizeRerollCondition() => MCMSettings.Instance?.TournamentPrizeRerollCondition.SelectedValue.EnumValue ?? ModConfiguration.TournamentPrizeRerollConditionDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmTournamentEnableGoldPrizes() => MCMSettings.Instance?.TournamentEnableGoldPrizes ?? ModConfiguration.TournamentEnableGoldPrizesDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmTournamentEnablePrizeScaling() => MCMSettings.Instance?.TournamentEnablePrizeScaling ?? ModConfiguration.TournamentEnablePrizeScalingDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmTournamentEnableHighQualityPrizes() => MCMSettings.Instance?.TournamentEnableHighQualityPrizes ?? ModConfiguration.TournamentEnableHighQualityPrizesDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static CultureRestrictedPrizesMode McmTournamentCultureRestrictedPrizes() => MCMSettings.Instance?.TournamentCultureRestrictedPrizes.SelectedValue.EnumValue ?? ModConfiguration.TournamentCultureRestrictedPrizesDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTournamentRoundWonReward() => MCMSettings.Instance?.TournamentRoundWonReward ?? ModConfiguration.TournamentRoundWonRewardDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTournamentBaseRenownReward() => MCMSettings.Instance?.TournamentBaseRenownReward ?? ModConfiguration.TournamentBaseRenownRewardDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTournamentTakedownRenownReward() => MCMSettings.Instance?.TournamentTakedownRenownReward ?? ModConfiguration.TournamentTakedownRenownRewardDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTournamentInfluenceReward() => MCMSettings.Instance?.TournamentInfluenceReward ?? ModConfiguration.TournamentInfluenceRewardDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static float McmTournamentExperienceRate() => MCMSettings.Instance?.TournamentExperienceRate ?? ModConfiguration.TournamentExperienceRateDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmTournamentEnableInjuries() => MCMSettings.Instance?.TournamentEnableInjuries ?? ModConfiguration.TournamentEnableInjuriesDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTournamentInjuryHitPointsLoss() => MCMSettings.Instance?.TournamentInjuryHitPointsLoss ?? ModConfiguration.TournamentInjuryHitPointsLossDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmTournamentHostingEnabled() => MCMSettings.Instance?.TournamentHostingEnabled ?? ModConfiguration.TournamentHostingEnabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTournamentHostingBaseGold() => MCMSettings.Instance?.TournamentHostingBaseGold ?? ModConfiguration.TournamentHostingBaseGoldDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTournamentHostingBaseInfluence() => MCMSettings.Instance?.TournamentHostingBaseInfluence ?? ModConfiguration.TournamentHostingBaseInfluenceDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTournamentHostingCooldownDays() => MCMSettings.Instance?.TournamentHostingCooldownDays ?? ModConfiguration.TournamentHostingCooldownDaysDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmTournamentHostingRequireAffiliation() => MCMSettings.Instance?.TournamentHostingRequireAffiliation ?? ModConfiguration.TournamentHostingRequireAffiliationDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmGrandTournamentEnabled() => MCMSettings.Instance?.GrandTournamentEnabled ?? ModConfiguration.GrandTournamentEnabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmKingdomTournamentsEnabled() => MCMSettings.Instance?.KingdomTournamentsEnabled ?? ModConfiguration.KingdomTournamentsEnabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static GrandTournamentSeason McmGrandTournamentSeason() => MCMSettings.Instance?.GrandTournamentSeason.SelectedValue.EnumValue ?? ModConfiguration.GrandTournamentSeasonDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmGrandTournamentDayOfSeason() => MCMSettings.Instance?.GrandTournamentDayOfSeason ?? ModConfiguration.GrandTournamentDayOfSeasonDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmKingdomTournamentDayOfSeason() => MCMSettings.Instance?.KingdomTournamentDayOfSeason ?? ModConfiguration.KingdomTournamentDayOfSeasonDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmGrandTournamentAnnounceDays() => MCMSettings.Instance?.GrandTournamentAnnounceDays ?? ModConfiguration.GrandTournamentAnnounceDaysDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmGrandTournamentDurationDays() => MCMSettings.Instance?.GrandTournamentDurationDays ?? ModConfiguration.GrandTournamentDurationDaysDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmGrandTournamentLegendaryPrize() => MCMSettings.Instance?.GrandTournamentLegendaryPrize ?? ModConfiguration.GrandTournamentLegendaryPrizeDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmGrandTournamentGoldPrize() => MCMSettings.Instance?.GrandTournamentGoldPrize ?? ModConfiguration.GrandTournamentGoldPrizeDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmGrandTournamentRenownReward() => MCMSettings.Instance?.GrandTournamentRenownReward ?? ModConfiguration.GrandTournamentRenownRewardDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmGrandTournamentInfluenceReward() => MCMSettings.Instance?.GrandTournamentInfluenceReward ?? ModConfiguration.GrandTournamentInfluenceRewardDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmGrandTournamentLeaderboardWeight() => MCMSettings.Instance?.GrandTournamentLeaderboardWeight ?? ModConfiguration.GrandTournamentLeaderboardWeightDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmKingdomTournamentLeaderboardWeight() => MCMSettings.Instance?.KingdomTournamentLeaderboardWeight ?? ModConfiguration.KingdomTournamentLeaderboardWeightDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmKingdomTournamentRewardPercent() => MCMSettings.Instance?.KingdomTournamentRewardPercent ?? ModConfiguration.KingdomTournamentRewardPercentDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmGrandTournamentAnnouncePopup() => MCMSettings.Instance?.GrandTournamentAnnouncePopup ?? ModConfiguration.GrandTournamentAnnouncePopupDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmGrandTournamentResultPopup() => MCMSettings.Instance?.GrandTournamentResultPopup ?? ModConfiguration.GrandTournamentResultPopupDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmKingdomTournamentIntervalSeasons() => MCMSettings.Instance?.KingdomTournamentIntervalSeasons ?? ModConfiguration.KingdomTournamentIntervalSeasonsDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmTeamTournamentEnabled() => MCMSettings.Instance?.TeamTournamentEnabled ?? ModConfiguration.TeamTournamentEnabledDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamTournamentTeamSizeMin() => MCMSettings.Instance?.TeamTournamentTeamSizeMin ?? ModConfiguration.TeamTournamentTeamSizeMinDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamTournamentTeamSizeMax() => MCMSettings.Instance?.TeamTournamentTeamSizeMax ?? ModConfiguration.TeamTournamentTeamSizeMaxDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamTournamentTeamsCountMax() => MCMSettings.Instance?.TeamTournamentTeamsCountMax.SelectedValue ?? ModConfiguration.TeamTournamentTeamsCountMaxDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmTeamTournamentScoresForWinningTeam() => MCMSettings.Instance?.TeamTournamentScoresForWinningTeam ?? ModConfiguration.TeamTournamentScoresForWinningTeamDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamTournamentTeamOneColor() => MCMSettings.Instance?.TeamTournamentTeamOneColor ?? ModConfiguration.TeamTournamentTeamOneColorDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamTournamentTeamTwoColor() => MCMSettings.Instance?.TeamTournamentTeamTwoColor ?? ModConfiguration.TeamTournamentTeamTwoColorDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamTournamentTeamThreeColor() => MCMSettings.Instance?.TeamTournamentTeamThreeColor ?? ModConfiguration.TeamTournamentTeamThreeColorDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmTeamTournamentTeamFourColor() => MCMSettings.Instance?.TeamTournamentTeamFourColor ?? ModConfiguration.TeamTournamentTeamFourColorDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int McmPracticeLoadoutStages() => MCMSettings.Instance?.PracticeLoadoutStages ?? ModConfiguration.PracticeLoadoutStagesDefault;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool McmLoggingEnabled() => MCMSettings.Instance?.LoggingEnabled ?? ModConfiguration.LoggingEnabledDefault;
    #endregion

    #region ModConfiguration.xml
    private static ModConfiguration LoadFromFile()
    {
        string path = Path.Combine(ModInfo.ModuleRoot, ModInfo.ConfigurationFileName);
        ModConfiguration configuration = new();

        if (!File.Exists(path))
        {
            AorLog.Warn(ModInfo.ConfigurationFileName + " was missing. Created it with the default settings.");
            TryWrite(path, configuration);
            return configuration;
        }

        try
        {
            configuration = Read(path);
            if (!HasEverySetting(path, configuration))
            {
                AorLog.Warn(ModInfo.ConfigurationFileName + " had missing settings. Added them with their default values.");
                TryWrite(path, configuration);
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or XmlException)
        {
            AorLog.Warn(ModInfo.ConfigurationFileName + " was broken and was replaced with the default settings. Reason: " + (ex.InnerException?.Message ?? ex.Message));
            configuration = new ModConfiguration();
            TryWrite(path, configuration);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AorLog.Warn(ModInfo.ConfigurationFileName + " could not be opened. Using the default settings. Reason: " + ex.Message);
            configuration = new ModConfiguration();
        }

        // Out-of-range values are corrected in memory only; the file keeps what the player typed.
        configuration.Clamp();
        return configuration;
    }

    private static ModConfiguration Read(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Serializer.Deserialize(stream) as ModConfiguration ?? new ModConfiguration();
    }

    private static void TryWrite(string path, ModConfiguration configuration)
    {
        try
        {
            XmlWriterSettings writerSettings = new() { Indent = true, Encoding = Encoding.UTF8 };
            using XmlWriter writer = XmlWriter.Create(path, writerSettings);
            Serializer.Serialize(writer, configuration, NoNamespaces());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AorLog.Warn(ModInfo.ConfigurationFileName + " could not be written. The settings are kept in memory for this session. Reason: " + ex.Message);
        }
    }

    private static bool HasEverySetting(string path, ModConfiguration configuration)
    {
        XDocument complete = new();
        using (XmlWriter writer = complete.CreateWriter())
        {
            Serializer.Serialize(writer, configuration, NoNamespaces());
        }

        XElement? fileRoot = XDocument.Load(path).Root;
        return fileRoot != null && complete.Root != null && ContainsEveryElement(fileRoot, complete.Root);
    }

    private static bool ContainsEveryElement(XElement actual, XElement expected)
    {
        foreach (XElement expectedChild in expected.Elements())
        {
            XElement? actualChild = actual.Element(expectedChild.Name);
            if (actualChild == null || !ContainsEveryElement(actualChild, expectedChild)) return false;
        }

        return true;
    }

    private static XmlSerializerNamespaces NoNamespaces()
    {
        XmlSerializerNamespaces namespaces = new();
        namespaces.Add(string.Empty, string.Empty);
        return namespaces;
    }
    #endregion
}