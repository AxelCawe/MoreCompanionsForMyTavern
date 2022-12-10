using MoreCompanionsForMyTavern.Behavior;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MoreCompanionsForMyTavern
{
    public class SubModule : MBSubModuleBase
    {

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);
            AddBehaviors(gameStarter as CampaignGameStarter);
        }

        private void AddBehaviors(CampaignGameStarter gameStarter)
        {
            //gameStarter.AddBehavior(new SaveDataBehaviour());
            gameStarter.AddBehavior(new SpawnCompanionsBehavior());
        }
    }
}
