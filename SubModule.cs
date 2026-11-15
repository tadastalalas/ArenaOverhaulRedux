using ArenaOverhaul.Behaviors;
using ArenaOverhaul.Compat;
using ArenaOverhaul.Core;
using ArenaOverhaul.Diagnostics;
using ArenaOverhaul.Managers;
using ArenaOverhaul.Missions;
using ArenaOverhaul.Models;

using Bannerlord.ButterLib.HotKeys;
using Bannerlord.UIExtenderEx;

using HarmonyLib;

using System;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace ArenaOverhaul;

public class SubModule : MBSubModuleBase
{
    private static readonly UIExtender Extender = UIExtender.Create(ModInfo.UIExtenderId);
    private static HotKeyManager? _hotKeyManager;

    private Harmony? _harmony;
    private bool _initialized;

    protected override void OnSubModuleLoad()
    {
        base.OnSubModuleLoad();

        Extender.Register(typeof(SubModule).Assembly);
        Extender.Enable();
    }

    // PatchAll runs here, once, when the main menu is about to open: the patches are inactive before it.
    protected override void OnBeforeInitialModuleScreenSetAsRoot()
    {
        base.OnBeforeInitialModuleScreenSetAsRoot();
        if (_initialized)
            return;
        _initialized = true;

        try
        {
            // Settings are read here too, so a failed transpiler can write its details when "Logging for debugging" is on.
            ModSettings.Reload();

            if (PatchAll())
                AorLog.Debug("Harmony patches applied.");

            _hotKeyManager ??= HotKeyManager.Create(ModInfo.HotKeyGroupId);
            if (_hotKeyManager is not null)
            {
                _hotKeyManager.Add<TeamPracticeHotKeyController>();
                _hotKeyManager.Build();
            }
        }
        catch (Exception ex)
        {
            AorLog.Error("Initialization error.", ex);
        }
    }

    protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
    {
        base.OnGameStart(game, gameStarterObject);
        ModSettings.Reload();

        if (game.GameType is Campaign)
        {
            CampaignGameStarter gameStarter = (CampaignGameStarter) gameStarterObject;
            AOArenaBehaviorManager.EnsureHeroPracticeSettings();
            gameStarter.AddBehavior(new AOArenaBehavior());
            AddGameModels(gameStarter);
        }
    }

    public override void OnAfterGameInitializationFinished(Game game, object starterObject)
    {
        if (game.GameType is not Campaign)
            return;

        AOArenaBehaviorManager.RegisterHeroPracticeSettings();
    }

    public override void OnGameEnd(Game game) => AOArenaBehaviorManager.UnregisterHeroPracticeSettings();

    private bool PatchAll()
    {
        try
        {
            _harmony ??= new Harmony(ModInfo.HarmonyId);
            _harmony.PatchAll(typeof(SubModule).Assembly);
            TournamentsXPandedCompat.TryApply(_harmony);
            return true;
        }
        catch (Exception ex)
        {
            AorLog.Error("The Harmony patches could not be applied.", ex);
            return false;
        }
    }

    // Each model wraps the one registered before it, so models of other mods keep working.
    private static void AddGameModels(CampaignGameStarter gameStarter)
    {
        gameStarter.AddModel(new ArenaOverhaulTournamentModel(GetCurrentModel<TournamentModel>(gameStarter) ?? new DefaultTournamentModel()));
        gameStarter.AddModel(new ArenaOverhaulSettlementAccessModel(GetCurrentModel<SettlementAccessModel>(gameStarter) ?? new DefaultSettlementAccessModel()));
    }

    private static T? GetCurrentModel<T>(IGameStarter gameStarter) where T : GameModel
    {
        GameModel[] models = gameStarter.Models.ToArray();
        for (int index = models.Length - 1; index >= 0; --index)
        {
            if (models[index] is T model)
                return model;
        }

        AorLog.Warn("No default model of type \"" + typeof(T).FullName + "\" was found.");
        return null;
    }
}