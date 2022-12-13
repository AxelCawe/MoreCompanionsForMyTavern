using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreCompanionsForMyTavern.Settings
{
    internal interface ISettingVariables
    {
        bool overrideAge { get; set; }
        int minCompanionAge { get; set; }
        int maxCompanionAge { get; set; }
        int maxCompanionsPerTown { get; set;} 

        bool autoRefreshEveryWeek { get; set; }
    }
}
