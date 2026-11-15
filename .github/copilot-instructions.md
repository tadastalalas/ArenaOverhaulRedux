---
apply: always
---

# ArenaOverhaulRedux: workspace instructions (they override the global file)

## Start here
- Read `FEATURES.md` (repo root) first. It maps every feature to its gate (setting) and files, so open only the files you need.
- Keep `FEATURES.md` up to date in the same change set.

## Identity
- <Prefix>: AOR (Aor in type names). <Root> namespace: ArenaOverhaul. ModuleId: ArenaOverhaulRedux ($(MSBuildProjectName)).
  ModuleName: Arena Overhaul Redux.
  Repo folder and GitHub repo: ArenaOverhaulRedux (https://github.com/tadastalalas/ArenaOverhaulRedux).
- Logger: `Diagnostics\AorLog.cs` (Debug / Warn / Error; `AorLog.IsDebugEnabled` before building strings on hot paths).
  Failed transpiler hooks are reported through `Diagnostics\TranspilerReport.cs` (always written).
- Settings: `Core\ModSettings.cs` only (MCM when loaded, else `ModConfiguration.xml`). `Core\ModInfo.cs` holds the ModuleId,
  file and folder names, the Harmony / UIExtender / hotkey ids and the module root path.
- Patches: `SubModule.PatchAll` runs once in `OnBeforeInitialModuleScreenSetAsRoot` (attribute patches, then
  `TournamentsXPandedCompat.TryApply`). Patches are inactive before the main menu opens. Never call `PatchAll` anywhere else.
- Models are decorators around the model that was registered before them: `SubModule.OnGameStart` passes the current
  `TournamentModel` / `SettlementAccessModel` to `ArenaOverhaulTournamentModel` / `ArenaOverhaulSettlementAccessModel`.
- Optional-mod compat: `Compat\TournamentsXPandedCompat.cs` (TryApply) and `Compat\TournamentsXPandedRewards.cs` (reflection lookup).
- Requires Harmony, ButterLib (hotkeys, `SyncDataAsJson`) and UIExtenderEx (practice fight prefab and VM mixin). MCM is optional.

## Layout deviations
- `Managers\`: static game-logic services used by the behavior, patches and missions (practice modes, prizes, rewards,
  applicants, hosting, injuries, Grand Tournament scheduler / roster / prizes / notifier, team tournament helpers).
- Shared helpers live in `Core\`: `EnumExtensions.cs`, `StringExtensions.cs`, `MessageHelper.cs`, `MathHelper.cs`,
  `ItemHelper.cs`, `FieldAccessHelper.cs`, `ArenaPracticeMode.cs`.
- `Missions\` also holds the team tournament mission view (`MissionGauntletTeamTournamentView.cs`) and `ArenaPreloadView.cs`.
- `_Module\ModuleData\module_strings.xml` holds the vanilla-style game texts the mod adds (loaded through the `Xmls` block of `SubModule.xml`).

## Multi-version (1.5.7, 1.4.8, 1.4.7)
- 1.4.7 / 1.4.8 behavior is the reference. 1.5.7 adaptations live only in `#else` / `#if !LOWER_THAN_1_5` and must behave the same.
- The only `#if` block so far: `Missions\TeamTournamentBehavior.cs` (`OnAfterMissionLoadingFinished`, 1.5.7 only).
- Not checked by the compiler, re-verify on every new game version: the string-named patch targets and the transpilers in
  `Patches\`, the `FieldAccessHelper` members, the prefab XPath in `UI\ArenaPracticeFightPrefabExtension.cs`
  (`ArenaPracticeFight`, `ListPanel[@IsVisible='@IsPlayerPracticing']`), the copied vanilla tournament VM bindings in `UI\TeamTournament*VM.cs`.

## Settings without MCM
- Per-hero practice settings (loadout options, three weapon priorities) exist only with MCM (per-save settings built in
  `Data\MCMSettings.cs`). Without MCM every clan hero uses the "Default Arena Practice settings for heroes" values and no weapon priorities.
- Dropdown enums (`Data\PracticeEquipmentType.cs`, `ChampionPrizeCalculationType.cs`, `CultureRestrictedPrizesMode.cs`,
  `PrizeRerollCondition.cs`, `GrandTournamentSeason.cs`, `WeaponPreference.cs`): the enum value is the dropdown index. Append only.

## Saves
- The mod saves data. `Behaviors\AOArenaBehavior.cs` stores `AOArenaBehaviorManager` under the SyncData key
  `_AOArenaBehaviorManager` (ButterLib `SyncDataAsJson`).
- `Data\ArenaOverhaulSaveableTypeDefiner.cs`: base id 2003477000, class ids 1, 2, 3, 100. Append only.
- Saved types stay public: `GrandTournamentGame`, `GrandTournamentMapNotification`, `PrizeItemInfo`, `AOArenaBehaviorManager`.

## Kept exceptions (released identifiers, never change)
- Root namespace `ArenaOverhaul` (the ModuleId is `ArenaOverhaulRedux` since v1.5.7.0; the namespace was not renamed).
- MCM Id "ArenaOverhaulRedux" and FolderName "Arena Overhaul Redux"; per-save hero settings id "CompanionPracticeSettings_v<major>"
  and folder "AO Hero Practice Settings"; MCM property names and types in `Data\MCMSettings.cs`
  (including `Dropdown<DropdownEnumItem<T>>` and `Dropdown<int> TeamTournamentTeamsCountMax`).
- Harmony id `Bannerlord.ArenaOverhaul`, UIExtender id `ArenaOverhaulUI`, hotkey group `ArenaOverhaul`, class name `TeamPracticeHotKeyController`.
- Behavior class name `AOArenaBehavior` (data-store id) and the SyncData key above (it predates the `aor_<field>` rule).
- Existing text ids (random 9 characters, `AOgt…`, `AOth…`, `AOti…`, `AOtl…`). New ids use `AOR_` + 7 random characters.
- Dialog and menu ids keep their released values (no `aor_` prefix).
- Kept exception (my decision, the code-style pass was skipped): the gameplay code in `Patches\`, `Managers\`, `Missions\`,
  `Behaviors\`, `UI\`, `Models\` and the `Core\` helpers keeps its older style (patch class shape and names, no feature headers,
  numbers inside the code, LINQ, public where internal would do). Do not restyle it unasked; new or changed lines follow the global rules.
