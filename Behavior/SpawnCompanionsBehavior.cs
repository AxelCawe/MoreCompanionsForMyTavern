using MoreCompanionsForMyTavern.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using TaleWorlds.LinQuick;
using MCM.Abstractions.Base.Global;
using TaleWorlds.Library;
using Helpers;

namespace MoreCompanionsForMyTavern.Behavior
{
    internal class SpawnCompanionsBehavior : CampaignBehaviorBase
    {
        private Dictionary<CharacterObject, int> _companionTemplates;

        public SpawnCompanionsBehavior()
        {
            _companionTemplates = new Dictionary<CharacterObject, int>();
            GlobalSettings<MCMSettings>.Instance.RefreshWanderers = this.RefreshCompanions;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnGameLoaded));
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(DailyTick));
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, new Action(WeeklyTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
            //throw new NotImplementedException();
        }

        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            this.InitializeCompanionTemplateList();
            DailyTick();
        }

        private void DailyTick()
        {
            //InitializeCompanionTemplateList();
            this.SwapCompanions();
            this.SpawnNewCompanionIfNeeded();
        }

        private void WeeklyTick()
        {
            if (GlobalSettings<MCMSettings>.Instance.autoRefreshEveryWeek)
                this.RefreshCompanions();
        }

        private void InitializeCompanionTemplateList()
        {
            this._companionTemplates = new Dictionary<CharacterObject, int>();
            foreach (CultureObject cultureObject in MBObjectManager.Instance.GetObjectTypeList<CultureObject>())
            {
                if (cultureObject.IsMainCulture)
                {
                    foreach (CharacterObject key in cultureObject.NotableAndWandererTemplates.WhereQ((CharacterObject x) => x.Occupation == Occupation.Wanderer))
                    {
                        this._companionTemplates.Add(key, 0);
                    }
                }
            }
            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                if (hero.IsWanderer)
                {
                    if (this._companionTemplates.ContainsKey(hero.Template))
                    {
                        Dictionary<CharacterObject, int> companionTemplates = this._companionTemplates;
                        CharacterObject template = hero.Template;
                        int num = companionTemplates[template];
                        companionTemplates[template] = num + 1;
                    }
                    else
                    {
                        this._companionTemplates.Add(hero.Template, 1);
                    }
                }
            }
            foreach (Hero hero2 in Hero.DeadOrDisabledHeroes)
            {
                if (hero2.IsWanderer)
                {
                    if (this._companionTemplates.ContainsKey(hero2.Template))
                    {
                        Dictionary<CharacterObject, int> companionTemplates2 = this._companionTemplates;
                        CharacterObject template = hero2.Template;
                        int num = companionTemplates2[template];
                        companionTemplates2[template] = num + 1;
                    }
                    else
                    {
                        this._companionTemplates.Add(hero2.Template, 1);
                    }
                }
            }
        }

        public void RefreshCompanions()
        {
            foreach (Town townElement in Town.AllTowns)
            {
                var allWanderers = Enumerable.Where<Hero>(townElement.Settlement.HeroesWithoutParty, (Hero x) => x.IsWanderer && x.CompanionOf == null).ToList();
                foreach (Hero heroElement in allWanderers)
                {
                    LeaveSettlementAction.ApplyForCharacterOnly(heroElement);
                    heroElement.AddDeathMark(null, KillCharacterAction.KillCharacterActionDetail.Lost);
                    heroElement.ChangeState(Hero.CharacterStates.Disabled);
                }
            }

            SpawnNewCompanionIfNeeded();
        }

        private void SwapCompanions()
        {
            //RemoveCompanions();

            int num = Town.AllTowns.Count / 5;
            int num2 = MBRandom.RandomInt(Town.AllTowns.Count % 5);
            Town town = Town.AllTowns[num2 + MBRandom.RandomInt(num)];
            Hero hero = Enumerable.Where<Hero>(town.Settlement.HeroesWithoutParty, (Hero x) => x.IsWanderer && x.CompanionOf == null).GetRandomElementInefficiently<Hero>();
            for (int i = 1; i < 5; i++)
            {
                Town town2 = Town.AllTowns[i * num + num2 + MBRandom.RandomInt(num)];
                IEnumerable<Hero> enumerable = Enumerable.Where<Hero>(town2.Settlement.HeroesWithoutParty, (Hero x) => x.IsWanderer && x.CompanionOf == null);
                Hero hero2 = null;
                if (Enumerable.Any<Hero>(enumerable))
                {
                    hero2 = enumerable.GetRandomElementInefficiently<Hero>();
                    LeaveSettlementAction.ApplyForCharacterOnly(hero2);
                }
                if (hero != null)
                {
                    EnterSettlementAction.ApplyForCharacterOnly(hero, town2.Settlement);
                }
                hero = hero2;
            }
            if (hero != null)
            {
                EnterSettlementAction.ApplyForCharacterOnly(hero, town.Settlement);
            }
        }

        private void SpawnNewCompanionIfNeeded()
        {
            InitializeCompanionTemplateList();
            foreach (Town town in Town.AllTowns)
            {
                int wandererCount = 0;
                Location tavern = town.Settlement.LocationComplex.GetLocationWithId("tavern");
                foreach (var character in town.Settlement.HeroesWithoutParty)
                {
                    if (character.IsWanderer && character.CompanionOf == null && character.DeathMark != KillCharacterAction.KillCharacterActionDetail.Lost)
                        ++wandererCount;
                }
        
                if (wandererCount < GlobalSettings<MCMSettings>.Instance.maxCompanionsPerTown)
                {
                    int companionsToSpawn = GlobalSettings<MCMSettings>.Instance.maxCompanionsPerTown - wandererCount;
                    for (int i = 0; i < companionsToSpawn; ++i)
                    {
                        CreateCompanionAndAddToSettlement(town.Settlement);
                    }
                }
            }
        }

        private void CreateCompanionAndAddToSettlement(Settlement settlement)
        {
            List<ValueTuple<CharacterObject, float>> list = new List<ValueTuple<CharacterObject, float>>();
            foreach (KeyValuePair<CharacterObject, int> keyValuePair in this._companionTemplates)
            {
                CharacterObject key = keyValuePair.Key;
                int value = keyValuePair.Value;
                float num = 2f / ((float)Math.Pow((double)value, 2.0) + 0.1f);
                list.Add(new ValueTuple<CharacterObject, float>(key, num));
            }
            CharacterObject companionTemplate = MBRandom.ChooseWeighted<CharacterObject>(list);
            Dictionary<CharacterObject, int> companionTemplates = this._companionTemplates;
            CharacterObject companionTemplate2 = companionTemplate;
            int num2 = companionTemplates[companionTemplate2];
            companionTemplates[companionTemplate2] = num2 + 1;
            Town randomElementWithPredicate = Town.AllTowns.GetRandomElementWithPredicate((Town x) => x.Culture == companionTemplate.Culture);
            Settlement settlement2 = (randomElementWithPredicate != null) ? randomElementWithPredicate.Settlement : null;
            if (settlement2 != null)
            {
                List<Settlement> list2 = new List<Settlement>();
                foreach (Village village in Village.All)
                {
                    if (Campaign.Current.Models.MapDistanceModel.GetDistance(village.Settlement, settlement2) < 30f)
                    {
                        list2.Add(village.Settlement);
                    }
                }
                settlement2 = ((list2.Count > 0) ? list2.GetRandomElement<Settlement>().Village.Bound : settlement2);
            }
            else
            {
                settlement2 = Town.AllTowns.GetRandomElement<Town>().Settlement;
            }

            int age = 0;
            if (GlobalSettings<MCMSettings>.Instance.minCompanionAge > GlobalSettings<MCMSettings>.Instance.maxCompanionAge)
                age = MBRandom.RandomInt(GlobalSettings<MCMSettings>.Instance.maxCompanionAge, GlobalSettings<MCMSettings>.Instance.minCompanionAge);
            else
                age = MBRandom.RandomInt(GlobalSettings<MCMSettings>.Instance.minCompanionAge, GlobalSettings<MCMSettings>.Instance.maxCompanionAge);
            Hero hero = HeroCreator.CreateSpecialHero(companionTemplate, settlement2, null, null, age);
            if (GlobalSettings<MCMSettings>.Instance.overrideAge)
                hero.SetBirthDay(HeroHelper.GetRandomBirthDayForAge(age));
            else
            {
                age = Campaign.Current.Models.AgeModel.HeroComesOfAge + 5 + MBRandom.RandomInt(27);
                hero.SetBirthDay(HeroHelper.GetRandomBirthDayForAge(age));
            }
            this.AdjustEquipment(hero);
            hero.ChangeState(Hero.CharacterStates.Active);
            EnterSettlementAction.ApplyForCharacterOnly(hero, settlement);
        }

        private void AdjustEquipment(Hero hero)
        {
            this.AdjustEquipmentImp(hero.BattleEquipment);
            this.AdjustEquipmentImp(hero.CivilianEquipment);
        }

        // Token: 0x060033BF RID: 13247 RVA: 0x000DB534 File Offset: 0x000D9734
        private void AdjustEquipmentImp(Equipment equipment)
        {
            ItemModifier @object = MBObjectManager.Instance.GetObject<ItemModifier>("companion_armor");
            ItemModifier object2 = MBObjectManager.Instance.GetObject<ItemModifier>("companion_weapon");
            ItemModifier object3 = MBObjectManager.Instance.GetObject<ItemModifier>("companion_horse");
            for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumEquipmentSetSlots; equipmentIndex++)
            {
                EquipmentElement equipmentElement = equipment[equipmentIndex];
                if (equipmentElement.Item != null)
                {
                    if (equipmentElement.Item.ArmorComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, @object, null, false);
                    }
                    else if (equipmentElement.Item.HorseComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, object3, null, false);
                    }
                    else if (equipmentElement.Item.WeaponComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, object2, null, false);
                    }
                }
            }
        }
    }
}
