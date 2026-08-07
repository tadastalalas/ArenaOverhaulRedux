using System;
using ArenaOverhaul.CampaignBehaviors.BehaviorManagers;
using ArenaOverhaul.ModSettings;
using ArenaOverhaul.Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace ArenaOverhaul.Models
{
    public class ArenaOverhaulTournamentModel : TournamentModel
    {
        private readonly TournamentModel _previouslyAssignedModel;

        public ArenaOverhaulTournamentModel(TournamentModel previouslyAssignedModel)
        {
            _previouslyAssignedModel = previouslyAssignedModel;
        }

        public override Equipment GetParticipantArmor(CharacterObject participant)
        {
            var practiceEquipmentSetting = AOArenaBehaviorManager.Instance!.GetParticipantArmorType();
            var practiceEquipment = practiceEquipmentSetting switch
            {
                PracticeEquipmentType.PracticeClothes => GetRandomPracticeClothes(),
                PracticeEquipmentType.CivilianEquipment => participant.RandomCivilianEquipment,
                PracticeEquipmentType.BattleEquipment => participant.RandomBattleEquipment,
                _ => null,
            };

            var equipment = Mission.Current.Mode != MissionMode.Tournament ? practiceEquipment : null;
            return equipment ?? _previouslyAssignedModel.GetParticipantArmor(participant);
        }

        public override TournamentGame CreateTournament(Town town) => _previouslyAssignedModel.CreateTournament(town);

        public override int GetInfluenceReward(Hero winner, Town town) => _previouslyAssignedModel.GetInfluenceReward(winner, town);

        public override int GetNumLeaderboardVictoriesAtGameStart() => _previouslyAssignedModel.GetNumLeaderboardVictoriesAtGameStart();

        public override int GetRenownReward(Hero winner, Town town) => _previouslyAssignedModel.GetRenownReward(winner, town);

        public override (SkillObject skill, int xp) GetSkillXpGainFromTournament(Town town) => _previouslyAssignedModel.GetSkillXpGainFromTournament(town);

        public override float GetTournamentEndChance(TournamentGame tournament) => _previouslyAssignedModel.GetTournamentEndChance(tournament);

        public override float GetTournamentSimulationScore(CharacterObject character) => _previouslyAssignedModel.GetTournamentSimulationScore(character);

        public override float GetTournamentStartChance(Town town) => _previouslyAssignedModel.GetTournamentStartChance(town);

        public override MBList<ItemObject> GetRegularRewardItems(Town town, int regularRewardMinValue, int regularRewardMaxValue)
        {
            //Prize scaling - replaces the Native hardcoded 1600/5000 value bounds (old CachePossibleRegularRewardItems transpiler)
            var rewardItems = _previouslyAssignedModel.GetRegularRewardItems(town, GetRegularRewardItemMinValue(), GetRegularRewardItemMaxValue());

            //Cultural restriction of banner rewards (old CachePossibleBannerItems postfix, isElite: false)
            if (Settings.Instance!.CultureRestrictedTournamentPrizes.SelectedIndex >= 1 && town.Culture is CultureObject townCulture)
            {
                FilterByCulture(rewardItems, townCulture, ignoreEverythingButBanners: true);
            }
            return rewardItems;
        }

        public override MBList<ItemObject> GetEliteRewardItems(Town town, int regularRewardMinValue, int regularRewardMaxValue)
        {
            int culturalPrizesSelectedIndex = Settings.Instance!.CultureRestrictedTournamentPrizes.SelectedIndex;

            MBList<ItemObject> rewardItems;
            if (Settings.Instance!.EnableTournamentPrizeScaling && Clan.PlayerClan.Tier >= 2)
            {
                //Custom high quality reward pool (old CachePossibleEliteRewardItems prefix)
                rewardItems = BuildScaledEliteRewardItems(town, culturalPrizesSelectedIndex);
            }
            else
            {
                rewardItems = _previouslyAssignedModel.GetEliteRewardItems(town, regularRewardMinValue, regularRewardMaxValue);
                //Old CachePossibleEliteRewardItems postfix
                if (culturalPrizesSelectedIndex > 1 && town.Culture is CultureObject postfixCulture)
                {
                    FilterByCulture(rewardItems, postfixCulture, ignoreMounts: culturalPrizesSelectedIndex == 2);
                }
            }

            //Fix Native banner sorting bug and apply cultural restrictions regardless of scaling (old CachePossibleBannerItems postfix, isElite: true)
            if (culturalPrizesSelectedIndex >= 1 && town.Culture is CultureObject townCulture)
            {
                FilterByCulture(rewardItems, townCulture, ignoreEverythingButBanners: true);
            }
            else
            {
                rewardItems.Sort((x, y) => x.Value.CompareTo(y.Value));
            }
            return rewardItems;
        }

        private static MBList<ItemObject> BuildScaledEliteRewardItems(Town town, int culturalPrizesSelectedIndex)
        {
            var itemObjectList = new MBList<ItemObject>();
            var townCulture = town.Culture;
            CultureObject? requiredCulture = culturalPrizesSelectedIndex >= 1 ? townCulture : null;
            List<ItemObject>? itemObjectCandidates = default;

            //Top 10 most valued standard items by type, with town culture
            if (requiredCulture != null)
            {
                itemObjectCandidates =
                    Items.All
                    .Where(itemObject => IsSuitablePrize(itemObject, requiredCulture)).ToList()
                    .GroupBy(itemObject => itemObject.Type)
                    .Select(accessGroup => (GroupKey: accessGroup.Key, TopTenItems: accessGroup.OrderByDescending(subg => subg.Value).Take(10)))
                    .SelectMany(x => x.TopTenItems.Select(y => (x.GroupKey, Item: y)))
                    .Select(x => x.Item).ToList();
            }

            //Fallback without cultural restrictions - top 5 most valued by type and culture
            if (itemObjectCandidates is null || itemObjectCandidates.Count <= 0)
            {
                itemObjectCandidates =
                    Items.All
                    .Where(itemObject => IsSuitablePrize(itemObject, null)).ToList()
                    .GroupBy(itemObject => (itemObject.Type, itemObject.Culture))
                    .Select(accessGroup => (GroupKey: accessGroup.Key, TopFiveItems: accessGroup.OrderByDescending(subg => subg.Value).Take(5)))
                    .SelectMany(x => x.TopFiveItems.Select(y => (x.GroupKey, Item: y)))
                    .Select(x => x.Item).ToList();
            }
            itemObjectList.AddRange(itemObjectCandidates);

            //Unique weapons and armors
            requiredCulture = culturalPrizesSelectedIndex >= 2 ? townCulture : null;
            itemObjectList.AddRange(Items.All.Where(itemObject => IsUniqueWeapon(itemObject, requiredCulture)));
            itemObjectList.AddRange(Items.All.Where(itemObject => IsUniqueArmor(itemObject, requiredCulture)));

            //Unique mounts
            requiredCulture = culturalPrizesSelectedIndex == 3 ? townCulture : null;
            itemObjectList.AddRange(Items.All.Where(itemObject => IsUniqueMount(itemObject, requiredCulture)));

            return itemObjectList;
        }

        private static Equipment? GetRandomPracticeClothes()
        {
            if (CampaignMission.Current is not { } misson || misson.Mode != MissionMode.Battle || Settlement.CurrentSettlement is not { } settlement || AOArenaBehaviorManager.Instance!.PracticeMode != ArenaPracticeMode.Team)
            {
                return null;
            }

            var settlementCultureId = settlement.MapFaction?.Culture?.StringId ?? "empire";
            var dummyCharacter = Game.Current.ObjectManager.GetObject<CharacterObject>("gear_team_practice_dummy_" + settlementCultureId);
            return dummyCharacter?.RandomBattleEquipment;
        }
        
        private static void FilterByCulture(List<ItemObject> list, CultureObject townCulture, bool ignoreMounts = false, bool ignoreEverythingButBanners = false)
        {
            if (list.Count > 0)
            {
                var filteredList = list.Where(x => x.Culture is null || !x.Culture.CanHaveSettlement || x.Culture == townCulture || (ignoreMounts && x.IsMountable) || (ignoreEverythingButBanners && !x.HasBannerComponent)).ToList();
                filteredList.Sort((x, y) => x.Value.CompareTo(y.Value));
                list.Clear();
                list.AddRange(filteredList);
            }
        }

        private static bool IsSuitablePrize(ItemObject itemObject, CultureObject? requiredCulture) =>
            (itemObject.Tier is ItemObject.ItemTiers.Tier5 or ItemObject.ItemTiers.Tier6)
            && itemObject.Value <= GetMaxItemValueForElitePrize()
            && (requiredCulture is null || itemObject.Culture is null || itemObject.Culture == requiredCulture || !itemObject.Culture.CanHaveSettlement)
            && !itemObject.NotMerchandise
            && (itemObject.IsCraftedWeapon || itemObject.IsMountable || itemObject.ArmorComponent != null)
            && !itemObject.IsCraftedByPlayer;

        private static bool IsUniqueItemObject(ItemObject itemObject, CultureObject? requiredCulture) =>
            itemObject.NotMerchandise && (int) itemObject.Tier >= 2 && !itemObject.IsCraftedByPlayer
            && (requiredCulture is null || itemObject.Culture is null || itemObject.Culture == requiredCulture || !itemObject.Culture.CanHaveSettlement);

        private static bool IsUniqueWeapon(ItemObject itemObject, CultureObject? requiredCulture) =>
            IsUniqueItemObject(itemObject, requiredCulture)
            && itemObject.IsCraftedWeapon
            //Remove practice weapons
            && !itemObject.StringId.EndsWith("_blunt")
            && !itemObject.StringId.StartsWith("practice_")
            //Remove cheap weapons
            && !itemObject.StringId.StartsWith("peasant_");

        private static bool IsUniqueArmor(ItemObject itemObject, CultureObject? requiredCulture) =>
            IsUniqueItemObject(itemObject, requiredCulture)
            && itemObject.ArmorComponent != null
            //Remove practice armors
            && !itemObject.StringId.StartsWith("dummy_")
            //Remove magic armors
            && itemObject.StringId != "celtic_frost" && itemObject.StringId != "saddle_of_aeneas" && itemObject.StringId != "fortunas_choice";

        private static bool IsUniqueMount(ItemObject itemObject, CultureObject? requiredCulture) => IsUniqueItemObject(itemObject, requiredCulture) && itemObject.IsMountable;
        
        internal static int GetRegularRewardItemMinValue()
        {
            return Settings.Instance!.EnableTournamentPrizeScaling ? 1600 + GetRenownRegularRewardItemValueIncrease(true) : 1600;
        }

        internal static int GetRegularRewardItemMaxValue()
        {
            return Settings.Instance!.EnableTournamentPrizeScaling ? 5000 + GetRenownRegularRewardItemValueIncrease() : 5000;
        }

        private static int GetRenownRegularRewardItemValueIncrease(bool isForMinValue = false)
        {
            int playerRenown = MathHelper.GetSoftCappedValue(Clan.PlayerClan.Renown);
            return Clan.PlayerClan.Tier switch
            {
                0 => isForMinValue ? 0 : 0,
                1 => isForMinValue ? 400 : 0,
                2 => isForMinValue ? 1400 : 1000,
                3 => isForMinValue ? 1400 : 2000,
                4 => isForMinValue ? 2400 : 3000,
                5 => isForMinValue ? 2400 : 4000,
                _ => isForMinValue ? 3400 : 5000 + Math.Max(playerRenown - (Campaign.Current.Models.ClanTierModel is DefaultClanTierModel clanTierModel ? FieldAccessHelper.DCTMTierLowerRenownLimitsByRef()[6] : 6000), 0) * 10
            };
        }

        private static int GetMaxItemValueForElitePrize() =>
            Clan.PlayerClan.Tier switch
            {
                < 3 => 50000,
                3 => 200000,
                4 => 300000,
                5 => 400000,
                _ => 500000,
            };
    }
}