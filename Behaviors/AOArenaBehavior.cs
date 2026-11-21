using ArenaOverhaul.Core;
using ArenaOverhaul.Data;
using ArenaOverhaul.Managers;

using Bannerlord.ButterLib.Common.Helpers;
using Bannerlord.ButterLib.SaveSystem.Extensions;

using Helpers;

using SandBox.CampaignBehaviors;
using SandBox.Missions.MissionLogics.Arena;
using SandBox.View.Menu;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Menu.Overlay;

using static TaleWorlds.Core.ItemObject;

using TWHelpers = Helpers;

namespace ArenaOverhaul.Behaviors;

public class AOArenaBehavior : CampaignBehaviorBase
{
    private ArenaMasterCampaignBehavior? _arenaMasterBehavior;

    private int _tournamentListOffset;
    private int _tournamentListTotalCount;
    private const int _tournamentListEntriesPerPage = 3;

    private CampaignGameStarter? _campaignGame;
    private readonly List<CultureObject> _visitedCultures = [];

    private AOArenaBehaviorManager _AOArenaBehaviorManager = new();

    public AOArenaBehaviorManager BehaviorManager => _AOArenaBehaviorManager!;

    public AOArenaBehavior()
    {
        AOArenaBehaviorManager.Instance = _AOArenaBehaviorManager;
    }

    public override void RegisterEvents()
    {
        CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener(this, OnAfterSessionLaunched);
        CampaignEvents.AfterMissionStarted.AddNonSerializedListener(this, AfterMissionStarted);
        CampaignEvents.TournamentFinished.AddNonSerializedListener(this, OnTournamentFinished);
        CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);

        //UpdateCompanionSettings
        CampaignEvents.HeroComesOfAgeEvent.AddNonSerializedListener(this, OnHeroComesOfAge);
        CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilledEvent);
        CampaignEvents.CompanionRemoved.AddNonSerializedListener(this, OnCompanionRemoved);
        CampaignEvents.NewCompanionAdded.AddNonSerializedListener(this, OnNewCompanionAdded);
        
        CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, GrandTournamentScheduler.OnDailyTick);
    }

    private void OnAfterSessionLaunched(CampaignGameStarter campaignGameStarter)
    {
        AddGameMenus(campaignGameStarter);
    }

    public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
    {
        _arenaMasterBehavior = Campaign.Current.CampaignBehaviorManager.GetBehavior<ArenaMasterCampaignBehavior>();
        _campaignGame = campaignGameStarter;
        _AOArenaBehaviorManager.SetStandardPracticeMode();

        AddDialogs(campaignGameStarter);
        TournamentRewardManager.Initialize();

        if (Settlement.CurrentSettlement != null)
        {
            AddLoadoutDialogues(_campaignGame, Settlement.CurrentSettlement);
        }

        TeamTournamentInfo.Current?.Finish();
    }

    private void AddGameMenus(CampaignGameStarter campaignGameStarter)
    {
        campaignGameStarter.AddGameMenuOption("town_arena", "town_arena_host_tournament", Texts.Ids.MenuHostTournament, new GameMenuOption.OnConditionDelegate(game_menu_host_tournament_on_condition), new GameMenuOption.OnConsequenceDelegate(game_menu_host_tournament_on_consequence), false, 1, false);
        campaignGameStarter.AddGameMenuOption("town_arena", "town_arena_enter_expansive_practice_fight", Texts.Ids.MenuEnterExpansivePracticeFight, new GameMenuOption.OnConditionDelegate(game_menu_enter_expansive_practice_fight_on_condition), new GameMenuOption.OnConsequenceDelegate(game_menu_enter_expansive_practice_fight_on_consequence), false, 1, false);
        campaignGameStarter.AddGameMenuOption("town_arena", "town_arena_enter_parry_practice_fight", Texts.Ids.MenuEnterParryPracticeFight, new GameMenuOption.OnConditionDelegate(game_menu_enter_parry_practice_fight_on_condition), new GameMenuOption.OnConsequenceDelegate(game_menu_enter_parry_practice_fight_on_consequence), false, 2, false);
        campaignGameStarter.AddGameMenuOption("town_arena", "town_arena_enter_team_practice_fight", Texts.Ids.MenuEnterTeamPracticeFight, new GameMenuOption.OnConditionDelegate(game_menu_enter_team_practice_fight_on_condition), new GameMenuOption.OnConsequenceDelegate(game_menu_enter_team_practice_fight_on_consequence), false, 3, false);
        campaignGameStarter.AddGameMenuOption("town_arena", "town_arena_nearby_tournaments", Texts.Ids.MenuNearbyTournaments, args => { _tournamentListOffset = 0; args.optionLeaveType = GameMenuOption.LeaveType.Submenu; return true; }, x => GameMenu.SwitchToMenu("nearby_tournaments_list"), false, 4, false);
        campaignGameStarter.AddGameMenu("nearby_tournaments_list", Texts.Ids.RawMenuText, new OnInitDelegate(game_menu_nearby_tournaments_list_on_init), GameMenu.MenuOverlayType.SettlementWithBoth, GameMenu.MenuFlags.None, null);
        campaignGameStarter.AddGameMenuOption("nearby_tournaments_list", "nearby_tournaments_list_nextpage", Texts.Ids.MenuNextPage, args => { args.optionLeaveType = GameMenuOption.LeaveType.Continue; return _tournamentListOffset * _tournamentListEntriesPerPage + _tournamentListEntriesPerPage < _tournamentListTotalCount; }, x => { ++_tournamentListOffset; GameMenu.SwitchToMenu("nearby_tournaments_list"); }, false, 30, false);
        campaignGameStarter.AddGameMenuOption("nearby_tournaments_list", "nearby_tournaments_list_previouspage", Texts.Ids.MenuPreviousPage, args => { args.optionLeaveType = GameMenuOption.LeaveType.LeaveTroopsAndFlee; return _tournamentListOffset > 0; }, x => { --_tournamentListOffset; GameMenu.SwitchToMenu("nearby_tournaments_list"); }, false, 20, false);
        campaignGameStarter.AddGameMenuOption("nearby_tournaments_list", "nearby_tournaments_list_leave", Texts.Ids.MenuBackToArena, args => { args.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; }, x => GameMenu.SwitchToMenu("town_arena"), true, 10, false);
        campaignGameStarter.AddGameMenuOption("menu_town_tournament_join", "participate_as_team", Texts.Ids.MenuParticipateAsTeam, new GameMenuOption.OnConditionDelegate(team_game_select_roster_condition), new GameMenuOption.OnConsequenceDelegate(team_game_select_roster_consequence), false, 1, false);
    }

    protected void AddDialogs(CampaignGameStarter campaignGameStarter)
    {
        //Expansive practice introduction and rewards
        campaignGameStarter.AddDialogLine("arena_training_expansive_practice_fight_intro_1a_line", "arena_expansive_practice_fight_rules", "arena_intro_4", Texts.Ids.DialogExpansivePracticeFightIntro1a, null, null, 100, null);
        campaignGameStarter.AddPlayerLine("arena_training_expansive_practice_fight_intro_3_line", "arena_prizes_amounts", "arena_expansive_practice_fight_reward", Texts.Ids.DialogExpansivePracticeFightIntro3, null, null, 95, null, null);
        campaignGameStarter.AddDialogLine("arena_training_expansive_practice_fight_intro_reward_line", "arena_expansive_practice_fight_reward", "arena_joining_ask", Texts.Ids.RawArenaReward, new ConversationSentence.OnConditionDelegate(conversation_arena_expansive_practice_fight_explain_reward_on_condition), null, 100, null);

        //Companion choice
        campaignGameStarter.AddPlayerLine("arena_master_practice_choose_companion_line", "arena_master_enter_practice_fight_confirm", "arena_master_practice_choose_companion_request", Texts.Ids.DialogPracticeChooseCompanion, new ConversationSentence.OnConditionDelegate(conversation_arena_companion_choice_allowed_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_arena_companion_choice_request_on_consequence), 200, null, null);
        campaignGameStarter.AddDialogLine("arena_master_practice_choose_companion_master_confirm_line", "arena_master_practice_choose_companion_request", "arena_master_practice_companions_list", Texts.Ids.DialogPracticeChooseCompanionMasterConfirm, null, null, 100, null);
        campaignGameStarter.AddRepeatablePlayerLine("arena_master_practice_choose_companion_choice_line", "arena_master_practice_companions_list", "arena_master_enter_custom_practice_fight", Texts.Ids.RawHeroName, Texts.Ids.DialogDifferentPerson, "arena_master_practice_choose_companion_request", new ConversationSentence.OnConditionDelegate(conversation_arena_master_practice_choose_companion_choice_repeatable_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_arena_master_practice_choose_companion_choice_repeatable_on_consequence), 100, null);
        campaignGameStarter.AddPlayerLine("arena_master_practice_choose_companion_return_line", "arena_master_practice_companions_list", "arena_master_enter_practice_fight", Texts.Ids.DialogActuallyNevermind, () => !_AOArenaBehaviorManager.IsAfterPracticeTalk, new ConversationSentence.OnConsequenceDelegate(_AOArenaBehaviorManager.ResetCompanionChoice), 10, null, null);
        campaignGameStarter.AddPlayerLine("arena_master_practice_choose_companion_return_line2", "arena_master_practice_companions_list", "arena_master_custom_post_practice_fight_talk", Texts.Ids.DialogActuallyNevermind, () => _AOArenaBehaviorManager.IsAfterPracticeTalk, new ConversationSentence.OnConsequenceDelegate(_AOArenaBehaviorManager.ResetCompanionChoice), 10, null, null);

        //Loadout choice
        campaignGameStarter.AddPlayerLine("arena_master_practice_choose_weapon_line", "arena_master_enter_practice_fight_confirm", "arena_master_practice_choose_weapon_request", Texts.Ids.DialogPracticeChooseWeapon, new ConversationSentence.OnConditionDelegate(conversation_arena_weapon_choice_allowed_on_condition), null, 200, null, null);
        campaignGameStarter.AddDialogLine("arena_master_practice_choose_weapon_master_confirm_line", "arena_master_practice_choose_weapon_request", "arena_master_practice_weapons_list", Texts.Ids.DialogPracticeChooseWeaponMasterConfirm, new ConversationSentence.OnConditionDelegate(conversation_town_arena_weapon_choice_request_confirm_on_condition), null, 100, null);
        campaignGameStarter.AddPlayerLine("arena_master_practice_choose_better_weapon_line", "arena_master_practice_weapons_list", "arena_master_practice_choose_weapon_request", Texts.Ids.DialogPracticeChooseBetterWeapon, new ConversationSentence.OnConditionDelegate(conversation_town_arena_weapons_list_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_town_arena_weapons_list_get_better_on_consequence), 20, null, null);
        campaignGameStarter.AddPlayerLine("arena_master_practice_choose_weapon_return_line1", "arena_master_practice_weapons_list", "arena_master_enter_practice_fight", Texts.Ids.DialogActuallyNevermind, () => !_AOArenaBehaviorManager.IsAfterPracticeTalk && _AOArenaBehaviorManager.ChosenCharacter is null, new ConversationSentence.OnConsequenceDelegate(_AOArenaBehaviorManager.ResetWeaponChoice), 10, null, null);
        campaignGameStarter.AddPlayerLine("arena_master_practice_choose_weapon_return_line2", "arena_master_practice_weapons_list", "arena_master_enter_custom_practice_fight", Texts.Ids.DialogActuallyNevermind, () => !_AOArenaBehaviorManager.IsAfterPracticeTalk && _AOArenaBehaviorManager.ChosenCharacter != null, new ConversationSentence.OnConsequenceDelegate(_AOArenaBehaviorManager.ResetWeaponChoice), 10, null, null);
        campaignGameStarter.AddPlayerLine("arena_master_practice_choose_weapon_return_line3", "arena_master_practice_weapons_list", "arena_master_custom_post_practice_fight_talk", Texts.Ids.DialogActuallyNevermind, () => _AOArenaBehaviorManager.IsAfterPracticeTalk, new ConversationSentence.OnConsequenceDelegate(_AOArenaBehaviorManager.ResetWeaponChoice), 10, null, null);

        //Custom pre-fight
        campaignGameStarter.AddDialogLine("arena_master_practice_custom_master_confirm_line", "arena_master_enter_custom_practice_fight", "arena_master_enter_custom_practice_fight_confirm", Texts.Ids.DialogPracticeCustomMasterConfirm, null, null, 100, null);
        campaignGameStarter.AddPlayerLine("arena_master_practice_choose_weapon_line2", "arena_master_enter_custom_practice_fight_confirm", "arena_master_practice_choose_weapon_request", Texts.Ids.DialogPracticeChooseWeapon2, new ConversationSentence.OnConditionDelegate(conversation_arena_weapon_choice_allowed_on_condition), null, 200, null, null);
        campaignGameStarter.AddPlayerLine("arena_master_enter_custom_practice_fight_confirm_line", "arena_master_enter_custom_practice_fight_confirm", "close_window", Texts.Ids.DialogEnterCustomPracticeFightConfirm, null, new ConversationSentence.OnConsequenceDelegate(conversation_arena_join_fight_with_default_weapons_on_consequence), 95, new ConversationSentence.OnClickableConditionDelegate(conversation_arena_join_fight_with_default_weapons_on_condition), null);
        campaignGameStarter.AddPlayerLine("arena_master_practice_reset_companion_choice_line", "arena_master_enter_custom_practice_fight_confirm", "arena_master_enter_practice_fight", Texts.Ids.DialogPracticeResetCompanionChoice, null, new ConversationSentence.OnConsequenceDelegate(_AOArenaBehaviorManager.ResetCompanionChoice), 20, null, null);
        campaignGameStarter.AddPlayerLine("arena_master_enter_custom_practice_fight_decline_line", "arena_master_enter_custom_practice_fight_confirm", "arena_master_pre_talk", Texts.Ids.DialogEnterCustomPracticeFightDecline, null, null, 10, null, null);

        //Expansive practice request
        campaignGameStarter.AddPlayerLine("arena_master_ask_for_expansive_practice_fight_fight_line", "arena_master_talk", "arena_master_enter_expansive_practice_fight", Texts.Ids.DialogAskForExpansivePracticeFight, null, new ConversationSentence.OnConsequenceDelegate(_AOArenaBehaviorManager.SetExpansivePracticeMode), 100, new ConversationSentence.OnClickableConditionDelegate(conversation_town_arena_fight_join_check_on_condition), null);
        campaignGameStarter.AddDialogLine("arena_master_enter_expansive_practice_fight_master_confirm_line", "arena_master_enter_expansive_practice_fight", "arena_master_enter_practice_fight_confirm", Texts.Ids.DialogEnterExpansivePracticeFightMasterConfirm, new ConversationSentence.OnConditionDelegate(conversation_arena_can_join_practice_fight_on_condition), null, 100, null);
        campaignGameStarter.AddDialogLine("arena_master_enter_expansive_practice_fight_master_decline_line", "arena_master_enter_expansive_practice_fight", "close_window", Texts.Ids.DialogTournamentGoingOn, null, null, 100, null);

        //Special practice request
        campaignGameStarter.AddPlayerLine("arena_master_ask_for_speacial_practice_fight_line", "arena_master_talk", "arena_master_request_speacial_practice_fight", Texts.Ids.DialogAskForSpecialPracticeFight, new ConversationSentence.OnConditionDelegate(conversation_arena_ask_for_speacial_practice_fight_on_condition), null, 100, new ConversationSentence.OnClickableConditionDelegate(conversation_town_arena_fight_join_check_on_condition), null);
        campaignGameStarter.AddDialogLine("arena_master_request_speacial_practice_fight_master_confirm_line", "arena_master_request_speacial_practice_fight", "arena_master_speacial_practice_fight_list", Texts.Ids.DialogRequestSpecialPracticeFightMasterConfirm, new ConversationSentence.OnConditionDelegate(conversation_arena_can_join_practice_fight_on_condition), null, 100, null);
        campaignGameStarter.AddDialogLine("arena_master_request_speacial_practice_fight_master_decline_line", "arena_master_request_speacial_practice_fight", "close_window", Texts.Ids.DialogTournamentGoingOn, null, null, 100, null);
        campaignGameStarter.AddPlayerLine("arena_master_request_speacial_practice_fight_return_line", "arena_master_speacial_practice_fight_list", "arena_master_pre_talk", Texts.Ids.DialogActuallyNevermind, null, new ConversationSentence.OnConsequenceDelegate(_AOArenaBehaviorManager.ResetWeaponChoice), 10, null, null);

        //Parry practice request
        campaignGameStarter.AddPlayerLine("arena_master_ask_for_parry_practice_fight_fight_line", "arena_master_speacial_practice_fight_list", "arena_master_enter_parry_practice_fight", Texts.Ids.DialogAskForParryPracticeFight, new ConversationSentence.OnConditionDelegate(conversation_arena_parry_practice_enabled_on_condition), new ConversationSentence.OnConsequenceDelegate(_AOArenaBehaviorManager.SetParryPracticeMode), 100, new ConversationSentence.OnClickableConditionDelegate(conversation_town_arena_parry_fight_join_check_on_condition), null);
        campaignGameStarter.AddDialogLine("arena_master_enter_parry_practice_fight_master_confirm_line", "arena_master_enter_parry_practice_fight", "arena_master_enter_practice_fight_confirm", Texts.Ids.DialogEnterParryPracticeFightMasterConfirm, new ConversationSentence.OnConditionDelegate(conversation_arena_join_special_practice_fight_confirm_on_condition), null, 100, null);
        campaignGameStarter.AddDialogLine("arena_master_enter_parry_practice_fight_master_decline_line", "arena_master_enter_parry_practice_fight", "close_window", Texts.Ids.DialogTournamentGoingOn, null, null, 100, null);

        //Team practice request
        campaignGameStarter.AddPlayerLine("arena_master_ask_for_team_practice_fight_fight_line", "arena_master_speacial_practice_fight_list", "arena_master_enter_team_practice_fight", Texts.Ids.DialogAskForTeamPracticeFight, new ConversationSentence.OnConditionDelegate(conversation_arena_team_practice_enabled_on_condition), new ConversationSentence.OnConsequenceDelegate(_AOArenaBehaviorManager.SetTeamPracticeMode), 100, new ConversationSentence.OnClickableConditionDelegate(conversation_town_arena_fight_join_check_on_condition), null);
        campaignGameStarter.AddDialogLine("arena_master_enter_team_practice_fight_master_confirm_line", "arena_master_enter_team_practice_fight", "arena_master_enter_practice_fight_confirm", Texts.Ids.DialogEnterTeamPracticeFightMasterConfirm, new ConversationSentence.OnConditionDelegate(conversation_arena_join_special_practice_fight_confirm_on_condition), null, 100, null);
        campaignGameStarter.AddDialogLine("arena_master_enter_team_practice_fight_master_decline_line", "arena_master_enter_team_practice_fight", "close_window", Texts.Ids.DialogTournamentGoingOn, null, null, 100, null);

        //Loadout choice post match
        campaignGameStarter.AddPlayerLine("arena_master_practice_choose_companion_line2", "arena_master_post_practice_fight_talk", "arena_master_practice_choose_companion_request", Texts.Ids.DialogPracticeChooseCompanion2, () => conversation_arena_companion_choice_post_fight_allowed_on_condition(false), new ConversationSentence.OnConsequenceDelegate(conversation_arena_companion_choice_request_on_consequence), 95, null, null);
        campaignGameStarter.AddPlayerLine("arena_master_practice_choose_companion_line3", "arena_master_post_practice_fight_talk", "arena_master_practice_choose_companion_request", Texts.Ids.DialogPracticeChooseCompanion3, () => conversation_arena_companion_choice_post_fight_allowed_on_condition(true), new ConversationSentence.OnConsequenceDelegate(conversation_arena_companion_choice_request_on_consequence), 95, null, null);
        campaignGameStarter.AddPlayerLine("arena_master_practice_reset_companion_choice_line2", "arena_master_post_practice_fight_talk", "arena_master_custom_post_practice_fight_talk", Texts.Ids.DialogPracticeResetCompanionChoice2, () => _AOArenaBehaviorManager.ChosenCharacter != null, new ConversationSentence.OnConsequenceDelegate(_AOArenaBehaviorManager.ResetCompanionChoice), 90, null, null);
        campaignGameStarter.AddPlayerLine("arena_master_post_practice_fight_take_default_loadout_line", "arena_master_post_practice_fight_talk", "close_window", Texts.Ids.RawDefaultWeaponsChoiceLine, new ConversationSentence.OnConditionDelegate(conversation_arena_return_to_default_choice_allowed_on_condition), new ConversationSentence.OnConsequenceDelegate(conversation_arena_join_fight_with_default_weapons_on_consequence), 85, new ConversationSentence.OnClickableConditionDelegate(conversation_arena_join_fight_with_default_weapons_on_condition), null);
        campaignGameStarter.AddPlayerLine("arena_master_post_practice_fight_take_new_loadout_line", "arena_master_post_practice_fight_talk", "arena_master_practice_choose_weapon_request", Texts.Ids.RawNewLoadoutChoiceLine, new ConversationSentence.OnConditionDelegate(conversation_arena_make_new_weapon_choice_on_condition), new ConversationSentence.OnConsequenceDelegate(_AOArenaBehaviorManager.ResetWeaponChoice), 80, conversation_arena_join_fight_with_new_weapons_on_condition, null);

        campaignGameStarter.AddDialogLine("arena_master_practice_custom_post_fight_line", "arena_master_custom_post_practice_fight_talk", "arena_master_post_practice_fight_talk", Texts.Ids.DialogPracticeCustomPostFight, null, null, 100, null);

        //Disabled stand-in for the vanilla sign-up line (priority 110, set by ArenaMasterPatch) while a Grand/kingdom tournament is locked.
        campaignGameStarter.AddPlayerLine("arena_master_sign_up_grand_tournament_locked_line", "arena_master_talk", "close_window", Texts.Ids.DialogSignUpForTournament, () => GrandTournamentGame.IsJoinLocked(Settlement.CurrentSettlement, out _), null, 110, new ConversationSentence.OnClickableConditionDelegate(conversation_grand_tournament_join_locked_on_clickable_condition), null);
    }

    private bool conversation_grand_tournament_join_locked_on_clickable_condition(out TextObject explanation)
    {
        GrandTournamentGame.IsJoinLocked(Settlement.CurrentSettlement, out explanation);
        return false;
    }

    public void AfterMissionStarted(IMission obj)
    {
        if (!_AOArenaBehaviorManager.EnteredPracticeFightFromMenu)
        {
            return;
        }

        AOArenaBehaviorManager.Instance!.PayForPracticeMatch();
        Mission.Current.SetMissionMode(_AOArenaBehaviorManager.GetArenaPracticeMissionMode(), true);
        Mission.Current.GetMissionBehavior<ArenaPracticeFightMissionController>().StartPlayerPractice();
        _AOArenaBehaviorManager.EnteredPracticeFightFromMenu = false;
    }

    public void OnTournamentFinished(CharacterObject winner, MBReadOnlyList<CharacterObject> participants, Town town, ItemObject prize)
    {
        TournamentRewardManager.ResolveTournament(winner, participants, town);
    }

    public void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
    {
        if (mobileParty != MobileParty.MainParty || !settlement.IsTown)
        {
            return;
        }
        _AOArenaBehaviorManager.ResetCompanionChoice();
        _AOArenaBehaviorManager.ResetWeaponChoice();
        AddLoadoutDialogues(_campaignGame!, settlement);
    }

    public void OnHeroComesOfAge(Hero hero)
    {
        if (hero.Clan != null && hero.Clan == Clan.PlayerClan)
        {
            UpdateCompanionSettings();
        }
    }

    public void OnHeroKilledEvent(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
    {
        if (victim.Clan != null && victim.Clan == Clan.PlayerClan)
        {
            UpdateCompanionSettings();
        }
    }

    public void OnNewCompanionAdded(Hero hero)
    {
        UpdateCompanionSettings();
    }

    public void OnCompanionRemoved(Hero hero, RemoveCompanionAction.RemoveCompanionDetail detail)
    {
        UpdateCompanionSettings();
    }

    protected void UpdateCompanionSettings()
    {
        AOArenaBehaviorManager.RefreshHeroPracticeSettings();

    }

    protected void AddLoadoutDialogues(CampaignGameStarter campaignGameStarter, Settlement settlement)
    {
        if (settlement is null || !settlement.IsTown)
        {
            return;
        }

        CultureObject? settlementCulture = settlement.MapFaction?.Culture;
        if (settlementCulture is null || _visitedCultures.Contains(settlementCulture))
            return;

        int practiceLoadoutStages = ModSettings.PracticeLoadoutStages;
        for (int practiceStage = 1; practiceStage <= practiceLoadoutStages; practiceStage++)
        {
            CharacterObject? characterObject =
                Game.Current.ObjectManager.GetObject<CharacterObject>("weapon_practice_stage_" + practiceStage + "_" + settlementCulture.StringId)
                ?? Game.Current.ObjectManager.GetObject<CharacterObject>("weapon_practice_stage_" + practiceStage + "_empire");
            if (characterObject is null)
                continue;

            List<Equipment> battleEquipments = characterObject.BattleEquipments.ToList();
            AddLoadoutDialoguesInternal(campaignGameStarter, settlementCulture, practiceStage, battleEquipments, "arena_practice_", ArenaPracticeMode.ExceptParry);
            AddLoadoutDialoguesInternal(campaignGameStarter, settlementCulture, practiceStage, AOArenaBehaviorManager.FilterAvailableWeapons(battleEquipments, ArenaPracticeMode.Parry), "arena_parry_practice_", ArenaPracticeMode.Parry);
        }
        _visitedCultures.Add(settlementCulture);
    }

    private void AddLoadoutDialoguesInternal(CampaignGameStarter campaignGameStarter, CultureObject settlementCulture, int practiceStage, List<Equipment> battleEquipments, string dialogueIDPrefix, ArenaPracticeMode practiceMode)
    {
        List<(int EquipmentStage, string Loadout)> listOfExistingLoadouts = [];
        for (int i = 0; i < battleEquipments.Count; i++)
        {
            string[] dialogueIdArr = new string[4];
            string[] dialogueTextArr = new string[4];
            string dialogueID = dialogueIDPrefix + "stage_" + practiceStage.ToString() + "_loadout_";
            string dialogueText = "";
            int loadout = i;
            int equipmentStage = practiceStage;

            for (int x = 0; x < 4; x++)
            {
                EquipmentElement equipmentFromSlot = battleEquipments[i].GetEquipmentFromSlot((EquipmentIndex) x);
                if (equipmentFromSlot.Item != null)
                {
                    dialogueIdArr[x] = equipmentFromSlot.Item.StringId;
                    dialogueTextArr[x] = equipmentFromSlot.Item.Name.ToString();
                }
            }
            dialogueID = dialogueID + string.Join("_", dialogueIdArr.Where(s => !string.IsNullOrEmpty(s)));
            dialogueText = string.Join(", ", dialogueTextArr.Where(s => !string.IsNullOrEmpty(s)));

            (int equipmentStage, string dialogueText) equipmentEntry = (equipmentStage, dialogueText);
            if (!listOfExistingLoadouts.Contains(equipmentEntry))
            {
                listOfExistingLoadouts.Add(equipmentEntry);
                campaignGameStarter.AddPlayerLine(dialogueID, "arena_master_practice_weapons_list", "close_window", dialogueText, new ConversationSentence.OnConditionDelegate(() => conversation_town_arena_culture_match_on_condition(settlementCulture, equipmentStage, practiceMode)), new ConversationSentence.OnConsequenceDelegate(() => conversation_arena_join_fight_with_selected_loadout_on_consequence(loadout)), 100, new ConversationSentence.OnClickableConditionDelegate((out TextObject? explanation) => conversation_town_arena_afford_loadout_choice_on_condition(out explanation, equipmentStage)), null);
            }
        }
    }

    private bool game_menu_host_tournament_on_condition(MenuCallbackArgs args)
    {
        Town? town = Settlement.CurrentSettlement?.Town;
        if (town is null || !ModSettings.TournamentHostingEnabled || Campaign.Current.TournamentManager.GetTournamentGame(town) is not null)
            return false;
        args.optionLeaveType = GameMenuOption.LeaveType.Manage;
        args.IsEnabled = TournamentHostingManager.CanHost(town, out TextObject explanation);
        args.Tooltip = explanation;
        return true;
    }

    private void game_menu_host_tournament_on_consequence(MenuCallbackArgs args)
    {
        Town? town = Settlement.CurrentSettlement?.Town;
        if (town is null)
            return;
        TextObject body = new(Texts.Ids.HostTournamentConfirmBody);
        int influence = TournamentHostingManager.GetInfluenceCost();
        body.SetTextVariable("TOWN", town.Name);
        body.SetTextVariable("GOLD", TournamentHostingManager.GetGoldCost(town));
        body.SetTextVariable("INFLUENCE", influence);
        body.SetTextVariable("HAS_INFLUENCE", influence > 0 ? 1 : 0);
        InformationManager.ShowInquiry(new InquiryData(
            new TextObject(Texts.Ids.HostTournamentConfirmTitle).ToString(), body.ToString(), true, true,
            GameTexts.FindText("str_yes").ToString(), GameTexts.FindText("str_no").ToString(),
            () => { TournamentHostingManager.Host(town); GameMenu.SwitchToMenu("town_arena"); }, null));
    }
    
    private bool conversation_arena_ask_for_speacial_practice_fight_on_condition() => ModSettings.ParryPracticeEnabled || ModSettings.TeamPracticeEnabled;

    private bool conversation_arena_parry_practice_enabled_on_condition() => AOArenaBehaviorManager.IsPracticeModeEnabled(ArenaPracticeMode.Parry);
    private bool conversation_arena_team_practice_enabled_on_condition() => AOArenaBehaviorManager.IsPracticeModeEnabled(ArenaPracticeMode.Team);

    public bool conversation_arena_companion_choice_allowed_on_condition() => _AOArenaBehaviorManager.IsAgentSwitchingAllowed();
    public bool conversation_arena_companion_choice_post_fight_allowed_on_condition(bool isRepickLine) => _AOArenaBehaviorManager.IsAgentSwitchingAllowed() && (isRepickLine ? _AOArenaBehaviorManager.ChosenCharacter != null : _AOArenaBehaviorManager.ChosenCharacter is null);

    public bool conversation_arena_weapon_choice_allowed_on_condition() => _AOArenaBehaviorManager.IsWeaponChoiceAllowed();

    public bool conversation_arena_weapon_choice_forbidden_on_condition() => !_AOArenaBehaviorManager.IsWeaponChoiceAllowed();

    private bool conversation_town_arena_culture_match_on_condition(CultureObject culture, int stage, ArenaPracticeMode practiceMode) =>
        culture.Equals(Settlement.CurrentSettlement.MapFaction?.Culture) && stage == _AOArenaBehaviorManager.ChosenLoadoutStage && practiceMode.Contains(_AOArenaBehaviorManager.PracticeMode);

    private bool conversation_arena_join_fight_with_default_weapons_on_condition(out TextObject? explanation) => _AOArenaBehaviorManager.CheckAffordabilityForNextPracticeRound(_AOArenaBehaviorManager.PracticeMode, 0, out explanation);

    private bool conversation_arena_join_fight_with_new_weapons_on_condition(out TextObject? explanation) =>
        _AOArenaBehaviorManager.CheckAffordabilityForNextPracticeRound(_AOArenaBehaviorManager.PracticeMode, 1, out explanation);

    private bool conversation_arena_master_practice_choose_companion_choice_repeatable_on_condition()
    {
        TextObject selectedRepeatLine = ConversationSentence.SelectedRepeatLine;
        if (ConversationSentence.CurrentProcessedRepeatObject is not CharacterObject processedRepeatObject)
        {
            return false;
        }

        StringHelpers.SetRepeatableCharacterProperties("HERO", processedRepeatObject);
        return true;
    }

    private bool conversation_town_arena_weapon_choice_request_confirm_on_condition()
    {
        int price = _AOArenaBehaviorManager.GetWeaponLoadoutChoiceCost();
        MBTextManager.SetTextVariable("WEAPON_CHOICE_HAS_PRICE", (price > 0 ? 1 : 0).ToString(), false);
        MBTextManager.SetTextVariable("WEAPON_CHOICE_PRICE", (_AOArenaBehaviorManager.ChosenLoadoutStage * price).ToString(), false);
        return true;
    }

    private bool conversation_town_arena_weapons_list_on_condition() => _AOArenaBehaviorManager.ChosenLoadoutStage < ModSettings.PracticeLoadoutStages;

    private bool conversation_town_arena_fight_join_check_on_condition(out TextObject? explanation)
    {
        if (Hero.MainHero.IsWounded && Campaign.Current.IsMainHeroDisguised)
        {
            explanation = new TextObject(Texts.Ids.WoundedAndInDisguise, null);
            return false;
        }
        if (Hero.MainHero.IsWounded)
        {
            explanation = new TextObject(Texts.Ids.Wounded, null);
            return false;
        }
        if (Campaign.Current.IsMainHeroDisguised)
        {
            explanation = new TextObject(Texts.Ids.InDisguise, null);
            return false;
        }

        explanation = null;
        return true;
    }

    private bool conversation_town_arena_parry_fight_join_check_on_condition(out TextObject? explanation)
    {
        if (!conversation_town_arena_fight_join_check_on_condition(out explanation) || !_AOArenaBehaviorManager.CheckAvailabilityOfWeaponLoadouts(ArenaPracticeMode.Parry, out explanation))
        {
            return false;
        }

        explanation = null;
        return true;
    }

    private bool conversation_arena_return_to_default_choice_allowed_on_condition()
    {
        TextObject textObject = _AOArenaBehaviorManager.ChosenCharacter is null
            ? new(Texts.Ids.DialogStandardGearPlayer)
            : new(Texts.Ids.DialogStandardGearCompanion, new() { ["COMPANION_GENDER"] = (_AOArenaBehaviorManager.ChosenCharacter.IsFemale ? 1 : 0).ToString() });
        MBTextManager.SetTextVariable("DEFAULT_WEAPONS_CHOICE_LINE", textObject, false);
        return conversation_arena_weapon_choice_allowed_on_condition() && _AOArenaBehaviorManager.ChosenLoadout >= 0;
    }

    private bool conversation_arena_make_new_weapon_choice_on_condition()
    {
        TextObject textObject = _AOArenaBehaviorManager.ChosenCharacter is null
            ? new(Texts.Ids.DialogNewLoadoutPlayer)
            : new(Texts.Ids.DialogNewLoadoutCompanion, new() { ["COMPANION_NAME"] = _AOArenaBehaviorManager.ChosenCharacter.Name });
        MBTextManager.SetTextVariable("NEW_LOADOUT_CHOICE_LINE", textObject, false);
        return conversation_arena_weapon_choice_allowed_on_condition();
    }

    private bool conversation_town_arena_afford_loadout_choice_on_condition(out TextObject? explanation, int stage)
    {
        return _AOArenaBehaviorManager.CheckAffordabilityForNextPracticeRound(_AOArenaBehaviorManager.PracticeMode, stage, out explanation);
    }

    internal bool conversation_town_arena_afford_loadout_choice_on_condition(out TextObject? explanation)
    {
        return _AOArenaBehaviorManager.CheckAffordabilityForNextPracticeRound(out explanation);
    }

    private static bool conversation_arena_expansive_practice_fight_explain_reward_on_condition()
    {
        PracticePrizeManager.ExplainPracticeReward(ArenaPracticeMode.Expansive);
        return true;
    }

    private bool conversation_arena_can_join_practice_fight_on_condition() => !Settlement.CurrentSettlement.Town.HasTournament;

    private bool conversation_arena_join_special_practice_fight_confirm_on_condition()
    {
        int price = _AOArenaBehaviorManager.GetPracticeModeChoiceCost();
        MBTextManager.SetTextVariable("PRACTICE_CHOICE_HAS_PRICE", (price > 0 ? 1 : 0).ToString(), false);
        MBTextManager.SetTextVariable("PRACTICE_CHOICE_PRICE", price.ToString(), false);

        return !Settlement.CurrentSettlement.Town.HasTournament;
    }

    private void conversation_arena_companion_choice_request_on_consequence()
    {
        ConversationSentence.SetObjectsToRepeatOver((IReadOnlyList<object>) AOArenaBehaviorManager.GetAvailableHeroes());
    }

    private void conversation_arena_master_practice_choose_companion_choice_repeatable_on_consequence()
    {
        if (ConversationSentence.SelectedRepeatObject is not CharacterObject selectedRepeatObject)
        {
            return;
        }
        _AOArenaBehaviorManager.ChosenCharacter = selectedRepeatObject;
    }

    private void conversation_arena_join_fight_with_selected_loadout_on_consequence(int loadout)
    {
        _AOArenaBehaviorManager.ChosenLoadout = loadout;
        conversation_arena_join_fight_with_pre_selected_loadout_on_consequence();
    }

    public void conversation_arena_join_fight_with_pre_selected_loadout_on_consequence()
    {
        Campaign.Current.ConversationManager.ConversationEndOneShot += new Action(StartPlayerPracticeAfterConversationEnd);
    }

    private void conversation_town_arena_weapons_list_get_better_on_consequence() => _AOArenaBehaviorManager.ChosenLoadoutStage++;

    public void conversation_arena_join_fight_with_default_weapons_on_consequence()
    {
        _AOArenaBehaviorManager.ResetWeaponChoice();
        Campaign.Current.ConversationManager.ConversationEndOneShot += new Action(StartPlayerPracticeAfterConversationEnd);
    }

    private static void StartPlayerPracticeAfterConversationEnd()
    {
        AOArenaBehaviorManager.Instance!.PayForPracticeMatch();
        Mission.Current.SetMissionMode(AOArenaBehaviorManager.Instance!.GetArenaPracticeMissionMode(), false);
        Mission.Current.GetMissionBehavior<ArenaPracticeFightMissionController>().StartPlayerPractice();
    }

    private void game_menu_nearby_tournaments_list_on_init(MenuCallbackArgs args)
    {
        List<Town> nearbyTournamentTowns = GetNearbyTournaments();
        List<float> nearbyTownDistances = Town.AllTowns.Where(x => x != Settlement.CurrentSettlement.Town).Select(town => town.Settlement.GetPosition2D.DistanceSquared(Settlement.CurrentSettlement.GetPosition2D)).OrderBy(dist => dist).ToList();
        _tournamentListTotalCount = nearbyTournamentTowns.Count;

        int pageCount = (_tournamentListTotalCount + _tournamentListEntriesPerPage - 1) / _tournamentListEntriesPerPage;
        string entrySeparator = Environment.NewLine + Environment.NewLine;

        TextObject tournamentList = new(Texts.Ids.NearbyTournamentsList);
        tournamentList.SetTextVariable("NEW_LINE", Environment.NewLine);
        tournamentList.SetTextVariable("SHOW_PAGES", pageCount > 1 ? 1 : 0);
        tournamentList.SetTextVariable("PAGE", _tournamentListOffset + 1);
        tournamentList.SetTextVariable("PAGES", pageCount);
        LocalizationHelper.SetNumericVariable(tournamentList, "TOURNAMENTS", nearbyTournamentTowns.Count);
        LocalizationHelper.SetListVariable(tournamentList, "TORNAMENT_LIST", GetTournamentDescriptions(nearbyTournamentTowns, nearbyTownDistances, _tournamentListOffset, _tournamentListEntriesPerPage), entrySeparator);

        MBTextManager.SetTextVariable("MENU_TEXT", tournamentList.ToString(), false);
        return;

        //local methods
        static List<Town> GetNearbyTournaments()
        {
            return Town.AllTowns.Where(x => Campaign.Current.TournamentManager.GetTournamentGame(x) != null && x != Settlement.CurrentSettlement.Town).OrderBy(x => x.Settlement.GetPosition2D.DistanceSquared(Settlement.CurrentSettlement.GetPosition2D)).ToList();
        }

        static string GetDistanceEstimate(Town town, List<float> nearbyTownDistances, out bool isCloseBy)
        {
            int oneFourthIdx = (nearbyTownDistances.Count / 4) - 1;
            int twoFourthIdx = (nearbyTownDistances.Count / 2) - 1;
            int threeFourthIdx = (nearbyTownDistances.Count * 3 / 4) - 1;
            int count = nearbyTownDistances.Count;
            if (count == 0)
            {
                isCloseBy = true;
                return new TextObject(Texts.Ids.DistanceNearby).ToString();
            }
            float oneFourthDistance = nearbyTownDistances[Math.Max(count / 4 - 1, 0)];
            float twoFourthDistance = nearbyTownDistances[Math.Max(count / 2 - 1, 0)];
            float threeFourthDistance = nearbyTownDistances[Math.Max(count * 3 / 4 - 1, 0)];

            float distanceInQuestion = town.Settlement.GetPosition2D.DistanceSquared(Settlement.CurrentSettlement.GetPosition2D);
            if (distanceInQuestion < twoFourthDistance)
            {
                isCloseBy = true;
                return distanceInQuestion < oneFourthDistance ? new TextObject(Texts.Ids.DistanceNearby).ToString() : new TextObject(Texts.Ids.DistanceReasonablyClose).ToString();
            }
            else
            {
                isCloseBy = false;
                return distanceInQuestion < threeFourthDistance ? new TextObject(Texts.Ids.DistanceReasonablyFar).ToString() : new TextObject(Texts.Ids.DistanceVeryFar).ToString();
            }
        }

        static List<string> GetTournamentDescriptions(List<Town> nearbyTournamentTowns, List<float> nearbyTownDistances, int offset, int entriesCount)
        {
            List<string> nearbyTournamentDescriptions = new();
            int startEntry = offset * entriesCount;
            for (int i = startEntry; i < nearbyTournamentTowns.Count && i < startEntry + entriesCount; i++)
            {
                Town town = nearbyTournamentTowns[i];
                TournamentGame tournamentGame = Campaign.Current.TournamentManager.GetTournamentGame(town);

                string distanceEstimate = GetDistanceEstimate(town, nearbyTownDistances, out bool isCloseBy);
                int tournamentAge = (int) tournamentGame.CreationTime.ElapsedDaysUntilNow;
                int textVariation = (isCloseBy ? 0 : 3) + (tournamentAge <= 4 ? 0 : tournamentAge > 12 ? 2 : 1);

                ItemObject prizeItemObject = tournamentGame.Prize;
                TextObject prizeTextObject = new(Texts.Ids.NearbyTournamentPrize);
                prizeTextObject.SetTextVariable("NEW_LINE", Environment.NewLine);
                LocalizationHelper.SetNumericVariable(prizeTextObject, "TIER", (int) prizeItemObject.Tier + 1);
                prizeTextObject.SetTextVariable("TYPE", GetItemTypeName(prizeItemObject.Type));
                prizeTextObject.SetTextVariable("NAME", TournamentRewardManager.GetPrizeItemName(tournamentGame));
                prizeTextObject.SetTextVariable("GOLD", prizeItemObject.Value);

                TextObject textObject = GameTexts.FindText("str_menu_nearby_tournaments_list_entry");
                textObject.SetTextVariable("NEW_LINE", Environment.NewLine);
                textObject.SetTextVariable("TAG", GameTexts.FindText("str_menu_nearby_tournaments_list_tag", textVariation.ToString()));
                textObject.SetTextVariable("INDEX", i + 1);
                textObject.SetTextVariable("TOWN_LINK", town.Settlement.EncyclopediaLinkWithName);
                textObject.SetTextVariable("DISTANCE_ESTIMATE", distanceEstimate);
                LocalizationHelper.SetNumericVariable(textObject, "TOURNAMENT_AGE", tournamentAge);
                textObject.SetTextVariable("ITEM_PRIZE", prizeTextObject);
                textObject.SetTextVariable("GOLD_PRIZES_ENABLED", ModSettings.TournamentEnableGoldPrizes ? 1 : 0);
                textObject.SetTextVariable("GOLD_PRIZE", TournamentRewardManager.GetTournamentGoldPrize(town));

                nearbyTournamentDescriptions.Add(textObject.ToString());
            }
            return nearbyTournamentDescriptions;
        }

        static string GetItemTypeName(ItemTypeEnum itemTypeEnum) =>
            itemTypeEnum switch
            {
                ItemTypeEnum.Invalid => Texts.Ids.ItemTypeInvalidItem,
                ItemTypeEnum.Horse => Texts.Ids.ItemTypeHorse,
                ItemTypeEnum.OneHandedWeapon => Texts.Ids.ItemTypeOneHandedWeapon,
                ItemTypeEnum.TwoHandedWeapon => Texts.Ids.ItemTypeTwoHandedWeapon,
                ItemTypeEnum.Polearm => Texts.Ids.ItemTypePolearm,
                ItemTypeEnum.Arrows => Texts.Ids.ItemTypeArrows,
                ItemTypeEnum.Bolts => Texts.Ids.ItemTypeBolts,
                ItemTypeEnum.Shield => Texts.Ids.ItemTypeShield,
                ItemTypeEnum.Bow => Texts.Ids.ItemTypeBow,
                ItemTypeEnum.Crossbow => Texts.Ids.ItemTypeCrossbow,
                ItemTypeEnum.Thrown => Texts.Ids.ItemTypeThrownWeapon,
                ItemTypeEnum.Goods => Texts.Ids.ItemTypeGoods,
                ItemTypeEnum.HeadArmor => Texts.Ids.ItemTypeHeadArmor,
                ItemTypeEnum.BodyArmor => Texts.Ids.ItemTypeBodyArmor,
                ItemTypeEnum.LegArmor => Texts.Ids.ItemTypeLegArmor,
                ItemTypeEnum.HandArmor => Texts.Ids.ItemTypeHandArmor,
                ItemTypeEnum.Pistol => Texts.Ids.ItemTypePistol,
                ItemTypeEnum.Musket => Texts.Ids.ItemTypeMusket,
                ItemTypeEnum.Bullets => Texts.Ids.ItemTypeBullets,
                ItemTypeEnum.Animal => Texts.Ids.ItemTypeAnimal,
                ItemTypeEnum.Book => Texts.Ids.ItemTypeBook,
                ItemTypeEnum.ChestArmor => Texts.Ids.ItemTypeChestArmor,
                ItemTypeEnum.Cape => Texts.Ids.ItemTypeCape,
                ItemTypeEnum.HorseHarness => Texts.Ids.ItemTypeHorseHarness,
                ItemTypeEnum.Banner => Texts.Ids.ItemTypeBanner,
                _ => Texts.Ids.ItemTypeItem
            };
    }

    private void game_menu_enter_expansive_practice_fight_on_consequence(MenuCallbackArgs args) => StartPlayerPracticeFromMenu(ArenaPracticeMode.Expansive);

    private void game_menu_enter_parry_practice_fight_on_consequence(MenuCallbackArgs args) => StartPlayerPracticeFromMenu(ArenaPracticeMode.Parry);

    private void game_menu_enter_team_practice_fight_on_consequence(MenuCallbackArgs args) => StartPlayerPracticeFromMenu(ArenaPracticeMode.Team);

    private void StartPlayerPracticeFromMenu(ArenaPracticeMode practiceMode)
    {
        if (!FieldAccessHelper.ArenaMasterHasMetInSettlementsByRef(_arenaMasterBehavior!).Contains(Settlement.CurrentSettlement))
        {
            FieldAccessHelper.ArenaMasterHasMetInSettlementsByRef(_arenaMasterBehavior!).Add(Settlement.CurrentSettlement);
        }
        _AOArenaBehaviorManager.IsPlayerPrePractice = true;
        _AOArenaBehaviorManager.SetPracticeMode(practiceMode);
        PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(LocationComplex.Current.GetLocationWithId("arena"), null, null, null);
        _AOArenaBehaviorManager.EnteredPracticeFightFromMenu = true;
    }

    private bool game_menu_enter_expansive_practice_fight_on_condition(MenuCallbackArgs args) => CheckCustomPracticeFightMenuAvailability(args, ArenaPracticeMode.Expansive);

    private bool game_menu_enter_parry_practice_fight_on_condition(MenuCallbackArgs args) => CheckCustomPracticeFightMenuAvailability(args, ArenaPracticeMode.Parry);

    private bool game_menu_enter_team_practice_fight_on_condition(MenuCallbackArgs args) => CheckCustomPracticeFightMenuAvailability(args, ArenaPracticeMode.Team);

    private bool CheckCustomPracticeFightMenuAvailability(MenuCallbackArgs args, ArenaPracticeMode practiceMode)
    {
        if (!AOArenaBehaviorManager.IsPracticeModeEnabled(practiceMode))
        {
            return false;
        }

        Settlement currentSettlement = Settlement.CurrentSettlement;
        args.optionLeaveType = GameMenuOption.LeaveType.PracticeFight;
        if (!FieldAccessHelper.ArenaMasterKnowTournamentsByRef(_arenaMasterBehavior!))
        {
            args.Tooltip = new TextObject(Texts.Ids.LearnMoreAboutArena, null);
            args.IsEnabled = false;
            return true;
        }
        if (Hero.MainHero.IsWounded && Campaign.Current.IsMainHeroDisguised)
        {
            args.Tooltip = new TextObject(Texts.Ids.WoundedAndInDisguise, null);
            args.IsEnabled = false;
            return true;
        }
        if (Hero.MainHero.IsWounded)
        {
            args.Tooltip = new TextObject(Texts.Ids.Wounded, null);
            args.IsEnabled = false;
            return true;
        }
        if (Campaign.Current.IsMainHeroDisguised)
        {
            args.Tooltip = new TextObject(Texts.Ids.InDisguise, null);
            args.IsEnabled = false;
            return true;
        }
        if (currentSettlement.Town.HasTournament)
        {
            args.Tooltip = new TextObject(Texts.Ids.NoPracticeBecauseOfTournament, null);
            args.IsEnabled = false;
            return true;
        }
        if (!_AOArenaBehaviorManager.CheckAvailabilityOfWeaponLoadouts(practiceMode, out args.Tooltip))
        {
            args.IsEnabled = false;
            return true;
        }
        if (!_AOArenaBehaviorManager.CheckAffordabilityForNextPracticeRound(practiceMode, 0, out args.Tooltip, true))
        {
            args.IsEnabled = false;
            return true;
        }

        return true;
    }

    public bool team_game_select_roster_condition(MenuCallbackArgs args)
    {
        //Grand/kingdom tournaments are strictly one lord per team.
        if (Settlement.CurrentSettlement?.Town is Town currentTown && Campaign.Current.TournamentManager.GetTournamentGame(currentTown) is GrandTournamentGame)
        {
            return false;
        }
        
        if (!ModSettings.TeamTournamentEnabled)
        {
            return false;
        }

        bool canPlayerDo = Campaign.Current.Models.SettlementAccessModel.CanMainHeroDoSettlementAction(
            Settlement.CurrentSettlement,
            SettlementAccessModel.SettlementAction.JoinTournament,
            out bool shouldBeDisabled,
            out TextObject disabledText
        );

        args.optionLeaveType = GameMenuOption.LeaveType.TroopSelection;

        // if this town has a tournament, activate
        shouldBeDisabled &= TeamTournamentHelpers.IsTournamentActive;
        canPlayerDo &= TeamTournamentHelpers.IsTournamentActive;

        if (!shouldBeDisabled && string.IsNullOrEmpty(disabledText?.ToString()))
            disabledText = new TextObject(Texts.Ids.RosterOnlyForTeamTournaments);

        return TWHelpers.MenuHelper.SetOptionProperties(args, canPlayerDo, shouldBeDisabled, disabledText);
    }

    public void team_game_select_roster_consequence(MenuCallbackArgs args)
    {
        if (args.MenuContext.Handler is MenuViewContext menuViewContext)
        {
            TeamTournamentInfo info = TeamTournamentInfo.Current is { } current && current.Town == Settlement.CurrentSettlement.Town
                ? current
                : new TeamTournamentInfo();
            info.OpenSelectionMenu(menuViewContext);
        }
    }
/*
#if DEBUG // only needed when debugging for testing
    private bool AddTournamentCondition(MenuCallbackArgs args) => !Settlement.CurrentSettlement.Town.HasTournament;
    private void AddTournamentConsequence(MenuCallbackArgs args)
    {
        Campaign.Current.TournamentManager.AddTournament(new FightTournamentGame(Settlement.CurrentSettlement.Town));
        GameMenu.SwitchToMenu("town_arena");
    }

    private bool ResolveTournamentCondition(MenuCallbackArgs args) => Settlement.CurrentSettlement.Town.HasTournament;
    private void ResolveTournamentConsequence(MenuCallbackArgs args)
    {
        var town = Settlement.CurrentSettlement.Town;
        if (town.HasTournament)
        {
            var tournament = Campaign.Current.TournamentManager.GetTournamentGame(town);
            Campaign.Current.TournamentManager.ResolveTournament(tournament, town);
        }
        GameMenu.SwitchToMenu("town_arena");
    }
#endif
*/
#pragma warning restore IDE1006 // Naming Styles

    public override void SyncData(IDataStore dataStore)
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        dataStore.SyncDataAsJson("_AOArenaBehaviorManager", ref _AOArenaBehaviorManager);
#pragma warning restore CS8601 // Possible null reference assignment.
        if (dataStore.IsLoading)
        {
            _AOArenaBehaviorManager ??= new AOArenaBehaviorManager();
            _AOArenaBehaviorManager.Sync();
        }
        AOArenaBehaviorManager.Instance = _AOArenaBehaviorManager;
    }
}