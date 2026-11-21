## AOR v1.5.7.1

- Merged a new Russian translation into `_Module\ModuleData\Languages\RU\std_module_strings_xml_rus.xml` (329 strings changed, ids not in the English file skipped: `miUO4w1hn`, `5KIeqRJJX`, `LRhyA9mqB`, `awGzg6S1d`, `FOtSSoEAh`, `37S9xfodK`).
  - Fixed in the supplied text: lost `[ib:]`/`[if:]` tags (9 dialog lines), missing `{VALOR_PRIZES_DESCRIPTION}{CHAMPION_PRIZE_DESCRIPTION}` in `gaJDVkvHa`, invalid `{.%}` in `AOgtMenuG1`/`AOgtMenuK1`, missing spaces in `8k8br8lfq`/`LTfWl2OPC`.
  - Restored female forms (`{?COMPANION.GENDER}`) in the companion result lines; setting hints now use the dropdown names ("Суммарный", "Множительный", reroll options).
  - 33 ids added after the paste (`AOgt*Pop*`, `AOgtNtf*`, `AOR_*`, …) keep their previous Russian text. Save-compat: none. Test: Russian UI, arena dialogs with a female companion.
- Fixed a possible native crash (`System.AccessViolationException`) during team tournament fights on 1.5.7 (reported while spectating, round 1, fight 5; no crash report).
  - The fight controller now matches the 1.5.7 vanilla `TournamentFightMissionController` where our copy differed. All in `Missions\TeamTournamentMissionController.cs`, inside `#if !LOWER_THAN_1_5` / `#else`; the `LOWER_THAN_1_5` branch keeps the old code (1.4.7 / 1.4.8 unchanged).
  - Routing: `OnBehaviorInitialize` / `OnEndMission` add / remove `CanAgentRout` (returns `false`) on `Mission.CanAgentRout_AdditionalCondition`, as vanilla (1.4.8 vanilla has it too; not added there, 1.4.x had no problems).
  - Own agent lists `_currentTournamentAgents` / `_currentTournamentMountAgents`: filled in `SpawnAgentWithRandomItems`, fighter removed in `OnAgentRemoved`, cleared in `OnMatchEnded`.
  - `OnMatchEnded` fades out only those lists (was every active agent of `Mission.Agents` with `TeamIndex >= 0`).
  - `IsMatchEnded`: victory cheer once per match (`_cheerStarted`, reset in `StartMatch`), only on our own AI fighters through `StartVictoryCheer` (was every AI agent of `Mission.Agents`, horses included, and could repeat).
  - `CheckIfIsThereAnyEnemies`, `Simulate` (alive-list reset) and `IsThereAnyPlayerAgent` use the own list, as vanilla.
  - Not mirrored: vanilla 1.5.7 spawns the player first in a team and no longer sets `ClothingColor2`.
  - Ruled out: UIExtenderEx prefab compile (finishes when the mission opens), `TeamTournamentBehavior` (matches 1.5.7 `TournamentBehavior`), `TournamentMissionStarterPatch` behavior list (matches 1.5.7).
  - Save-compat: none. Test on 1.5.7: team tournament as spectator and as participant (all rounds, free camera), skip / forfeit a match, victory cheer once per match; 1.4.8 team tournament still works.
- Bug-hunt fixes (all versions, no `#if`). Save-compat: none.
  - `Behaviors\AOArenaBehavior.cs` `team_game_select_roster_condition`: the access model's disabled reason was overwritten by `RosterOnlyForTeamTournaments`; the default text is now set only when not disabled and no reason was given.
  - `Managers\AOArenaBehaviorManager.cs` `FillParticipantsForExpansivePractice`: troops need `WoundedNumber < Number` (as Team practice) and the troop branch skips heroes (wounded heroes fell through it). `Sync()`: `PlayerHostedTournaments ??= []`.
  - `Patches\ArenaPracticeFightMissionControllerPatch.cs` `GetParticipantCharactersTranspiler`: `codes[i + 1]` is now bounds-checked (could throw on the last instruction and stop `PatchAll`).
  - Silent transpilers now report through `TranspilerReport.HooksNotFound` and skip when their members are null: `TournamentBehaviorPatch` `GetMaximumBet` / `CalculateBet` (loop starts at 1, reads `codes[i - 1]`), `DefaultTournamentModelPatch` `GetRenownReward` (loop ends at `Count - 1`), `TournamentFightMissionControllerPatch` `Simulate` (2 edits).
  - Removed unused `TeamTournamentBehavior.MaximumBetInstance`.
- `_Module\GUI\Brushes\ArenaOverhaulMapNotification.xml` rebuilt from the vanilla 1.5.7 SandBox `MapNotification.xml` (it replaces the vanilla `Map.Notification.*` brushes).
  - Added the missing 1.5.7 `blood_feud_*` layers and styles (6 each); blood feud notifications had no icon with the mod loaded. The new layers are hidden unless a notification selects them.
  - Vanilla layout (2-space indent, vanilla order); other mods' entries grouped under one comment, ours last. Same entries otherwise; kept the 1.5 s `Map.Notification.More.Panel` pulse (vanilla 1.5.7: 0.75 s).
  - Note comment placed inside `<Layers>` (comments there already worked), not before `<Brushes>`. Save-compat: none. Test on 1.5.7: blood feud and Grand Tournament notification icons; 1.4.8 notifications still show.

## AOR v1.5.7.0

- Migrated the mod to the multi-version standard (one code base, one DLL per game version, `Bannerlord.ModuleLoader`).
  - `ArenaOverhaul.csproj`: `Bannerlord.BuildResourcesLite.Sdk` 1.2.3.41, `netstandard2.0`, AssemblyName `$(ModuleId).v$(GameVersion)`, `BuildForWindowsStore`, `v<ver>` define.
    ModuleId `ArenaOverhaul` (kept; renamed later in this version, see below), ModuleName "Arena Overhaul Redux", ModuleUrl Nexus mods/11459.
  - Removed `build\common.props`, `src\Directory.Build.*`, the repo's own `supported-game-versions.txt` (the shared one above the repo is used), game references, TargetFrameworks.
  - Packages: `Bannerlord.ModuleLoader.Injector` 1.0.1.50 (new), `Lib.Harmony` 2.4.3 -> 2.4.2 (2.4.3+ forbidden), `Harmony.Extensions` 3.2.0.77,
    `Bannerlord.ButterLib` 2.11.1, `Bannerlord.MCM` 5.13.1, `Bannerlord.UIExtenderEx` 3.0.3, `Newtonsoft.Json` 13.0.5-beta1 (all compile-only where another module ships the DLL).
  - Version 1.6.5 -> 1.5.7.0 (VERSION rule: newest game version + release count).
  - `_Module\SubModule.xml`: tokens, single "Bannerlord Module Loader" submodule; Harmony, ButterLib, UIExtenderEx required; MCM moved to optional `LoadBeforeThis` metadata; the `Xmls` block is kept.
    The required versions now come from the package versions (`$harmonyversion$`, `$butterlibversion$`, `$uiextenderexversion$`, `$mcmversion$`).
  - `ArenaOverhaul.sln` rewritten for the root csproj (Debug / Release, x64). `Properties\launchSettings.json`: `$(ModuleName)` -> `$(ModuleId)` (ModuleName now has spaces).
- Restructured the repo: root folder `Bannerlord.ArenaOverhaul` -> `ArenaOverhaulRedux`, `src\ArenaOverhaul\…` -> one level of folders next to the csproj
  (`Behaviors`, `Compat`, `Core`, `Data`, `Diagnostics`, `Managers`, `Missions`, `Models`, `Patches`, `UI`, `_Module`, `Properties`). Done with `git mv`; namespaces are file-scoped `ArenaOverhaul.<Folder>`.
  - Deleted: `.github\workflows\*`, `.github\dependabot.yml`, root `changelog.txt`, `Data\CompanionPracticeSettings.cs`, `Diagnostics\DebugHelper.cs`, `Diagnostics\HarmonyHelper.cs`, `Diagnostics\PatchExtension.cs`,
    `_Module\ModuleData\Languages\language_data.xml` (English override).
  - `_Module\AOR_Changelog.txt` -> `_Module\Changelog.txt`. `NexusDescription.txt` moved to the repo root and rewritten from the template.
  - `dotnet format`: no `var`, file-scoped namespaces. No other style change (see "Not done").
- Settings (SETTINGS rule):
  - `Data\ModConfiguration.cs`: 123 settings + `LoggingEnabled`, 8 sections in MCM group order, defaults / min / max as consts, `Clamp` on load.
  - `Data\MCMSettings.cs`: the only file with MCM types. Id "ArenaOverhaulRedux" and FolderName "Arena Overhaul Redux" kept, property names and types kept, presets kept.
    Per-save hero practice settings kept ("CompanionPracticeSettings_v<major>", folder "AO Hero Practice Settings"); the weapon priorities are read through
    `Func<WeaponPreference>` sources in `Data\HeroPracticeSettings.cs`, so no MCM type leaves the file.
  - `Core\ModSettings.cs`: MCM values read live, xml values cached; `Reload()` in `SubModule.OnBeforeInitialModuleScreenSetAsRoot` and `OnGameStart`.
  - `_Module\ModConfiguration.xml` ships with the defaults. Without MCM every clan hero uses the "Default Arena Practice settings for heroes" values, no weapon priorities (my decision).
  - "Log technical Transpiler info" removed; "Logging for debugging" (`LoggingEnabled`, default false) added as the last entry of the new last group "Technical settings" (GroupOrder 101).
- Texts (TEXT / UI rule):
  - All 198 `"{=id}…"` literals moved to `Core\Texts.cs` (`Texts.Ids.X`, 4 static `TextObject`s). English table `std_module_strings_xml.xml` regenerated: 464 rows.
  - Where code and xml disagreed the code text won (xml typos "alowed", "wepon", "nubmber", "campion", a Cyrillic "Е", outdated defaults); `AOgtMenuG1` / `AOgtMenuK1` use the code text.
  - `{GOLD ICON}` -> `{GOLD_ICON}` in `_Module\ModuleData\module_strings.xml` and the English table.
  - New ids: `AOR_m3XpR8d` (HeroQueuedHint), `AOR_J5sWy1F` (AwaitingHero), `AOR_q7Hn2Lw` (LoggingEnabled), `AOR_Vz4kT9c` (LoggingEnabledHint).
    Dropped unused rows: `miUO4w1hn`, `5KIeqRJJX`, `LRhyA9mqB`, `awGzg6S1d`, `FOtSSoEAh`, `37S9xfodK`.
  - 12 languages added (BR, CNs, CNt, DE, FR, IT, JP, KO, PL, RU, SP, TR), 464 rows each, AI translated; checked for row count, ids, tokens and encoding.
    "Technical settings" / "Logging for debugging" / its hint reuse the BanditMilitiasRedux translations.
- Logger (LOGGING rule): `Diagnostics\AorLog.cs` writes to `Modules\ArenaOverhaulRedux\logs`. `Diagnostics\TranspilerReport.cs` replaces the on-screen / debug transpiler messages
  (`HooksNotFound`, `HookNotApplied`), always written. The "Loaded Arena Overhaul!" on-screen message is gone.
- `SubModule.cs` rewritten: `PatchAll` once in `OnBeforeInitialModuleScreenSetAsRoot` (as before: before the main menu), then `TournamentsXPandedCompat.TryApply`;
  behavior and decorator models registered in `OnGameStart`; `OnGameEnd` unregisters the per-save hero settings. `Core\ModInfo.cs` holds the ids and paths.
- `Compat\TournamentsXPandedCompat.cs` (`TryApply`, `AccessTools.TypeByName`, null checks, log) and `Compat\TournamentsXPandedRewards.cs` (reflection lookup) replace the old optional-mod patch code.
- Internal renames (no saved name changed): `EnteredPracticeFightFromMenu`, `_heroPracticeSettings`, `LastPlayerRelatedCharacters`;
  `AOArenaBehaviorManager.EnsureHeroPracticeSettings` / `RegisterHeroPracticeSettings` / `UnregisterHeroPracticeSettings` / `RefreshHeroPracticeSettings`; `Core\ArenaPracticeMode.cs` split out.
- 1.5.7:
  - All three versions compile from the same files.
  - `Missions\TeamTournamentBehavior.cs`: `#if !LOWER_THAN_1_5` override `OnAfterMissionLoadingFinished` -> `Mission.OnInitialSpawnCompleted()`.
    Vanilla 1.5.7 `TournamentBehavior` (which this class replaces in team tournaments) gained that override; without it `Mission.OnDeploymentFinished` would never run in a team tournament on 1.5.7.
  - Checked against the installed 1.5.7 game with a test program (Harmony 2.4.2 from `Bannerlord.Harmony`): all 15 patch classes apply, 47 vanilla methods patched, no exception,
    no null reflection handle except the optional Tournaments XPanded ones, every transpiler changed its method.
  - Compared 1.4.8 with 1.5.7 (decompiled): identical `ArenaMasterCampaignBehavior`, `FightTournamentGame`, `TournamentGame`, `TournamentCampaignBehavior`, `TournamentFightMissionController`,
    `TournamentManager`, `TournamentVM` and its sub-VMs, `DefaultTournamentModel`, `TournamentModel`, `MapNotificationItemBaseVM`, `TournamentMissionStarter`, `TournamentMissionViews`,
    `MissionArenaPracticeFightVM`, `SettlementAccessModel`, `InformationData`. Prefab `ArenaPracticeFight` still has `ListPanel[@IsVisible='@IsPlayerPracticing']`.
  - Not compared (test in game): the changed vanilla files `SandBoxMissionViews`, `SandBoxMissions`, `SandBoxViewCreator`, `Mission`, `Clan`; the mod's copies of vanilla code in
    `Missions\TeamTournamentMissionController.cs` and `UI\TeamTournament*VM.cs` line by line.
- Not done (my decision, to keep the cost down): the code-style pass. The gameplay code in `Patches\`, `Managers\`, `Missions\`, `Behaviors\`, `UI\`, `Models\` and the `Core\` helpers
  keeps its older style (patch class shape and names, no feature headers, numbers inside the code, LINQ, public where internal would do). Listed as a kept exception in `.github\copilot-instructions.md`.
- New files: `FEATURES.md`, `DevChangelog.md`, `.github\copilot-instructions.md`.
- Save compatibility: none. SyncData key `_AOArenaBehaviorManager`, definer base id 2003477000 (class ids 1, 2, 3, 100), saved type names and the behavior class name are unchanged.
  MCM values are kept (same Id, folder and property names); only the removed "Log technical Transpiler info" value is ignored.
- To test (1.5.7, 1.4.8, 1.4.7; with MCM and without MCM):
  - Old save loads; Grand / Kingdom Tournament state, map popups and hosting cooldowns are still there.
  - With MCM: old setting values are still there; the per-save "Hero Practice Settings" menu appears in a campaign, weapon priorities work, presets work.
  - Without MCM: `Modules\ArenaOverhaulRedux\ModConfiguration.xml` values are used; deleting or breaking the file recreates it.
  - Every practice mode from the arena master and from the arena menu: loadout choice, playing as a companion, Team Practice hero switching (hotkey), rewards panel, champion prize.
  - A normal tournament: prize, reroll, betting, round rewards, results screen, injuries. A team tournament on 1.5.7 (new `#if` block): starts, rounds advance, bracket screen.
  - Hosting a tournament; a Grand and a Kingdom Tournament (announcement, join lock, results popup).
  - Texts in another language; "Logging for debugging" on -> a file in `Modules\ArenaOverhaulRedux\logs`.
  - With Tournaments XPanded: no error in the log, prize options hidden for Grand / Kingdom Tournaments.
- Renamed the project and the module: `ArenaOverhaul.csproj` / `.sln` -> `ArenaOverhaulRedux.csproj` / `.sln`; ModuleId `ArenaOverhaul` -> `ArenaOverhaulRedux` (`$(MSBuildProjectName)`).
  - Deployed folder `Modules\ArenaOverhaulRedux`; DLLs `ArenaOverhaulRedux.v<ver>.dll`, loader `Bannerlord.ModuleLoader.ArenaOverhaulRedux.dll`.
  - `Core\ModInfo.ModuleId` = `ArenaOverhaulRedux`; `UI\ArenaPracticeFightPrefabExtension` loads the prefab through `ModInfo.ModuleId` (was the literal `"ArenaOverhaul"`).
  - Unchanged: namespaces `ArenaOverhaul.*`, Harmony id `Bannerlord.ArenaOverhaul`, hotkey group `ArenaOverhaul`, UIExtender id, MCM Id, SyncData key, save definer ids.
  - Save-compat: old saves list the module `ArenaOverhaul`; the game warns that it is missing on load. Accepted (owner's decision). Test: load an old save.
