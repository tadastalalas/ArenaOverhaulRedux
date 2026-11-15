using ArenaOverhaul.Core;

using HarmonyLib.BUTR.Extensions;

using MCM.Abstractions;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base;
using MCM.Abstractions.Base.Global;
using MCM.Abstractions.Base.PerSave;
using MCM.Abstractions.FluentBuilder;
using MCM.Common;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;

namespace ArenaOverhaul.Data;

// Used only when MCM is loaded. The only file that may use MCM types.
// Property names and types are released (MCM saves values by them): never rename or retype.
internal class MCMSettings : AttributeGlobalSettings<MCMSettings>
{
    // Released Id and FolderName: changing them resets every player's saved settings.
    private const string SettingsId = "ArenaOverhaulRedux";
    private const string SettingsFolderName = "Arena Overhaul Redux";
    private const string SaveFormat = "json2";
    private const string PercentFormat = "#0.0%";
    private const string WholePercentFormat = "0'%'";
    private const string SubGroupSeparator = "/";
    private const string PresetSavingMoneyId = "saving_money";
    private const string PresetBetterTrainingId = "better_training";

    private const string GroupPracticeRewards = Texts.Ids.GroupPractice + SubGroupSeparator + Texts.Ids.GroupRewards;
    private const string GroupPracticeExperience = Texts.Ids.GroupPractice + SubGroupSeparator + Texts.Ids.GroupExperience;
    private const string GroupExpansivePracticeRewards = Texts.Ids.GroupExpansivePractice + SubGroupSeparator + Texts.Ids.GroupRewards;
    private const string GroupExpansivePracticeExperience = Texts.Ids.GroupExpansivePractice + SubGroupSeparator + Texts.Ids.GroupExperience;
    private const string GroupParryPracticeExperience = Texts.Ids.GroupParryPractice + SubGroupSeparator + Texts.Ids.GroupExperience;
    private const string GroupTeamPracticeRewards = Texts.Ids.GroupTeamPractice + SubGroupSeparator + Texts.Ids.GroupRewards;
    private const string GroupTeamPracticeExperience = Texts.Ids.GroupTeamPractice + SubGroupSeparator + Texts.Ids.GroupExperience;
    private const string GroupTeamPracticeTechnicalSettings = Texts.Ids.GroupTeamPractice + SubGroupSeparator + Texts.Ids.GroupTechnical;
    private const string GroupTournamentsMaterialRewards = Texts.Ids.GroupTournaments + SubGroupSeparator + Texts.Ids.GroupMaterialRewards;
    private const string GroupTournamentsIntangibleRewards = Texts.Ids.GroupTournaments + SubGroupSeparator + Texts.Ids.GroupIntangibleRewards;
    private const string GroupTournamentsExperience = Texts.Ids.GroupTournaments + SubGroupSeparator + Texts.Ids.GroupExperience;
    private const string GroupTournamentsInjuries = Texts.Ids.GroupTournaments + SubGroupSeparator + Texts.Ids.GroupInjuries;
    private const string GroupTournamentsHosting = Texts.Ids.GroupTournaments + SubGroupSeparator + Texts.Ids.GroupHosting;
    private const string GroupTournamentsGrand = Texts.Ids.GroupTournaments + SubGroupSeparator + Texts.Ids.GroupGrandTournaments;
    private const string GroupTournamentsTeamGame = Texts.Ids.GroupTournaments + SubGroupSeparator + Texts.Ids.GroupTeamTournaments;

    public override string Id => SettingsId;

    public override string DisplayName => Texts.ModName.ToString();

    public override string FolderName => SettingsFolderName;

    public override string FormatType => SaveFormat;

    #region Default Arena Practice settings for heroes
    [SettingPropertyBool(Texts.Ids.HeroEnableLoadoutChoice, Order = 0, RequireRestart = true, HintText = Texts.Ids.HeroDefaultEnableLoadoutChoiceHint)]
    [SettingPropertyGroup(Texts.Ids.GroupHeroPracticeDefaults, GroupOrder = 0)]
    public bool HeroDefaultEnableLoadoutChoice { get; set; } = ModConfiguration.HeroDefaultEnableLoadoutChoiceDefault;

    [SettingPropertyBool(Texts.Ids.HeroOnlyPriorityLoadouts, Order = 1, RequireRestart = true, HintText = Texts.Ids.HeroDefaultOnlyPriorityLoadoutsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupHeroPracticeDefaults, GroupOrder = 0)]
    public bool HeroDefaultOnlyPriorityLoadouts { get; set; } = ModConfiguration.HeroDefaultOnlyPriorityLoadoutsDefault;

    [SettingPropertyBool(Texts.Ids.HeroPrioritizeExpensiveEquipment, Order = 2, RequireRestart = true, HintText = Texts.Ids.HeroDefaultPrioritizeExpensiveEquipmentHint)]
    [SettingPropertyGroup(Texts.Ids.GroupHeroPracticeDefaults, GroupOrder = 0)]
    public bool HeroDefaultPrioritizeExpensiveEquipment { get; set; } = ModConfiguration.HeroDefaultPrioritizeExpensiveEquipmentDefault;
    #endregion

    #region Arena Practice settings
    [SettingPropertyBool(Texts.Ids.EnableAgentSwitching, Order = 0, RequireRestart = false, HintText = Texts.Ids.PracticeEnableAgentSwitchingHint)]
    [SettingPropertyGroup(Texts.Ids.GroupPractice, GroupOrder = 10)]
    public bool PracticeEnableAgentSwitching { get; set; } = ModConfiguration.PracticeEnableAgentSwitchingDefault;

    [SettingPropertyInteger(Texts.Ids.TotalParticipants, ModConfiguration.PracticeTotalParticipantsMin, ModConfiguration.PracticeTotalParticipantsMax, Order = 10, RequireRestart = false, HintText = Texts.Ids.PracticeTotalParticipantsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupPractice, GroupOrder = 10)]
    public int PracticeTotalParticipants { get; set; } = ModConfiguration.PracticeTotalParticipantsDefault;

    [SettingPropertyInteger(Texts.Ids.ActiveParticipants, ModConfiguration.PracticeActiveParticipantsMin, ModConfiguration.PracticeActiveParticipantsMax, Order = 11, RequireRestart = false, HintText = Texts.Ids.PracticeActiveParticipantsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupPractice, GroupOrder = 10)]
    public int PracticeActiveParticipants { get; set; } = ModConfiguration.PracticeActiveParticipantsDefault;

    [SettingPropertyInteger(Texts.Ids.ActiveParticipantsMinimum, ModConfiguration.PracticeActiveParticipantsMinimumMin, ModConfiguration.PracticeActiveParticipantsMinimumMax, Order = 12, RequireRestart = false, HintText = Texts.Ids.PracticeActiveParticipantsMinimumHint)]
    [SettingPropertyGroup(Texts.Ids.GroupPractice, GroupOrder = 10)]
    public int PracticeActiveParticipantsMinimum { get; set; } = ModConfiguration.PracticeActiveParticipantsMinimumDefault;

    [SettingPropertyInteger(Texts.Ids.InitialParticipants, ModConfiguration.PracticeInitialParticipantsMin, ModConfiguration.PracticeInitialParticipantsMax, Order = 13, RequireRestart = false, HintText = Texts.Ids.PracticeInitialParticipantsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupPractice, GroupOrder = 10)]
    public int PracticeInitialParticipants { get; set; } = ModConfiguration.PracticeInitialParticipantsDefault;

    [SettingPropertyInteger(Texts.Ids.AITeamsCount, ModConfiguration.PracticeAITeamsCountMin, ModConfiguration.PracticeAITeamsCountMax, Order = 14, RequireRestart = false, HintText = Texts.Ids.PracticeAITeamsCountHint)]
    [SettingPropertyGroup(Texts.Ids.GroupPractice, GroupOrder = 10)]
    public int PracticeAITeamsCount { get; set; } = ModConfiguration.PracticeAITeamsCountDefault;

    [SettingPropertyBool(Texts.Ids.EnableLoadoutChoice, Order = 20, RequireRestart = false, HintText = Texts.Ids.PracticeEnableLoadoutChoiceHint)]
    [SettingPropertyGroup(Texts.Ids.GroupPractice, GroupOrder = 10)]
    public bool PracticeEnableLoadoutChoice { get; set; } = ModConfiguration.PracticeEnableLoadoutChoiceDefault;

    [SettingPropertyInteger(Texts.Ids.LoadoutChoiceCost, ModConfiguration.PracticeLoadoutChoiceCostMin, ModConfiguration.PracticeLoadoutChoiceCostMax, Order = 21, RequireRestart = false, HintText = Texts.Ids.PracticeLoadoutChoiceCostHint)]
    [SettingPropertyGroup(Texts.Ids.GroupPractice, GroupOrder = 10)]
    public int PracticeLoadoutChoiceCost { get; set; } = ModConfiguration.PracticeLoadoutChoiceCostDefault;

    // Item order = PracticeEquipmentType (MCM saves the index).
    [SettingPropertyDropdown(Texts.Ids.DefensivePracticeEquipment, Order = 22, RequireRestart = false, HintText = Texts.Ids.PracticeEquipmentHint)]
    [SettingPropertyGroup(Texts.Ids.GroupPractice, GroupOrder = 10)]
    public Dropdown<DropdownEnumItem<PracticeEquipmentType>> PracticeEquipment { get; set; } = NewDropdown(ModConfiguration.PracticeEquipmentDefault);
    #endregion

    #region Arena Practice settings / Rewards
    [SettingPropertyInteger(Texts.Ids.ValorReward1, ModConfiguration.PracticeValorReward1Min, ModConfiguration.PracticeValorReward1Max, Order = 0, RequireRestart = false, HintText = Texts.Ids.PracticeValorReward1Hint)]
    [SettingPropertyGroup(GroupPracticeRewards, GroupOrder = 0)]
    public int PracticeValorReward1 { get; set; } = ModConfiguration.PracticeValorReward1Default;

    [SettingPropertyInteger(Texts.Ids.ValorReward2, ModConfiguration.PracticeValorReward2Min, ModConfiguration.PracticeValorReward2Max, Order = 1, RequireRestart = false, HintText = Texts.Ids.PracticeValorReward2Hint)]
    [SettingPropertyGroup(GroupPracticeRewards, GroupOrder = 0)]
    public int PracticeValorReward2 { get; set; } = ModConfiguration.PracticeValorReward2Default;

    [SettingPropertyInteger(Texts.Ids.ValorReward3, ModConfiguration.PracticeValorReward3Min, ModConfiguration.PracticeValorReward3Max, Order = 2, RequireRestart = false, HintText = Texts.Ids.PracticeValorReward3Hint)]
    [SettingPropertyGroup(GroupPracticeRewards, GroupOrder = 0)]
    public int PracticeValorReward3 { get; set; } = ModConfiguration.PracticeValorReward3Default;

    [SettingPropertyInteger(Texts.Ids.ValorReward4, ModConfiguration.PracticeValorReward4Min, ModConfiguration.PracticeValorReward4Max, Order = 3, RequireRestart = false, HintText = Texts.Ids.PracticeValorReward4Hint)]
    [SettingPropertyGroup(GroupPracticeRewards, GroupOrder = 0)]
    public int PracticeValorReward4 { get; set; } = ModConfiguration.PracticeValorReward4Default;

    [SettingPropertyInteger(Texts.Ids.ValorReward5, ModConfiguration.PracticeValorReward5Min, ModConfiguration.PracticeValorReward5Max, Order = 4, RequireRestart = false, HintText = Texts.Ids.PracticeValorReward5Hint)]
    [SettingPropertyGroup(GroupPracticeRewards, GroupOrder = 0)]
    public int PracticeValorReward5 { get; set; } = ModConfiguration.PracticeValorReward5Default;

    // Item order = ChampionPrizeCalculationType (MCM saves the index).
    [SettingPropertyDropdown(Texts.Ids.ChampionPrizeCalculation, Order = 5, RequireRestart = false, HintText = Texts.Ids.PracticeChampionPrizeCalculationHint)]
    [SettingPropertyGroup(GroupPracticeRewards, GroupOrder = 0)]
    public Dropdown<DropdownEnumItem<ChampionPrizeCalculationType>> PracticeChampionPrizeCalculation { get; set; } = NewDropdown(ModConfiguration.PracticeChampionPrizeCalculationDefault);

    [SettingPropertyInteger(Texts.Ids.ChampionReward, ModConfiguration.PracticeChampionRewardMin, ModConfiguration.PracticeChampionRewardMax, Order = 6, RequireRestart = false, HintText = Texts.Ids.PracticeChampionRewardHint)]
    [SettingPropertyGroup(GroupPracticeRewards, GroupOrder = 0)]
    public int PracticeChampionReward { get; set; } = ModConfiguration.PracticeChampionRewardDefault;
    #endregion

    #region Arena Practice settings / Experience
    [SettingPropertyFloatingInteger(Texts.Ids.ExperienceRate, ModConfiguration.ExperienceRateMin, ModConfiguration.ExperienceRateMax, PercentFormat, Order = 0, RequireRestart = false, HintText = Texts.Ids.PracticeExperienceRateHint)]
    [SettingPropertyGroup(GroupPracticeExperience, GroupOrder = 1)]
    public float PracticeExperienceRate { get; set; } = ModConfiguration.PracticeExperienceRateDefault;
    #endregion

    #region Expansive Arena Practice settings
    [SettingPropertyBool(Texts.Ids.EnableAgentSwitching, Order = 0, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeEnableAgentSwitchingHint)]
    [SettingPropertyGroup(Texts.Ids.GroupExpansivePractice, GroupOrder = 11)]
    public bool ExpansivePracticeEnableAgentSwitching { get; set; } = ModConfiguration.ExpansivePracticeEnableAgentSwitchingDefault;

    [SettingPropertyInteger(Texts.Ids.TotalParticipants, ModConfiguration.ExpansivePracticeTotalParticipantsMin, ModConfiguration.ExpansivePracticeTotalParticipantsMax, Order = 10, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeTotalParticipantsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupExpansivePractice, GroupOrder = 11)]
    public int ExpansivePracticeTotalParticipants { get; set; } = ModConfiguration.ExpansivePracticeTotalParticipantsDefault;

    [SettingPropertyInteger(Texts.Ids.ActiveParticipants, ModConfiguration.ExpansivePracticeActiveParticipantsMin, ModConfiguration.ExpansivePracticeActiveParticipantsMax, Order = 11, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeActiveParticipantsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupExpansivePractice, GroupOrder = 11)]
    public int ExpansivePracticeActiveParticipants { get; set; } = ModConfiguration.ExpansivePracticeActiveParticipantsDefault;

    [SettingPropertyInteger(Texts.Ids.ActiveParticipantsMinimum, ModConfiguration.ExpansivePracticeActiveParticipantsMinimumMin, ModConfiguration.ExpansivePracticeActiveParticipantsMinimumMax, Order = 12, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeActiveParticipantsMinimumHint)]
    [SettingPropertyGroup(Texts.Ids.GroupExpansivePractice, GroupOrder = 11)]
    public int ExpansivePracticeActiveParticipantsMinimum { get; set; } = ModConfiguration.ExpansivePracticeActiveParticipantsMinimumDefault;

    [SettingPropertyInteger(Texts.Ids.InitialParticipants, ModConfiguration.ExpansivePracticeInitialParticipantsMin, ModConfiguration.ExpansivePracticeInitialParticipantsMax, Order = 13, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeInitialParticipantsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupExpansivePractice, GroupOrder = 11)]
    public int ExpansivePracticeInitialParticipants { get; set; } = ModConfiguration.ExpansivePracticeInitialParticipantsDefault;

    [SettingPropertyInteger(Texts.Ids.AITeamsCount, ModConfiguration.ExpansivePracticeAITeamsCountMin, ModConfiguration.ExpansivePracticeAITeamsCountMax, Order = 14, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeAITeamsCountHint)]
    [SettingPropertyGroup(Texts.Ids.GroupExpansivePractice, GroupOrder = 11)]
    public int ExpansivePracticeAITeamsCount { get; set; } = ModConfiguration.ExpansivePracticeAITeamsCountDefault;

    [SettingPropertyBool(Texts.Ids.EnableLoadoutChoice, Order = 20, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeEnableLoadoutChoiceHint)]
    [SettingPropertyGroup(Texts.Ids.GroupExpansivePractice, GroupOrder = 11)]
    public bool ExpansivePracticeEnableLoadoutChoice { get; set; } = ModConfiguration.ExpansivePracticeEnableLoadoutChoiceDefault;

    [SettingPropertyInteger(Texts.Ids.LoadoutChoiceCost, ModConfiguration.ExpansivePracticeLoadoutChoiceCostMin, ModConfiguration.ExpansivePracticeLoadoutChoiceCostMax, Order = 21, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeLoadoutChoiceCostHint)]
    [SettingPropertyGroup(Texts.Ids.GroupExpansivePractice, GroupOrder = 11)]
    public int ExpansivePracticeLoadoutChoiceCost { get; set; } = ModConfiguration.ExpansivePracticeLoadoutChoiceCostDefault;

    // Item order = PracticeEquipmentType (MCM saves the index).
    [SettingPropertyDropdown(Texts.Ids.DefensivePracticeEquipment, Order = 22, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeEquipmentHint)]
    [SettingPropertyGroup(Texts.Ids.GroupExpansivePractice, GroupOrder = 11)]
    public Dropdown<DropdownEnumItem<PracticeEquipmentType>> ExpansivePracticeEquipment { get; set; } = NewDropdown(ModConfiguration.ExpansivePracticeEquipmentDefault);
    #endregion

    #region Expansive Arena Practice settings / Rewards
    [SettingPropertyInteger(Texts.Ids.ValorReward1, ModConfiguration.ExpansivePracticeValorReward1Min, ModConfiguration.ExpansivePracticeValorReward1Max, Order = 0, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeValorReward1Hint)]
    [SettingPropertyGroup(GroupExpansivePracticeRewards, GroupOrder = 0)]
    public int ExpansivePracticeValorReward1 { get; set; } = ModConfiguration.ExpansivePracticeValorReward1Default;

    [SettingPropertyInteger(Texts.Ids.ValorReward2, ModConfiguration.ExpansivePracticeValorReward2Min, ModConfiguration.ExpansivePracticeValorReward2Max, Order = 1, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeValorReward2Hint)]
    [SettingPropertyGroup(GroupExpansivePracticeRewards, GroupOrder = 0)]
    public int ExpansivePracticeValorReward2 { get; set; } = ModConfiguration.ExpansivePracticeValorReward2Default;

    [SettingPropertyInteger(Texts.Ids.ValorReward3, ModConfiguration.ExpansivePracticeValorReward3Min, ModConfiguration.ExpansivePracticeValorReward3Max, Order = 2, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeValorReward3Hint)]
    [SettingPropertyGroup(GroupExpansivePracticeRewards, GroupOrder = 0)]
    public int ExpansivePracticeValorReward3 { get; set; } = ModConfiguration.ExpansivePracticeValorReward3Default;

    [SettingPropertyInteger(Texts.Ids.ValorReward4, ModConfiguration.ExpansivePracticeValorReward4Min, ModConfiguration.ExpansivePracticeValorReward4Max, Order = 3, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeValorReward4Hint)]
    [SettingPropertyGroup(GroupExpansivePracticeRewards, GroupOrder = 0)]
    public int ExpansivePracticeValorReward4 { get; set; } = ModConfiguration.ExpansivePracticeValorReward4Default;

    [SettingPropertyInteger(Texts.Ids.ValorReward5, ModConfiguration.ExpansivePracticeValorReward5Min, ModConfiguration.ExpansivePracticeValorReward5Max, Order = 4, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeValorReward5Hint)]
    [SettingPropertyGroup(GroupExpansivePracticeRewards, GroupOrder = 0)]
    public int ExpansivePracticeValorReward5 { get; set; } = ModConfiguration.ExpansivePracticeValorReward5Default;

    // Item order = ChampionPrizeCalculationType (MCM saves the index).
    [SettingPropertyDropdown(Texts.Ids.ChampionPrizeCalculation, Order = 5, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeChampionPrizeCalculationHint)]
    [SettingPropertyGroup(GroupExpansivePracticeRewards, GroupOrder = 0)]
    public Dropdown<DropdownEnumItem<ChampionPrizeCalculationType>> ExpansivePracticeChampionPrizeCalculation { get; set; } = NewDropdown(ModConfiguration.ExpansivePracticeChampionPrizeCalculationDefault);

    [SettingPropertyInteger(Texts.Ids.ChampionReward, ModConfiguration.ExpansivePracticeChampionRewardMin, ModConfiguration.ExpansivePracticeChampionRewardMax, Order = 6, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeChampionRewardHint)]
    [SettingPropertyGroup(GroupExpansivePracticeRewards, GroupOrder = 0)]
    public int ExpansivePracticeChampionReward { get; set; } = ModConfiguration.ExpansivePracticeChampionRewardDefault;
    #endregion

    #region Expansive Arena Practice settings / Experience
    [SettingPropertyBool(Texts.Ids.EnableViewerExperienceGain, Order = 0, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeEnableViewerExperienceGainHint)]
    [SettingPropertyGroup(GroupExpansivePracticeExperience, GroupOrder = 1)]
    public bool ExpansivePracticeEnableViewerExperienceGain { get; set; } = ModConfiguration.ExpansivePracticeEnableViewerExperienceGainDefault;

    [SettingPropertyFloatingInteger(Texts.Ids.ExperienceRate, ModConfiguration.ExperienceRateMin, ModConfiguration.ExperienceRateMax, PercentFormat, Order = 1, RequireRestart = false, HintText = Texts.Ids.ExpansivePracticeExperienceRateHint)]
    [SettingPropertyGroup(GroupExpansivePracticeExperience, GroupOrder = 1)]
    public float ExpansivePracticeExperienceRate { get; set; } = ModConfiguration.ExpansivePracticeExperienceRateDefault;
    #endregion

    #region Parry Practice settings
    [SettingPropertyBool(Texts.Ids.ParryPracticeEnabled, Order = 0, RequireRestart = false, HintText = Texts.Ids.ParryPracticeEnabledHint)]
    [SettingPropertyGroup(Texts.Ids.GroupParryPractice, GroupOrder = 12)]
    public bool ParryPracticeEnabled { get; set; } = ModConfiguration.ParryPracticeEnabledDefault;

    [SettingPropertyInteger(Texts.Ids.ParryPracticeSetupCost, ModConfiguration.ParryPracticeSetupCostMin, ModConfiguration.ParryPracticeSetupCostMax, Order = 1, RequireRestart = false, HintText = Texts.Ids.ParryPracticeSetupCostHint)]
    [SettingPropertyGroup(Texts.Ids.GroupParryPractice, GroupOrder = 12)]
    public int ParryPracticeSetupCost { get; set; } = ModConfiguration.ParryPracticeSetupCostDefault;

    [SettingPropertyBool(Texts.Ids.EnableAgentSwitching, Order = 2, RequireRestart = false, HintText = Texts.Ids.ParryPracticeEnableAgentSwitchingHint)]
    [SettingPropertyGroup(Texts.Ids.GroupParryPractice, GroupOrder = 12)]
    public bool ParryPracticeEnableAgentSwitching { get; set; } = ModConfiguration.ParryPracticeEnableAgentSwitchingDefault;

    [SettingPropertyInteger(Texts.Ids.TotalParticipants, ModConfiguration.ParryPracticeTotalParticipantsMin, ModConfiguration.ParryPracticeTotalParticipantsMax, Order = 10, RequireRestart = false, HintText = Texts.Ids.ParryPracticeTotalParticipantsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupParryPractice, GroupOrder = 12)]
    public int ParryPracticeTotalParticipants { get; set; } = ModConfiguration.ParryPracticeTotalParticipantsDefault;

    [SettingPropertyInteger(Texts.Ids.ActiveParticipants, ModConfiguration.ParryPracticeActiveParticipantsMin, ModConfiguration.ParryPracticeActiveParticipantsMax, Order = 11, RequireRestart = false, HintText = Texts.Ids.ParryPracticeActiveParticipantsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupParryPractice, GroupOrder = 12)]
    public int ParryPracticeActiveParticipants { get; set; } = ModConfiguration.ParryPracticeActiveParticipantsDefault;

    [SettingPropertyInteger(Texts.Ids.ActiveParticipantsMinimum, ModConfiguration.ParryPracticeActiveParticipantsMinimumMin, ModConfiguration.ParryPracticeActiveParticipantsMinimumMax, Order = 12, RequireRestart = false, HintText = Texts.Ids.ParryPracticeActiveParticipantsMinimumHint)]
    [SettingPropertyGroup(Texts.Ids.GroupParryPractice, GroupOrder = 12)]
    public int ParryPracticeActiveParticipantsMinimum { get; set; } = ModConfiguration.ParryPracticeActiveParticipantsMinimumDefault;

    [SettingPropertyInteger(Texts.Ids.InitialParticipants, ModConfiguration.ParryPracticeInitialParticipantsMin, ModConfiguration.ParryPracticeInitialParticipantsMax, Order = 13, RequireRestart = false, HintText = Texts.Ids.ParryPracticeInitialParticipantsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupParryPractice, GroupOrder = 12)]
    public int ParryPracticeInitialParticipants { get; set; } = ModConfiguration.ParryPracticeInitialParticipantsDefault;

    [SettingPropertyInteger(Texts.Ids.AITeamsCount, ModConfiguration.ParryPracticeAITeamsCountMin, ModConfiguration.ParryPracticeAITeamsCountMax, Order = 14, RequireRestart = false, HintText = Texts.Ids.ParryPracticeAITeamsCountHint)]
    [SettingPropertyGroup(Texts.Ids.GroupParryPractice, GroupOrder = 12)]
    public int ParryPracticeAITeamsCount { get; set; } = ModConfiguration.ParryPracticeAITeamsCountDefault;

    [SettingPropertyBool(Texts.Ids.EnableLoadoutChoice, Order = 20, RequireRestart = false, HintText = Texts.Ids.ParryPracticeEnableLoadoutChoiceHint)]
    [SettingPropertyGroup(Texts.Ids.GroupParryPractice, GroupOrder = 12)]
    public bool ParryPracticeEnableLoadoutChoice { get; set; } = ModConfiguration.ParryPracticeEnableLoadoutChoiceDefault;

    [SettingPropertyInteger(Texts.Ids.LoadoutChoiceCost, ModConfiguration.ParryPracticeLoadoutChoiceCostMin, ModConfiguration.ParryPracticeLoadoutChoiceCostMax, Order = 21, RequireRestart = false, HintText = Texts.Ids.ParryPracticeLoadoutChoiceCostHint)]
    [SettingPropertyGroup(Texts.Ids.GroupParryPractice, GroupOrder = 12)]
    public int ParryPracticeLoadoutChoiceCost { get; set; } = ModConfiguration.ParryPracticeLoadoutChoiceCostDefault;

    // Item order = PracticeEquipmentType (MCM saves the index).
    [SettingPropertyDropdown(Texts.Ids.DefensivePracticeEquipment, Order = 22, RequireRestart = false, HintText = Texts.Ids.ParryPracticeEquipmentHint)]
    [SettingPropertyGroup(Texts.Ids.GroupParryPractice, GroupOrder = 12)]
    public Dropdown<DropdownEnumItem<PracticeEquipmentType>> ParryPracticeEquipment { get; set; } = NewDropdown(ModConfiguration.ParryPracticeEquipmentDefault);
    #endregion

    #region Parry Practice settings / Experience
    [SettingPropertyFloatingInteger(Texts.Ids.ExperienceRate, ModConfiguration.ExperienceRateMin, ModConfiguration.ExperienceRateMax, PercentFormat, Order = 0, RequireRestart = false, HintText = Texts.Ids.ParryPracticeExperienceRateHint)]
    [SettingPropertyGroup(GroupParryPracticeExperience, GroupOrder = 0)]
    public float ParryPracticeExperienceRate { get; set; } = ModConfiguration.ParryPracticeExperienceRateDefault;
    #endregion

    #region Team Arena Practice settings
    [SettingPropertyBool(Texts.Ids.TeamPracticeEnabled, Order = 0, RequireRestart = false, HintText = Texts.Ids.TeamPracticeEnabledHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTeamPractice, GroupOrder = 13)]
    public bool TeamPracticeEnabled { get; set; } = ModConfiguration.TeamPracticeEnabledDefault;

    [SettingPropertyInteger(Texts.Ids.TeamPracticeSetupCost, ModConfiguration.TeamPracticeSetupCostMin, ModConfiguration.TeamPracticeSetupCostMax, Order = 1, RequireRestart = false, HintText = Texts.Ids.TeamPracticeSetupCostHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTeamPractice, GroupOrder = 13)]
    public int TeamPracticeSetupCost { get; set; } = ModConfiguration.TeamPracticeSetupCostDefault;

    [SettingPropertyBool(Texts.Ids.EnableAgentSwitching, Order = 2, RequireRestart = false, HintText = Texts.Ids.TeamPracticeEnableAgentSwitchingHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTeamPractice, GroupOrder = 13)]
    public bool TeamPracticeEnableAgentSwitching { get; set; } = ModConfiguration.TeamPracticeEnableAgentSwitchingDefault;

    [SettingPropertyInteger(Texts.Ids.TotalParticipants, ModConfiguration.TeamPracticeTotalParticipantsMin, ModConfiguration.TeamPracticeTotalParticipantsMax, Order = 10, RequireRestart = false, HintText = Texts.Ids.TeamPracticeTotalParticipantsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTeamPractice, GroupOrder = 13)]
    public int TeamPracticeTotalParticipants { get; set; } = ModConfiguration.TeamPracticeTotalParticipantsDefault;

    [SettingPropertyInteger(Texts.Ids.ActiveParticipants, ModConfiguration.TeamPracticeActiveParticipantsMin, ModConfiguration.TeamPracticeActiveParticipantsMax, Order = 11, RequireRestart = false, HintText = Texts.Ids.TeamPracticeActiveParticipantsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTeamPractice, GroupOrder = 13)]
    public int TeamPracticeActiveParticipants { get; set; } = ModConfiguration.TeamPracticeActiveParticipantsDefault;

    [SettingPropertyInteger(Texts.Ids.ActiveParticipantsMinimum, ModConfiguration.TeamPracticeActiveParticipantsMinimumMin, ModConfiguration.TeamPracticeActiveParticipantsMinimumMax, Order = 12, RequireRestart = false, HintText = Texts.Ids.TeamPracticeActiveParticipantsMinimumHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTeamPractice, GroupOrder = 13)]
    public int TeamPracticeActiveParticipantsMinimum { get; set; } = ModConfiguration.TeamPracticeActiveParticipantsMinimumDefault;

    [SettingPropertyInteger(Texts.Ids.InitialParticipants, ModConfiguration.TeamPracticeInitialParticipantsMin, ModConfiguration.TeamPracticeInitialParticipantsMax, Order = 13, RequireRestart = false, HintText = Texts.Ids.TeamPracticeInitialParticipantsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTeamPractice, GroupOrder = 13)]
    public int TeamPracticeInitialParticipants { get; set; } = ModConfiguration.TeamPracticeInitialParticipantsDefault;

    [SettingPropertyInteger(Texts.Ids.AITeamsCount, ModConfiguration.TeamPracticeAITeamsCountMin, ModConfiguration.TeamPracticeAITeamsCountMax, Order = 14, RequireRestart = false, HintText = Texts.Ids.TeamPracticeAITeamsCountHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTeamPractice, GroupOrder = 13)]
    public int TeamPracticeAITeamsCount { get; set; } = ModConfiguration.TeamPracticeAITeamsCountDefault;

    [SettingPropertyBool(Texts.Ids.EnableLoadoutChoice, Order = 20, RequireRestart = false, HintText = Texts.Ids.TeamPracticeEnableLoadoutChoiceHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTeamPractice, GroupOrder = 13)]
    public bool TeamPracticeEnableLoadoutChoice { get; set; } = ModConfiguration.TeamPracticeEnableLoadoutChoiceDefault;

    [SettingPropertyInteger(Texts.Ids.LoadoutChoiceCost, ModConfiguration.TeamPracticeLoadoutChoiceCostMin, ModConfiguration.TeamPracticeLoadoutChoiceCostMax, Order = 21, RequireRestart = false, HintText = Texts.Ids.TeamPracticeLoadoutChoiceCostHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTeamPractice, GroupOrder = 13)]
    public int TeamPracticeLoadoutChoiceCost { get; set; } = ModConfiguration.TeamPracticeLoadoutChoiceCostDefault;

    // Item order = PracticeEquipmentType (MCM saves the index).
    [SettingPropertyDropdown(Texts.Ids.DefensivePracticeEquipment, Order = 22, RequireRestart = false, HintText = Texts.Ids.TeamPracticeEquipmentHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTeamPractice, GroupOrder = 13)]
    public Dropdown<DropdownEnumItem<PracticeEquipmentType>> TeamPracticeEquipment { get; set; } = NewDropdown(ModConfiguration.TeamPracticeEquipmentDefault);
    #endregion

    #region Team Arena Practice settings / Rewards
    [SettingPropertyInteger(Texts.Ids.ValorReward1, ModConfiguration.TeamPracticeValorReward1Min, ModConfiguration.TeamPracticeValorReward1Max, Order = 0, RequireRestart = false, HintText = Texts.Ids.TeamPracticeValorReward1Hint)]
    [SettingPropertyGroup(GroupTeamPracticeRewards, GroupOrder = 0)]
    public int TeamPracticeValorReward1 { get; set; } = ModConfiguration.TeamPracticeValorReward1Default;

    [SettingPropertyInteger(Texts.Ids.ValorReward2, ModConfiguration.TeamPracticeValorReward2Min, ModConfiguration.TeamPracticeValorReward2Max, Order = 1, RequireRestart = false, HintText = Texts.Ids.TeamPracticeValorReward2Hint)]
    [SettingPropertyGroup(GroupTeamPracticeRewards, GroupOrder = 0)]
    public int TeamPracticeValorReward2 { get; set; } = ModConfiguration.TeamPracticeValorReward2Default;

    [SettingPropertyInteger(Texts.Ids.ValorReward3, ModConfiguration.TeamPracticeValorReward3Min, ModConfiguration.TeamPracticeValorReward3Max, Order = 2, RequireRestart = false, HintText = Texts.Ids.TeamPracticeValorReward3Hint)]
    [SettingPropertyGroup(GroupTeamPracticeRewards, GroupOrder = 0)]
    public int TeamPracticeValorReward3 { get; set; } = ModConfiguration.TeamPracticeValorReward3Default;

    [SettingPropertyInteger(Texts.Ids.ValorReward4, ModConfiguration.TeamPracticeValorReward4Min, ModConfiguration.TeamPracticeValorReward4Max, Order = 3, RequireRestart = false, HintText = Texts.Ids.TeamPracticeValorReward4Hint)]
    [SettingPropertyGroup(GroupTeamPracticeRewards, GroupOrder = 0)]
    public int TeamPracticeValorReward4 { get; set; } = ModConfiguration.TeamPracticeValorReward4Default;

    [SettingPropertyInteger(Texts.Ids.ValorReward5, ModConfiguration.TeamPracticeValorReward5Min, ModConfiguration.TeamPracticeValorReward5Max, Order = 4, RequireRestart = false, HintText = Texts.Ids.TeamPracticeValorReward5Hint)]
    [SettingPropertyGroup(GroupTeamPracticeRewards, GroupOrder = 0)]
    public int TeamPracticeValorReward5 { get; set; } = ModConfiguration.TeamPracticeValorReward5Default;

    // Item order = ChampionPrizeCalculationType (MCM saves the index).
    [SettingPropertyDropdown(Texts.Ids.ChampionPrizeCalculation, Order = 5, RequireRestart = false, HintText = Texts.Ids.TeamPracticeChampionPrizeCalculationHint)]
    [SettingPropertyGroup(GroupTeamPracticeRewards, GroupOrder = 0)]
    public Dropdown<DropdownEnumItem<ChampionPrizeCalculationType>> TeamPracticeChampionPrizeCalculation { get; set; } = NewDropdown(ModConfiguration.TeamPracticeChampionPrizeCalculationDefault);

    [SettingPropertyInteger(Texts.Ids.ChampionReward, ModConfiguration.TeamPracticeChampionRewardMin, ModConfiguration.TeamPracticeChampionRewardMax, Order = 6, RequireRestart = false, HintText = Texts.Ids.TeamPracticeChampionRewardHint)]
    [SettingPropertyGroup(GroupTeamPracticeRewards, GroupOrder = 0)]
    public int TeamPracticeChampionReward { get; set; } = ModConfiguration.TeamPracticeChampionRewardDefault;
    #endregion

    #region Team Arena Practice settings / Experience
    [SettingPropertyBool(Texts.Ids.EnableViewerExperienceGain, Order = 0, RequireRestart = false, HintText = Texts.Ids.TeamPracticeEnableViewerExperienceGainHint)]
    [SettingPropertyGroup(GroupTeamPracticeExperience, GroupOrder = 1)]
    public bool TeamPracticeEnableViewerExperienceGain { get; set; } = ModConfiguration.TeamPracticeEnableViewerExperienceGainDefault;

    [SettingPropertyFloatingInteger(Texts.Ids.ExperienceRate, ModConfiguration.ExperienceRateMin, ModConfiguration.ExperienceRateMax, PercentFormat, Order = 1, RequireRestart = false, HintText = Texts.Ids.TeamPracticeExperienceRateHint)]
    [SettingPropertyGroup(GroupTeamPracticeExperience, GroupOrder = 1)]
    public float TeamPracticeExperienceRate { get; set; } = ModConfiguration.TeamPracticeExperienceRateDefault;
    #endregion

    #region Team Arena Practice settings / Technical settings
    [SettingPropertyInteger(Texts.Ids.TeamPracticeTeamOneColor, ModConfiguration.ColorIndexMin, ModConfiguration.ColorIndexMax, Order = 10, RequireRestart = false, HintText = Texts.Ids.TeamColorIndexHint)]
    [SettingPropertyGroup(GroupTeamPracticeTechnicalSettings, GroupOrder = 10)]
    public int TeamPracticeTeamOneColor { get; set; } = ModConfiguration.TeamPracticeTeamOneColorDefault;

    [SettingPropertyInteger(Texts.Ids.TeamPracticeTeamTwoColor, ModConfiguration.ColorIndexMin, ModConfiguration.ColorIndexMax, Order = 11, RequireRestart = false, HintText = Texts.Ids.TeamColorIndexHint)]
    [SettingPropertyGroup(GroupTeamPracticeTechnicalSettings, GroupOrder = 10)]
    public int TeamPracticeTeamTwoColor { get; set; } = ModConfiguration.TeamPracticeTeamTwoColorDefault;

    [SettingPropertyInteger(Texts.Ids.TeamPracticeTeamThreeColor, ModConfiguration.ColorIndexMin, ModConfiguration.ColorIndexMax, Order = 12, RequireRestart = false, HintText = Texts.Ids.TeamColorIndexHint)]
    [SettingPropertyGroup(GroupTeamPracticeTechnicalSettings, GroupOrder = 10)]
    public int TeamPracticeTeamThreeColor { get; set; } = ModConfiguration.TeamPracticeTeamThreeColorDefault;

    [SettingPropertyInteger(Texts.Ids.TeamPracticeTeamFourColor, ModConfiguration.ColorIndexMin, ModConfiguration.ColorIndexMax, Order = 13, RequireRestart = false, HintText = Texts.Ids.TeamColorIndexHint)]
    [SettingPropertyGroup(GroupTeamPracticeTechnicalSettings, GroupOrder = 10)]
    public int TeamPracticeTeamFourColor { get; set; } = ModConfiguration.TeamPracticeTeamFourColorDefault;

    [SettingPropertyInteger(Texts.Ids.TeamPracticeTeamFiveColor, ModConfiguration.ColorIndexMin, ModConfiguration.ColorIndexMax, Order = 14, RequireRestart = false, HintText = Texts.Ids.TeamColorIndexHint)]
    [SettingPropertyGroup(GroupTeamPracticeTechnicalSettings, GroupOrder = 10)]
    public int TeamPracticeTeamFiveColor { get; set; } = ModConfiguration.TeamPracticeTeamFiveColorDefault;

    [SettingPropertyInteger(Texts.Ids.TeamPracticeTeamSixColor, ModConfiguration.ColorIndexMin, ModConfiguration.ColorIndexMax, Order = 15, RequireRestart = false, HintText = Texts.Ids.TeamColorIndexHint)]
    [SettingPropertyGroup(GroupTeamPracticeTechnicalSettings, GroupOrder = 10)]
    public int TeamPracticeTeamSixColor { get; set; } = ModConfiguration.TeamPracticeTeamSixColorDefault;
    #endregion

    #region Tournament settings
    [SettingPropertyInteger(Texts.Ids.TournamentMaximumBet, ModConfiguration.TournamentMaximumBetMin, ModConfiguration.TournamentMaximumBetMax, Order = 0, RequireRestart = false, HintText = Texts.Ids.TournamentMaximumBetHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTournaments, GroupOrder = 20)]
    public int TournamentMaximumBet { get; set; } = ModConfiguration.TournamentMaximumBetDefault;

    [SettingPropertyBool(Texts.Ids.TournamentEnableRandomizedBettingOdds, Order = 1, RequireRestart = false, HintText = Texts.Ids.TournamentEnableRandomizedBettingOddsHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTournaments, GroupOrder = 20)]
    public bool TournamentEnableRandomizedBettingOdds { get; set; } = ModConfiguration.TournamentEnableRandomizedBettingOddsDefault;

    [SettingPropertyBool(Texts.Ids.TournamentAllowNotablesParticipation, Order = 2, RequireRestart = false, HintText = Texts.Ids.TournamentAllowNotablesParticipationHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTournaments, GroupOrder = 20)]
    public bool TournamentAllowNotablesParticipation { get; set; } = ModConfiguration.TournamentAllowNotablesParticipationDefault;

    // Item order = PrizeRerollCondition (MCM saves the index).
    [SettingPropertyDropdown(Texts.Ids.TournamentPrizeRerollCondition, Order = 3, RequireRestart = false, HintText = Texts.Ids.TournamentPrizeRerollConditionHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTournaments, GroupOrder = 20)]
    public Dropdown<DropdownEnumItem<PrizeRerollCondition>> TournamentPrizeRerollCondition { get; set; } = NewDropdown(ModConfiguration.TournamentPrizeRerollConditionDefault);
    #endregion

    #region Tournament settings / Material rewards
    [SettingPropertyBool(Texts.Ids.TournamentEnableGoldPrizes, Order = 0, RequireRestart = false, HintText = Texts.Ids.TournamentEnableGoldPrizesHint)]
    [SettingPropertyGroup(GroupTournamentsMaterialRewards, GroupOrder = 0)]
    public bool TournamentEnableGoldPrizes { get; set; } = ModConfiguration.TournamentEnableGoldPrizesDefault;

    [SettingPropertyBool(Texts.Ids.TournamentEnablePrizeScaling, Order = 10, RequireRestart = false, HintText = Texts.Ids.TournamentEnablePrizeScalingHint)]
    [SettingPropertyGroup(GroupTournamentsMaterialRewards, GroupOrder = 0)]
    public bool TournamentEnablePrizeScaling { get; set; } = ModConfiguration.TournamentEnablePrizeScalingDefault;

    [SettingPropertyBool(Texts.Ids.TournamentEnableHighQualityPrizes, Order = 11, RequireRestart = false, HintText = Texts.Ids.TournamentEnableHighQualityPrizesHint)]
    [SettingPropertyGroup(GroupTournamentsMaterialRewards, GroupOrder = 0)]
    public bool TournamentEnableHighQualityPrizes { get; set; } = ModConfiguration.TournamentEnableHighQualityPrizesDefault;

    // Item order = CultureRestrictedPrizesMode (MCM saves the index).
    [SettingPropertyDropdown(Texts.Ids.TournamentCultureRestrictedPrizes, Order = 20, RequireRestart = false, HintText = Texts.Ids.TournamentCultureRestrictedPrizesHint)]
    [SettingPropertyGroup(GroupTournamentsMaterialRewards, GroupOrder = 0)]
    public Dropdown<DropdownEnumItem<CultureRestrictedPrizesMode>> TournamentCultureRestrictedPrizes { get; set; } = NewDropdown(ModConfiguration.TournamentCultureRestrictedPrizesDefault);

    [SettingPropertyInteger(Texts.Ids.TournamentRoundWonReward, ModConfiguration.TournamentRoundWonRewardMin, ModConfiguration.TournamentRoundWonRewardMax, Order = 30, RequireRestart = false, HintText = Texts.Ids.TournamentRoundWonRewardHint)]
    [SettingPropertyGroup(GroupTournamentsMaterialRewards, GroupOrder = 0)]
    public int TournamentRoundWonReward { get; set; } = ModConfiguration.TournamentRoundWonRewardDefault;
    #endregion

    #region Tournament settings / Intangible rewards
    [SettingPropertyInteger(Texts.Ids.TournamentBaseRenownReward, ModConfiguration.TournamentBaseRenownRewardMin, ModConfiguration.TournamentBaseRenownRewardMax, Order = 0, RequireRestart = false, HintText = Texts.Ids.TournamentBaseRenownRewardHint)]
    [SettingPropertyGroup(GroupTournamentsIntangibleRewards, GroupOrder = 1)]
    public int TournamentBaseRenownReward { get; set; } = ModConfiguration.TournamentBaseRenownRewardDefault;

    [SettingPropertyInteger(Texts.Ids.TournamentTakedownRenownReward, ModConfiguration.TournamentTakedownRenownRewardMin, ModConfiguration.TournamentTakedownRenownRewardMax, Order = 1, RequireRestart = false, HintText = Texts.Ids.TournamentTakedownRenownRewardHint)]
    [SettingPropertyGroup(GroupTournamentsIntangibleRewards, GroupOrder = 1)]
    public int TournamentTakedownRenownReward { get; set; } = ModConfiguration.TournamentTakedownRenownRewardDefault;

    [SettingPropertyInteger(Texts.Ids.TournamentInfluenceReward, ModConfiguration.TournamentInfluenceRewardMin, ModConfiguration.TournamentInfluenceRewardMax, Order = 2, RequireRestart = false, HintText = Texts.Ids.TournamentInfluenceRewardHint)]
    [SettingPropertyGroup(GroupTournamentsIntangibleRewards, GroupOrder = 1)]
    public int TournamentInfluenceReward { get; set; } = ModConfiguration.TournamentInfluenceRewardDefault;
    #endregion

    #region Tournament settings / Experience
    [SettingPropertyFloatingInteger(Texts.Ids.ExperienceRate, ModConfiguration.ExperienceRateMin, ModConfiguration.ExperienceRateMax, PercentFormat, Order = 0, RequireRestart = false, HintText = Texts.Ids.TournamentExperienceRateHint)]
    [SettingPropertyGroup(GroupTournamentsExperience, GroupOrder = 2)]
    public float TournamentExperienceRate { get; set; } = ModConfiguration.TournamentExperienceRateDefault;
    #endregion

    #region Tournament settings / Injuries
    [SettingPropertyBool(Texts.Ids.TournamentEnableInjuries, Order = 0, RequireRestart = false, HintText = Texts.Ids.TournamentEnableInjuriesHint)]
    [SettingPropertyGroup(GroupTournamentsInjuries, GroupOrder = 3)]
    public bool TournamentEnableInjuries { get; set; } = ModConfiguration.TournamentEnableInjuriesDefault;

    [SettingPropertyInteger(Texts.Ids.TournamentInjuryHitPointsLoss, ModConfiguration.TournamentInjuryHitPointsLossMin, ModConfiguration.TournamentInjuryHitPointsLossMax, WholePercentFormat, Order = 1, RequireRestart = false, HintText = Texts.Ids.TournamentInjuryHitPointsLossHint)]
    [SettingPropertyGroup(GroupTournamentsInjuries, GroupOrder = 3)]
    public int TournamentInjuryHitPointsLoss { get; set; } = ModConfiguration.TournamentInjuryHitPointsLossDefault;
    #endregion

    #region Tournament settings / Hosting
    [SettingPropertyBool(Texts.Ids.TournamentHostingEnabled, Order = 0, RequireRestart = false, HintText = Texts.Ids.TournamentHostingEnabledHint)]
    [SettingPropertyGroup(GroupTournamentsHosting, GroupOrder = 4)]
    public bool TournamentHostingEnabled { get; set; } = ModConfiguration.TournamentHostingEnabledDefault;

    [SettingPropertyInteger(Texts.Ids.TournamentHostingBaseGold, ModConfiguration.TournamentHostingBaseGoldMin, ModConfiguration.TournamentHostingBaseGoldMax, Order = 1, RequireRestart = false, HintText = Texts.Ids.TournamentHostingBaseGoldHint)]
    [SettingPropertyGroup(GroupTournamentsHosting, GroupOrder = 4)]
    public int TournamentHostingBaseGold { get; set; } = ModConfiguration.TournamentHostingBaseGoldDefault;

    [SettingPropertyInteger(Texts.Ids.TournamentHostingBaseInfluence, ModConfiguration.TournamentHostingBaseInfluenceMin, ModConfiguration.TournamentHostingBaseInfluenceMax, Order = 2, RequireRestart = false, HintText = Texts.Ids.TournamentHostingBaseInfluenceHint)]
    [SettingPropertyGroup(GroupTournamentsHosting, GroupOrder = 4)]
    public int TournamentHostingBaseInfluence { get; set; } = ModConfiguration.TournamentHostingBaseInfluenceDefault;

    [SettingPropertyInteger(Texts.Ids.TournamentHostingCooldownDays, ModConfiguration.TournamentHostingCooldownDaysMin, ModConfiguration.TournamentHostingCooldownDaysMax, Order = 3, RequireRestart = false, HintText = Texts.Ids.TournamentHostingCooldownDaysHint)]
    [SettingPropertyGroup(GroupTournamentsHosting, GroupOrder = 4)]
    public int TournamentHostingCooldownDays { get; set; } = ModConfiguration.TournamentHostingCooldownDaysDefault;

    [SettingPropertyBool(Texts.Ids.TournamentHostingRequireAffiliation, Order = 4, RequireRestart = false, HintText = Texts.Ids.TournamentHostingRequireAffiliationHint)]
    [SettingPropertyGroup(GroupTournamentsHosting, GroupOrder = 4)]
    public bool TournamentHostingRequireAffiliation { get; set; } = ModConfiguration.TournamentHostingRequireAffiliationDefault;
    #endregion

    #region Tournament settings / Grand and Kingdom Tournaments
    [SettingPropertyBool(Texts.Ids.GrandTournamentEnabled, Order = 0, RequireRestart = false, HintText = Texts.Ids.GrandTournamentEnabledHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public bool GrandTournamentEnabled { get; set; } = ModConfiguration.GrandTournamentEnabledDefault;

    [SettingPropertyBool(Texts.Ids.KingdomTournamentsEnabled, Order = 1, RequireRestart = false, HintText = Texts.Ids.KingdomTournamentsEnabledHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public bool KingdomTournamentsEnabled { get; set; } = ModConfiguration.KingdomTournamentsEnabledDefault;

    // Item order = GrandTournamentSeason (MCM saves the index).
    [SettingPropertyDropdown(Texts.Ids.GrandTournamentSeason, Order = 2, RequireRestart = false, HintText = Texts.Ids.GrandTournamentSeasonHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public Dropdown<DropdownEnumItem<GrandTournamentSeason>> GrandTournamentSeason { get; set; } = NewDropdown(ModConfiguration.GrandTournamentSeasonDefault);

    [SettingPropertyInteger(Texts.Ids.GrandTournamentDayOfSeason, ModConfiguration.GrandTournamentDayOfSeasonMin, ModConfiguration.GrandTournamentDayOfSeasonMax, Order = 3, RequireRestart = false, HintText = Texts.Ids.GrandTournamentDayOfSeasonHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public int GrandTournamentDayOfSeason { get; set; } = ModConfiguration.GrandTournamentDayOfSeasonDefault;

    [SettingPropertyInteger(Texts.Ids.KingdomTournamentDayOfSeason, ModConfiguration.KingdomTournamentDayOfSeasonMin, ModConfiguration.KingdomTournamentDayOfSeasonMax, Order = 4, RequireRestart = false, HintText = Texts.Ids.KingdomTournamentDayOfSeasonHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public int KingdomTournamentDayOfSeason { get; set; } = ModConfiguration.KingdomTournamentDayOfSeasonDefault;

    [SettingPropertyInteger(Texts.Ids.GrandTournamentAnnounceDays, ModConfiguration.GrandTournamentAnnounceDaysMin, ModConfiguration.GrandTournamentAnnounceDaysMax, Order = 5, RequireRestart = false, HintText = Texts.Ids.GrandTournamentAnnounceDaysHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public int GrandTournamentAnnounceDays { get; set; } = ModConfiguration.GrandTournamentAnnounceDaysDefault;

    [SettingPropertyInteger(Texts.Ids.GrandTournamentDurationDays, ModConfiguration.GrandTournamentDurationDaysMin, ModConfiguration.GrandTournamentDurationDaysMax, Order = 6, RequireRestart = false, HintText = Texts.Ids.GrandTournamentDurationDaysHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public int GrandTournamentDurationDays { get; set; } = ModConfiguration.GrandTournamentDurationDaysDefault;

    [SettingPropertyBool(Texts.Ids.GrandTournamentLegendaryPrize, Order = 7, RequireRestart = false, HintText = Texts.Ids.GrandTournamentLegendaryPrizeHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public bool GrandTournamentLegendaryPrize { get; set; } = ModConfiguration.GrandTournamentLegendaryPrizeDefault;

    [SettingPropertyInteger(Texts.Ids.GrandTournamentGoldPrize, ModConfiguration.GrandTournamentGoldPrizeMin, ModConfiguration.GrandTournamentGoldPrizeMax, Order = 8, RequireRestart = false, HintText = Texts.Ids.GrandTournamentGoldPrizeHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public int GrandTournamentGoldPrize { get; set; } = ModConfiguration.GrandTournamentGoldPrizeDefault;

    [SettingPropertyInteger(Texts.Ids.GrandTournamentRenownReward, ModConfiguration.GrandTournamentRenownRewardMin, ModConfiguration.GrandTournamentRenownRewardMax, Order = 9, RequireRestart = false, HintText = Texts.Ids.GrandTournamentRenownRewardHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public int GrandTournamentRenownReward { get; set; } = ModConfiguration.GrandTournamentRenownRewardDefault;

    [SettingPropertyInteger(Texts.Ids.GrandTournamentInfluenceReward, ModConfiguration.GrandTournamentInfluenceRewardMin, ModConfiguration.GrandTournamentInfluenceRewardMax, Order = 10, RequireRestart = false, HintText = Texts.Ids.GrandTournamentInfluenceRewardHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public int GrandTournamentInfluenceReward { get; set; } = ModConfiguration.GrandTournamentInfluenceRewardDefault;

    [SettingPropertyInteger(Texts.Ids.GrandTournamentLeaderboardWeight, ModConfiguration.GrandTournamentLeaderboardWeightMin, ModConfiguration.GrandTournamentLeaderboardWeightMax, Order = 11, RequireRestart = false, HintText = Texts.Ids.GrandTournamentLeaderboardWeightHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public int GrandTournamentLeaderboardWeight { get; set; } = ModConfiguration.GrandTournamentLeaderboardWeightDefault;

    [SettingPropertyInteger(Texts.Ids.KingdomTournamentLeaderboardWeight, ModConfiguration.KingdomTournamentLeaderboardWeightMin, ModConfiguration.KingdomTournamentLeaderboardWeightMax, Order = 12, RequireRestart = false, HintText = Texts.Ids.KingdomTournamentLeaderboardWeightHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public int KingdomTournamentLeaderboardWeight { get; set; } = ModConfiguration.KingdomTournamentLeaderboardWeightDefault;

    [SettingPropertyInteger(Texts.Ids.KingdomTournamentRewardPercent, ModConfiguration.KingdomTournamentRewardPercentMin, ModConfiguration.KingdomTournamentRewardPercentMax, Order = 13, RequireRestart = false, HintText = Texts.Ids.KingdomTournamentRewardPercentHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public int KingdomTournamentRewardPercent { get; set; } = ModConfiguration.KingdomTournamentRewardPercentDefault;

    [SettingPropertyBool(Texts.Ids.GrandTournamentAnnouncePopup, Order = 14, RequireRestart = false, HintText = Texts.Ids.GrandTournamentAnnouncePopupHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public bool GrandTournamentAnnouncePopup { get; set; } = ModConfiguration.GrandTournamentAnnouncePopupDefault;

    [SettingPropertyBool(Texts.Ids.GrandTournamentResultPopup, Order = 15, RequireRestart = false, HintText = Texts.Ids.GrandTournamentResultPopupHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public bool GrandTournamentResultPopup { get; set; } = ModConfiguration.GrandTournamentResultPopupDefault;

    [SettingPropertyInteger(Texts.Ids.KingdomTournamentIntervalSeasons, ModConfiguration.KingdomTournamentIntervalSeasonsMin, ModConfiguration.KingdomTournamentIntervalSeasonsMax, Order = 16, RequireRestart = false, HintText = Texts.Ids.KingdomTournamentIntervalSeasonsHint)]
    [SettingPropertyGroup(GroupTournamentsGrand, GroupOrder = 5)]
    public int KingdomTournamentIntervalSeasons { get; set; } = ModConfiguration.KingdomTournamentIntervalSeasonsDefault;
    #endregion

    #region Tournament settings / Team Tournaments
    [SettingPropertyBool(Texts.Ids.TeamTournamentEnabled, Order = 0, RequireRestart = false, HintText = Texts.Ids.TeamTournamentEnabledHint)]
    [SettingPropertyGroup(GroupTournamentsTeamGame, GroupOrder = 10)]
    public bool TeamTournamentEnabled { get; set; } = ModConfiguration.TeamTournamentEnabledDefault;

    [SettingPropertyInteger(Texts.Ids.TeamTournamentTeamSizeMin, ModConfiguration.TeamTournamentTeamSizeMinMin, ModConfiguration.TeamTournamentTeamSizeMinMax, Order = 5, RequireRestart = false, HintText = Texts.Ids.TeamTournamentTeamSizeMinHint)]
    [SettingPropertyGroup(GroupTournamentsTeamGame, GroupOrder = 10)]
    public int TeamTournamentTeamSizeMin { get; set; } = ModConfiguration.TeamTournamentTeamSizeMinDefault;

    [SettingPropertyInteger(Texts.Ids.TeamTournamentTeamSizeMax, ModConfiguration.TeamTournamentTeamSizeMaxMin, ModConfiguration.TeamTournamentTeamSizeMaxMax, Order = 6, RequireRestart = false, HintText = Texts.Ids.TeamTournamentTeamSizeMaxHint)]
    [SettingPropertyGroup(GroupTournamentsTeamGame, GroupOrder = 10)]
    public int TeamTournamentTeamSizeMax { get; set; } = ModConfiguration.TeamTournamentTeamSizeMaxDefault;

    // Item order = ModConfiguration.TeamTournamentTeamsCountMaxChoices (MCM saves the index).
    [SettingPropertyDropdown(Texts.Ids.TeamTournamentTeamsCountMax, Order = 10, RequireRestart = false, HintText = Texts.Ids.TeamTournamentTeamsCountMaxHint)]
    [SettingPropertyGroup(GroupTournamentsTeamGame, GroupOrder = 10)]
    public Dropdown<int> TeamTournamentTeamsCountMax { get; set; } = new(ModConfiguration.TeamTournamentTeamsCountMaxChoices, Array.IndexOf(ModConfiguration.TeamTournamentTeamsCountMaxChoices, ModConfiguration.TeamTournamentTeamsCountMaxDefault));

    [SettingPropertyBool(Texts.Ids.TeamTournamentScoresForWinningTeam, Order = 15, RequireRestart = false, HintText = Texts.Ids.TeamTournamentScoresForWinningTeamHint)]
    [SettingPropertyGroup(GroupTournamentsTeamGame, GroupOrder = 10)]
    public bool TeamTournamentScoresForWinningTeam { get; set; } = ModConfiguration.TeamTournamentScoresForWinningTeamDefault;

    [SettingPropertyInteger(Texts.Ids.TeamTournamentTeamOneColor, ModConfiguration.ColorIndexMin, ModConfiguration.ColorIndexMax, Order = 25, RequireRestart = false, HintText = Texts.Ids.TeamColorIndexHint)]
    [SettingPropertyGroup(GroupTournamentsTeamGame, GroupOrder = 10)]
    public int TeamTournamentTeamOneColor { get; set; } = ModConfiguration.TeamTournamentTeamOneColorDefault;

    [SettingPropertyInteger(Texts.Ids.TeamTournamentTeamTwoColor, ModConfiguration.ColorIndexMin, ModConfiguration.ColorIndexMax, Order = 26, RequireRestart = false, HintText = Texts.Ids.TeamColorIndexHint)]
    [SettingPropertyGroup(GroupTournamentsTeamGame, GroupOrder = 10)]
    public int TeamTournamentTeamTwoColor { get; set; } = ModConfiguration.TeamTournamentTeamTwoColorDefault;

    [SettingPropertyInteger(Texts.Ids.TeamTournamentTeamThreeColor, ModConfiguration.ColorIndexMin, ModConfiguration.ColorIndexMax, Order = 27, RequireRestart = false, HintText = Texts.Ids.TeamColorIndexHint)]
    [SettingPropertyGroup(GroupTournamentsTeamGame, GroupOrder = 10)]
    public int TeamTournamentTeamThreeColor { get; set; } = ModConfiguration.TeamTournamentTeamThreeColorDefault;

    [SettingPropertyInteger(Texts.Ids.TeamTournamentTeamFourColor, ModConfiguration.ColorIndexMin, ModConfiguration.ColorIndexMax, Order = 28, RequireRestart = false, HintText = Texts.Ids.TeamColorIndexHint)]
    [SettingPropertyGroup(GroupTournamentsTeamGame, GroupOrder = 10)]
    public int TeamTournamentTeamFourColor { get; set; } = ModConfiguration.TeamTournamentTeamFourColorDefault;
    #endregion

    #region Compatibility settings
    [SettingPropertyInteger(Texts.Ids.PracticeLoadoutStages, ModConfiguration.PracticeLoadoutStagesMin, ModConfiguration.PracticeLoadoutStagesMax, Order = 0, RequireRestart = true, HintText = Texts.Ids.PracticeLoadoutStagesHint)]
    [SettingPropertyGroup(Texts.Ids.GroupCompatibility, GroupOrder = 100)]
    public int PracticeLoadoutStages { get; set; } = ModConfiguration.PracticeLoadoutStagesDefault;
    #endregion

    // "Technical settings" is always the last group and "Logging for debugging" its last entry.
    [SettingPropertyBool(Texts.Ids.LoggingEnabled, Order = 0, RequireRestart = false, HintText = Texts.Ids.LoggingEnabledHint)]
    [SettingPropertyGroup(Texts.Ids.GroupTechnical, GroupOrder = 101)]
    public bool LoggingEnabled { get; set; } = ModConfiguration.LoggingEnabledDefault;

    public override IEnumerable<ISettingsPreset> GetBuiltInPresets()
    {
        // Include all the presets that MCM provides
        foreach (ISettingsPreset preset in base.GetBuiltInPresets())
        {
            yield return preset;
        }

        yield return new MemorySettingsPreset(Id, PresetSavingMoneyId, Texts.PresetSavingMoney.ToString(), () => new MCMSettings()
        {
            HeroDefaultEnableLoadoutChoice = false,
        });

        yield return new MemorySettingsPreset(Id, PresetBetterTrainingId, Texts.PresetBetterTraining.ToString(), () => new MCMSettings()
        {
            HeroDefaultEnableLoadoutChoice = true,
            HeroDefaultOnlyPriorityLoadouts = false,
            HeroDefaultPrioritizeExpensiveEquipment = true
        });
    }

    private static Dropdown<DropdownEnumItem<T>> NewDropdown<T>(T selected) where T : struct =>
        new(DropdownEnumItem<T>.SetDropdownListFromEnum(), Convert.ToInt32(selected));

    #region Per-hero practice settings (per save)
    // Released ids: MCM stores the per-save values under them.
    private const string HeroPracticeSettingsIdPrefix = "CompanionPracticeSettings_v";
    private const string HeroPracticeSettingsFolderName = "AO Hero Practice Settings";
    private const string EnableLoadoutChoiceIdSuffix = "_enable_loadout_choice";
    private const string OnlyPriorityLoadoutsIdSuffix = "_only_priority_loadouts";
    private const string PrioritizeExpensiveEquipmentIdSuffix = "_prioritize_expensive_equipment";
    private const string FirstPriorityIdSuffix = "_weapons_first_priority";
    private const string SecondPriorityIdSuffix = "_weapons_second_priority";
    private const string ThirdPriorityIdSuffix = "_weapons_third_priority";
    private const int IdVersionFieldCount = 1;
    private const int DisplayVersionFieldCount = 3;
    private const int ClanLeaderGroupOrder = 1;
    private const int FamilyMembersGroupOrder = 2;
    private const int CompanionsGroupOrder = 3;
    private const int EnableLoadoutChoiceOrder = 0;
    private const int OnlyPriorityLoadoutsOrder = 1;
    private const int PrioritizeExpensiveEquipmentOrder = 2;
    private const int FirstPriorityOrder = 10;
    private const int SecondPriorityOrder = 11;
    private const int ThirdPriorityOrder = 12;

    private static FluentPerSaveSettings? _heroPracticeSettings;

    private static Version? CurrentVersion => typeof(MCMSettings).Assembly.GetName().Version;

    private static string HeroPracticeSettingsId => HeroPracticeSettingsIdPrefix + CurrentVersion?.ToString(IdVersionFieldCount);

    private static string HeroPracticeSettingsName => $"{Texts.HeroPracticeSettingsName} {CurrentVersion!.ToString(DisplayVersionFieldCount)}";

    internal static void RegisterHeroPracticeSettings(Dictionary<Hero, HeroPracticeSettings> heroPracticeSettings)
    {
        ISettingsBuilder builder = BuildHeroPracticeSettings(heroPracticeSettings);
        _heroPracticeSettings = builder.BuildAsPerSave();
        _heroPracticeSettings?.Register();
    }

    internal static void UnregisterHeroPracticeSettings()
    {
        _heroPracticeSettings?.Unregister();
        _heroPracticeSettings = null;
    }

    private static ISettingsBuilder BuildHeroPracticeSettings(Dictionary<Hero, HeroPracticeSettings> heroPracticeSettings)
    {
        ISettingsBuilder builder = BaseSettingsBuilder.Create(HeroPracticeSettingsId, HeroPracticeSettingsName)!
            .SetFormat(SaveFormat)
            .SetFolderName(HeroPracticeSettingsFolderName);

        List<Hero> clanMembers = Hero.FindAll(x => x.IsAlive && !x.IsChild && x.Clan == Clan.PlayerClan && x != Hero.MainHero).OrderBy(x => x.Name.ToString()).ToList();

        AddClanHeroes(builder, Texts.Ids.HeadingClanLeader, [Hero.MainHero], ClanLeaderGroupOrder, heroPracticeSettings);
        AddClanHeroes(builder, Texts.Ids.HeadingFamilyMembers, clanMembers.Where(x => !x.IsWanderer).ToList(), FamilyMembersGroupOrder, heroPracticeSettings);
        AddClanHeroes(builder, Texts.Ids.HeadingCompanions, clanMembers.Where(x => x.IsWanderer).ToList(), CompanionsGroupOrder, heroPracticeSettings);

        builder.CreatePreset(BaseSettings.DefaultPresetId, BaseSettings.DefaultPresetName, presetBuilder => BuildDefaultPreset(presetBuilder, clanMembers));

        return builder;
    }

    private static void AddClanHeroes(ISettingsBuilder builder, string heading, List<Hero> heroes, int groupOrder, Dictionary<Hero, HeroPracticeSettings> heroPracticeSettings)
    {
        builder.CreateGroup(heading, groupBuilder => groupBuilder.SetGroupOrder(groupOrder));
        int heroGroupOrder = 0;
        foreach (Hero hero in heroes)
        {
            builder.CreateGroup(heading + SubGroupSeparator + hero.Name, heroGroupBuilder => BuildForHero(heroGroupBuilder, hero, heroGroupOrder++, heroPracticeSettings));
        }
    }

    private static void BuildForHero(ISettingsPropertyGroupBuilder builder, Hero hero, int groupOrder, Dictionary<Hero, HeroPracticeSettings> heroPracticeSettings)
    {
        if (!heroPracticeSettings.TryGetValue(hero, out HeroPracticeSettings? settingContainer))
        {
            settingContainer = new HeroPracticeSettings();
            heroPracticeSettings[hero] = settingContainer;
        }

        // MCM keeps the chosen weapon priorities in these dropdowns; the hero settings read them live.
        HeroWeaponPriorities priorities = new();
        settingContainer.FirstPrioritySource = () => priorities.First.SelectedValue.EnumValue;
        settingContainer.SecondPrioritySource = () => priorities.Second.SelectedValue.EnumValue;
        settingContainer.ThirdPrioritySource = () => priorities.Third.SelectedValue.EnumValue;

        bool isPlayer = hero == Hero.MainHero;
        builder
            .AddBool(hero.StringId + EnableLoadoutChoiceIdSuffix, isPlayer ? Texts.Ids.PlayerEnableLoadoutChoice : Texts.Ids.HeroEnableLoadoutChoice, new PropertyRef(SymbolExtensions2.GetPropertyInfo((HeroPracticeSettings x) => x.EnableLoadoutChoice)!, settingContainer),
                propBuilder => propBuilder.SetRequireRestart(false).SetOrder(EnableLoadoutChoiceOrder).SetHintText(isPlayer ? Texts.Ids.PlayerEnableLoadoutChoiceHint : Texts.Ids.HeroEnableLoadoutChoiceHint))
            .AddBool(hero.StringId + OnlyPriorityLoadoutsIdSuffix, Texts.Ids.HeroOnlyPriorityLoadouts, new PropertyRef(SymbolExtensions2.GetPropertyInfo((HeroPracticeSettings x) => x.OnlyPriorityLoadouts)!, settingContainer),
                propBuilder => propBuilder.SetRequireRestart(false).SetOrder(OnlyPriorityLoadoutsOrder).SetHintText(isPlayer ? Texts.Ids.PlayerOnlyPriorityLoadoutsHint : Texts.Ids.HeroOnlyPriorityLoadoutsHint))
            .AddBool(hero.StringId + PrioritizeExpensiveEquipmentIdSuffix, Texts.Ids.HeroPrioritizeExpensiveEquipment, new PropertyRef(SymbolExtensions2.GetPropertyInfo((HeroPracticeSettings x) => x.PrioritizeExpensiveEquipment)!, settingContainer),
                propBuilder => propBuilder.SetRequireRestart(false).SetOrder(PrioritizeExpensiveEquipmentOrder).SetHintText(isPlayer ? Texts.Ids.PlayerPrioritizeExpensiveEquipmentHint : Texts.Ids.HeroPrioritizeExpensiveEquipmentHint))
            .AddDropdown(hero.StringId + FirstPriorityIdSuffix, Texts.Ids.FirstPriority, priorities.First.SelectedIndex,
                new PropertyRef(SymbolExtensions2.GetPropertyInfo((HeroWeaponPriorities x) => x.First)!, priorities),
                propBuilder => propBuilder.SetRequireRestart(false).SetOrder(FirstPriorityOrder).SetHintText(isPlayer ? Texts.Ids.PlayerFirstPriorityHint : Texts.Ids.HeroFirstPriorityHint))
            .AddDropdown(hero.StringId + SecondPriorityIdSuffix, Texts.Ids.SecondPriority, priorities.Second.SelectedIndex,
                new PropertyRef(SymbolExtensions2.GetPropertyInfo((HeroWeaponPriorities x) => x.Second)!, priorities),
                propBuilder => propBuilder.SetRequireRestart(false).SetOrder(SecondPriorityOrder).SetHintText(isPlayer ? Texts.Ids.PlayerSecondPriorityHint : Texts.Ids.HeroSecondPriorityHint))
            .AddDropdown(hero.StringId + ThirdPriorityIdSuffix, Texts.Ids.ThirdPriority, priorities.Third.SelectedIndex,
                new PropertyRef(SymbolExtensions2.GetPropertyInfo((HeroWeaponPriorities x) => x.Third)!, priorities),
                propBuilder => propBuilder.SetRequireRestart(false).SetOrder(ThirdPriorityOrder).SetHintText(isPlayer ? Texts.Ids.PlayerThirdPriorityHint : Texts.Ids.HeroThirdPriorityHint))
            .SetGroupOrder(groupOrder);
    }

    private static void BuildDefaultPreset(ISettingsPresetBuilder builder, List<Hero> clanMembers)
    {
        foreach (Hero hero in (List<Hero>) [Hero.MainHero, .. clanMembers])
        {
            HeroPracticeSettings defaultSettingContainer = new();
            HeroWeaponPriorities defaultPriorities = new();
            builder
                .SetPropertyValue(hero.StringId + EnableLoadoutChoiceIdSuffix, defaultSettingContainer.EnableLoadoutChoice)
                .SetPropertyValue(hero.StringId + OnlyPriorityLoadoutsIdSuffix, defaultSettingContainer.OnlyPriorityLoadouts)
                .SetPropertyValue(hero.StringId + PrioritizeExpensiveEquipmentIdSuffix, defaultSettingContainer.PrioritizeExpensiveEquipment)
                .SetPropertyValue(hero.StringId + FirstPriorityIdSuffix, defaultPriorities.First)
                .SetPropertyValue(hero.StringId + SecondPriorityIdSuffix, defaultPriorities.Second)
                .SetPropertyValue(hero.StringId + ThirdPriorityIdSuffix, defaultPriorities.Third);
        }
    }

    // The three weapon priority dropdowns of one hero (item order = WeaponPreference, MCM saves the index).
    private sealed class HeroWeaponPriorities
    {
        internal Dropdown<DropdownEnumItem<WeaponPreference>> First { get; set; } = NewDropdown(WeaponPreference.None);
        internal Dropdown<DropdownEnumItem<WeaponPreference>> Second { get; set; } = NewDropdown(WeaponPreference.None);
        internal Dropdown<DropdownEnumItem<WeaponPreference>> Third { get; set; } = NewDropdown(WeaponPreference.None);
    }
    #endregion
}