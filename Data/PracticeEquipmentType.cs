using ArenaOverhaul.Core;

using System.ComponentModel;

namespace ArenaOverhaul.Data;

public enum PracticeEquipmentType : byte
{
    [Description(Texts.Ids.PracticeEquipmentTypePracticeClothes)]
    PracticeClothes = 0,
    [Description(Texts.Ids.PracticeEquipmentTypeTournamentArmor)]
    TournamentArmor = 1,
    [Description(Texts.Ids.PracticeEquipmentTypeCivilianEquipment)]
    CivilianEquipment = 2,
    [Description(Texts.Ids.PracticeEquipmentTypeBattleEquipment)]
    BattleEquipment = 3
}