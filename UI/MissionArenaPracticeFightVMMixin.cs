using ArenaOverhaul.Core;
using ArenaOverhaul.Managers;

using Bannerlord.ButterLib.Common.Helpers;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;

using SandBox.Missions.MissionLogics.Arena;
using SandBox.ViewModelCollection.Missions;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ArenaOverhaul.UI;

[ViewModelMixin(nameof(MissionArenaPracticeFightVM.UpdatePrizeText))]
internal sealed class MissionArenaPracticeFightVMMixin(MissionArenaPracticeFightVM vm) : BaseViewModelMixin<MissionArenaPracticeFightVM>(vm)
{
    private bool _isStandardPanelVisible;
    private bool _isParryPanelVisible;
    private bool _isTeamPanelVisible;
    private bool _isTeamSpawnPanelVisible;

    private bool _isSpecialPanelVisible;

    private string _successfulBlocksText = "";
    private string _perfectBlocksText = "";
    private string _chamberBlocksText = "";
    private string _hitsTakenText = "";
    private string _alliesRemainingText = "";
    private string _awaitingText;

    private readonly MissionArenaPracticeFightVM baseVM = vm;
    private readonly ArenaPracticeFightMissionController? _practiceMissionController = FieldAccessHelper.MAPFVMPracticeMissionControllerByRef(vm);

    [DataSourceProperty]
    public bool IsStandardPanelVisible
    {
        get => _isStandardPanelVisible;
        set
        {
            if (value != _isStandardPanelVisible)
            {
                _isStandardPanelVisible = value;
                OnPropertyChangedWithValue(value, nameof(IsStandardPanelVisible));
            }
        }
    }

    [DataSourceProperty]
    public bool IsSpecialPanelVisible
    {
        get => _isSpecialPanelVisible;
        set
        {
            if (value != _isSpecialPanelVisible)
            {
                _isSpecialPanelVisible = value;
                OnPropertyChangedWithValue(value, nameof(IsSpecialPanelVisible));
            }
        }
    }

    [DataSourceProperty]
    public bool IsParryPanelVisible
    {
        get => _isParryPanelVisible;
        set
        {
            if (value != _isParryPanelVisible)
            {
                _isParryPanelVisible = value;
                OnPropertyChangedWithValue(value, nameof(IsParryPanelVisible));
            }
        }
    }

    [DataSourceProperty]
    public bool IsTeamPanelVisible
    {
        get => _isTeamPanelVisible;
        set
        {
            if (value != _isTeamPanelVisible)
            {
                _isTeamPanelVisible = value;
                OnPropertyChangedWithValue(value, nameof(IsTeamPanelVisible));
            }
        }
    }

    [DataSourceProperty]
    public bool IsTeamSpawnPanelVisible
    {
        get => _isTeamSpawnPanelVisible;
        set
        {
            if (value != _isTeamSpawnPanelVisible)
            {
                _isTeamSpawnPanelVisible = value;
                OnPropertyChangedWithValue(value, nameof(IsTeamSpawnPanelVisible));
            }
        }
    }

    [DataSourceProperty]
    public string SuccessfulBlocksText
    {
        get => _successfulBlocksText;
        set
        {
            if (value != _successfulBlocksText)
            {
                _successfulBlocksText = value;
                OnPropertyChangedWithValue(value, nameof(SuccessfulBlocksText));
            }
        }
    }

    [DataSourceProperty]
    public string PerfectBlocksText
    {
        get => _perfectBlocksText;
        set
        {
            if (value != _perfectBlocksText)
            {
                _perfectBlocksText = value;
                OnPropertyChangedWithValue(value, nameof(PerfectBlocksText));
            }
        }
    }

    [DataSourceProperty]
    public string ChamberBlocksText
    {
        get => _chamberBlocksText;
        set
        {
            if (value != _chamberBlocksText)
            {
                _chamberBlocksText = value;
                OnPropertyChangedWithValue(value, nameof(ChamberBlocksText));
            }
        }
    }

    [DataSourceProperty]
    public string HitsTakenText
    {
        get => _hitsTakenText;
        set
        {
            if (value != _hitsTakenText)
            {
                _hitsTakenText = value;
                OnPropertyChangedWithValue(value, nameof(HitsTakenText));
            }
        }
    }

    [DataSourceProperty]
    public string AlliesRemainingText
    {
        get => _alliesRemainingText;
        set
        {
            if (value != _alliesRemainingText)
            {
                _alliesRemainingText = value;
                OnPropertyChangedWithValue(value, nameof(AlliesRemainingText));
            }
        }
    }

    [DataSourceProperty]
    public string AwaitingText
    {
        get => _awaitingText;
        set
        {
            if (value != _awaitingText)
            {
                _awaitingText = value;
                OnPropertyChangedWithValue(value, nameof(AwaitingText));
            }
        }
    }

    public override void OnRefresh()
    {
        UpdatePanelsVisibility();

        if (IsParryPanelVisible)
        {
            UpdateParryPanelStats();
        }

        if (IsTeamPanelVisible)
        {
            UpdateTeamPanelStats();
        }

        if (!IsParryPanelVisible)
        {
            UpdatePrizeText();
        }
    }

    private void UpdatePanelsVisibility()
    {
        bool isParryPractice = AOArenaBehaviorManager.Instance!.PracticeMode.Contains(ArenaPracticeMode.Parry);
        bool isTeamPractice = AOArenaBehaviorManager.Instance!.PracticeMode.Contains(ArenaPracticeMode.Team);

        IsStandardPanelVisible = baseVM.IsPlayerPracticing && !isParryPractice && !isTeamPractice;
        IsParryPanelVisible = baseVM.IsPlayerPracticing && isParryPractice;
        IsTeamPanelVisible = baseVM.IsPlayerPracticing && isTeamPractice;
        IsTeamSpawnPanelVisible = IsTeamPanelVisible && TeamPracticeController.CharacterObjectToSwitchTo != null;

        IsSpecialPanelVisible = IsParryPanelVisible || IsTeamPanelVisible;
    }

    private void UpdateParryPanelStats()
    {
        TextObject successfulBlocks = new TextObject(Texts.Ids.PreparedBlocks, new() { ["PREPARED_BLOCKS"] = ParryPracticeStatsManager.PreparedBlocks });
        TextObject perfectBlocks = new TextObject(Texts.Ids.PerfectBlocks, new() { ["PERFECT_BLOCKS"] = ParryPracticeStatsManager.PerfectBlocks });
        TextObject chamberBlocks = new TextObject(Texts.Ids.ChamberBlocks, new() { ["CHAMBER_BLOCKS"] = ParryPracticeStatsManager.ChamberBlocks });
        TextObject hitsTaken = new TextObject(Texts.Ids.HitsTaken, new() { ["HITS_TAKEN"] = ParryPracticeStatsManager.HitsTaken });

        SuccessfulBlocksText = successfulBlocks.ToString();
        PerfectBlocksText = perfectBlocks.ToString();
        ChamberBlocksText = chamberBlocks.ToString();
        HitsTakenText = hitsTaken.ToString();
    }

    private void UpdateTeamPanelStats()
    {
        TextObject alliesRemaining = new TextObject(Texts.Ids.AlliesRemaining, new() { ["ALLIES_REMAINING"] = TeamPracticeStatsManager.RemainingAlliesCount });
        AlliesRemainingText = alliesRemaining.ToString();

        GameTexts.SetVariable("BEATEN_OPPONENT_COUNT", _practiceMissionController!.OpponentCountBeatenByPlayer);
        baseVM.OpponentsBeatenText = GameTexts.FindText("str_beaten_opponent").ToString();

        if (IsTeamSpawnPanelVisible)
        {
            CharacterObject? characterToSwitchTo = TeamPracticeController.CharacterObjectToSwitchTo;
            int positionInLine = AOArenaBehaviorManager.LastPlayerRelatedCharacters!.IndexOf(characterToSwitchTo!) - TeamPracticeStatsManager.SpawnedAliedAgentCount + 1;
            TextObject awaitingTextObject = new TextObject(Texts.Ids.AwaitingHero, new() { ["POSITION_IN_LINE"] = positionInLine });
            LocalizationHelper.SetEntityProperties(awaitingTextObject, "HERO", characterToSwitchTo!.HeroObject);
            AwaitingText = awaitingTextObject.ToString();
        }
    }

    private void UpdatePrizeText()
    {
        int remainingOpponentCount = _practiceMissionController!.RemainingOpponentCount;
        int countBeatenByPlayer = _practiceMissionController!.OpponentCountBeatenByPlayer;

        int prizeAmount = PracticePrizeManager.GetPrizeAmount(remainingOpponentCount, countBeatenByPlayer);
        GameTexts.SetVariable("DENAR_AMOUNT", prizeAmount);
        GameTexts.SetVariable("GOLD_ICON", Texts.Ids.GoldIcon);
        baseVM.PrizeText = GameTexts.FindText("str_earned_denar", null).ToString();
    }
}