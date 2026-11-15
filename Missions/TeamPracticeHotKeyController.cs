using ArenaOverhaul.Core;
using ArenaOverhaul.Managers;

using Bannerlord.ButterLib.Common.Helpers;
using Bannerlord.ButterLib.HotKeys;

using SandBox.Missions.MissionLogics.Arena;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Core.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

using HotKeyManager = Bannerlord.ButterLib.HotKeys.HotKeyManager;

namespace ArenaOverhaul.Missions;

public class TeamPracticeHotKeyController : HotKeyBase
{
    private bool _isInquiryActive = false;

    protected override string DisplayName { get; }
    protected override string Description { get; }
    protected override InputKey DefaultKey { get; }
    protected override string Category { get; }

    public TeamPracticeHotKeyController() : base(nameof(TeamPracticeHotKeyController))
    {
        DisplayName = Texts.Ids.HotKeySwitchHero;
        Description = Texts.Ids.HotKeySwitchHeroDescription;
        DefaultKey = InputKey.Slash;
        Category = HotKeyManager.Categories[HotKeyCategory.Action];

        Predicate = () => (Mission.Current?.GetMissionBehavior<ArenaPracticeFightMissionController>()?.IsPlayerPracticing ?? false)
                          && (AOArenaBehaviorManager.Instance?.PracticeMode ?? ArenaPracticeMode.Standard) == ArenaPracticeMode.Team;
    }

    protected override void OnReleased()
    {
        if (_isInquiryActive)
        {
            return;
        }

        Mission mission = Mission.Current;
        Agent mainAgent = mission.MainAgent;
        if (GetSwitchingAvailability(mainAgent))
        {
            _isInquiryActive = true;

            List<InquiryElement> inquiryElementList = new List<InquiryElement>();
            mission.PlayerTeam.ActiveAgents
                .Where(IsFittingToSwitchTo).ToList()
                .ForEach(x => inquiryElementList.Add(new InquiryElement(x, x.Name.ToString(), new CharacterImageIdentifier(CharacterCode.CreateFrom(x.Character)))));

            for (int i = TeamPracticeStatsManager.SpawnedAliedAgentCount; i < AOArenaBehaviorManager.LastPlayerRelatedCharacters!.Count; i++)
            {
                CharacterObject characterObject = AOArenaBehaviorManager.LastPlayerRelatedCharacters![i];
                if (characterObject.IsHero)
                {
                    TextObject hint = new(Texts.Ids.HeroQueuedHint);
                    LocalizationHelper.SetNumericVariable(hint, "QUEUE_NUMBER", i - TeamPracticeStatsManager.SpawnedAliedAgentCount + 1);
                    inquiryElementList.Add(new InquiryElement(characterObject, characterObject.Name.ToString(), new CharacterImageIdentifier(CharacterCode.CreateFrom(characterObject)), true, hint.ToString()));
                }
            }

            if (inquiryElementList.Count > 0)
            {
                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(Texts.Ids.AvailableCharacters.ToLocalizedString(),
                    Texts.Ids.SelectTeammate.ToLocalizedString(), inquiryElementList, true, 1, 1, GameTexts.FindText("str_ok").ToString(), GameTexts.FindText("str_cancel").ToString(), inquiryElements =>
                    {
                        if (inquiryElements is not null && inquiryElements.Count > 0)
                        {
                            switch (inquiryElements.First().Identifier)
                            {
                                case CharacterObject character:
                                    TeamPracticeController.CharacterObjectToSwitchTo = character;
                                    break;
                                case Agent agent:
                                    if (mainAgent != null)
                                    {
                                        mainAgent.Controller = AgentControllerType.AI;
                                        mainAgent.SetWatchState(Agent.WatchState.Alarmed);
                                    }
                                    agent.Controller = AgentControllerType.Player;
                                    break;
                                default:
                                    break;
                            }
                        }
                        _isInquiryActive = false;
                    }, inquiryElements => _isInquiryActive = false, ""), true);
            }
            else
            {
                _isInquiryActive = false;
                MessageHelper.TechnicalMessage(Texts.Ids.NoCharactersToSwitchTo.ToLocalizedString());
            }
        }
        else
        {
            MessageHelper.TechnicalMessage(Texts.Ids.CannotSwitchNow.ToLocalizedString());
        }
    }

    private static bool GetSwitchingAvailability(Agent mainAgent)
    {
        return mainAgent is null || !mainAgent.IsActive();
    }

    private static bool IsFittingToSwitchTo(Agent agent)
    {
        return agent.IsHero && agent.IsAIControlled;
    }

    internal static bool HasTargetsToSwitchTo() => Mission.Current.PlayerTeam.ActiveAgents.Any(IsFittingToSwitchTo);
}