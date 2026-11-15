using ArenaOverhaul.Core;
using ArenaOverhaul.Data;

using Bannerlord.ButterLib.Common.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace ArenaOverhaul.Managers;

public class AOArenaBehaviorManager
{
    private ArenaPracticeMode _practiceMode;
    private bool _isPlayerPrePractice = false;
    private bool _isAfterPracticeTalk = false;
    internal bool EnteredPracticeFightFromMenu { get; set; }

    private CharacterObject? _chosenCharacter;

    private int _chosenLoadout = -1;
    private int _chosenLoadoutStage = 1;

    private static Dictionary<Hero, HeroPracticeSettings>? _heroPracticeSettings;
    internal static List<CharacterObject>? LastPlayerRelatedCharacters { get; private set; }

    public ArenaPracticeMode PracticeMode => _practiceMode;
    public bool IsPlayerPrePractice { get => _isPlayerPrePractice; internal set => _isPlayerPrePractice = value; }
    public bool IsAfterPracticeTalk { get => _isAfterPracticeTalk; internal set => _isAfterPracticeTalk = value; }

    public CharacterObject? ChosenCharacter { get => _chosenCharacter; internal set => _chosenCharacter = value; }
    public int ChosenLoadout { get => _chosenLoadout; internal set => _chosenLoadout = value; }
    public int ChosenLoadoutStage { get => _chosenLoadoutStage; internal set => _chosenLoadoutStage = value; }

    public Dictionary<Hero, (int ChosenLoadoutStage, int ChosenLoadout)> CompanionLoadouts { get; private set; } = [];
    internal Dictionary<CultureObject, List<WeaponLoadoutInfo>> WeaponLoadoutInformation { get; private set; } = [];
    internal Dictionary<CultureObject, List<WeaponLoadoutInfo>> ParryWeaponLoadoutInformation { get; private set; } = [];

    public static AOArenaBehaviorManager? Instance { get; internal set; }

    [SaveableProperty(1)]
    public Dictionary<Town, PrizeItemInfo?> TournamentPrizes { get; private set; } = [];
    
    [SaveableProperty(2)]
    public Dictionary<Town, CampaignTime> PlayerHostedTournaments { get; private set; } = [];
    
    [SaveableProperty(3)]
    public int LastKingdomTournamentRound { get; set; } = -1;

    [SaveableProperty(4)]
    public int LastGrandTournamentRound { get; set; } = -1;

    internal static void EnsureHeroPracticeSettings() => _heroPracticeSettings ??= [];

    // Fills the per-hero practice settings (MCM per-save menu, or the hero defaults without MCM).
    internal static void RegisterHeroPracticeSettings() => ModSettings.RegisterHeroPracticeSettings(_heroPracticeSettings ??= []);

    internal static void UnregisterHeroPracticeSettings()
    {
        ModSettings.UnregisterHeroPracticeSettings();
        _heroPracticeSettings = null;
    }

    // The clan changed (hero came of age, died, joined or left): build the per-hero settings again.
    internal static void RefreshHeroPracticeSettings()
    {
        UnregisterHeroPracticeSettings();
        RegisterHeroPracticeSettings();
    }

    public void ResetCompanionChoice()
    {
        _chosenCharacter = null;
    }

    public void ResetWeaponChoice()
    {
        _chosenLoadout = -1;
        _chosenLoadoutStage = 1;
    }

    public void SetStandardPracticeMode() => SetPracticeMode(ArenaPracticeMode.Standard);
    public void SetExpansivePracticeMode() => SetPracticeMode(ArenaPracticeMode.Expansive);
    public void SetParryPracticeMode() => SetPracticeMode(ArenaPracticeMode.Parry);
    public void SetTeamPracticeMode() => SetPracticeMode(ArenaPracticeMode.Team);

    public void SetPracticeMode(ArenaPracticeMode mode)
    {
        _practiceMode = mode;
        ResetCompanionChoice();
        ResetWeaponChoice();
    }

    public bool CheckAffordabilityForNextPracticeRound(out TextObject? explanation) => CheckAffordabilityForNextPracticeRound(PracticeMode, ChosenLoadout >= 0 ? ChosenLoadoutStage : 0, out explanation);

    public bool CheckAffordabilityForNextPracticeRound(ArenaPracticeMode practiceMode, int playerLoadoutStage, out TextObject? explanation, bool isMenuCall = false)
    {
        int weaponLoadoutChoiceCost = GetWeaponLoadoutChoiceCost(practiceMode);
        PrepareCompanionLoadouts(practiceMode, isMenuCall, false, out int menuPlayerLoadoutStage);

        int matchPrice = GetPracticeModeChoiceCost(practiceMode);
        int playerLoadoutPrice = (isMenuCall ? menuPlayerLoadoutStage : playerLoadoutStage) * weaponLoadoutChoiceCost;
        int companionLoadoutPrice = GetCompanionLoadoutsPrice(practiceMode, weaponLoadoutChoiceCost);

        int totalPrice = matchPrice + playerLoadoutPrice + companionLoadoutPrice;
        if (totalPrice > 0)
        {
            TextObject arrangementExpenses = new TextObject(Texts.Ids.ArrangementExpenses, new()
            {
                ["PRACTICE_CHOICE_HAS_PRICE"] = (matchPrice > 0 ? 1 : 0).ToString(),
                ["PRACTICE_CHOICE_PRICE"] = matchPrice.ToString(),
                ["NEW_LINE"] = Environment.NewLine
            });
            TextObject playerLoadoutExpenses = new TextObject(Texts.Ids.PlayerLoadoutExpenses, new()
            {
                ["WEAPON_CHOICE_HAS_PRICE"] = (playerLoadoutPrice > 0 ? 1 : 0).ToString(),
                ["WEAPON_CHOICE_PRICE"] = playerLoadoutPrice.ToString(),
                ["NEW_LINE"] = Environment.NewLine
            });
            TextObject companionLoadoutExpenses = new TextObject(Texts.Ids.CompanionLoadoutExpenses, new()
            {
                ["COMPANION_WEAPON_CHOICES_HAVE_PRICE"] = (companionLoadoutPrice > 0 ? 1 : 0).ToString(),
                ["COMPANION_WEAPON_CHOICES_PRICE"] = companionLoadoutPrice.ToString(),
                ["NEW_LINE"] = Environment.NewLine
            });
            TextObject expensesBreakdown = new TextObject(Texts.Ids.ExpensesBreakdown, new()
            {
                ["TOTAL_EXPENSES"] = totalPrice.ToString(),
                ["ARRANGEMENT_EXPENSES"] = arrangementExpenses,
                ["PLAYER_LOADOUT_EXPENSES"] = playerLoadoutExpenses,
                ["COMPANION_LOADOUT_EXPENSES"] = companionLoadoutExpenses,
                ["NEW_LINE"] = Environment.NewLine
            });

            if (Hero.MainHero.Gold < totalPrice)
            {
                explanation = new TextObject(Texts.Ids.NotEnoughGold, new()
                {
                    ["EXPENSES_BREAKDOWN"] = expensesBreakdown,
                    ["NEW_LINE"] = Environment.NewLine
                });
                return false;
            }
            else
            {
                explanation = new TextObject(Texts.Ids.AffordableRounds, new()
                {
                    ["EXPENSES_BREAKDOWN"] = expensesBreakdown,
                    ["NEW_LINE"] = Environment.NewLine
                });
                LocalizationHelper.SetNumericVariable(explanation, "AFFORDABLE_ROUNDS", Hero.MainHero.Gold / totalPrice);
                return true;
            }
        }
        explanation = null;
        return true;
    }

    public bool CheckAvailabilityOfWeaponLoadouts(ArenaPracticeMode practiceMode, out TextObject? explanation)
    {
        if (practiceMode == ArenaPracticeMode.Parry)
        {
            Settlement settlement = Settlement.CurrentSettlement;
            CultureObject? settlementCulture = settlement?.MapFaction?.Culture;
            if (settlementCulture is null || !ParryWeaponLoadoutInformation.TryGetValue(settlementCulture, out List<WeaponLoadoutInfo>? cultureParryLoadoutsInformation) || !HasALoadoutForEachStage(cultureParryLoadoutsInformation))
            {
                explanation = new TextObject(Texts.Ids.NoParryLoadouts);
                return false;
            }
        }
        explanation = null;
        return true;

        /* local method */
        static bool HasALoadoutForEachStage(List<WeaponLoadoutInfo> cultureParryLoadoutsInformation)
        {
            int practiceLoadoutStages = ModSettings.PracticeLoadoutStages;
            for (int practiceStage = 1; practiceStage <= practiceLoadoutStages; practiceStage++)
            {
                if (!cultureParryLoadoutsInformation.Any(info => info.LoadoutStage == practiceStage))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public List<Equipment> FilterAvailableWeapons(List<Equipment> loadoutList)
    {
        return FilterAvailableWeapons(loadoutList, PracticeMode);
    }

    public static List<Equipment> FilterAvailableWeapons(List<Equipment> loadoutList, ArenaPracticeMode practiceMode)
    {
        if (practiceMode != ArenaPracticeMode.Parry)
        {
            return loadoutList;
        }
        return loadoutList.Where(x =>
        {
            for (int index = 0; index < 4; ++index)
            {
                EquipmentElement itemSlot = x[index];
                if (itemSlot.IsEmpty || itemSlot.Item.PrimaryWeapon is not { } weapon)
                    continue;
                switch (weapon.WeaponClass)
                {
                    case > WeaponClass.LowGripPolearm:
                    case WeaponClass.OneHandedPolearm or WeaponClass.TwoHandedPolearm or WeaponClass.LowGripPolearm when weapon.SwingDamage <= 5:
                        return false;
                }
            }
            return true;
        }).ToList();
    }

    public void PayForPracticeMatch()
    {
        int weaponLoadoutChoiceCost = GetWeaponLoadoutChoiceCost();
        PrepareCompanionLoadouts(PracticeMode, EnteredPracticeFightFromMenu);

        int matchPrice = GetPracticeModeChoiceCost();
        int playerLoadoutPrice = ChosenLoadout >= 0 ? ChosenLoadoutStage * weaponLoadoutChoiceCost : 0;
        int companionLoadoutPrice = GetCompanionLoadoutsPrice(PracticeMode, weaponLoadoutChoiceCost);

        int totalPrice = matchPrice + playerLoadoutPrice + companionLoadoutPrice;
        if (totalPrice > 0)
        {
            GiveGoldAction.ApplyForCharacterToSettlement(Hero.MainHero, Settlement.CurrentSettlement, totalPrice);
        }
    }

    public void PrepareCompanionLoadouts(ArenaPracticeMode practiceMode, bool isMenuCall = false) => PrepareCompanionLoadouts(practiceMode, isMenuCall, true, out _);

    private void PrepareCompanionLoadouts(ArenaPracticeMode practiceMode, bool isMenuCall, bool applyPlayerLoadout, out int autoPlayerLoadoutStage)
    {
        autoPlayerLoadoutStage = 0;
        CompanionLoadouts = [];

        Settlement settlement = Settlement.CurrentSettlement;
        CultureObject? settlementCulture = settlement?.MapFaction?.Culture;

        GetAvailableLoadoutInfo(settlementCulture);
        if (settlementCulture is null || _heroPracticeSettings is null || !WeaponLoadoutInformation.TryGetValue(settlementCulture, out List<WeaponLoadoutInfo>? cultureGeneralLoadoutsInformation) || !ParryWeaponLoadoutInformation.TryGetValue(settlementCulture, out List<WeaponLoadoutInfo>? cultureParryLoadoutsInformation))
        {
            return;
        }
        List<WeaponLoadoutInfo> cultureLoadoutsInformation = (practiceMode == ArenaPracticeMode.Parry) ? cultureParryLoadoutsInformation : cultureGeneralLoadoutsInformation;

        //Player
        if (isMenuCall && (!applyPlayerLoadout || ChosenLoadout < 0))
        {
            GetHeroLoadout(cultureLoadoutsInformation, Hero.MainHero, out int playerLoadoutStage, out int playerLoadoutIndex);
            if (playerLoadoutStage > 0 && playerLoadoutIndex >= 0)
            {
                autoPlayerLoadoutStage = playerLoadoutStage;
                if (applyPlayerLoadout)
                {
                    ChosenLoadoutStage = playerLoadoutStage;
                    ChosenLoadout = playerLoadoutIndex;
                }
            }
        }

        //Companions
        int companionCap = practiceMode switch
        {
            ArenaPracticeMode.Expansive => GetTotalParticipantsCount(practiceMode) * 2 / 3,
            ArenaPracticeMode.Team => GetTotalParticipantsCount(practiceMode) / GetAITeamsCount(practiceMode),
            _ => 0,
        };
        List<CharacterObject> availableHeroes = GetAvailableHeroes();
        for (int i = 0; i < availableHeroes.Count && i < companionCap; i++)
        {
            Hero companion = availableHeroes[i].HeroObject;
            GetHeroLoadout(cultureLoadoutsInformation, companion, out int loadoutStage, out int loadoutIndex);
            if (loadoutStage > 0 && loadoutIndex >= 0)
                CompanionLoadouts[companion] = (loadoutStage, loadoutIndex);
        }
    }

    private static void GetHeroLoadout(List<WeaponLoadoutInfo> cultureLoadoutsInformation, Hero companion, out int loadoutStage, out int loadoutIndex)
    {
        loadoutStage = -1; loadoutIndex = -1;
        if (!_heroPracticeSettings!.TryGetValue(companion, out HeroPracticeSettings? heroSettings) || !heroSettings.EnableLoadoutChoice)
        {
            return;
        }

        List<(int LoadoutStage, int LoadoutIndex, int LoadoutScore, bool PreferenceIsMet)> scoredLoadouts = cultureLoadoutsInformation
            .Select(x => (x.LoadoutStage, x.LoadoutIndex, LoadoutScore: x.GetScore(companion, [heroSettings.FirstPriorityWeapons, heroSettings.SecondPriorityWeapons, heroSettings.ThirdPriorityWeapons], out bool weaponPreferenceIsMet), PreferenceIsMet: weaponPreferenceIsMet))
            .Where(x => x.PreferenceIsMet || !heroSettings.OnlyPriorityLoadouts).ToList();

        if (scoredLoadouts.Count <= 0)
        {
            return;
        }

        (loadoutStage, loadoutIndex, _, _) = heroSettings.PrioritizeExpensiveEquipment
            ? scoredLoadouts.OrderByDescending(x => x.LoadoutScore).ThenByDescending(x => x.LoadoutStage).First()
            : scoredLoadouts.OrderBy(x => x.LoadoutStage).ThenByDescending(x => x.LoadoutScore).First();
    }

    private void GetAvailableLoadoutInfo(CultureObject? settlementCulture)
    {
        if (settlementCulture is null || (WeaponLoadoutInformation.TryGetValue(settlementCulture, out List<WeaponLoadoutInfo>? cultureLoadoutsInformation) && cultureLoadoutsInformation != null))
            return;

        List<WeaponLoadoutInfo> listOfExistingLoadouts = [];
        List<WeaponLoadoutInfo> listOfExistingParryLoadouts = [];
        CultureObject defaultCulture = Game.Current.ObjectManager.GetObject<CultureObject>("empire") ?? Game.Current.ObjectManager.GetObject<CultureObject>(x => x.IsMainCulture);
        int practiceLoadoutStages = ModSettings.PracticeLoadoutStages;
        for (int practiceStage = 1; practiceStage <= practiceLoadoutStages; practiceStage++)
        {
            CharacterObject? characterObject =
                Game.Current.ObjectManager.GetObject<CharacterObject>("weapon_practice_stage_" + practiceStage + "_" + settlementCulture.StringId)
                ?? Game.Current.ObjectManager.GetObject<CharacterObject>("weapon_practice_stage_" + practiceStage + "_empire");
            
            if (characterObject is null)
                continue;

            List<Equipment> battleEquipments = characterObject.BattleEquipments.ToList();
            UpdateStageLoadouts(practiceStage, battleEquipments, listOfExistingLoadouts);
            UpdateStageLoadouts(practiceStage, FilterAvailableWeapons(battleEquipments, ArenaPracticeMode.Parry), listOfExistingParryLoadouts);
        }

        WeaponLoadoutInformation[settlementCulture] = listOfExistingLoadouts;
        ParryWeaponLoadoutInformation[settlementCulture] = listOfExistingParryLoadouts;
    }

    private static void UpdateStageLoadouts(int practiceStage, List<Equipment> battleEquipments, List<WeaponLoadoutInfo> listOfExistingLoadouts)
    {
        for (int i = 0; i < battleEquipments.Count; i++)
        {
            string[] itemIdArr = new string[4];
            int loadout = i;
            int equipmentStage = practiceStage;

            int oneHandedScore = 0;
            int twoHandedScore = 0;
            int polearmScore = 0;
            int bowScore = 0;
            int crossbowScore = 0;
            int throwingScore = 0;

            for (int x = 0; x < 4; x++)
            {
                EquipmentElement equipmentFromSlot = battleEquipments[i].GetEquipmentFromSlot((EquipmentIndex) x);
                if (equipmentFromSlot.Item != null)
                {
                    itemIdArr[x] = equipmentFromSlot.Item.StringId;
                    if (equipmentFromSlot.Item?.WeaponComponent?.PrimaryWeapon is WeaponComponentData weaponData)
                    {
                        if (weaponData.RelevantSkill == DefaultSkills.OneHanded)
                        {
                            oneHandedScore += weaponData.SwingDamage + weaponData.ThrustDamage / 2 + (weaponData.IsShield ? weaponData.MaxDataValue / 5 : 0);
                        }
                        else if (weaponData.RelevantSkill == DefaultSkills.TwoHanded)
                        {
                            twoHandedScore += weaponData.SwingDamage + weaponData.ThrustDamage / 2;
                        }
                        else if (weaponData.RelevantSkill == DefaultSkills.Polearm)
                        {
                            polearmScore += weaponData.SwingDamage + weaponData.ThrustDamage;
                        }
                        else if (weaponData.RelevantSkill == DefaultSkills.Bow)
                        {
                            bowScore += weaponData.MissileDamage + (weaponData.IsAmmo ? weaponData.MaxDataValue : 0);
                        }
                        else if (weaponData.RelevantSkill == DefaultSkills.Crossbow)
                        {
                            crossbowScore += weaponData.MissileDamage + (weaponData.IsAmmo ? weaponData.MaxDataValue : 0);
                        }
                        else if (weaponData.RelevantSkill == DefaultSkills.Throwing)
                        {
                            throwingScore += (weaponData.MaxDataValue > 0 ? weaponData.MaxDataValue * weaponData.MissileDamage / 4 : weaponData.MissileDamage) + (weaponData.SwingDamage + weaponData.ThrustDamage) / 4;
                        }
                    }
                }
            }
            string itemIDs = string.Join("_", itemIdArr.Where(s => !string.IsNullOrEmpty(s)));
            WeaponLoadoutInfo loadoutEntry = new WeaponLoadoutInfo(itemIDs, equipmentStage, loadout, oneHandedScore, twoHandedScore, polearmScore, bowScore, crossbowScore, throwingScore);

            if (!listOfExistingLoadouts.Any(x => x.ItemIDs == loadoutEntry.ItemIDs && x.LoadoutStage == loadoutEntry.LoadoutStage))
            {
                listOfExistingLoadouts.Add(loadoutEntry);
            }
        }
    }

    private int GetCompanionLoadoutsPrice(ArenaPracticeMode practiceMode, int weaponLoadoutChoiceCost)
    {
        int companionLoadoutPrice = CompanionLoadouts.Values.Sum(x => x.ChosenLoadoutStage > 0 && x.ChosenLoadout >= 0 ? x.ChosenLoadoutStage * weaponLoadoutChoiceCost : 0);
        return companionLoadoutPrice;
    }
    
    //PartyBelongedTo can be null; then there are no party members to bring along.
    private static List<TroopRosterElement> GetMainHeroPartyTroops() => Hero.MainHero.PartyBelongedTo?.MemberRoster.GetTroopRoster() ?? [];

    internal static List<CharacterObject> GetAvailableHeroes()
    {
        List<CharacterObject> characterObjectList = [];
        foreach (TroopRosterElement troopRosterElement in GetMainHeroPartyTroops())
        {
            if (troopRosterElement.Character == Hero.MainHero.CharacterObject || (Instance?.ChosenCharacter is not null && troopRosterElement.Character == Instance.ChosenCharacter))
            {
                continue;
            }
            else if (!characterObjectList.Contains(troopRosterElement.Character) && troopRosterElement.Character.IsHero && !troopRosterElement.Character.HeroObject.IsWounded)
            {
                characterObjectList.Add(troopRosterElement.Character);
            }
        }
        return characterObjectList;
    }

    internal static List<CharacterObject> GetPlayerRelatedParticipantCharacters(ArenaPracticeMode practiceMode, int maxParticipantCount)
    {
        List<CharacterObject> characterObjectList = [];
        switch (practiceMode)
        {
            case ArenaPracticeMode.Expansive:
            {
                FillParticipantsForExpansivePractice(maxParticipantCount, characterObjectList);
                LastPlayerRelatedCharacters = new(characterObjectList);
                return characterObjectList;
            }
            case ArenaPracticeMode.Team:
            {
                FillParticipantsForTeamPractice(maxParticipantCount, characterObjectList);
                //Heroes first so family/companions always spawn; a big party would otherwise bury them among the troops.
                List<CharacterObject> heroes = [.. characterObjectList.Where(x => x.IsHero)];
                List<CharacterObject> troops = [.. characterObjectList.Where(x => !x.IsHero)];
                heroes.Shuffle();
                troops.Shuffle();
                LastPlayerRelatedCharacters = [.. heroes, .. troops];
                return [];
            }
            default:
            {
                LastPlayerRelatedCharacters = [];
                return characterObjectList;
            }
        }

        /* Local functions */
        static void FillParticipantsForExpansivePractice(int maxParticipantCount, List<CharacterObject> characterObjectList)
        {
            maxParticipantCount = maxParticipantCount * 2 / 3;
            if (characterObjectList.Count >= maxParticipantCount)
            {
                return;
            }

            int requiredParticipantsCount = maxParticipantCount - characterObjectList.Count;
            Dictionary<int, int> troopTierCounter = [];
            foreach (TroopRosterElement troopRosterElement in GetMainHeroPartyTroops())
            {
                if (troopRosterElement.Character == Hero.MainHero.CharacterObject || (Instance?.ChosenCharacter is not null && troopRosterElement.Character == Instance.ChosenCharacter))
                {
                    continue;
                }
                if (!characterObjectList.Contains(troopRosterElement.Character) && troopRosterElement.Character.IsHero && !troopRosterElement.Character.HeroObject.IsWounded)
                {
                    characterObjectList.Add(troopRosterElement.Character);
                }
                else if (!characterObjectList.Contains(troopRosterElement.Character) && troopRosterElement.Character.Tier >= 3)
                {
                    troopTierCounter.TryGetValue(troopRosterElement.Character.Tier, out int currentCount);
                    if (requiredParticipantsCount * Math.Max(0.5 - 0.1 * (troopRosterElement.Character.Tier - 3), 0.1) > currentCount)
                    {
                        characterObjectList.Add(troopRosterElement.Character);
                        troopTierCounter[troopRosterElement.Character.Tier] = ++currentCount;
                    }
                }
                if (characterObjectList.Count >= maxParticipantCount)
                {
                    return;
                }
            }
        }

        static void FillParticipantsForTeamPractice(int maxParticipantCount, List<CharacterObject> characterObjectList)
        {
            maxParticipantCount = maxParticipantCount / GetAITeamsCount(ArenaPracticeMode.Team);
            if (characterObjectList.Count >= maxParticipantCount)
            {
                return;
            }

            List<TroopRosterElement> troopRosterFull = GetMainHeroPartyTroops();
            foreach (TroopRosterElement troopRosterElement in troopRosterFull.Where(x => x.Character.IsHero).ToList())
            {
                if (troopRosterElement.Character == Hero.MainHero.CharacterObject || (Instance?.ChosenCharacter is not null && troopRosterElement.Character == Instance.ChosenCharacter))
                {
                    continue;
                }
                if (!characterObjectList.Contains(troopRosterElement.Character) && !troopRosterElement.Character.HeroObject.IsWounded)
                {
                    characterObjectList.Add(troopRosterElement.Character);
                }
                if (characterObjectList.Count >= maxParticipantCount)
                {
                    return;
                }
            }

            int iterationAddCount = 1;
            List<TroopRosterElement> troopRoster = troopRosterFull.Where(x => !x.Character.IsHero && x.Character.Tier >= 3 && x.WoundedNumber < x.Number).OrderByDescending(x => x.Character.Tier).ToList();
            while (iterationAddCount > 0)
            {
                iterationAddCount = 0;
                foreach (TroopRosterElement troopRosterElement in troopRoster)
                {
                    if (characterObjectList.Count(x => x == troopRosterElement.Character) < troopRosterElement.Number - troopRosterElement.WoundedNumber)
                    {
                        iterationAddCount++;
                        characterObjectList.Add(troopRosterElement.Character);
                    }
                    if (characterObjectList.Count >= maxParticipantCount)
                    {
                        return;
                    }
                }
            }
        }
    }

    #region Settings by Practice Mode

    public int GetPracticeModeChoiceCost() => GetPracticeModeChoiceCost(PracticeMode);

    public static int GetPracticeModeChoiceCost(ArenaPracticeMode practiceMode)
    {
        return practiceMode switch
        {
            ArenaPracticeMode.Parry => ModSettings.ParryPracticeSetupCost,
            ArenaPracticeMode.Team => ModSettings.TeamPracticeSetupCost,
            _ => 0,
        };
    }

    internal static int GetTotalParticipantsCount(ArenaPracticeMode practiceMode)
    {
        return practiceMode switch
        {
            ArenaPracticeMode.Standard => ModSettings.PracticeTotalParticipants, //30
            ArenaPracticeMode.Expansive => ModSettings.ExpansivePracticeTotalParticipants, //90
            ArenaPracticeMode.Parry => ModSettings.ParryPracticeTotalParticipants, //15
            ArenaPracticeMode.Team => ModSettings.TeamPracticeTotalParticipants, //300
            _ => throw new NotImplementedException(),
        };
    }

    internal static int GetActiveOpponentCount(ArenaPracticeMode practiceMode)
    {
        return practiceMode switch
        {
            ArenaPracticeMode.Standard => ModSettings.PracticeActiveParticipants, //6
            ArenaPracticeMode.Expansive => ModSettings.ExpansivePracticeActiveParticipants, //9
            ArenaPracticeMode.Parry => ModSettings.ParryPracticeActiveParticipants, //1
            ArenaPracticeMode.Team => ModSettings.TeamPracticeActiveParticipants, //30
            _ => throw new NotImplementedException(),
        };
    }

    internal static int GetMinimumActiveOpponentCount(ArenaPracticeMode practiceMode)
    {
        return practiceMode switch
        {
            ArenaPracticeMode.Standard => ModSettings.PracticeActiveParticipantsMinimum, //2
            ArenaPracticeMode.Expansive => ModSettings.ExpansivePracticeActiveParticipantsMinimum, //4
            ArenaPracticeMode.Parry => ModSettings.ParryPracticeActiveParticipantsMinimum, //1
            ArenaPracticeMode.Team => ModSettings.TeamPracticeActiveParticipantsMinimum, //20
            _ => throw new NotImplementedException(),
        };
    }

    internal static int GetInitialParticipantsCount(ArenaPracticeMode practiceMode)
    {
        return practiceMode switch
        {
            ArenaPracticeMode.Standard => ModSettings.PracticeInitialParticipants, //6
            ArenaPracticeMode.Expansive => ModSettings.ExpansivePracticeInitialParticipants, //7
            ArenaPracticeMode.Parry => ModSettings.ParryPracticeInitialParticipants, //1
            ArenaPracticeMode.Team => ModSettings.TeamPracticeInitialParticipants, //20
            _ => throw new NotImplementedException(),
        };
    }

    internal static int GetAITeamsCount(ArenaPracticeMode practiceMode)
    {
        int settingValue = practiceMode switch
        {
            ArenaPracticeMode.Standard => ModSettings.PracticeAITeamsCount, //0
            ArenaPracticeMode.Expansive => ModSettings.ExpansivePracticeAITeamsCount, //0
            ArenaPracticeMode.Parry => ModSettings.ParryPracticeAITeamsCount, //1
            ArenaPracticeMode.Team => ModSettings.TeamPracticeAITeamsCount, //4
            _ => throw new NotImplementedException(),
        };
        return settingValue > 0 ? settingValue : GetActiveOpponentCount(practiceMode);
    }

    internal static uint GetAITeamColor(ArenaPracticeMode practiceMode, int teamIndex)
    {
        if (practiceMode != ArenaPracticeMode.Team)
        {
            return uint.MaxValue;
        }

        //TODO: make settings for team colors with Team Practice header
        int colorIndex = teamIndex switch
        {
            0 => ModSettings.TeamPracticeTeamOneColor,
            1 => ModSettings.TeamPracticeTeamTwoColor,
            2 => ModSettings.TeamPracticeTeamThreeColor,
            3 => ModSettings.TeamPracticeTeamFourColor,
            4 => ModSettings.TeamPracticeTeamFiveColor,
            5 => ModSettings.TeamPracticeTeamSixColor,
            _ => -1
        };

        return colorIndex >= 0 ? BannerManager.GetColor(colorIndex) : uint.MaxValue;
    }

    internal bool IsPracticeModeEnabled() => IsPracticeModeEnabled(PracticeMode);

    internal static bool IsPracticeModeEnabled(ArenaPracticeMode practiceMode)
    {
        return practiceMode switch
        {
            ArenaPracticeMode.Standard => true,
            ArenaPracticeMode.Expansive => true,
            ArenaPracticeMode.Parry => ModSettings.ParryPracticeEnabled,
            ArenaPracticeMode.Team => ModSettings.TeamPracticeEnabled,
            _ => throw new NotImplementedException(),
        };
    }

    internal bool IsAgentSwitchingAllowed() => IsAgentSwitchingAllowed(PracticeMode);

    internal static bool IsAgentSwitchingAllowed(ArenaPracticeMode practiceMode)
    {
        return practiceMode switch
        {
            ArenaPracticeMode.Standard => ModSettings.PracticeEnableAgentSwitching,
            ArenaPracticeMode.Expansive => ModSettings.ExpansivePracticeEnableAgentSwitching,
            ArenaPracticeMode.Parry => ModSettings.ParryPracticeEnableAgentSwitching,
            ArenaPracticeMode.Team => ModSettings.TeamPracticeEnableAgentSwitching,
            _ => throw new NotImplementedException(),
        };
    }

    public bool IsWeaponChoiceAllowed()
    {
        return PracticeMode switch
        {
            ArenaPracticeMode.Standard => ModSettings.PracticeEnableLoadoutChoice,
            ArenaPracticeMode.Expansive => ModSettings.ExpansivePracticeEnableLoadoutChoice,
            ArenaPracticeMode.Parry => ModSettings.ParryPracticeEnableLoadoutChoice,
            ArenaPracticeMode.Team => ModSettings.TeamPracticeEnableLoadoutChoice,
            _ => throw new NotImplementedException(),
        };
    }

    public int GetWeaponLoadoutChoiceCost() => GetWeaponLoadoutChoiceCost(PracticeMode);

    public int GetWeaponLoadoutChoiceCost(ArenaPracticeMode practiceMode)
    {
        return practiceMode switch
        {
            ArenaPracticeMode.Standard => ModSettings.PracticeLoadoutChoiceCost,
            ArenaPracticeMode.Expansive => ModSettings.ExpansivePracticeLoadoutChoiceCost,
            ArenaPracticeMode.Parry => ModSettings.ParryPracticeLoadoutChoiceCost,
            ArenaPracticeMode.Team => ModSettings.TeamPracticeLoadoutChoiceCost,
            _ => throw new NotImplementedException(),
        };
    }

    public PracticeEquipmentType GetParticipantArmorType()
    {
        return PracticeMode switch
        {
            ArenaPracticeMode.Standard => ModSettings.PracticeEquipment,
            ArenaPracticeMode.Expansive => ModSettings.ExpansivePracticeEquipment,
            ArenaPracticeMode.Parry => ModSettings.ParryPracticeEquipment,
            ArenaPracticeMode.Team => ModSettings.TeamPracticeEquipment,
            _ => throw new NotImplementedException(),
        };
    }

    public MissionMode GetArenaPracticeMissionMode()
    {
        int practiceEquipmentSetting = PracticeMode switch
        {
            ArenaPracticeMode.Standard => (int) ModSettings.PracticeEquipment,
            ArenaPracticeMode.Expansive => (int) ModSettings.ExpansivePracticeEquipment,
            ArenaPracticeMode.Parry => (int) ModSettings.ParryPracticeEquipment,
            ArenaPracticeMode.Team => (int) ModSettings.TeamPracticeEquipment,
            _ => throw new NotImplementedException(),
        };
        return practiceEquipmentSetting switch
        {
            1 => MissionMode.Tournament,
            _ => MissionMode.Battle,
        };
    }

    public float GetPracticeExperienceRate()
    {
        return PracticeMode switch
        {
            ArenaPracticeMode.Standard => ModSettings.PracticeExperienceRate,
            ArenaPracticeMode.Expansive => ModSettings.ExpansivePracticeExperienceRate,
            ArenaPracticeMode.Parry => ModSettings.ParryPracticeExperienceRate,
            ArenaPracticeMode.Team => ModSettings.TeamPracticeExperienceRate,
            _ => throw new NotImplementedException(),
        };
    }

    #endregion Settings by Practice Mode

    internal void Sync()
    {
        CompanionLoadouts ??= [];
        WeaponLoadoutInformation ??= [];
        ParryWeaponLoadoutInformation ??= [];
        TournamentPrizes ??= [];
    }

    internal record class WeaponLoadoutInfo(string ItemIDs, int LoadoutStage, int LoadoutIndex, int OneHandedScore, int TwoHandedScore, int PolearmScore, int BowScore, int CrossbowScore, int ThrowingScore)
    {
        public int GetScore(Hero hero, WeaponPreference[] priorities, out bool weaponPreferenceIsMet)
        {
            weaponPreferenceIsMet = false;
            return
                GetWeightedScore(OneHandedScore, WeaponPreference.OneHanded, hero, priorities, ref weaponPreferenceIsMet) +
                GetWeightedScore(TwoHandedScore, WeaponPreference.TwoHanded, hero, priorities, ref weaponPreferenceIsMet) +
                GetWeightedScore(PolearmScore, WeaponPreference.Polearm, hero, priorities, ref weaponPreferenceIsMet) +
                GetWeightedScore(BowScore, WeaponPreference.Bow, hero, priorities, ref weaponPreferenceIsMet) +
                GetWeightedScore(CrossbowScore, WeaponPreference.Crossbow, hero, priorities, ref weaponPreferenceIsMet) +
                GetWeightedScore(ThrowingScore, WeaponPreference.Throwing, hero, priorities, ref weaponPreferenceIsMet);
        }

        private static int GetWeightedScore(int score, WeaponPreference weaponGroupToAccess, Hero hero, WeaponPreference[] priorities, ref bool weaponPreferenceIsMet)
        {
            int weightedScore = score * GetWeight(weaponGroupToAccess, hero, priorities, out bool groupWasPrioritized);
            if (weightedScore > 0 && groupWasPrioritized)
            {
                weaponPreferenceIsMet = true;
            }
            return weightedScore;
        }

        private static int GetWeight(WeaponPreference weaponGroupToAccess, Hero hero, WeaponPreference[] priorities, out bool groupWasPrioritized)
        {
            groupWasPrioritized = false;
            int count = priorities.Length;
            for (int i = 0; i < count; i++)
            {
                if (weaponGroupToAccess == priorities[i])
                {
                    groupWasPrioritized = true;
                    return 1000 + (count - i) * 100; //(int) Math.Pow(10, count - i + 2);
                }
            }
            return GetWeightBySkill(weaponGroupToAccess, hero);
        }

        private static int GetWeightBySkill(WeaponPreference weaponGroupToAccess, Hero hero)
        {
            int oneHandedSV = hero.GetSkillValue(DefaultSkills.OneHanded);
            int twoHandedSV = hero.GetSkillValue(DefaultSkills.TwoHanded);
            int polearmSV = hero.GetSkillValue(DefaultSkills.Polearm);
            int bowSV = hero.GetSkillValue(DefaultSkills.Bow);
            int crossbowSV = hero.GetSkillValue(DefaultSkills.Crossbow);
            int throwingSV = hero.GetSkillValue(DefaultSkills.Throwing);

            int weaponGroupSV = weaponGroupToAccess switch
            {
                WeaponPreference.OneHanded => oneHandedSV,
                WeaponPreference.TwoHanded => twoHandedSV,
                WeaponPreference.Polearm => polearmSV,
                WeaponPreference.Bow => bowSV,
                WeaponPreference.Crossbow => crossbowSV,
                WeaponPreference.Throwing => throwingSV,
                _ => 0,
            };

            List<int> list = [oneHandedSV, twoHandedSV, polearmSV, bowSV, crossbowSV, throwingSV];
            list.Sort();
            int heroSkillRange = list.Last() - list.First();

            return heroSkillRange > 0 ? 100 * (weaponGroupSV - list.First()) / heroSkillRange : 100;
        }
    }
}
