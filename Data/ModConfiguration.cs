using System;

namespace ArenaOverhaul.Data;

// Shape of ModConfiguration.xml and the only place where defaults and ranges live (MCMSettings uses them too).
// One section per MCM group, same names and order; the settings of a sub-group follow the group's own settings.
public class ModConfiguration
{
    // Default Arena Practice settings for heroes
    public const bool HeroDefaultEnableLoadoutChoiceDefault = true;
    public const bool HeroDefaultOnlyPriorityLoadoutsDefault = true;
    public const bool HeroDefaultPrioritizeExpensiveEquipmentDefault = false;

    // Arena Practice settings
    public const bool PracticeEnableAgentSwitchingDefault = true;
    public const int PracticeTotalParticipantsDefault = 30;
    public const int PracticeTotalParticipantsMin = 10;
    public const int PracticeTotalParticipantsMax = 150;
    public const int PracticeActiveParticipantsDefault = 6;
    public const int PracticeActiveParticipantsMin = 1;
    public const int PracticeActiveParticipantsMax = 21;
    public const int PracticeActiveParticipantsMinimumDefault = 2;
    public const int PracticeActiveParticipantsMinimumMin = 1;
    public const int PracticeActiveParticipantsMinimumMax = 10;
    public const int PracticeInitialParticipantsDefault = 6;
    public const int PracticeInitialParticipantsMin = 1;
    public const int PracticeInitialParticipantsMax = 15;
    public const int PracticeAITeamsCountDefault = 0;
    public const int PracticeAITeamsCountMin = 0;
    public const int PracticeAITeamsCountMax = 21;
    public const bool PracticeEnableLoadoutChoiceDefault = true;
    public const int PracticeLoadoutChoiceCostDefault = 10;
    public const int PracticeLoadoutChoiceCostMin = 0;
    public const int PracticeLoadoutChoiceCostMax = 100;
    public const PracticeEquipmentType PracticeEquipmentDefault = PracticeEquipmentType.PracticeClothes;
    public const int PracticeValorReward1Default = 5;
    public const int PracticeValorReward1Min = 0;
    public const int PracticeValorReward1Max = 50;
    public const int PracticeValorReward2Default = 10;
    public const int PracticeValorReward2Min = 0;
    public const int PracticeValorReward2Max = 100;
    public const int PracticeValorReward3Default = 25;
    public const int PracticeValorReward3Min = 0;
    public const int PracticeValorReward3Max = 250;
    public const int PracticeValorReward4Default = 60;
    public const int PracticeValorReward4Min = 0;
    public const int PracticeValorReward4Max = 600;
    public const int PracticeValorReward5Default = 150;
    public const int PracticeValorReward5Min = 0;
    public const int PracticeValorReward5Max = 1500;
    public const ChampionPrizeCalculationType PracticeChampionPrizeCalculationDefault = ChampionPrizeCalculationType.Additive;
    public const int PracticeChampionRewardDefault = 250;
    public const int PracticeChampionRewardMin = 0;
    public const int PracticeChampionRewardMax = 2500;
    public const float PracticeExperienceRateDefault = 0.165f;

    // Expansive Arena Practice settings
    public const bool ExpansivePracticeEnableAgentSwitchingDefault = true;
    public const int ExpansivePracticeTotalParticipantsDefault = 90;
    public const int ExpansivePracticeTotalParticipantsMin = 10;
    public const int ExpansivePracticeTotalParticipantsMax = 300;
    public const int ExpansivePracticeActiveParticipantsDefault = 9;
    public const int ExpansivePracticeActiveParticipantsMin = 1;
    public const int ExpansivePracticeActiveParticipantsMax = 30;
    public const int ExpansivePracticeActiveParticipantsMinimumDefault = 4;
    public const int ExpansivePracticeActiveParticipantsMinimumMin = 1;
    public const int ExpansivePracticeActiveParticipantsMinimumMax = 20;
    public const int ExpansivePracticeInitialParticipantsDefault = 7;
    public const int ExpansivePracticeInitialParticipantsMin = 1;
    public const int ExpansivePracticeInitialParticipantsMax = 15;
    public const int ExpansivePracticeAITeamsCountDefault = 0;
    public const int ExpansivePracticeAITeamsCountMin = 0;
    public const int ExpansivePracticeAITeamsCountMax = 30;
    public const bool ExpansivePracticeEnableLoadoutChoiceDefault = true;
    public const int ExpansivePracticeLoadoutChoiceCostDefault = 10;
    public const int ExpansivePracticeLoadoutChoiceCostMin = 0;
    public const int ExpansivePracticeLoadoutChoiceCostMax = 100;
    public const PracticeEquipmentType ExpansivePracticeEquipmentDefault = PracticeEquipmentType.PracticeClothes;
    public const int ExpansivePracticeValorReward1Default = 10;
    public const int ExpansivePracticeValorReward1Min = 0;
    public const int ExpansivePracticeValorReward1Max = 50;
    public const int ExpansivePracticeValorReward2Default = 20;
    public const int ExpansivePracticeValorReward2Min = 0;
    public const int ExpansivePracticeValorReward2Max = 100;
    public const int ExpansivePracticeValorReward3Default = 50;
    public const int ExpansivePracticeValorReward3Min = 0;
    public const int ExpansivePracticeValorReward3Max = 250;
    public const int ExpansivePracticeValorReward4Default = 120;
    public const int ExpansivePracticeValorReward4Min = 0;
    public const int ExpansivePracticeValorReward4Max = 600;
    public const int ExpansivePracticeValorReward5Default = 300;
    public const int ExpansivePracticeValorReward5Min = 0;
    public const int ExpansivePracticeValorReward5Max = 1500;
    public const ChampionPrizeCalculationType ExpansivePracticeChampionPrizeCalculationDefault = ChampionPrizeCalculationType.Additive;
    public const int ExpansivePracticeChampionRewardDefault = 500;
    public const int ExpansivePracticeChampionRewardMin = 0;
    public const int ExpansivePracticeChampionRewardMax = 2500;
    public const bool ExpansivePracticeEnableViewerExperienceGainDefault = true;
    public const float ExpansivePracticeExperienceRateDefault = 0.333f;

    // Parry Practice settings
    public const bool ParryPracticeEnabledDefault = true;
    public const int ParryPracticeSetupCostDefault = 200;
    public const int ParryPracticeSetupCostMin = 0;
    public const int ParryPracticeSetupCostMax = 1000;
    public const bool ParryPracticeEnableAgentSwitchingDefault = true;
    public const int ParryPracticeTotalParticipantsDefault = 15;
    public const int ParryPracticeTotalParticipantsMin = 3;
    public const int ParryPracticeTotalParticipantsMax = 30;
    public const int ParryPracticeActiveParticipantsDefault = 1;
    public const int ParryPracticeActiveParticipantsMin = 1;
    public const int ParryPracticeActiveParticipantsMax = 5;
    public const int ParryPracticeActiveParticipantsMinimumDefault = 1;
    public const int ParryPracticeActiveParticipantsMinimumMin = 1;
    public const int ParryPracticeActiveParticipantsMinimumMax = 5;
    public const int ParryPracticeInitialParticipantsDefault = 1;
    public const int ParryPracticeInitialParticipantsMin = 1;
    public const int ParryPracticeInitialParticipantsMax = 5;
    public const int ParryPracticeAITeamsCountDefault = 1;
    public const int ParryPracticeAITeamsCountMin = 1;
    public const int ParryPracticeAITeamsCountMax = 5;
    public const bool ParryPracticeEnableLoadoutChoiceDefault = true;
    public const int ParryPracticeLoadoutChoiceCostDefault = 10;
    public const int ParryPracticeLoadoutChoiceCostMin = 0;
    public const int ParryPracticeLoadoutChoiceCostMax = 100;
    public const PracticeEquipmentType ParryPracticeEquipmentDefault = PracticeEquipmentType.BattleEquipment;
    public const float ParryPracticeExperienceRateDefault = 0.06f;

    // Team Arena Practice settings
    public const bool TeamPracticeEnabledDefault = true;
    public const int TeamPracticeSetupCostDefault = 200;
    public const int TeamPracticeSetupCostMin = 0;
    public const int TeamPracticeSetupCostMax = 1000;
    public const bool TeamPracticeEnableAgentSwitchingDefault = true;
    public const int TeamPracticeTotalParticipantsDefault = 300;
    public const int TeamPracticeTotalParticipantsMin = 50;
    public const int TeamPracticeTotalParticipantsMax = 600;
    public const int TeamPracticeActiveParticipantsDefault = 20;
    public const int TeamPracticeActiveParticipantsMin = 10;
    public const int TeamPracticeActiveParticipantsMax = 120;
    public const int TeamPracticeActiveParticipantsMinimumDefault = 12;
    public const int TeamPracticeActiveParticipantsMinimumMin = 5;
    public const int TeamPracticeActiveParticipantsMinimumMax = 90;
    public const int TeamPracticeInitialParticipantsDefault = 20;
    public const int TeamPracticeInitialParticipantsMin = 5;
    public const int TeamPracticeInitialParticipantsMax = 90;
    public const int TeamPracticeAITeamsCountDefault = 4;
    public const int TeamPracticeAITeamsCountMin = 1;
    public const int TeamPracticeAITeamsCountMax = 6;
    public const bool TeamPracticeEnableLoadoutChoiceDefault = true;
    public const int TeamPracticeLoadoutChoiceCostDefault = 20;
    public const int TeamPracticeLoadoutChoiceCostMin = 0;
    public const int TeamPracticeLoadoutChoiceCostMax = 100;
    public const PracticeEquipmentType TeamPracticeEquipmentDefault = PracticeEquipmentType.PracticeClothes;
    public const int TeamPracticeValorReward1Default = 30;
    public const int TeamPracticeValorReward1Min = 0;
    public const int TeamPracticeValorReward1Max = 50;
    public const int TeamPracticeValorReward2Default = 60;
    public const int TeamPracticeValorReward2Min = 0;
    public const int TeamPracticeValorReward2Max = 100;
    public const int TeamPracticeValorReward3Default = 150;
    public const int TeamPracticeValorReward3Min = 0;
    public const int TeamPracticeValorReward3Max = 250;
    public const int TeamPracticeValorReward4Default = 350;
    public const int TeamPracticeValorReward4Min = 0;
    public const int TeamPracticeValorReward4Max = 600;
    public const int TeamPracticeValorReward5Default = 800;
    public const int TeamPracticeValorReward5Min = 0;
    public const int TeamPracticeValorReward5Max = 1500;
    public const ChampionPrizeCalculationType TeamPracticeChampionPrizeCalculationDefault = ChampionPrizeCalculationType.Additive;
    public const int TeamPracticeChampionRewardDefault = 1000;
    public const int TeamPracticeChampionRewardMin = 0;
    public const int TeamPracticeChampionRewardMax = 2500;
    public const bool TeamPracticeEnableViewerExperienceGainDefault = true;
    public const float TeamPracticeExperienceRateDefault = 0.333f;
    public const int TeamPracticeTeamOneColorDefault = 83;
    public const int TeamPracticeTeamTwoColorDefault = 119;
    public const int TeamPracticeTeamThreeColorDefault = 88;
    public const int TeamPracticeTeamFourColorDefault = 84;
    public const int TeamPracticeTeamFiveColorDefault = 82;
    public const int TeamPracticeTeamSixColorDefault = 35;

    // Tournament settings
    public const int TournamentMaximumBetDefault = 500;
    public const int TournamentMaximumBetMin = 100;
    public const int TournamentMaximumBetMax = 1500;
    public const bool TournamentEnableRandomizedBettingOddsDefault = true;
    public const bool TournamentAllowNotablesParticipationDefault = true;
    public const PrizeRerollCondition TournamentPrizeRerollConditionDefault = PrizeRerollCondition.Never;
    public const bool TournamentEnableGoldPrizesDefault = true;
    public const bool TournamentEnablePrizeScalingDefault = true;
    public const bool TournamentEnableHighQualityPrizesDefault = true;
    public const CultureRestrictedPrizesMode TournamentCultureRestrictedPrizesDefault = CultureRestrictedPrizesMode.AlwaysExceptForMounts;
    public const int TournamentRoundWonRewardDefault = 0;
    public const int TournamentRoundWonRewardMin = 0;
    public const int TournamentRoundWonRewardMax = 1000;
    public const int TournamentBaseRenownRewardDefault = 3;
    public const int TournamentBaseRenownRewardMin = 0;
    public const int TournamentBaseRenownRewardMax = 10;
    public const int TournamentTakedownRenownRewardDefault = 1;
    public const int TournamentTakedownRenownRewardMin = 0;
    public const int TournamentTakedownRenownRewardMax = 5;
    public const int TournamentInfluenceRewardDefault = 10;
    public const int TournamentInfluenceRewardMin = 0;
    public const int TournamentInfluenceRewardMax = 100;
    public const float TournamentExperienceRateDefault = 0.666f;
    public const bool TournamentEnableInjuriesDefault = true;
    public const int TournamentInjuryHitPointsLossDefault = 30;
    public const int TournamentInjuryHitPointsLossMin = 5;
    public const int TournamentInjuryHitPointsLossMax = 100;
    public const bool TournamentHostingEnabledDefault = true;
    public const int TournamentHostingBaseGoldDefault = 2000;
    public const int TournamentHostingBaseGoldMin = 500;
    public const int TournamentHostingBaseGoldMax = 10000;
    public const int TournamentHostingBaseInfluenceDefault = 20;
    public const int TournamentHostingBaseInfluenceMin = 0;
    public const int TournamentHostingBaseInfluenceMax = 200;
    public const int TournamentHostingCooldownDaysDefault = 7;
    public const int TournamentHostingCooldownDaysMin = 0;
    public const int TournamentHostingCooldownDaysMax = 60;
    public const bool TournamentHostingRequireAffiliationDefault = true;
    public const bool GrandTournamentEnabledDefault = true;
    public const bool KingdomTournamentsEnabledDefault = true;
    public const GrandTournamentSeason GrandTournamentSeasonDefault = GrandTournamentSeason.Spring;
    public const int GrandTournamentDayOfSeasonDefault = 11;
    public const int GrandTournamentDayOfSeasonMin = 1;
    public const int GrandTournamentDayOfSeasonMax = 21;
    public const int KingdomTournamentDayOfSeasonDefault = 11;
    public const int KingdomTournamentDayOfSeasonMin = 1;
    public const int KingdomTournamentDayOfSeasonMax = 21;
    public const int GrandTournamentAnnounceDaysDefault = 7;
    public const int GrandTournamentAnnounceDaysMin = 0;
    public const int GrandTournamentAnnounceDaysMax = 20;
    public const int GrandTournamentDurationDaysDefault = 2;
    public const int GrandTournamentDurationDaysMin = 1;
    public const int GrandTournamentDurationDaysMax = 7;
    public const bool GrandTournamentLegendaryPrizeDefault = true;
    public const int GrandTournamentGoldPrizeDefault = 100000;
    public const int GrandTournamentGoldPrizeMin = 0;
    public const int GrandTournamentGoldPrizeMax = 200000;
    public const int GrandTournamentRenownRewardDefault = 50;
    public const int GrandTournamentRenownRewardMin = 0;
    public const int GrandTournamentRenownRewardMax = 1000;
    public const int GrandTournamentInfluenceRewardDefault = 300;
    public const int GrandTournamentInfluenceRewardMin = 0;
    public const int GrandTournamentInfluenceRewardMax = 1000;
    public const int GrandTournamentLeaderboardWeightDefault = 3;
    public const int GrandTournamentLeaderboardWeightMin = 1;
    public const int GrandTournamentLeaderboardWeightMax = 10;
    public const int KingdomTournamentLeaderboardWeightDefault = 2;
    public const int KingdomTournamentLeaderboardWeightMin = 1;
    public const int KingdomTournamentLeaderboardWeightMax = 10;
    public const int KingdomTournamentRewardPercentDefault = 40;
    public const int KingdomTournamentRewardPercentMin = 0;
    public const int KingdomTournamentRewardPercentMax = 100;
    public const bool GrandTournamentAnnouncePopupDefault = true;
    public const bool GrandTournamentResultPopupDefault = true;
    public const int KingdomTournamentIntervalSeasonsDefault = 3;
    public const int KingdomTournamentIntervalSeasonsMin = 1;
    public const int KingdomTournamentIntervalSeasonsMax = 8;
    public const bool TeamTournamentEnabledDefault = true;
    public const int TeamTournamentTeamSizeMinDefault = 2;
    public const int TeamTournamentTeamSizeMinMin = 2;
    public const int TeamTournamentTeamSizeMinMax = 16;
    public const int TeamTournamentTeamSizeMaxDefault = 8;
    public const int TeamTournamentTeamSizeMaxMin = 2;
    public const int TeamTournamentTeamSizeMaxMax = 16;
    public const int TeamTournamentTeamsCountMaxDefault = 16;
    public const bool TeamTournamentScoresForWinningTeamDefault = true;
    public const int TeamTournamentTeamOneColorDefault = 83;
    public const int TeamTournamentTeamTwoColorDefault = 119;
    public const int TeamTournamentTeamThreeColorDefault = 88;
    public const int TeamTournamentTeamFourColorDefault = 84;

    // Compatibility settings
    public const int PracticeLoadoutStagesDefault = 3;
    public const int PracticeLoadoutStagesMin = 1;
    public const int PracticeLoadoutStagesMax = 10;

    // Technical settings
    public const bool LoggingEnabledDefault = false;

    // Shared range for every "Experience gain rate" setting (1 = 100%).
    public const float ExperienceRateMin = 0f;
    public const float ExperienceRateMax = 1f;

    // Shared range for every banner color index setting.
    public const int ColorIndexMin = 0;
    public const int ColorIndexMax = 157;

    // Allowed values of "Maximum number of teams" (order = MCM dropdown index: append only).
    public static readonly int[] TeamTournamentTeamsCountMaxChoices = [8, 16, 32];

    public HeroPracticeDefaultsSection HeroPracticeDefaults { get; set; } = new();
    public ArenaPracticeSection ArenaPractice { get; set; } = new();
    public ExpansiveArenaPracticeSection ExpansiveArenaPractice { get; set; } = new();
    public ParryPracticeSection ParryPractice { get; set; } = new();
    public TeamArenaPracticeSection TeamArenaPractice { get; set; } = new();
    public TournamentsSection Tournaments { get; set; } = new();
    public CompatibilitySection Compatibility { get; set; } = new();

    // Always the last section.
    public TechnicalSettingsSection TechnicalSettings { get; set; } = new();

    internal void Clamp()
    {
        HeroPracticeDefaults ??= new HeroPracticeDefaultsSection();
        ArenaPractice ??= new ArenaPracticeSection();
        ExpansiveArenaPractice ??= new ExpansiveArenaPracticeSection();
        ParryPractice ??= new ParryPracticeSection();
        TeamArenaPractice ??= new TeamArenaPracticeSection();
        Tournaments ??= new TournamentsSection();
        Compatibility ??= new CompatibilitySection();
        TechnicalSettings ??= new TechnicalSettingsSection();

        ArenaPractice.PracticeTotalParticipants = Clamp(ArenaPractice.PracticeTotalParticipants, PracticeTotalParticipantsMin, PracticeTotalParticipantsMax);
        ArenaPractice.PracticeActiveParticipants = Clamp(ArenaPractice.PracticeActiveParticipants, PracticeActiveParticipantsMin, PracticeActiveParticipantsMax);
        ArenaPractice.PracticeActiveParticipantsMinimum = Clamp(ArenaPractice.PracticeActiveParticipantsMinimum, PracticeActiveParticipantsMinimumMin, PracticeActiveParticipantsMinimumMax);
        ArenaPractice.PracticeInitialParticipants = Clamp(ArenaPractice.PracticeInitialParticipants, PracticeInitialParticipantsMin, PracticeInitialParticipantsMax);
        ArenaPractice.PracticeAITeamsCount = Clamp(ArenaPractice.PracticeAITeamsCount, PracticeAITeamsCountMin, PracticeAITeamsCountMax);
        ArenaPractice.PracticeLoadoutChoiceCost = Clamp(ArenaPractice.PracticeLoadoutChoiceCost, PracticeLoadoutChoiceCostMin, PracticeLoadoutChoiceCostMax);
        ArenaPractice.PracticeValorReward1 = Clamp(ArenaPractice.PracticeValorReward1, PracticeValorReward1Min, PracticeValorReward1Max);
        ArenaPractice.PracticeValorReward2 = Clamp(ArenaPractice.PracticeValorReward2, PracticeValorReward2Min, PracticeValorReward2Max);
        ArenaPractice.PracticeValorReward3 = Clamp(ArenaPractice.PracticeValorReward3, PracticeValorReward3Min, PracticeValorReward3Max);
        ArenaPractice.PracticeValorReward4 = Clamp(ArenaPractice.PracticeValorReward4, PracticeValorReward4Min, PracticeValorReward4Max);
        ArenaPractice.PracticeValorReward5 = Clamp(ArenaPractice.PracticeValorReward5, PracticeValorReward5Min, PracticeValorReward5Max);
        ArenaPractice.PracticeChampionReward = Clamp(ArenaPractice.PracticeChampionReward, PracticeChampionRewardMin, PracticeChampionRewardMax);
        ArenaPractice.PracticeExperienceRate = Clamp(ArenaPractice.PracticeExperienceRate, ExperienceRateMin, ExperienceRateMax, PracticeExperienceRateDefault);

        ExpansiveArenaPractice.ExpansivePracticeTotalParticipants = Clamp(ExpansiveArenaPractice.ExpansivePracticeTotalParticipants, ExpansivePracticeTotalParticipantsMin, ExpansivePracticeTotalParticipantsMax);
        ExpansiveArenaPractice.ExpansivePracticeActiveParticipants = Clamp(ExpansiveArenaPractice.ExpansivePracticeActiveParticipants, ExpansivePracticeActiveParticipantsMin, ExpansivePracticeActiveParticipantsMax);
        ExpansiveArenaPractice.ExpansivePracticeActiveParticipantsMinimum = Clamp(ExpansiveArenaPractice.ExpansivePracticeActiveParticipantsMinimum, ExpansivePracticeActiveParticipantsMinimumMin, ExpansivePracticeActiveParticipantsMinimumMax);
        ExpansiveArenaPractice.ExpansivePracticeInitialParticipants = Clamp(ExpansiveArenaPractice.ExpansivePracticeInitialParticipants, ExpansivePracticeInitialParticipantsMin, ExpansivePracticeInitialParticipantsMax);
        ExpansiveArenaPractice.ExpansivePracticeAITeamsCount = Clamp(ExpansiveArenaPractice.ExpansivePracticeAITeamsCount, ExpansivePracticeAITeamsCountMin, ExpansivePracticeAITeamsCountMax);
        ExpansiveArenaPractice.ExpansivePracticeLoadoutChoiceCost = Clamp(ExpansiveArenaPractice.ExpansivePracticeLoadoutChoiceCost, ExpansivePracticeLoadoutChoiceCostMin, ExpansivePracticeLoadoutChoiceCostMax);
        ExpansiveArenaPractice.ExpansivePracticeValorReward1 = Clamp(ExpansiveArenaPractice.ExpansivePracticeValorReward1, ExpansivePracticeValorReward1Min, ExpansivePracticeValorReward1Max);
        ExpansiveArenaPractice.ExpansivePracticeValorReward2 = Clamp(ExpansiveArenaPractice.ExpansivePracticeValorReward2, ExpansivePracticeValorReward2Min, ExpansivePracticeValorReward2Max);
        ExpansiveArenaPractice.ExpansivePracticeValorReward3 = Clamp(ExpansiveArenaPractice.ExpansivePracticeValorReward3, ExpansivePracticeValorReward3Min, ExpansivePracticeValorReward3Max);
        ExpansiveArenaPractice.ExpansivePracticeValorReward4 = Clamp(ExpansiveArenaPractice.ExpansivePracticeValorReward4, ExpansivePracticeValorReward4Min, ExpansivePracticeValorReward4Max);
        ExpansiveArenaPractice.ExpansivePracticeValorReward5 = Clamp(ExpansiveArenaPractice.ExpansivePracticeValorReward5, ExpansivePracticeValorReward5Min, ExpansivePracticeValorReward5Max);
        ExpansiveArenaPractice.ExpansivePracticeChampionReward = Clamp(ExpansiveArenaPractice.ExpansivePracticeChampionReward, ExpansivePracticeChampionRewardMin, ExpansivePracticeChampionRewardMax);
        ExpansiveArenaPractice.ExpansivePracticeExperienceRate = Clamp(ExpansiveArenaPractice.ExpansivePracticeExperienceRate, ExperienceRateMin, ExperienceRateMax, ExpansivePracticeExperienceRateDefault);

        ParryPractice.ParryPracticeSetupCost = Clamp(ParryPractice.ParryPracticeSetupCost, ParryPracticeSetupCostMin, ParryPracticeSetupCostMax);
        ParryPractice.ParryPracticeTotalParticipants = Clamp(ParryPractice.ParryPracticeTotalParticipants, ParryPracticeTotalParticipantsMin, ParryPracticeTotalParticipantsMax);
        ParryPractice.ParryPracticeActiveParticipants = Clamp(ParryPractice.ParryPracticeActiveParticipants, ParryPracticeActiveParticipantsMin, ParryPracticeActiveParticipantsMax);
        ParryPractice.ParryPracticeActiveParticipantsMinimum = Clamp(ParryPractice.ParryPracticeActiveParticipantsMinimum, ParryPracticeActiveParticipantsMinimumMin, ParryPracticeActiveParticipantsMinimumMax);
        ParryPractice.ParryPracticeInitialParticipants = Clamp(ParryPractice.ParryPracticeInitialParticipants, ParryPracticeInitialParticipantsMin, ParryPracticeInitialParticipantsMax);
        ParryPractice.ParryPracticeAITeamsCount = Clamp(ParryPractice.ParryPracticeAITeamsCount, ParryPracticeAITeamsCountMin, ParryPracticeAITeamsCountMax);
        ParryPractice.ParryPracticeLoadoutChoiceCost = Clamp(ParryPractice.ParryPracticeLoadoutChoiceCost, ParryPracticeLoadoutChoiceCostMin, ParryPracticeLoadoutChoiceCostMax);
        ParryPractice.ParryPracticeExperienceRate = Clamp(ParryPractice.ParryPracticeExperienceRate, ExperienceRateMin, ExperienceRateMax, ParryPracticeExperienceRateDefault);

        TeamArenaPractice.TeamPracticeSetupCost = Clamp(TeamArenaPractice.TeamPracticeSetupCost, TeamPracticeSetupCostMin, TeamPracticeSetupCostMax);
        TeamArenaPractice.TeamPracticeTotalParticipants = Clamp(TeamArenaPractice.TeamPracticeTotalParticipants, TeamPracticeTotalParticipantsMin, TeamPracticeTotalParticipantsMax);
        TeamArenaPractice.TeamPracticeActiveParticipants = Clamp(TeamArenaPractice.TeamPracticeActiveParticipants, TeamPracticeActiveParticipantsMin, TeamPracticeActiveParticipantsMax);
        TeamArenaPractice.TeamPracticeActiveParticipantsMinimum = Clamp(TeamArenaPractice.TeamPracticeActiveParticipantsMinimum, TeamPracticeActiveParticipantsMinimumMin, TeamPracticeActiveParticipantsMinimumMax);
        TeamArenaPractice.TeamPracticeInitialParticipants = Clamp(TeamArenaPractice.TeamPracticeInitialParticipants, TeamPracticeInitialParticipantsMin, TeamPracticeInitialParticipantsMax);
        TeamArenaPractice.TeamPracticeAITeamsCount = Clamp(TeamArenaPractice.TeamPracticeAITeamsCount, TeamPracticeAITeamsCountMin, TeamPracticeAITeamsCountMax);
        TeamArenaPractice.TeamPracticeLoadoutChoiceCost = Clamp(TeamArenaPractice.TeamPracticeLoadoutChoiceCost, TeamPracticeLoadoutChoiceCostMin, TeamPracticeLoadoutChoiceCostMax);
        TeamArenaPractice.TeamPracticeValorReward1 = Clamp(TeamArenaPractice.TeamPracticeValorReward1, TeamPracticeValorReward1Min, TeamPracticeValorReward1Max);
        TeamArenaPractice.TeamPracticeValorReward2 = Clamp(TeamArenaPractice.TeamPracticeValorReward2, TeamPracticeValorReward2Min, TeamPracticeValorReward2Max);
        TeamArenaPractice.TeamPracticeValorReward3 = Clamp(TeamArenaPractice.TeamPracticeValorReward3, TeamPracticeValorReward3Min, TeamPracticeValorReward3Max);
        TeamArenaPractice.TeamPracticeValorReward4 = Clamp(TeamArenaPractice.TeamPracticeValorReward4, TeamPracticeValorReward4Min, TeamPracticeValorReward4Max);
        TeamArenaPractice.TeamPracticeValorReward5 = Clamp(TeamArenaPractice.TeamPracticeValorReward5, TeamPracticeValorReward5Min, TeamPracticeValorReward5Max);
        TeamArenaPractice.TeamPracticeChampionReward = Clamp(TeamArenaPractice.TeamPracticeChampionReward, TeamPracticeChampionRewardMin, TeamPracticeChampionRewardMax);
        TeamArenaPractice.TeamPracticeExperienceRate = Clamp(TeamArenaPractice.TeamPracticeExperienceRate, ExperienceRateMin, ExperienceRateMax, TeamPracticeExperienceRateDefault);
        TeamArenaPractice.TeamPracticeTeamOneColor = Clamp(TeamArenaPractice.TeamPracticeTeamOneColor, ColorIndexMin, ColorIndexMax);
        TeamArenaPractice.TeamPracticeTeamTwoColor = Clamp(TeamArenaPractice.TeamPracticeTeamTwoColor, ColorIndexMin, ColorIndexMax);
        TeamArenaPractice.TeamPracticeTeamThreeColor = Clamp(TeamArenaPractice.TeamPracticeTeamThreeColor, ColorIndexMin, ColorIndexMax);
        TeamArenaPractice.TeamPracticeTeamFourColor = Clamp(TeamArenaPractice.TeamPracticeTeamFourColor, ColorIndexMin, ColorIndexMax);
        TeamArenaPractice.TeamPracticeTeamFiveColor = Clamp(TeamArenaPractice.TeamPracticeTeamFiveColor, ColorIndexMin, ColorIndexMax);
        TeamArenaPractice.TeamPracticeTeamSixColor = Clamp(TeamArenaPractice.TeamPracticeTeamSixColor, ColorIndexMin, ColorIndexMax);

        Tournaments.TournamentMaximumBet = Clamp(Tournaments.TournamentMaximumBet, TournamentMaximumBetMin, TournamentMaximumBetMax);
        Tournaments.TournamentRoundWonReward = Clamp(Tournaments.TournamentRoundWonReward, TournamentRoundWonRewardMin, TournamentRoundWonRewardMax);
        Tournaments.TournamentBaseRenownReward = Clamp(Tournaments.TournamentBaseRenownReward, TournamentBaseRenownRewardMin, TournamentBaseRenownRewardMax);
        Tournaments.TournamentTakedownRenownReward = Clamp(Tournaments.TournamentTakedownRenownReward, TournamentTakedownRenownRewardMin, TournamentTakedownRenownRewardMax);
        Tournaments.TournamentInfluenceReward = Clamp(Tournaments.TournamentInfluenceReward, TournamentInfluenceRewardMin, TournamentInfluenceRewardMax);
        Tournaments.TournamentExperienceRate = Clamp(Tournaments.TournamentExperienceRate, ExperienceRateMin, ExperienceRateMax, TournamentExperienceRateDefault);
        Tournaments.TournamentInjuryHitPointsLoss = Clamp(Tournaments.TournamentInjuryHitPointsLoss, TournamentInjuryHitPointsLossMin, TournamentInjuryHitPointsLossMax);
        Tournaments.TournamentHostingBaseGold = Clamp(Tournaments.TournamentHostingBaseGold, TournamentHostingBaseGoldMin, TournamentHostingBaseGoldMax);
        Tournaments.TournamentHostingBaseInfluence = Clamp(Tournaments.TournamentHostingBaseInfluence, TournamentHostingBaseInfluenceMin, TournamentHostingBaseInfluenceMax);
        Tournaments.TournamentHostingCooldownDays = Clamp(Tournaments.TournamentHostingCooldownDays, TournamentHostingCooldownDaysMin, TournamentHostingCooldownDaysMax);
        Tournaments.GrandTournamentDayOfSeason = Clamp(Tournaments.GrandTournamentDayOfSeason, GrandTournamentDayOfSeasonMin, GrandTournamentDayOfSeasonMax);
        Tournaments.KingdomTournamentDayOfSeason = Clamp(Tournaments.KingdomTournamentDayOfSeason, KingdomTournamentDayOfSeasonMin, KingdomTournamentDayOfSeasonMax);
        Tournaments.GrandTournamentAnnounceDays = Clamp(Tournaments.GrandTournamentAnnounceDays, GrandTournamentAnnounceDaysMin, GrandTournamentAnnounceDaysMax);
        Tournaments.GrandTournamentDurationDays = Clamp(Tournaments.GrandTournamentDurationDays, GrandTournamentDurationDaysMin, GrandTournamentDurationDaysMax);
        Tournaments.GrandTournamentGoldPrize = Clamp(Tournaments.GrandTournamentGoldPrize, GrandTournamentGoldPrizeMin, GrandTournamentGoldPrizeMax);
        Tournaments.GrandTournamentRenownReward = Clamp(Tournaments.GrandTournamentRenownReward, GrandTournamentRenownRewardMin, GrandTournamentRenownRewardMax);
        Tournaments.GrandTournamentInfluenceReward = Clamp(Tournaments.GrandTournamentInfluenceReward, GrandTournamentInfluenceRewardMin, GrandTournamentInfluenceRewardMax);
        Tournaments.GrandTournamentLeaderboardWeight = Clamp(Tournaments.GrandTournamentLeaderboardWeight, GrandTournamentLeaderboardWeightMin, GrandTournamentLeaderboardWeightMax);
        Tournaments.KingdomTournamentLeaderboardWeight = Clamp(Tournaments.KingdomTournamentLeaderboardWeight, KingdomTournamentLeaderboardWeightMin, KingdomTournamentLeaderboardWeightMax);
        Tournaments.KingdomTournamentRewardPercent = Clamp(Tournaments.KingdomTournamentRewardPercent, KingdomTournamentRewardPercentMin, KingdomTournamentRewardPercentMax);
        Tournaments.KingdomTournamentIntervalSeasons = Clamp(Tournaments.KingdomTournamentIntervalSeasons, KingdomTournamentIntervalSeasonsMin, KingdomTournamentIntervalSeasonsMax);
        Tournaments.TeamTournamentTeamSizeMin = Clamp(Tournaments.TeamTournamentTeamSizeMin, TeamTournamentTeamSizeMinMin, TeamTournamentTeamSizeMinMax);
        Tournaments.TeamTournamentTeamSizeMax = Clamp(Tournaments.TeamTournamentTeamSizeMax, TeamTournamentTeamSizeMaxMin, TeamTournamentTeamSizeMaxMax);
        if (Array.IndexOf(TeamTournamentTeamsCountMaxChoices, Tournaments.TeamTournamentTeamsCountMax) < 0) Tournaments.TeamTournamentTeamsCountMax = TeamTournamentTeamsCountMaxDefault;
        Tournaments.TeamTournamentTeamOneColor = Clamp(Tournaments.TeamTournamentTeamOneColor, ColorIndexMin, ColorIndexMax);
        Tournaments.TeamTournamentTeamTwoColor = Clamp(Tournaments.TeamTournamentTeamTwoColor, ColorIndexMin, ColorIndexMax);
        Tournaments.TeamTournamentTeamThreeColor = Clamp(Tournaments.TeamTournamentTeamThreeColor, ColorIndexMin, ColorIndexMax);
        Tournaments.TeamTournamentTeamFourColor = Clamp(Tournaments.TeamTournamentTeamFourColor, ColorIndexMin, ColorIndexMax);

        Compatibility.PracticeLoadoutStages = Clamp(Compatibility.PracticeLoadoutStages, PracticeLoadoutStagesMin, PracticeLoadoutStagesMax);
    }

    private static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max);

    private static float Clamp(float value, float min, float max, float fallback) => float.IsNaN(value) ? fallback : Math.Min(Math.Max(value, min), max);

    public class HeroPracticeDefaultsSection
    {
        public bool HeroDefaultEnableLoadoutChoice { get; set; } = HeroDefaultEnableLoadoutChoiceDefault;
        public bool HeroDefaultOnlyPriorityLoadouts { get; set; } = HeroDefaultOnlyPriorityLoadoutsDefault;
        public bool HeroDefaultPrioritizeExpensiveEquipment { get; set; } = HeroDefaultPrioritizeExpensiveEquipmentDefault;
    }

    public class ArenaPracticeSection
    {
        public bool PracticeEnableAgentSwitching { get; set; } = PracticeEnableAgentSwitchingDefault;
        public int PracticeTotalParticipants { get; set; } = PracticeTotalParticipantsDefault;
        public int PracticeActiveParticipants { get; set; } = PracticeActiveParticipantsDefault;
        public int PracticeActiveParticipantsMinimum { get; set; } = PracticeActiveParticipantsMinimumDefault;
        public int PracticeInitialParticipants { get; set; } = PracticeInitialParticipantsDefault;
        public int PracticeAITeamsCount { get; set; } = PracticeAITeamsCountDefault;
        public bool PracticeEnableLoadoutChoice { get; set; } = PracticeEnableLoadoutChoiceDefault;
        public int PracticeLoadoutChoiceCost { get; set; } = PracticeLoadoutChoiceCostDefault;
        public PracticeEquipmentType PracticeEquipment { get; set; } = PracticeEquipmentDefault;
        public int PracticeValorReward1 { get; set; } = PracticeValorReward1Default;
        public int PracticeValorReward2 { get; set; } = PracticeValorReward2Default;
        public int PracticeValorReward3 { get; set; } = PracticeValorReward3Default;
        public int PracticeValorReward4 { get; set; } = PracticeValorReward4Default;
        public int PracticeValorReward5 { get; set; } = PracticeValorReward5Default;
        public ChampionPrizeCalculationType PracticeChampionPrizeCalculation { get; set; } = PracticeChampionPrizeCalculationDefault;
        public int PracticeChampionReward { get; set; } = PracticeChampionRewardDefault;
        public float PracticeExperienceRate { get; set; } = PracticeExperienceRateDefault;
    }

    public class ExpansiveArenaPracticeSection
    {
        public bool ExpansivePracticeEnableAgentSwitching { get; set; } = ExpansivePracticeEnableAgentSwitchingDefault;
        public int ExpansivePracticeTotalParticipants { get; set; } = ExpansivePracticeTotalParticipantsDefault;
        public int ExpansivePracticeActiveParticipants { get; set; } = ExpansivePracticeActiveParticipantsDefault;
        public int ExpansivePracticeActiveParticipantsMinimum { get; set; } = ExpansivePracticeActiveParticipantsMinimumDefault;
        public int ExpansivePracticeInitialParticipants { get; set; } = ExpansivePracticeInitialParticipantsDefault;
        public int ExpansivePracticeAITeamsCount { get; set; } = ExpansivePracticeAITeamsCountDefault;
        public bool ExpansivePracticeEnableLoadoutChoice { get; set; } = ExpansivePracticeEnableLoadoutChoiceDefault;
        public int ExpansivePracticeLoadoutChoiceCost { get; set; } = ExpansivePracticeLoadoutChoiceCostDefault;
        public PracticeEquipmentType ExpansivePracticeEquipment { get; set; } = ExpansivePracticeEquipmentDefault;
        public int ExpansivePracticeValorReward1 { get; set; } = ExpansivePracticeValorReward1Default;
        public int ExpansivePracticeValorReward2 { get; set; } = ExpansivePracticeValorReward2Default;
        public int ExpansivePracticeValorReward3 { get; set; } = ExpansivePracticeValorReward3Default;
        public int ExpansivePracticeValorReward4 { get; set; } = ExpansivePracticeValorReward4Default;
        public int ExpansivePracticeValorReward5 { get; set; } = ExpansivePracticeValorReward5Default;
        public ChampionPrizeCalculationType ExpansivePracticeChampionPrizeCalculation { get; set; } = ExpansivePracticeChampionPrizeCalculationDefault;
        public int ExpansivePracticeChampionReward { get; set; } = ExpansivePracticeChampionRewardDefault;
        public bool ExpansivePracticeEnableViewerExperienceGain { get; set; } = ExpansivePracticeEnableViewerExperienceGainDefault;
        public float ExpansivePracticeExperienceRate { get; set; } = ExpansivePracticeExperienceRateDefault;
    }

    public class ParryPracticeSection
    {
        public bool ParryPracticeEnabled { get; set; } = ParryPracticeEnabledDefault;
        public int ParryPracticeSetupCost { get; set; } = ParryPracticeSetupCostDefault;
        public bool ParryPracticeEnableAgentSwitching { get; set; } = ParryPracticeEnableAgentSwitchingDefault;
        public int ParryPracticeTotalParticipants { get; set; } = ParryPracticeTotalParticipantsDefault;
        public int ParryPracticeActiveParticipants { get; set; } = ParryPracticeActiveParticipantsDefault;
        public int ParryPracticeActiveParticipantsMinimum { get; set; } = ParryPracticeActiveParticipantsMinimumDefault;
        public int ParryPracticeInitialParticipants { get; set; } = ParryPracticeInitialParticipantsDefault;
        public int ParryPracticeAITeamsCount { get; set; } = ParryPracticeAITeamsCountDefault;
        public bool ParryPracticeEnableLoadoutChoice { get; set; } = ParryPracticeEnableLoadoutChoiceDefault;
        public int ParryPracticeLoadoutChoiceCost { get; set; } = ParryPracticeLoadoutChoiceCostDefault;
        public PracticeEquipmentType ParryPracticeEquipment { get; set; } = ParryPracticeEquipmentDefault;
        public float ParryPracticeExperienceRate { get; set; } = ParryPracticeExperienceRateDefault;
    }

    public class TeamArenaPracticeSection
    {
        public bool TeamPracticeEnabled { get; set; } = TeamPracticeEnabledDefault;
        public int TeamPracticeSetupCost { get; set; } = TeamPracticeSetupCostDefault;
        public bool TeamPracticeEnableAgentSwitching { get; set; } = TeamPracticeEnableAgentSwitchingDefault;
        public int TeamPracticeTotalParticipants { get; set; } = TeamPracticeTotalParticipantsDefault;
        public int TeamPracticeActiveParticipants { get; set; } = TeamPracticeActiveParticipantsDefault;
        public int TeamPracticeActiveParticipantsMinimum { get; set; } = TeamPracticeActiveParticipantsMinimumDefault;
        public int TeamPracticeInitialParticipants { get; set; } = TeamPracticeInitialParticipantsDefault;
        public int TeamPracticeAITeamsCount { get; set; } = TeamPracticeAITeamsCountDefault;
        public bool TeamPracticeEnableLoadoutChoice { get; set; } = TeamPracticeEnableLoadoutChoiceDefault;
        public int TeamPracticeLoadoutChoiceCost { get; set; } = TeamPracticeLoadoutChoiceCostDefault;
        public PracticeEquipmentType TeamPracticeEquipment { get; set; } = TeamPracticeEquipmentDefault;
        public int TeamPracticeValorReward1 { get; set; } = TeamPracticeValorReward1Default;
        public int TeamPracticeValorReward2 { get; set; } = TeamPracticeValorReward2Default;
        public int TeamPracticeValorReward3 { get; set; } = TeamPracticeValorReward3Default;
        public int TeamPracticeValorReward4 { get; set; } = TeamPracticeValorReward4Default;
        public int TeamPracticeValorReward5 { get; set; } = TeamPracticeValorReward5Default;
        public ChampionPrizeCalculationType TeamPracticeChampionPrizeCalculation { get; set; } = TeamPracticeChampionPrizeCalculationDefault;
        public int TeamPracticeChampionReward { get; set; } = TeamPracticeChampionRewardDefault;
        public bool TeamPracticeEnableViewerExperienceGain { get; set; } = TeamPracticeEnableViewerExperienceGainDefault;
        public float TeamPracticeExperienceRate { get; set; } = TeamPracticeExperienceRateDefault;
        public int TeamPracticeTeamOneColor { get; set; } = TeamPracticeTeamOneColorDefault;
        public int TeamPracticeTeamTwoColor { get; set; } = TeamPracticeTeamTwoColorDefault;
        public int TeamPracticeTeamThreeColor { get; set; } = TeamPracticeTeamThreeColorDefault;
        public int TeamPracticeTeamFourColor { get; set; } = TeamPracticeTeamFourColorDefault;
        public int TeamPracticeTeamFiveColor { get; set; } = TeamPracticeTeamFiveColorDefault;
        public int TeamPracticeTeamSixColor { get; set; } = TeamPracticeTeamSixColorDefault;
    }

    public class TournamentsSection
    {
        public int TournamentMaximumBet { get; set; } = TournamentMaximumBetDefault;
        public bool TournamentEnableRandomizedBettingOdds { get; set; } = TournamentEnableRandomizedBettingOddsDefault;
        public bool TournamentAllowNotablesParticipation { get; set; } = TournamentAllowNotablesParticipationDefault;
        public PrizeRerollCondition TournamentPrizeRerollCondition { get; set; } = TournamentPrizeRerollConditionDefault;
        public bool TournamentEnableGoldPrizes { get; set; } = TournamentEnableGoldPrizesDefault;
        public bool TournamentEnablePrizeScaling { get; set; } = TournamentEnablePrizeScalingDefault;
        public bool TournamentEnableHighQualityPrizes { get; set; } = TournamentEnableHighQualityPrizesDefault;
        public CultureRestrictedPrizesMode TournamentCultureRestrictedPrizes { get; set; } = TournamentCultureRestrictedPrizesDefault;
        public int TournamentRoundWonReward { get; set; } = TournamentRoundWonRewardDefault;
        public int TournamentBaseRenownReward { get; set; } = TournamentBaseRenownRewardDefault;
        public int TournamentTakedownRenownReward { get; set; } = TournamentTakedownRenownRewardDefault;
        public int TournamentInfluenceReward { get; set; } = TournamentInfluenceRewardDefault;
        public float TournamentExperienceRate { get; set; } = TournamentExperienceRateDefault;
        public bool TournamentEnableInjuries { get; set; } = TournamentEnableInjuriesDefault;
        public int TournamentInjuryHitPointsLoss { get; set; } = TournamentInjuryHitPointsLossDefault;
        public bool TournamentHostingEnabled { get; set; } = TournamentHostingEnabledDefault;
        public int TournamentHostingBaseGold { get; set; } = TournamentHostingBaseGoldDefault;
        public int TournamentHostingBaseInfluence { get; set; } = TournamentHostingBaseInfluenceDefault;
        public int TournamentHostingCooldownDays { get; set; } = TournamentHostingCooldownDaysDefault;
        public bool TournamentHostingRequireAffiliation { get; set; } = TournamentHostingRequireAffiliationDefault;
        public bool GrandTournamentEnabled { get; set; } = GrandTournamentEnabledDefault;
        public bool KingdomTournamentsEnabled { get; set; } = KingdomTournamentsEnabledDefault;
        public GrandTournamentSeason GrandTournamentSeason { get; set; } = GrandTournamentSeasonDefault;
        public int GrandTournamentDayOfSeason { get; set; } = GrandTournamentDayOfSeasonDefault;
        public int KingdomTournamentDayOfSeason { get; set; } = KingdomTournamentDayOfSeasonDefault;
        public int GrandTournamentAnnounceDays { get; set; } = GrandTournamentAnnounceDaysDefault;
        public int GrandTournamentDurationDays { get; set; } = GrandTournamentDurationDaysDefault;
        public bool GrandTournamentLegendaryPrize { get; set; } = GrandTournamentLegendaryPrizeDefault;
        public int GrandTournamentGoldPrize { get; set; } = GrandTournamentGoldPrizeDefault;
        public int GrandTournamentRenownReward { get; set; } = GrandTournamentRenownRewardDefault;
        public int GrandTournamentInfluenceReward { get; set; } = GrandTournamentInfluenceRewardDefault;
        public int GrandTournamentLeaderboardWeight { get; set; } = GrandTournamentLeaderboardWeightDefault;
        public int KingdomTournamentLeaderboardWeight { get; set; } = KingdomTournamentLeaderboardWeightDefault;
        public int KingdomTournamentRewardPercent { get; set; } = KingdomTournamentRewardPercentDefault;
        public bool GrandTournamentAnnouncePopup { get; set; } = GrandTournamentAnnouncePopupDefault;
        public bool GrandTournamentResultPopup { get; set; } = GrandTournamentResultPopupDefault;
        public int KingdomTournamentIntervalSeasons { get; set; } = KingdomTournamentIntervalSeasonsDefault;
        public bool TeamTournamentEnabled { get; set; } = TeamTournamentEnabledDefault;
        public int TeamTournamentTeamSizeMin { get; set; } = TeamTournamentTeamSizeMinDefault;
        public int TeamTournamentTeamSizeMax { get; set; } = TeamTournamentTeamSizeMaxDefault;
        public int TeamTournamentTeamsCountMax { get; set; } = TeamTournamentTeamsCountMaxDefault;
        public bool TeamTournamentScoresForWinningTeam { get; set; } = TeamTournamentScoresForWinningTeamDefault;
        public int TeamTournamentTeamOneColor { get; set; } = TeamTournamentTeamOneColorDefault;
        public int TeamTournamentTeamTwoColor { get; set; } = TeamTournamentTeamTwoColorDefault;
        public int TeamTournamentTeamThreeColor { get; set; } = TeamTournamentTeamThreeColorDefault;
        public int TeamTournamentTeamFourColor { get; set; } = TeamTournamentTeamFourColorDefault;
    }

    public class CompatibilitySection
    {
        public int PracticeLoadoutStages { get; set; } = PracticeLoadoutStagesDefault;
    }

    public class TechnicalSettingsSection
    {
        // Always the last entry.
        public bool LoggingEnabled { get; set; } = LoggingEnabledDefault;
    }
}
