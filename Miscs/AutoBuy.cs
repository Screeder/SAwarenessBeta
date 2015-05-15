using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SAssemblies;
using SAssemblies.Miscs;
using SAssemblies.Trackers;
using Menu = SAssemblies.Menu;

//Extend Picture with place for item panel
//left click on item panel item -> choose catergory -> choose item -> add item
//right lick on item panel item -> delete item

namespace SAwareness.Miscs
{
    class AutoBuy
    {

        public static Menu.MenuItemSettings AutoBuyMisc = new Menu.MenuItemSettings(typeof(AutoBuy));

        public AutoBuy()
        {

        }

        ~AutoBuy()
        {

        }

        public bool IsActive()
        {
            return Misc.Miscs.GetActive() && AutoBuyMisc.GetActive();
        }

        public static Menu.MenuItemSettings SetupMenu(LeagueSharp.Common.Menu menu)
        {
            AutoBuyMisc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu(Language.GetString("MISCS_AUTOBUY_MAIN"), "SAssembliesMiscsAutoBuy"));
            AutoBuyMisc.MenuItems.Add(
                AutoBuyMisc.Menu.AddItem(new MenuItem("SAssembliesMiscsAutoBuyActive", Language.GetString("GLOBAL_ACTIVE")).SetValue(false)));
            return AutoBuyMisc;
        }

        [Serializable]
        class Build
        {
            public String Name;
            private List<ItemId> _itemId = new List<ItemId>(36);

            public Build(string name)
            {
                Name = name;
                _itemId = new List<ItemId>();
            }

            public Build(string name, List<ItemId> itemId)
            {
                Name = name;
                _itemId = itemId;
            }

        }

        class ItemInfo
        {
            public enum ItemType
            {
                Jungle,
                Lane,
                Consumable,
                Vision,
                GoldIncome,
                Armor,
                MagicResist,
                Health,
                HealthRegen,
                AttackSpeed,
                CiritcialStrike,
                Damage,
                Lifesteal,
                CooldownReduction,
                Mana,
                ManaRegen,
                AbilityPower,
                Boots,
                MovementSpeed,
                SpellVampire,
                ArmorPenetration,
                MagicPenetration,
                Active,
                Aura,
                Slow,
                Stealth,
                Trinket,
                Tenacity,
                OnHit
            }

            public ItemId ItemId;
            public int Price;
            public List<ItemType> ItemTypeList;
            public List<GameMapId> MapId;
            public String Description;
            public List<ItemId> FromItemIds = new List<ItemId>();
            public List<ItemId> ToItemIds = new List<ItemId>();
            public int TotalGold;
        }

        class AutoBuyGUI
        {
            
        }

    }
}
