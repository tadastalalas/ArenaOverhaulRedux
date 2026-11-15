using ArenaOverhaul.Managers;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace ArenaOverhaul.Data;

internal class ArenaOverhaulSaveableTypeDefiner : SaveableTypeDefiner
{
    public ArenaOverhaulSaveableTypeDefiner() : base(2003477000) { }

    protected override void DefineClassTypes()
    {
        //BasicSavableClasses (1 through 99)
        base.AddClassDefinition(typeof(PrizeItemInfo), 1);
        base.AddClassDefinition(typeof(GrandTournamentGame), 2);
        base.AddClassDefinition(typeof(GrandTournamentMapNotification), 3);

        //BehaviorManagers (100 through 150)
        base.AddClassDefinition(typeof(AOArenaBehaviorManager), 100);
    }

    protected override void DefineContainerDefinitions()
    {
        base.ConstructContainerDefinition(typeof(Dictionary<Town, PrizeItemInfo?>));
        base.ConstructContainerDefinition(typeof(Dictionary<Town, CampaignTime>));
    }
}