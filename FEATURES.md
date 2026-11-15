# Arena Overhaul Redux — Feature Map

Feature → gate → files. Read this first, then open only the files listed.
Gate: `setting:<Name>` = setting read through `Core\ModSettings.cs` (MCM when loaded, else `ModConfiguration.xml`). `always` = no setting.
`1.5.7 only` = block inside `#if !LOWER_THAN_1_5`.

## Core / setup

| Feature | Gate | Files |
|---|---|---|
| Harmony patches applied once before the main menu, settings reload, behavior and model registration | always | `SubModule.cs` |
| Settings: MCM when loaded, otherwise ModConfiguration.xml | always | `Core\ModSettings.cs`, `Data\MCMSettings.cs`, `Data\ModConfiguration.cs`, `_Module\ModConfiguration.xml` |
| MCM presets (saving money, better training) | always (MCM) | `Data\MCMSettings.cs` |
| Dropdown enums and their items | always | `Data\DropdownEnumItem.cs`, `Data\PracticeEquipmentType.cs`, `Data\ChampionPrizeCalculationType.cs`, `Data\CultureRestrictedPrizesMode.cs`, `Data\PrizeRerollCondition.cs`, `Data\GrandTournamentSeason.cs`, `Data\WeaponPreference.cs`, `Core\EnumExtensions.cs` |
| Module id, file names, module root path | always | `Core\ModInfo.cs` |
| All user-facing strings | always | `Core\Texts.cs`, `_Module\ModuleData\Languages\`, `_Module\ModuleData\module_strings.xml` |
| Log file in `Modules\ArenaOverhaulRedux\logs`; failed transpiler hooks | problems: always; debug: setting:LoggingEnabled | `Diagnostics\AorLog.cs`, `Diagnostics\TranspilerReport.cs` |
| Saved state (the `[SaveableProperty]` members of `AOArenaBehaviorManager`, SyncData key `_AOArenaBehaviorManager`) | always | `Managers\AOArenaBehaviorManager.cs`, `Behaviors\AOArenaBehavior.cs`, `Data\ArenaOverhaulSaveableTypeDefiner.cs` |
| Shared helpers | always | `Core\MessageHelper.cs`, `Core\MathHelper.cs`, `Core\StringExtensions.cs`, `Core\FieldAccessHelper.cs`, `Core\ItemHelper.cs` |

## Arena practice

| Feature | Gate | Files |
|---|---|---|
| Practice modes (Standard, Expansive, Parry, Team): arena master dialogs and arena menu options | Parry: setting:ParryPracticeEnabled; Team: setting:TeamPracticeEnabled; others always | `Behaviors\AOArenaBehavior.cs`, `Patches\ArenaMasterCampaignBehaviorPatch.cs`, `Core\ArenaPracticeMode.cs`, `Managers\AOArenaBehaviorManager.cs` |
| Match size and pace per mode (total / active / initial participants, AI teams, defensive equipment) | always (values: `<Mode>TotalParticipants`, `<Mode>ActiveParticipants`, `<Mode>ActiveParticipantsMinimum`, `<Mode>InitialParticipants`, `<Mode>AITeamsCount`, `<Mode>Equipment`) | `Patches\ArenaPracticeFightMissionControllerPatch.cs`, `Managers\AOArenaBehaviorManager.cs` |
| Weapon loadout choice and its price | setting:`<Mode>EnableLoadoutChoice` (values: `<Mode>LoadoutChoiceCost`, `PracticeLoadoutStages`) | `Behaviors\AOArenaBehavior.cs`, `Patches\ArenaPracticeFightMissionControllerPatch.cs`, `Managers\AOArenaBehaviorManager.cs` |
| Playing as a companion, switching hero during Team Practice (hotkey) | setting:`<Mode>EnableAgentSwitching` | `Missions\TeamPracticeHotKeyController.cs`, `Managers\TeamPracticeController.cs`, `Managers\AOArenaBehaviorManager.cs` |
| Per-hero loadout options and weapon priorities | MCM loaded (per save); else setting:HeroDefaultEnableLoadoutChoice, HeroDefaultOnlyPriorityLoadouts, HeroDefaultPrioritizeExpensiveEquipment | `Data\MCMSettings.cs`, `Data\HeroPracticeSettings.cs`, `Managers\AOArenaBehaviorManager.cs` |
| Team Practice: clan heroes and troops as the player team, team colors, agent labels | setting:TeamPracticeEnabled (values: `TeamPracticeSetupCost`, `TeamPracticeTeam<N>Color`) | `Managers\TeamPracticeController.cs`, `Managers\TeamPracticeStatsManager.cs`, `Patches\SandBoxMissionViewsPatch.cs`, `Patches\ArenaPracticeFightMissionControllerPatch.cs` |
| Parry Practice statistics and setup cost | setting:ParryPracticeEnabled (value: `ParryPracticeSetupCost`) | `Managers\ParryPracticeStatsManager.cs`, `Managers\AOArenaBehaviorManager.cs` |
| Practice rewards (valor classes, champion prize) and their on-screen panel | always (values: `<Mode>ValorReward1`–`5`, `<Mode>ChampionReward`, `<Mode>ChampionPrizeCalculation`) | `Managers\PracticePrizeManager.cs`, `UI\MissionArenaPracticeFightVMMixin.cs`, `UI\ArenaPracticeFightPrefabExtension.cs` |
| Practice XP rate per mode | always (value: `<Mode>ExperienceRate`) | `Managers\AOArenaBehaviorManager.cs`, `Patches\ArenaPracticeFightMissionControllerPatch.cs` |
| Party troops gain XP while watching | setting:ExpansivePracticeEnableViewerExperienceGain, setting:TeamPracticeEnableViewerExperienceGain | `Patches\ArenaPracticeFightMissionControllerPatch.cs` |

## Tournaments

| Feature | Gate | Files |
|---|---|---|
| List of ongoing tournaments in the arena menu | always | `Behaviors\AOArenaBehavior.cs` |
| Prize pool: high quality items, culture restriction, scaling, reroll, no crafted items | setting:TournamentEnableHighQualityPrizes, setting:TournamentEnablePrizeScaling (values: `TournamentCultureRestrictedPrizes`, `TournamentPrizeRerollCondition`) | `Models\ArenaOverhaulTournamentModel.cs`, `Patches\FightTournamentGamePatch.cs`, `Patches\TournamentGamePatch.cs`, `Managers\TournamentRewardManager.cs`, `Data\PrizeItemInfo.cs`, `Core\ItemHelper.cs` |
| Gold, renown and influence rewards for every participant; results screen | setting:TournamentEnableGoldPrizes (values: `TournamentRoundWonReward`, `TournamentBaseRenownReward`, `TournamentTakedownRenownReward`, `TournamentInfluenceReward`) | `Managers\TournamentRewardManager.cs`, `Patches\TournamentBehaviorPatch.cs`, `Patches\TournamentManagerPatch.cs`, `Patches\TournamentCampaignBehaviorPatch.cs`, `Patches\TournamentFightMissionControllerPatch.cs`, `Patches\DefaultTournamentModelPatch.cs`, `Patches\TournamentVMPatch.cs` |
| Betting: maximum bet, randomized odds | setting:TournamentEnableRandomizedBettingOdds (value: `TournamentMaximumBet`) | `Patches\TournamentBehaviorPatch.cs`, `Missions\TeamTournamentBehavior.cs` |
| Tournament XP rate | always (value: `TournamentExperienceRate`) | `Patches\DefaultCombatXpModelPatch.cs` |
| Participant selection, notables in tournaments | setting:TournamentAllowNotablesParticipation | `Managers\AbstractTournamentApplicantManager.cs`, `Managers\FightTournamentApplicantManager.cs`, `Data\AbstractTournamentApplicant.cs`, `Data\FightTournamentApplicant.cs`, `Patches\FightTournamentGamePatch.cs` |
| Tournament injuries | setting:TournamentEnableInjuries (value: `TournamentInjuryHitPointsLoss`) | `Managers\TournamentInjuryManager.cs` |
| Tournament hosting | setting:TournamentHostingEnabled (values: `TournamentHostingBaseGold`, `TournamentHostingBaseInfluence`, `TournamentHostingCooldownDays`, `TournamentHostingRequireAffiliation`) | `Managers\TournamentHostingManager.cs`, `Behaviors\AOArenaBehavior.cs` |
| Grand Tournament of Calradia and Kingdom Tournaments: schedule, roster, fixed prizes, join lock | setting:GrandTournamentEnabled, setting:KingdomTournamentsEnabled (values: `GrandTournamentSeason`, `GrandTournamentDayOfSeason`, `GrandTournamentAnnounceDays`, `GrandTournamentDurationDays`, `KingdomTournamentDayOfSeason`, `KingdomTournamentIntervalSeasons`, `GrandTournamentGoldPrize`, `GrandTournamentRenownReward`, `GrandTournamentInfluenceReward`, `KingdomTournamentRewardPercent`, `GrandTournamentLeaderboardWeight`, `KingdomTournamentLeaderboardWeight`) | `Managers\GrandTournamentScheduler.cs`, `Managers\GrandTournamentRoster.cs`, `Managers\GrandTournamentPrizeManager.cs`, `Data\GrandTournamentGame.cs`, `Models\ArenaOverhaulSettlementAccessModel.cs`, `Patches\TournamentVMPatch.cs`, `Behaviors\AOArenaBehavior.cs` |
| Map popups for Grand / Kingdom Tournament announcements and results | setting:GrandTournamentAnnouncePopup, setting:GrandTournamentResultPopup | `Managers\GrandTournamentNotifier.cs`, `Data\GrandTournamentMapNotification.cs`, `UI\GrandTournamentMapNotificationVM.cs`, `Patches\MapNotificationVMPatch.cs` |
| Team tournaments: own mission behaviors, bracket screen, teams | setting:TeamTournamentEnabled (values: `TeamTournamentTeamsCountMax`, `TeamTournamentTeamSizeMin`, `TeamTournamentTeamSizeMax`, `TeamTournamentScoresForWinningTeam`, `TeamTournamentTeam<N>Color`) | `Patches\TournamentMissionStarterPatch.cs`, `Patches\TournamentMissionViewsPatch.cs`, `Missions\TeamTournamentBehavior.cs` (`OnAfterMissionLoadingFinished`: 1.5.7 only), `Missions\TeamTournamentMissionController.cs`, `Missions\MissionGauntletTeamTournamentView.cs`, `Missions\ArenaPreloadView.cs`, `Managers\TeamTournamentInfo.cs`, `Managers\TeamTournamentHelpers.cs`, `Data\TeamTournamentTeam.cs`, `Data\TeamTournamentMember.cs`, `Data\TeamTournamentMatch.cs`, `Data\TeamTournamentRound.cs`, `UI\TeamTournamentVM.cs`, `UI\TeamTournamentRoundVM.cs`, `UI\TeamTournamentMatchVM.cs`, `UI\TeamTournamentTeamVM.cs`, `UI\TeamTournamentMemberVM.cs`, `_Module\GUI\` |

## Compatibility

| Feature | Gate | Files |
|---|---|---|
| Tournaments XPanded: its prize re-roll and prize selection are hidden for Grand / Kingdom Tournaments; reward lookup | Tournaments XPanded loaded | `Compat\TournamentsXPandedCompat.cs`, `Compat\TournamentsXPandedRewards.cs` |
