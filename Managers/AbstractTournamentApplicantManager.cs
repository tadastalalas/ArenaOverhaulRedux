using ArenaOverhaul.Core;
using ArenaOverhaul.Data;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace ArenaOverhaul.Managers;

public abstract class AbstractTournamentApplicantManager
{
    protected const int DefaultPlayerImportance = 200000;
    protected const int DefaultPartyLeaderImportance = 100000;

    protected const int DefaultHeroImportance = 10000;
    protected const int HeroImportanceStep = 2000;

    protected const int DefaultTroopImportance = 15000;
    protected const int TroopImportanceStep = 2000;

    protected static int GetImportance(CharacterObject character, bool isPartyLeader)
    {
        bool isHighStatus = isPartyLeader || (character.IsHero && character.HeroObject.IsLord);
        int baseImportance = character.IsHero ? DefaultHeroImportance + HeroImportanceStep * character.Level : DefaultTroopImportance + TroopImportanceStep * character.Tier;
        return (character.IsPlayerCharacter ? DefaultPlayerImportance : 0) + (isHighStatus ? DefaultPartyLeaderImportance : 0) + GetRandomizedImportance(baseImportance);
    }

    internal static int GetRandomizedImportance(int baseImportance)
    {
        return MBRandom.RandomInt(3 * baseImportance / 4, 5 * baseImportance / 4 + 1);
    }

    public static bool IsRightTypeOfHero(Hero hero, bool allowNotables = false) =>
        hero != null && !hero.IsDead && !hero.IsWounded && !hero.IsPrisoner && !hero.IsNoncombatant && !hero.IsChild && (hero.IsLord || hero.IsWanderer || (allowNotables && hero.IsNotable));

    protected static void GetUpgradeTargets(CharacterObject troop, List<CharacterObject> list)
    {
        if (!list.Contains(troop))
        {
            list.Add(troop);
        }
        foreach (CharacterObject? upgradeTarget in troop.UpgradeTargets)
        {
            if (upgradeTarget != null)
            {
                GetUpgradeTargets(upgradeTarget, list);
            }
        }
    }
}

public abstract class AbstractTournamentApplicantManager<Ta, Tg> : AbstractTournamentApplicantManager
    where Ta : AbstractTournamentApplicant<Tg>
    where Tg : MBObjectBase
{
    protected List<Ta> GetAllApplicants(Settlement settlement, bool includePlayer = true)
    {
        List<Ta> applicantCharacters = new();

        AddPlayerParyToApplicants(settlement, applicantCharacters, includePlayer);
        AddOtherPartiesToApplicants(settlement, applicantCharacters);
        AddPartylessHeroesToApplicants(settlement, applicantCharacters);

        return applicantCharacters.OrderByDescending(x => x.Importance).ToList();
    }

    protected List<Ta> FillUpApplicants(List<Ta> applicantCharacters, Settlement settlement, int requiredCount)
    {
        List<Ta> extraApplicantCharacters = AddExtraTroops(applicantCharacters);
        FillUpWithRandomTroops(settlement, extraApplicantCharacters, requiredCount);

        return extraApplicantCharacters.OrderByDescending(x => x.Importance).ToList();
    }

    public virtual void AddPlayerParyToApplicants(Settlement settlement, List<Ta> applicantCharacters, bool includePlayer = true)
    {
        if (!includePlayer || Hero.MainHero.IsPrisoner)
        {
            return;
        }
        applicantCharacters.Add(GetPlayerApplicant());

        MobileParty? mainParty = Hero.MainHero.PartyBelongedTo;
        if (mainParty is null || mainParty.Party?.MemberRoster is null)
        {
            return;
        }

        AddApplicantsFromRoster(settlement, applicantCharacters, mainParty);
    }

    public virtual void AddOtherPartiesToApplicants(Settlement settlement, List<Ta> applicantCharacters)
    {
        foreach (MobileParty? party in settlement.Parties)
        {
            Hero leaderHero = party.LeaderHero;
            if (leaderHero != null)
            {
                CharacterObject leaderCharacter = leaderHero.CharacterObject;
                if (CanBeAParticipant(leaderCharacter, applicantCharacters, true))
                    applicantCharacters.Add(GetApplicant(leaderCharacter, party, true));
            }
            if (party.Party?.MemberRoster is null)
                continue;

            AddApplicantsFromRoster(settlement, applicantCharacters, party);
        }
    }

    public virtual void AddPartylessHeroesToApplicants(Settlement settlement, List<Ta> applicantCharacters)
    {
        bool allowNotablesParticipation = ModSettings.TournamentAllowNotablesParticipation;
        for (int index = 0; index < settlement.HeroesWithoutParty.Count; ++index)
        {
            Hero hero = settlement.HeroesWithoutParty[index];
            CharacterObject characterObject = hero.CharacterObject;
            if (CanBeAParticipant(characterObject, applicantCharacters, true, allowNotablesParticipation))
            {
                if (hero.CurrentSettlement != settlement)
                {
                    Debug.Print(hero.StringId + " is in settlement.HeroesWithoutParty list but current settlement is not, tournament settlement: " + settlement.StringId);
                }
                applicantCharacters.Add(GetApplicant(characterObject, null, false));
            }
        }
    }

    public virtual List<Ta> AddExtraTroops(List<Ta> applicantCharacters)
    {
        List<Ta> extraApplicantCharacters = new();
        List<Ta> troopsList = applicantCharacters.Where(x => x.AvailableCount > 1).ToList();
        troopsList.ForEach(x =>
        {
            for (int index = x.AvailableCount - 1; index > 0; --index)
            {
                extraApplicantCharacters.Add(GetApplicant(x.CharacterObject, x.OriginParty, false));
            }
            x.AvailableCount = 1;
        });

        return extraApplicantCharacters;
    }

    public virtual void FillUpWithRandomTroops(Settlement? settlement, List<Ta> extraApplicantCharacters, int requiredCount)
    {
        CultureObject defaultCulture = Game.Current.ObjectManager.GetObject<CultureObject>("empire") ?? Game.Current.ObjectManager.GetObject<CultureObject>(x => x.IsMainCulture);
        CultureObject baseSettlementCulture = settlement?.Culture ?? defaultCulture;
        CultureObject settlementFactionCulture = settlement?.MapFaction?.Culture ?? baseSettlementCulture;

        List<(CharacterObject Character, int Weight)> troopList = GetTroopList([(defaultCulture, 1), (baseSettlementCulture, 10), (settlementFactionCulture, 10)], extraApplicantCharacters);
        if (troopList.Count <= 0)
        {
            (CultureObject x, int)[] allCulturesArray = Game.Current.ObjectManager.GetObjectTypeList<CultureObject>().Where(x => !x.IsBandit).Select(x => (x, 1)).ToArray();
            troopList = GetTroopList(allCulturesArray!, extraApplicantCharacters);
            if (troopList.Count <= 0)
            {
                return;
            }
        }

        while (extraApplicantCharacters.Count <= requiredCount)
        {
            CharacterObject characterObject = GetRandomElementFromWeightedList(troopList);
            extraApplicantCharacters.Add(GetApplicant(characterObject, null, false));
        }
    }

    public virtual Ta GetApplicant(CharacterObject character, MobileParty? mobileParty, bool isPartyLeader = false, int availableCount = 1)
    {
        int importance = GetImportance(character, isPartyLeader);
        return GetApplicantInternal(character, mobileParty, importance, availableCount);
    }

    public virtual Ta GetPlayerApplicant()
    {
        CharacterObject character = CharacterObject.PlayerCharacter;
        MobileParty? mobileParty = character.HeroObject.PartyBelongedTo;
        bool isPartyLeader = mobileParty != null && mobileParty.LeaderHero == character.HeroObject;

        int importance = GetImportance(character, isPartyLeader);
        return GetApplicantInternal(character, mobileParty, importance);
    }

    protected abstract Ta GetApplicantInternal(CharacterObject characterObject, MobileParty? originParty, int importance, int availableCount = 1);

    public bool CanBeAParticipant(CharacterObject character, List<Ta> applicantCharacters, bool considerSkills, bool allowNotables = false)
    {
        if (!character.IsHero)
            return (!considerSkills || character.Tier >= 3);

        Hero hero = character.HeroObject;
        return
            IsRightTypeOfHero(hero, allowNotables)
            && hero != Hero.MainHero
            && (!considerSkills || hero.GetSkillValue(DefaultSkills.OneHanded) >= 100 || hero.GetSkillValue(DefaultSkills.TwoHanded) >= 100 || hero.GetSkillValue(DefaultSkills.Polearm) >= 100)
            && !applicantCharacters.Any(applicant => applicant.CharacterObject == character);
    }

    private void AddApplicantsFromRoster(Settlement settlement, List<Ta> applicantCharacters, MobileParty party)
    {
        List<CharacterObject> upgradeTargets = new();
        foreach (TroopRosterElement troopRosterElement in party.Party.MemberRoster.GetTroopRoster())
        {
            ProcessRosterElement(applicantCharacters, upgradeTargets, party, troopRosterElement);
        }
        upgradeTargets = upgradeTargets.Where(x => CanBeAParticipant(x, applicantCharacters, true) && !applicantCharacters.Any(a => a.CharacterObject == x)).ToList();
        upgradeTargets.ForEach(upgradedTroopCaharacter =>
        {
            if ((upgradedTroopCaharacter.Culture != null && upgradedTroopCaharacter.Culture == settlement.Culture ? 1.0f : 0.33f) > MBRandom.RandomFloat)
            {
                applicantCharacters.Add(GetApplicant(upgradedTroopCaharacter, null, false));
            }
        });
    }

    private void ProcessRosterElement(List<Ta> applicantCharacters, List<CharacterObject> upgradeTargets, MobileParty? party, TroopRosterElement troopRosterElement)
    {
        CharacterObject character = troopRosterElement.Character;
        if (character.IsHero)
        {
            if (CanBeAParticipant(character, applicantCharacters, true))
            {
                applicantCharacters.Add(GetApplicant(character, party, false));
            }
        }
        else
        {
            int availableCount = troopRosterElement.Number - troopRosterElement.WoundedNumber;
            if (availableCount > 0 && CanBeAParticipant(character, applicantCharacters, true))
            {
                applicantCharacters.Add(GetApplicant(character, party, false, availableCount));
            }
            GetUpgradeTargets(character, upgradeTargets);
        }
    }

    private static CharacterObject GetRandomElementFromWeightedList(List<(CharacterObject Character, int Weight)> troopList)
    {
        int totalWeight = troopList.Sum(x => x.Weight);
        int randomNumber = MBRandom.RandomInt(totalWeight);

        foreach ((CharacterObject? character, int weight) in troopList)
        {
            if (randomNumber < weight)
            {
                return character;
            }
            randomNumber -= weight;
        }

        // If we reach this point, it means there's a problem with the weights.
        return troopList.GetRandomElementInefficiently().Character;
    }

    private List<(CharacterObject Character, int Weight)> GetTroopList((CultureObject? Culture, int Weight)[] cultures, List<Ta> extraApplicantCharacters)
    {
        List<(CharacterObject Character, int Weight)> list = new List<(CharacterObject Character, int Weight)>();
        foreach ((CultureObject? cultureObject, int weight) in cultures)
        {
            if (cultureObject != null)
            {
                if (cultureObject.BasicTroop is CharacterObject basicTroop)
                {
                    list.Add((basicTroop, weight * 5));
                }
                if (cultureObject.EliteBasicTroop is CharacterObject eliteBasicTroop)
                {
                    list.Add((eliteBasicTroop, weight * 3));
                }
                list.AddRange(cultureObject.BasicMercenaryTroops.Where(x => x != null).Select(x => (x, weight * 2)));
            }
        }

        foreach ((CharacterObject? character, int weight) in list.ToList())
        {
            List<CharacterObject> upgradesList = new List<CharacterObject>();
            GetUpgradeTargets(character, upgradesList);
            list.AddRange(upgradesList.Select(x => (x, weight)));
        }

        list = list.Where(x => CanBeAParticipant(x.Character, extraApplicantCharacters, true)).OrderBy(x => x.Weight).ThenBy(x => x.Character.Tier).ToList();
        return list;
    }
}