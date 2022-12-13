using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using MCM.Abstractions.FluentBuilder;
using MoreCompanionsForMyTavern.Behavior;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreCompanionsForMyTavern.Settings
{
    internal class MCMSettings : AttributeGlobalSettings<MCMSettings>, ISettingVariables
    {
        public override string Id => "MoreCompanionsForMyTavern";

        public override string DisplayName => "More Companions For My Tavern";

        public override string FolderName => "MoreCompanionsForMyTavern";

        public override string FormatType => "xml";


        [SettingPropertyBool("{=settings_overrideAge}Should override age?", Order = 0, RequireRestart = false, HintText = "{=settings_overrideAgeDesc}Whether the mod should override the age of generated wanderers. If unticked, companions will follow template's age.", IsToggle = true)]
        [SettingPropertyGroup("{=settings_overrideAge}Should override age?", GroupOrder = 0)]
        public bool overrideAge { get; set; } = false;
        [SettingPropertyInteger("{=settings_minCompanionAge}Minimum age of wanderers to spawn", minValue: GlobalModSettings.minMinCompanionAge, maxValue: GlobalModSettings.maxMinCompanionAge, Order = 1, HintText = "{=settings_minCompanionAgeDesc}Minimum age of wanderers that can be spawned.", RequireRestart = false)]
        [SettingPropertyGroup("{=settings_overrideAge}Should override age?", GroupOrder = 0)]
        public int minCompanionAge { get; set; } = 18;
        [SettingPropertyInteger("{=settings_maxCompanionAge}Maximum age of wanderers to spawn", minValue: GlobalModSettings.minMaxCompanionAge, maxValue: GlobalModSettings.maxMaxCompanionAge, Order = 2, HintText = "{=settings_maxCompanionAgeDesc}Maximum age of wanderers that can be spawned.", RequireRestart = false)]
        [SettingPropertyGroup("{=settings_overrideAge}Should override age?", GroupOrder = 0)]
        public int maxCompanionAge { get; set; } = 40;
        [SettingPropertyInteger("{=settings_maxCompanions}Maximum number of wanderers in town", minValue: GlobalModSettings.minCompanionsPerTown, maxValue: GlobalModSettings.maxCompanionsPerTown, Order = 3, HintText = "{=settings_maxCompanionsDesc}The maximum number of wanderers that can be in a town at once. Expect lag/temporary freezes if you set a high number.", RequireRestart = false)]
        public int maxCompanionsPerTown { get; set; } = 40;

        [SettingPropertyBool("{=settings_refreshWeekly}Auto-refresh Wanderers weekly", Order = 4, HintText = "{=settings_refreshWeeklyDesc}Automatically refreshes the wanderers at the start of every week. May cause lag!")]
        public bool autoRefreshEveryWeek { get; set; } = false;
        
        [SettingPropertyButton("{=settings_refresh}Refresh Wanderers", Content = "{=settings_refreshButtonContent}Press to refresh", Order = 5, RequireRestart = false, HintText = "{=settings_refreshDesc}Removes all current wanderers and spawn new ones. MAY CAUSE TEMPORARY FREEZE, NO NEED TO WORRY! :D")]
        public Action RefreshWanderers{ get; set; }
    }
}
